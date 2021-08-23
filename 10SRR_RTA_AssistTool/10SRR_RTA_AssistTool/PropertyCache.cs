using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenSRR_RTA_AssistTool {
	class PropertyCache {

		private static PropertyCache mInstance;
		private PropertyCache () {} // Private Constructor
		public static PropertyCache Instance {
			get {
				if ( mInstance == null ) mInstance = new PropertyCache();
				return mInstance;
			}
		}

		private const string FILE_NAME = @"PropertyCache.ini";

		public const int DEFAULT_PREVIEW_VIDEO = 1;
		public const int DEFAULT_PLAY_AUDIO = 0;
		public const int DEFAULT_SPLIT_SENDING = 1;

		public int mPreviewVideo { get; set; } = DEFAULT_PREVIEW_VIDEO;
		public int mPlayAudio { get; set; } = DEFAULT_PLAY_AUDIO;
		public int mSplitSending { get; set; } = DEFAULT_SPLIT_SENDING;

		public void Initialize() {
 			if (!File.Exists(FILE_NAME)) {
				ResetToDefault();
				SaveToFile();
			}
			List<string> settingList = new List<string>();
			using(StreamReader r = new StreamReader(FILE_NAME, Encoding.UTF8))
			{
				while (r.Peek() >= 0) {
					settingList.Add(r.ReadLine());
				}
			}
			LoadSetting(settingList);
		}

		private void ResetToDefault()
		{
			mPreviewVideo = DEFAULT_PREVIEW_VIDEO;
			mPlayAudio = DEFAULT_PLAY_AUDIO;
			mSplitSending = DEFAULT_SPLIT_SENDING;
		}

		private void LoadSetting(List<string> settingList) {
			
			ResetToDefault();

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
					case "PREVIEW_VIDEO":
						int previewVideo;
						if (int.TryParse(param, out previewVideo)) {
							mPreviewVideo = previewVideo;
						}
						break;
					case "PLAY_AUDIO":
						int playAudio;
						if (int.TryParse(param, out playAudio)) {
							mPlayAudio = playAudio;
						}
						break;
					case "SPLIT_SENDING":
						int splitSending;
						if (int.TryParse(param, out splitSending))
						{
							mSplitSending = splitSending;
						}
						break;
				}
			}
		}

		public void SaveToFile() {
			using(StreamWriter w = new StreamWriter(FILE_NAME, false, Encoding.UTF8))
			{
				w.WriteLine("PREVIEW_VIDEO=" + mPreviewVideo);
				w.WriteLine("PLAY_AUDIO=" + mPlayAudio);
				w.WriteLine("SPLIT_SENDING=" + mSplitSending);
			}
		}

	}
}
