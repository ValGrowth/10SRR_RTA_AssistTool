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
	class IGTAnalyzer
	{
		Color NUMBER_BLUE = new Color();

		private const int ORIGINAL_WIDTH = 1920;
		private const int ORIGINAL_HEIGHT = 1080;
		private const int BIN_THRESH = 128; // ２値画像で１になるしきい値
		private const int X_SPAN = 20; // TODO: 数字同士のX座標の間隔の画素数
		private const int SCAN_INTERVAL = 2; // 数字画像の一致チェックでスキャンするときの１ステップでの移動距離
		private const int SCAN_INTERVAL2 = SCAN_INTERVAL * SCAN_INTERVAL;
		private const double NUM_RATE_THRESH = 0.9; // 数字検出で検出される一致度の下限
		private const int ALLOWED_DIFF = 150; // 一致とみなされるための画素値の差の合計の上限(RGB画像を比較するとき)

		// 数字画像は大きいもの*10と小さいもの*10と『10.00』の画像で合計21枚
		private Bitmap[] mNumberImagesOriginal = new Bitmap[21]; // オリジナル数字画像
		private FastBitmap[] mNumberImages = new FastBitmap[21]; // 数字画像
		private List<List<List<Color>>> mNumRGBs = new List<List<List<Color>>>(); // 各数字画像の各ピクセルの色
		private List<List<List<bool>>> mNumBinFlgs = new List<List<List<bool>>>(); // 各数字画像を２値画像に変換したときの各ピクセルが0か1か
		private int[] mNumTotalPixel = new int[21]; // 各数字画像のピクセル数
		private int[] mNumAllowPixel = new int[21]; // 各数字画像の許容ピクセル数
		private int mNumMaxHeight = 0; // 数字画像の最大の高さ
		private int mNumMaxWidth = 0; // 数字画像の最大の幅

		public IGTAnalyzer()
		{
			NUMBER_BLUE = Color.FromArgb(0, 0, 255);
			mNumMaxHeight = 0;
			mNumMaxWidth = 0;
			for (int i = 0; i < 21; ++i)
			{
				int number = -1;
				string path = "";
				if (i == 20)
				{
					number = 10;
					path = "./Images/IGTNumbers/10.png";
				}
				else
				{
					number = i % 10;
					string dir = (i < 10 ? "Big" : "Small");
					path = "./Images/IGTNumbers/" + dir + "/" + number + ".png";
				}
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
				if (number != 10)
				{
					mNumMaxHeight = Math.Max(mNumMaxHeight, mNumberImages[i].Height);
					mNumMaxWidth = Math.Max(mNumMaxWidth, mNumberImages[i].Width);
				}
				mNumTotalPixel[i] = mNumberImages[i].Height * mNumberImages[i].Width / SCAN_INTERVAL2;
				mNumAllowPixel[i] = (int)(mNumTotalPixel[i] * (1.0 - NUM_RATE_THRESH));
			}
		}

		// コースNo.を取得する。
		public double DetectIGT(FastBitmap gameImage)
		{
			// 解像度チェック
			double xRate = (double)gameImage.Width / ORIGINAL_WIDTH;
			double yRate = (double)gameImage.Height / ORIGINAL_HEIGHT;

			int xmin = -1;
			int xmax = -1;
			int ymin = -1;
			int ymax = -1;

			// TODO: 青い範囲をチェック
			int chxmin = (int)(150 * xRate);
			int chxmax = (int)(400 * xRate);
			int chymin = (int)(100 * yRate);
			int chymax = (int)(200 * yRate);
			for (int y = chymin; y < chymax; y += (int)(5 * yRate))
			{
				for (int x = chxmin; x < chxmax; x += (int)(5 * xRate))
				{
					Color color = gameImage.GetPixel(x, y);
					if (Math.Abs(color.R - NUMBER_BLUE.R)
						+ Math.Abs(color.G - NUMBER_BLUE.G)
						+ Math.Abs(color.B - NUMBER_BLUE.B)
						> ALLOWED_DIFF)
					{
						return -1;
					}
				}
			}

			// TODO: 10秒かどうかチェック
			xmin = (int)(800 * xRate);
			xmax = (int)(1100 * xRate);
			ymin = (int)(150 * yRate);
			ymax = (int)(200 * yRate);

			FastBitmap tenSecImage = mNumberImages[20];
			int count = 0;
			for (int y = ymin; y < ymax; y += (int)(2 * yRate))
			{
				for (int x = xmin; x < xmax; x += (int)(2 * xRate))
				{
					int diff = 0;
					Color ac = tenSecImage.GetPixel(x, y);
					Color bc = gameImage.GetPixel(x, y);
					diff += Math.Abs(ac.R - bc.R);
					diff += Math.Abs(ac.G - bc.G);
					diff += Math.Abs(ac.B - bc.B);
					if (diff >= ALLOWED_DIFF)
					{
						++count;
						if (count > mNumAllowPixel[20])
						{
							break;
						}
					}
				}
				if (count > mNumAllowPixel[20])
				{
					break;
				}
			}
			if (count <= mNumAllowPixel[20])
			{
				Console.WriteLine("IGT:10.00");
				return 10.0;
			}


			// TODO: IGT取得
			xmin = (int)(900 * xRate);
			xmax = (int)(1100 * xRate);
			ymin = (int)(525 * yRate);
			ymax = (int)(575 * yRate);

			FastBitmap[] decimalNumberImages = new FastBitmap[10];
			for (int i = 0; i < 10; ++i)
			{
				decimalNumberImages[i] = mNumberImages[i];
			}
			int decimals = IdentifyNumbers(gameImage, decimalNumberImages, xmin, xmax, ymin, ymax);

			xmin = (int)(900 * xRate);
			xmax = (int)(1100 * xRate);
			ymin = (int)(525 * yRate);
			ymax = (int)(575 * yRate);

			FastBitmap[] floatNumberImages = new FastBitmap[10];
			for (int i = 0; i < 10; ++i)
			{
				floatNumberImages[i] = mNumberImages[10 + i];
			}
			int floats = IdentifyNumbers(gameImage, floatNumberImages, xmin, xmax, ymin, ymax);

			Console.WriteLine("IGT:" + decimals + "." + floats.ToString("d2"));

			if (decimals < 0 || floats < 0)
			{
				return -1;
			}

			return decimals + (floats * 0.01);
		}

		// 数字列を識別する
		private int IdentifyNumbers(FastBitmap gameImage, FastBitmap[] numberImages, int xmin, int xmax, int ymin, int ymax)
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
					Tuple<int, double> numInfo = TestNumber(gameImage, numberImages, ax, ay, scanInterval);
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
		private Tuple<int, double> TestNumber(FastBitmap image, FastBitmap[] numberImages, int ax, int ay, int scanInterval)
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
						if (dy >= numberImages[num].Height || dx >= numberImages[num].Width)
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
