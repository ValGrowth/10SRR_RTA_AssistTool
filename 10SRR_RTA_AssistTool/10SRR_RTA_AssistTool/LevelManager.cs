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

		private IDictionary<int, LevelData> mLevelDataList = new Dictionary<int, LevelData>();

		public void Initialize()
		{
			List<List<string>> csvData = CsvReader.ReadCsv("./LevelData/LevelData.csv", true, true);
			
			mLevelDataList.Clear();

			double totalIGT = 0.0;
			foreach (List<string> line in csvData)
			{
				LevelData levelData = new LevelData(line);
				totalIGT += levelData.mBestIGT;
				levelData.mBestTotalIGT = totalIGT;
				mLevelDataList.Add(levelData.mCourseNo, levelData);
			}
		}

		public LevelData GetLevelData(int courseNo)
		{
			if (!mLevelDataList.ContainsKey(courseNo))
            {
				return null;
            }
			return mLevelDataList[courseNo];
		}

		public ICollection<LevelData> GetAllLevels()
        {
			return mLevelDataList.Values;
        }

	}
}
