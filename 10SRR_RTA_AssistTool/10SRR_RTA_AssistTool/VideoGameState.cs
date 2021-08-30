using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenSRR_RTA_AssistTool
{
    class VideoGameState
    {
        public int mCourseNo;
        public double mIGT;
        public bool mIsRestart;
        public bool mIsFailure;
        public bool mIsLoading;

        public VideoGameState()
        {
            Reset();
        }

        public void Reset()
        {
            mCourseNo = 0;
            mIGT = 0.0;
            mIsRestart = false;
            mIsFailure = false;
            mIsLoading = false;
        }

        public bool isDeath()
		{
            return mIsRestart || mIsFailure;
		}
    }
}
