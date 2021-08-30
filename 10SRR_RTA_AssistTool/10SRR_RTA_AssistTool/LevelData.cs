using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenSRR_RTA_AssistTool
{
    class LevelData
    {
        public int mCourseNo;
        public double mBestIGT;
        public double mBestTotalIGT;

        public LevelData()
        {
            mCourseNo = 0;
            mBestIGT = 0.0;
            mBestTotalIGT = 0.0;
        }

        public LevelData(List<string> list)
        {
            if (!int.TryParse(list[0], out mCourseNo))
            {
                mCourseNo = -1;
            }
            if (!double.TryParse(list[1], out mBestIGT))
            {
                mBestIGT = -1;
            }
            mBestTotalIGT = 0.0;
        }

    }
}
