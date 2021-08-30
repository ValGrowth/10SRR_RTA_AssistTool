using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TenSRR_RTA_AssistTool
{
	class CourseNumAnalyzer
	{
		private const int ORIGINAL_WIDTH = 1920;
		private const int ORIGINAL_HEIGHT = 1080;
		private const int BIN_THRESH = 128; // ２値画像で１になるしきい値
		private const int X_SPAN = 20; // TODO: 数字同士のX座標の間隔の画素数
		private const int SCAN_INTERVAL = 2; // 数字画像の一致チェックでスキャンするときの１ステップでの移動距離
		private const int SCAN_INTERVAL2 = SCAN_INTERVAL * SCAN_INTERVAL;
		private const double NUM_RATE_THRESH = 0.9; // コインの数字検出で検出される一致度の下限
		private const int ALLOWED_DIFF = 150; // 一致とみなされるための画素値の差の合計の上限(RGB画像を比較するとき)

		private Bitmap[] mNumberImagesOriginal = new Bitmap[10];
		private FastBitmap[] mNumberImages = new FastBitmap[10];
		private List<List<List<Color>>> mNumRGBs = new List<List<List<Color>>>();
		private List<List<List<bool>>> mNumBinFlgs = new List<List<List<bool>>>();
		private int[] mNumTotalPixel = new int[10];
		private int[] mNumAllowPixel = new int[10];
		private int mNumMaxHeight;
		private int mNumMaxWidth;

		private Bitmap[] mIGTNumberImagesOriginal = new Bitmap[10];
		private FastBitmap[] mIGTNumberImages = new FastBitmap[10];
		private List<List<List<Color>>> mIGTNumRGBs = new List<List<List<Color>>>();
		private List<List<List<bool>>> mIGTNumBinFlgs = new List<List<List<bool>>>();
		private int[] mIGTNumTotalPixel = new int[10];
		private int[] mIGTNumAllowPixel = new int[10];
		private int mIGTNumMaxHeight;
		private int mIGTNumMaxWidth;

		private long mLastLevelNoCheckTime = 0;
		private long mLastCoinCheckTime = 0;
		private long mCheckPassCountLevelNo = 0;
		private long mCheckPassCountCoinNum = 0;

		private VideoGameState mVideoGameState = new VideoGameState();

		public VideoGameState GetVideoGameState()
		{
			return mVideoGameState;
		}

		public CourseNumAnalyzer()
		{
			mNumMaxHeight = 0;
			mNumMaxWidth = 0;
			for (int i = 0; i < 10; ++i)
			{
				string path = "./Images/CourseNumbers/" + i + ".png";
				mNumberImagesOriginal[i] = new Bitmap(path);
				mNumberImages[i] = new FastBitmap(mNumberImagesOriginal[i]);
				mNumRGBs.Add(new List<List<Color>>());
				for (int y = 0; y < mNumberImages[i].Height; ++y)
				{
					mNumRGBs[i].Add(new List<Color>());
					for (int x = 0; x < mNumberImages[i].Width; ++x)
					{
						mNumRGBs[i][y].Add(mNumberImages[i].GetPixel(x, y));
					}
				}
				mNumBinFlgs.Add(new List<List<bool>>());
				for (int y = 0; y < mNumberImages[i].Height; ++y)
				{
					mNumBinFlgs[i].Add(new List<bool>());
					for (int x = 0; x < mNumberImages[i].Width; ++x)
					{
						Color pixel = mNumberImages[i].GetPixel(x, y);
						bool flg = CalcBinFlg(pixel);
						mNumBinFlgs[i][y].Add(flg);
					}
				}
				mNumMaxHeight = Math.Max(mNumMaxHeight, mNumberImages[i].Height);
				mNumMaxWidth = Math.Max(mNumMaxWidth, mNumberImages[i].Width);
				mNumTotalPixel[i] = mNumberImages[i].Height * mNumberImages[i].Width / SCAN_INTERVAL2;
				mNumAllowPixel[i] = (int)(mNumTotalPixel[i] * (1.0 - NUM_RATE_THRESH));
			}
		}

		// コースNo.を取得する。
		public string DetectLevelNo(FastBitmap gameImage)
		{
			// 解像度チェック
			double xRate = (double)gameImage.Width / ORIGINAL_WIDTH;
			double yRate = (double)gameImage.Height / ORIGINAL_HEIGHT;

			// 黄色い範囲をチェック(左側)
			int chxmin = (int)(150 * xRate);
			int chxmax = (int)(400 * xRate);
			int chymin = (int)(100 * yRate);
			int chymax = (int)(200 * yRate);
			for (int y = chymin; y < chymax; y += (int)(5 * yRate))
			{
				for (int x = chxmin; x < chxmax; x += (int)(5 * xRate))
				{
					Color color = gameImage.GetPixel(x, y);
					if (Math.Abs(color.R - 255) + Math.Abs(color.G - 205) + Math.Abs(color.B - 0) > ALLOWED_DIFF)
					{
						return "";
					}
				}
			}
			// 黄色い範囲をチェック(右側)
			int chxmin2 = (int)(1600 * xRate);
			int chxmax2 = (int)(1700 * xRate);
			int chymin2 = (int)(100 * yRate);
			int chymax2 = (int)(200 * yRate);
			for (int y = chymin2; y < chymax2; y += (int)(5 * yRate))
			{
				for (int x = chxmin2; x < chxmax2; x += (int)(5 * xRate))
				{
					Color color = gameImage.GetPixel(x, y);
					if (Math.Abs(color.R - 255) + Math.Abs(color.G - 205) + Math.Abs(color.B - 0) > ALLOWED_DIFF)
					{
						return "";
					}
				}
			}

			// 数秒待つ
			long nowTime = Timer.Instance.GetUnixTime(DateTime.Now);
			if (nowTime - mLastLevelNoCheckTime > 5000)
			{
				// ５秒以上経過していたらタイマーリセット
				mCheckPassCountLevelNo = 0;
			}
			if (mCheckPassCountLevelNo == 1)
			{
				if (nowTime - mLastLevelNoCheckTime <= 1500)
				{
					// 映像が安定するまで待つ
					return "";
				}
				mCheckPassCountLevelNo = 0;
			}
			else
			{
				// 映像が安定するまで待つ処理を開始
				mCheckPassCountLevelNo = 1;
				mLastLevelNoCheckTime = nowTime;
				return "";
			}

			// 黒い範囲をチェック
			int ch2xmin = (int)(100 * xRate);
			int ch2xmax = (int)(600 * xRate);
			int ch2ymin = (int)(400 * yRate);
			int ch2ymax = (int)(700 * yRate);
			for (int y = ch2ymin; y < ch2ymax; y += (int)(5 * yRate))
			{
				for (int x = ch2xmin; x < ch2xmax; x += (int)(5 * xRate))
				{
					Color color = gameImage.GetPixel(x, y);
					if (Math.Abs(color.R - 0) + Math.Abs(color.G - 0) + Math.Abs(color.B - 0) > ALLOWED_DIFF)
					{
						return "";
					}
				}
			}

			// タイトル文字の場所だけ比較する
			int xmin = (int)(800 * xRate);
			int xmax = (int)(1100 * xRate);
			int ymin = (int)(150 * yRate);
			int ymax = (int)(200 * yRate);

			List<int> diffs = new List<int>();
			int levelIdx = 0;
			foreach (LevelData level in LevelManager.Instance.GetAllMinimumLevels())
			{
				if (level.mTitleImage == null)
				{
					continue;
				}
				diffs.Add(0);
				FastBitmap levelImage = level.mTitleImage;
				if (levelImage.Height != gameImage.Height || levelImage.Width != gameImage.Width)
				{
					continue;
				}
				else
				{
					int count = 0;
					for (int y = ymin; y < ymax; y += (int)(2 * yRate))
					{
						for (int x = xmin; x < xmax; x += (int)(2 * xRate))
						{
							int diff = 0;
							Color ac = levelImage.GetPixel(x, y);
							Color bc = gameImage.GetPixel(x, y);
							diff += Math.Abs(ac.R - bc.R);
							diff += Math.Abs(ac.G - bc.G);
							diff += Math.Abs(ac.B - bc.B);
							if (diff >= ALLOWED_DIFF)
							{
								++count;
							}
						}
					}
					diffs[levelIdx] = count;
				}
				++levelIdx;
			}

			string ret = "";
			int minCount = 99999999;
			levelIdx = 0;
			foreach (LevelData level in LevelManager.Instance.GetAllMinimumLevels())
			{
				if (level.mTitleImage == null)
				{
					continue;
				}
				if (diffs[levelIdx] < minCount)
				{
					minCount = diffs[levelIdx];
					ret = level.mLevelNo;
				}
				++levelIdx;
			}
			return ret;
		}

		// 獲得コイン枚数を取得する
		public Tuple<int, int> DetectCoinNum(FastBitmap gameImage)
		{
			// 解像度チェック
			double xRate = (double)gameImage.Width / ORIGINAL_WIDTH;
			double yRate = (double)gameImage.Height / ORIGINAL_HEIGHT;

			// 黄色い範囲をチェック
			int chxmin = (int)(500 * xRate);
			int chxmax = (int)(550 * xRate);
			int chymin = (int)(500 * yRate);
			int chymax = (int)(600 * yRate);
			for (int y = chymin; y < chymax; y += (int)(5 * yRate))
			{
				for (int x = chxmin; x < chxmax; x += (int)(5 * xRate))
				{
					Color color = gameImage.GetPixel(x, y);
					if (Math.Abs(color.R - 255) + Math.Abs(color.G - 205) + Math.Abs(color.B - 0) > ALLOWED_DIFF)
					{
						return new Tuple<int, int>(-1, -1);
					}
				}
			}

			// 数秒待つ
			long nowTime = Timer.GetUnixTime(DateTime.Now);
			if (nowTime - mLastCoinCheckTime > 5000)
			{
				// ５秒以上経過していたらタイマーリセット
				mCheckPassCountCoinNum = 0;
			}
			if (mCheckPassCountCoinNum == 1)
			{
				if (nowTime - mLastCoinCheckTime <= 1000)
				{
					// コインが動くモーションが終わるまで待つ
					return new Tuple<int, int>(-1, -1);
				}
				mCheckPassCountCoinNum = 0;
			}
			else
			{
				// コインが動くモーションを待つ処理を開始
				mCheckPassCountCoinNum = 1;
				mLastCoinCheckTime = nowTime;
				return new Tuple<int, int>(-1, -1);
			}

			// 先にRewardやアイテムがあるかどうかを検出する

			bool hasItem = true;
			int chixmin = (int)(600 * xRate);
			int chixmax = (int)(1200 * xRate);
			int chiymin = (int)(170 * yRate);
			int chiymax = (int)(220 * yRate);
			for (int y = chiymin; y < chiymax; y += (int)(5 * yRate))
			{
				for (int x = chixmin; x < chixmax; x += (int)(5 * xRate))
				{
					Color color = gameImage.GetPixel(x, y);
					if (Math.Abs(color.R - 255) + Math.Abs(color.G - 205) + Math.Abs(color.B - 0) > ALLOWED_DIFF)
					{
						hasItem = false;
						break;
					}
				}
				if (!hasItem)
				{
					break;
				}
			}
			Console.WriteLine("HasItem: " + hasItem.ToString());

			bool hasReward = true;
			if (hasItem)
			{
				hasReward = false;
			}
			else
			{
				int chrxmin = (int)(600 * xRate);
				int chrxmax = (int)(1200 * xRate);
				int chrymin = (int)(250 * yRate);
				int chrymax = (int)(300 * yRate);
				for (int y = chrymin; y < chrymax; y += (int)(5 * yRate))
				{
					for (int x = chrxmin; x < chrxmax; x += (int)(5 * xRate))
					{
						Color color = gameImage.GetPixel(x, y);
						if (Math.Abs(color.R - 255) + Math.Abs(color.G - 205) + Math.Abs(color.B - 0) > ALLOWED_DIFF)
						{
							hasReward = false;
							break;
						}
					}
					if (!hasReward)
					{
						break;
					}
				}
			}
			Console.WriteLine("HasReward: " + hasReward.ToString());

			int reward = 0;
			int coinInLevel = 0;

			bool rewardFound = false;
			bool coinInLevelFound = false;

			if (hasItem)
			{
				// 報酬なしの場合のコース内コイン
				int ixmin2 = (int)(900 * xRate);
				int ixmax2 = (int)(1100 * xRate);
				int iymin2 = (int)(285 * yRate);
				int iymax2 = (int)(335 * yRate);

				coinInLevel = IdentifyNumbers(gameImage, ixmin2, ixmax2, iymin2, iymax2);
				if (coinInLevel >= 0)
				{
					coinInLevelFound = true;
				}
				else
				{
					coinInLevel = 0;
				}

			}
			else if (hasReward)
			{
				// コイン枚数表示部付近だけ調べれば良い
				// 報酬コイン
				int rxmin = (int)(900 * xRate);
				int rxmax = (int)(1100 * xRate);
				int rymin = (int)(600 * yRate);
				int rymax = (int)(650 * yRate);

				reward = IdentifyNumbers(gameImage, rxmin, rxmax, rymin, rymax);
				if (reward >= 0)
				{
					rewardFound = true;
				}
				else
				{
					reward = 0;
				}

				// 報酬ありの場合のコース内コイン
				int ixmin1 = (int)(900 * xRate);
				int ixmax1 = (int)(1100 * xRate);
				int iymin1 = (int)(375 * yRate);
				int iymax1 = (int)(425 * yRate);

				coinInLevel = IdentifyNumbers(gameImage, ixmin1, ixmax1, iymin1, iymax1);
				if (coinInLevel >= 0)
				{
					coinInLevelFound = true;
				}
				else
				{
					coinInLevel = 0;
				}

			}
			else
			{
				// 報酬なしの場合のコース内コイン
				int ixmin2 = (int)(900 * xRate);
				int ixmax2 = (int)(1100 * xRate);
				int iymin2 = (int)(525 * yRate);
				int iymax2 = (int)(575 * yRate);

				coinInLevel = IdentifyNumbers(gameImage, ixmin2, ixmax2, iymin2, iymax2);
				if (coinInLevel >= 0)
				{
					coinInLevelFound = true;
				}
				else
				{
					coinInLevel = 0;
				}

			}

			Console.WriteLine("Reward: " + reward);
			Console.WriteLine("Coin in Level: " + coinInLevel);

			if (!rewardFound && !coinInLevelFound)
			{
				return new Tuple<int, int>(-1, -1);
			}

			return new Tuple<int, int>(reward, coinInLevel);
		}

		// 数字列を識別する
		private int IdentifyNumbers(FastBitmap gameImage, int xmin, int xmax, int ymin, int ymax)
		{
			double xRate = (double)gameImage.Width / ORIGINAL_WIDTH;
			double yRate = (double)gameImage.Height / ORIGINAL_HEIGHT;
			int xSpan = (int)(X_SPAN * xRate);
			int scanInterval = (int)(SCAN_INTERVAL * (xRate + yRate) / 2.0);

			List<Tuple<int, int, double>> tempFoundNumbers = new List<Tuple<int, int, double>>(); // x座標、数字、一致度
			for (int ax = xmin; ax < xmax; ++ax)
			{
				for (int ay = ymin; ay < ymax; ++ay)
				{
					Tuple<int, double> numInfo = TestNumber(gameImage, ax, ay, scanInterval);
					if (numInfo.Item1 != -1)
					{
						tempFoundNumbers.Add(new Tuple<int, int, double>(ax, numInfo.Item1, numInfo.Item2));
					}
				}
			}

			IDictionary<int, int> foundNumbers = new SortedDictionary<int, int>();
			bool[] xCheck = new bool[xmax - xmin];
			if (tempFoundNumbers.Count > 0)
			{
				// 一致度が高い順に並び替え
				tempFoundNumbers.Sort((a, b) => a.Item3 < b.Item3 ? 1 : (a.Item3 > b.Item3 ? -1 : 0));

				// 一致度が高い順に数字を選ぶ
				int idx = 0;
				while (true)
				{
					while (idx < tempFoundNumbers.Count && xCheck[tempFoundNumbers[idx].Item1 - xmin])
					{
						++idx;
					}
					if (idx >= tempFoundNumbers.Count)
					{
						break;
					}
					int x = tempFoundNumbers[idx].Item1;
					int num = tempFoundNumbers[idx].Item2;
					foundNumbers[x] = num;
					Console.WriteLine("Similarity of [" + num + "] is " + Math.Round(tempFoundNumbers[idx].Item3 * 100, 2) + "%");
					++idx;

					for (int dx = -xSpan; dx < xSpan; ++dx)
					{
						if (x + dx < xmin || x + dx >= xmax)
						{
							continue;
						}
						xCheck[x + dx - xmin] = true;
					}
				}
			}

			if (foundNumbers.Count == 0)
			{
				return -1;
			}

			int coinNum = 0;
			foreach (KeyValuePair<int, int> data in foundNumbers)
			{
				coinNum *= 10;
				coinNum += data.Value;
			}

			return coinNum;
		}

		// 最も一致度の高い数字を検出する
		// ２値画像で比較するバージョン
		private Tuple<int, double> TestNumber(FastBitmap image, int ax, int ay, int scanInterval)
		{
			int[] counts = new int[10];
			List<int> numbers = new List<int>();
			for (int i = 0; i < 10; ++i)
			{
				counts[i] = 0;
				numbers.Add(i);
			}
			for (int dy = 0; dy < mNumMaxHeight; dy += scanInterval)
			{
				for (int dx = 0; dx < mNumMaxWidth; dx += scanInterval)
				{
					Color ac = image.GetPixel(ax + dx, ay + dy);
					bool acFlg = CalcBinFlg(ac);
					for (int i = numbers.Count - 1; i >= 0; --i)
					{
						int num = numbers[i];
						if (dy >= mNumberImages[num].Height || dx >= mNumberImages[num].Width)
						{
							continue;
						}
						bool bcFlg = mNumBinFlgs[num][dy][dx];
						if (acFlg != bcFlg)
						{
							++counts[num];
						}
						if (counts[num] > mNumAllowPixel[num])
						{
							numbers.RemoveAt(i);
							if (numbers.Count == 0)
							{
								return new Tuple<int, double>(-1, -1);
							}
						}
					}
				}
			}
			double maxSimilarity = -1.0;
			int bestNum = -1;
			foreach (int num in numbers)
			{
				double similarity = 1.0 - ((double)counts[num] / mNumTotalPixel[num]);
				if (similarity > maxSimilarity)
				{
					maxSimilarity = similarity;
					bestNum = num;
				}
			}
			return new Tuple<int, double>(bestNum, maxSimilarity);
		}

		// RGB画素値を使って比較するバージョン
		//private int TestNumber(FastBitmap image, int ax, int ay)
		//{
		//	int totalPixel = mNumMaxHeight * mNumMaxWidth / SCAN_INTERVAL2;
		//	int allowPixel = (int)(totalPixel * (1.0 - 0.9));
		//	int[] counts = new int[10];
		//	List<int> numbers = new List<int>();
		//	for (int i = 0; i < 10; ++i)
		//	{
		//		counts[i] = 0;
		//		numbers.Add(i);
		//	}
		//	for (int dy = 0; dy < mNumMaxHeight; dy += SCAN_INTERVAL)
		//	{
		//		for (int dx = 0; dx < mNumMaxWidth; dx += SCAN_INTERVAL)
		//		{
		//			Color ac = image.GetPixel(ax + dx, ay + dy);
		//			for (int i = numbers.Count - 1; i >= 0; --i)
		//			{
		//				int num = numbers[i];
		//				Color bc = mNumRGBs[num][dy][dx];
		//				int diff = 0;
		//				diff += Math.Abs(ac.R - bc.R);
		//				diff += Math.Abs(ac.G - bc.G);
		//				diff += Math.Abs(ac.B - bc.B);
		//				if (diff >= ALLOWED_DIFF)
		//				{
		//					++counts[num];
		//				}
		//				if (counts[num] > allowPixel)
		//				{
		//					numbers.RemoveAt(i);
		//					if (numbers.Count == 0)
		//					{
		//						return -1;
		//					}
		//				}
		//			}
		//		}
		//	}
		//	numbers.Sort((a, b) => counts[b] - counts[a]);
		//	return numbers[0];
		//}

		private bool CalcBinFlg(Color color)
		{
			return color.R + color.G + color.B >= BIN_THRESH * 3;
		}
	}
}
