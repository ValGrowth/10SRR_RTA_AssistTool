using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenSRR_RTA_AssistTool {
	class Preferences {
		private static Preferences mInstance;
		private Preferences () {} // Private Constructor
		public static Preferences Instance {
			get {
				if ( mInstance == null ) mInstance = new Preferences();
				return mInstance;
			}
		}
		
		private const string FILE_NAME = @"Preferences.ini";

		public const string DEFAULT_VIDEO_DEVICE = "";
		public const string DEFAULT_AUDIO_DEVICE = "";

		public string mVideoDeviceName { get; set; } = DEFAULT_VIDEO_DEVICE;
		public string mAudioDeviceName { get; set; } = DEFAULT_AUDIO_DEVICE;

		public void Initialize()
		{
			if (!File.Exists(FILE_NAME))
			{
				ResetToDefault();
				SaveToFile();
			}
			List<string> settingList = new List<string>();
			using (StreamReader r = new StreamReader(FILE_NAME, Encoding.UTF8))
			{
				while (r.Peek() >= 0)
				{
					settingList.Add(r.ReadLine());
				}
			}
			LoadSetting(settingList);
		}

		private void ResetToDefault()
		{
			mVideoDeviceName = DEFAULT_VIDEO_DEVICE;
			mAudioDeviceName = DEFAULT_AUDIO_DEVICE;
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
					case "VIDEO_DEVICE_NAME":
						mVideoDeviceName = param;
						break;
					case "AUDIO_DEVICE_NAME":
						mAudioDeviceName = param;
						break;
				}
			}
		}

		public void SaveToFile() {
			using(StreamWriter w = new StreamWriter(FILE_NAME, false, Encoding.UTF8))
			{
				w.WriteLine("VIDEO_DEVICE_NAME=" + mVideoDeviceName);
				w.WriteLine("AUDIO_DEVICE_NAME=" + mAudioDeviceName);
			}
		}
	}
}
