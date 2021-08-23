using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenSRR_RTA_AssistTool {
	class LevelManager {

		private static LevelManager mInstance;
		private LevelManager() { } // Private Constructor
		public static LevelManager Instance {
			get {
				if ( mInstance == null ) mInstance = new LevelManager();
				return mInstance;
			}
		}

		public List<List<string>> mOriginalCsvData = new List<List<string>>();
		private IDictionary<string, LevelData> mLevelDataList = new Dictionary<string, LevelData>(); // チャートのコースリスト
		private IDictionary<string, LevelData> mLevelDataMinimumList = new Dictionary<string, LevelData>(); // LevelNo, コース名のみのリスト
		public int mFinalNeededCoin = 0;

		public void Initialize()
		{
			InitLevelDataMinimum();

			List<List<string>> csvData = CsvReader.ReadCsv("./LevelData/LevelData.csv", true, true);
			mOriginalCsvData = csvData;
			
			mLevelDataList.Clear();
			mFinalNeededCoin = 0;

			int coin = 0;
			int idx = 1;
			LevelData lastLevelData = null;
			foreach (List<string> line in csvData)
			{
				LevelData levelData = new LevelData(line);
				mLevelDataList.Add(levelData.mLevelCode, levelData);
				coin += levelData.mReward + levelData.mInLevelCoin + levelData.mAdditionalCoin - levelData.mNeededCoin;
				levelData.mAllowedLoss = new Tuple<int, int>(9999, 9999);
				mFinalNeededCoin = Math.Max(mFinalNeededCoin, levelData.mCumulativeCoin);
				if (coin <= 30)
				{
					foreach (KeyValuePair<string, LevelData> pr in mLevelDataList)
					{
						if (pr.Value.mAllowedLoss.Item1 > idx)
						{
							pr.Value.mAllowedLoss = new Tuple<int, int>(idx, coin);
						}
					}
				}
				if (idx == csvData.Count)
				{
					foreach (KeyValuePair<string, LevelData> pr in mLevelDataList)
					{
						// 最終許容ロスをセット
						pr.Value.mFinalAllowedLoss = new Tuple<int, int>(idx, coin);

						// 必須が計測されていない場合は最終許容ロスと同じにする
						if (pr.Value.mAllowedLoss.Item1 == 9999)
						{
							pr.Value.mAllowedLoss = new Tuple<int, int>(idx, coin);
						}
					}
				}
				if (lastLevelData != null)
				{
					lastLevelData.mNextLevel = levelData;
				}
				lastLevelData = levelData;
				++idx;
			}
		}

		private void InitLevelDataMinimum()
		{
			List<List<string>> csvData = CsvReader.ReadCsv("./LevelData/LevelData_Minimum.csv", true, true);

			mLevelDataMinimumList.Clear();

			foreach (List<string> line in csvData)
			{
				LevelData levelData = new LevelData();
				levelData.LoadAsMinimum(line);
				mLevelDataMinimumList.Add(levelData.mLevelNo, levelData);
			}
		}

		public LevelData GetLevelData(string levelCode)
		{
			if (!mLevelDataList.ContainsKey(levelCode))
            {
				return null;
            }
			return mLevelDataList[levelCode];
		}

		public LevelData GetLevelDataMinimum(string levelNo)
		{
			if (!mLevelDataMinimumList.ContainsKey(levelNo))
			{
				return null;
			}
			return mLevelDataMinimumList[levelNo];
		}

		public ICollection<LevelData> GetAllLevels()
        {
			return mLevelDataList.Values;
        }

		public ICollection<LevelData> GetAllMinimumLevels()
		{
			return mLevelDataMinimumList.Values;
		}
	}
}
