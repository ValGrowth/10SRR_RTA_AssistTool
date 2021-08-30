using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace TenSRR_RTA_AssistTool
{
	class GameState
	{

		public enum STATE { 
			PLAYING,
			PLAYED,

			NONE,
		};

		public static string[] CSV_HEADER = new string[] {
			"CourseNo.",
			"CurDeath",
			"CurFinalIGT",
			"CurFinalIGTDiff",
			"CurSumIGT",
			"CurSumIGTDiff",
			"TotalDeath",
			"TotalFinalIGT",
			"TotalFinalIGTDiff",
			"TotalSumIGT",
			"TotalSumIGTDiff",
		};

		private int mCourseNo;
		private double mCurFinalIGT; // 現在のコースのクリア時IGT
		private double mCurFinalIGTDiff;
		private double mCurSumIGT; // 現在のコースのIGT合計
		private double mCurSumIGTDiff;
		private int mCurDeath;
		private double mTotalFinalIGT; // 現在のコースまでのクリア時IGTの総計
		private double mTotalFinalIGTDiff;
		private double mTotalSumIGT; // 現在のコースまでのIGT合計の総計
		private double mTotalSumIGTDiff;
		private int mTotalDeath;

		public int GetCourseNo() { return mCurDeath; }
		public double GetCurFinalIGT() { return mCurFinalIGT; }
		public double GetCurFinalIGTDiff() { return mCurFinalIGTDiff; }
		public double GetCurSumIGT() { return mCurSumIGT; }
		public double GetCurSumIGTDiff() { return mCurSumIGTDiff; }
		public int GetCurDeath() { return mCurDeath; }
		public double GetTotalFinalIGT() { return mTotalFinalIGT; }
		public double GetTotalFinalIGTDiff() { return mTotalFinalIGTDiff; }
		public double GetTotalSumIGT() { return mTotalSumIGT; }
		public double GetTotalSumIGTDiff() { return mTotalSumIGTDiff; }
		public int GetTotalDeath() { return mTotalDeath; }

		public GameState()
		{
			Reset();
		}

		public void Reset()
		{
			mCourseNo = 0;
			mCurFinalIGT = 0.0;
			mCurSumIGT = 0.0;
			mCurDeath = 0;
			mTotalFinalIGT = 0.0;
			mTotalSumIGT = 0.0;
			mTotalDeath = 0;
		}

		//public void SetFromCSVLine(List<string> list)
		//{
		//	Reset();
		//
		//	if (!int.TryParse(list[0], out mCourseNo))
		//	{
		//		mCourseNo = -1;
		//	}
		//	if (!double.TryParse(list[1], out mCurFinalIGT))
		//	{
		//		mCurFinalIGT = 0.0;
		//	}
		//	if (!double.TryParse(list[2], out mCurSumIGT))
		//	{
		//		mCurSumIGT = 0.0;
		//	}
		//	if (!int.TryParse(list[3], out mCurDeath))
		//	{
		//		mCurDeath = 0;
		//	}
		//	if (!double.TryParse(list[4], out mTotalFinalIGT))
		//	{
		//		mTotalFinalIGT = 0.0;
		//	}
		//	if (!double.TryParse(list[5], out mTotalSumIGT))
		//	{
		//		mTotalSumIGT = 0.0;
		//	}
		//	if (!int.TryParse(list[6], out mTotalDeath))
		//	{
		//		mTotalDeath = 0;
		//	}
		//}

		public string MakeCSVLine()
		{
			string str = "";
			str += "\"" + mCourseNo + "\"";
			str += ",\"" + mCurFinalIGT.ToString("F2") + "\"";
			str += ",\"" + mCurFinalIGTDiff.ToString("F2") + "\"";
			str += ",\"" + mCurSumIGT.ToString("F2") + "\"";
			str += ",\"" + mCurSumIGTDiff.ToString("F2") + "\"";
			str += ",\"" + mCurDeath + "\"";
			str += ",\"" + mTotalFinalIGT.ToString("F2") + "\"";
			str += ",\"" + mTotalFinalIGTDiff.ToString("F2") + "\"";
			str += ",\"" + mTotalSumIGT.ToString("F2") + "\"";
			str += ",\"" + mTotalSumIGTDiff.ToString("F2") + "\"";
			str += ",\"" + mTotalDeath + "\"";
			return str;
		}

		public void UpdateState(int courseNo, double igt, bool isDeath)
		{
			mCourseNo = courseNo;

			mTotalFinalIGT -= mCurFinalIGT;
			
			mCurFinalIGT = igt;
			mCurSumIGT += igt;

			mTotalFinalIGT += igt;
			mTotalSumIGT += igt;

			if (isDeath)
			{
				mCurDeath += 1;
				mTotalDeath += 1;
			}

			LevelData levelData = LevelManager.Instance.GetLevelData(courseNo);
			if (levelData != null && !isDeath)
			{
				mCurFinalIGTDiff = levelData.mBestIGT - mCurFinalIGT;
				mCurSumIGTDiff = levelData.mBestIGT - mCurSumIGT;

				mTotalFinalIGTDiff = levelData.mBestTotalIGT - mTotalFinalIGT;
				mTotalSumIGTDiff = levelData.mBestTotalIGT - mTotalSumIGT;
			} else
			{
				mCurFinalIGTDiff = 0.0;
				mCurSumIGTDiff = 0.0;

				mTotalFinalIGTDiff = 0.0;
				mTotalSumIGTDiff = 0.0;
			}
		}

	}
}
