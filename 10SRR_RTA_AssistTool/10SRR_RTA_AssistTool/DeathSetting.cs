using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenSRR_RTA_AssistTool {
	class DeathSetting {

		private static DeathSetting mInstance;
		private DeathSetting () {} // Private Constructor
		public static DeathSetting Instance {
			get {
				if ( mInstance == null ) mInstance = new DeathSetting();
				return mInstance;
			}
		}

		public const double DEFAULT_PAST_TIME_RANGE = 0.6;
		public const int DEFAULT_DEATH_SPAN = 3;
		public const int DEFAULT_DEAD_PIXEL_VALUE_THRESHOLD = 0;
		public const int DEFAULT_DEAD_AMPLITUDE_THRESHOLD = 50;

		private double mPastTimeRange = DEFAULT_PAST_TIME_RANGE; // 最新のデータからこの秒数分の過去の音量を判定材料にする。
		private TimeSpan mDeadSpan = TimeSpan.FromSeconds(DEFAULT_DEATH_SPAN); // 死亡して次の死亡カウントを始めるまでの間隔[秒]
		private int mDeadPixelValueThreshold = DEFAULT_DEAD_PIXEL_VALUE_THRESHOLD;
		private int mDeadAmplitudeThreshold = DEFAULT_DEAD_AMPLITUDE_THRESHOLD; // 死んだと判定する音量(これより小さいと死んでる)

		public double PastTimeRange {
			get { return mPastTimeRange; }
			set { mPastTimeRange = value; }
		}
		public TimeSpan DeadSpan {
			get { return mDeadSpan; }
			set { mDeadSpan = value; }
		}
		public void setDeadSpan(int sec) { mDeadSpan = TimeSpan.FromSeconds(sec); }
		public int DeadPixelValueThreshold {
			get { return mDeadPixelValueThreshold; }
			set { mDeadPixelValueThreshold = value; }
		}
		public int DeadAmplitudeThreshold {
			get { return mDeadAmplitudeThreshold; }
			set { mDeadAmplitudeThreshold = value; }
		}

		public void Initialize() {
 			if (!File.Exists(@"DeathCounter.ini")) {
				using(StreamWriter w = new StreamWriter(@"DeathCounter.ini", false, Encoding.UTF8))
				{
					w.WriteLine("PAST_TIME_RANGE=" + DEFAULT_PAST_TIME_RANGE);
					w.WriteLine("DEATH_SPAN=" + DEFAULT_DEATH_SPAN);
					w.WriteLine("DEAD_PIXEL_VALUE_THRESHOLD=" + DEFAULT_DEAD_PIXEL_VALUE_THRESHOLD);
					w.WriteLine("DEAD_AMPLITUDE_THRESHOLD=" + DEFAULT_DEAD_AMPLITUDE_THRESHOLD);
				}
			} else {
				List<string> settingList = new List<string>();
				using(StreamReader r = new StreamReader(@"DeathCounter.ini", Encoding.UTF8))
				{
					while (r.Peek() >= 0) {
						settingList.Add(r.ReadLine());
					}
				}
				LoadSetting(settingList);
			}
		}

		private void LoadSetting(List<string> settingList) {
			foreach (string line in settingList) {
				int equalIndex = line.IndexOf("=");
				if (equalIndex < 0) {
					continue;
				}
				if (line.Length <= equalIndex + 1) {
					continue;
				}
				string item = line.Substring(0, equalIndex);
				string param = line.Substring(equalIndex + 1);

				switch (item) {
					case "PAST_TIME_RANGE":
						double pastTimeRange = 0.0;
						if (double.TryParse(param, out pastTimeRange)) {
							mPastTimeRange = pastTimeRange;
						}
						break;
					case "DEATH_SPAN":
						int deadSpan = 0;
						if (int.TryParse(param, out deadSpan)) {
							setDeadSpan(deadSpan);
						}
						break;
					case "DEAD_PIXEL_VALUE_THRESHOLD":
						int deadPixelValueThreshold = 0;
						if (int.TryParse(param, out deadPixelValueThreshold)) {
							mDeadPixelValueThreshold = deadPixelValueThreshold;
						}
						break;
					case "DEAD_AMPLITUDE_THRESHOLD":
						int deadAmplitudeThreshold = 0;
						if (int.TryParse(param, out deadAmplitudeThreshold)) {
							mDeadAmplitudeThreshold = deadAmplitudeThreshold;
						}
						break;
				}
			}
		}

		public void SaveToFile() {
				using(StreamWriter w = new StreamWriter(@"DeathCounter.ini", false, Encoding.UTF8))
				{
					w.WriteLine("PAST_TIME_RANGE=" + mPastTimeRange);
					w.WriteLine("DEATH_SPAN=" + mDeadSpan.Seconds);
					w.WriteLine("DEAD_PIXEL_VALUE_THRESHOLD=" + mDeadPixelValueThreshold);
					w.WriteLine("DEAD_AMPLITUDE_THRESHOLD=" + mDeadAmplitudeThreshold);
				}
		}

	}
}
