using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenSRR_RTA_AssistTool
{
	class Timer
	{
		private static Timer mInstance;
		private Timer() { } // Private Constructor
		public static Timer Instance {
			get {
				if (mInstance == null) mInstance = new Timer();
				return mInstance;
			}
		}

		// UNIXエポックを表すDateTimeオブジェクトを取得
		private static DateTime UNIX_EPOCH =
          new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public static long GetUnixTime(DateTime targetTime)
        {
            // UTC時間に変換
            targetTime = targetTime.ToUniversalTime();

            // UNIXエポックからの経過時間を取得
            TimeSpan elapsedTime = targetTime - UNIX_EPOCH;

            // 経過秒数に変換
            return (long)elapsedTime.TotalMilliseconds;
        }
    }
}
