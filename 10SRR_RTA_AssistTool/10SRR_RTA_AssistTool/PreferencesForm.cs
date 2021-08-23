using DirectShowLib;
using System;
using System.Windows.Forms;

namespace TenSRR_RTA_AssistTool
{
	public partial class PreferencesForm : Form
	{
		public PreferencesForm()
		{
			InitializeComponent();

			// 現環境のデバイスリスト取得
			string[] videoDeviceNameList = GraphFactory.GetDeviceNameList(FilterCategory.VideoInputDevice);
			//string[] audioDeviceNameList = GraphFactory.GetDeviceNameList(FilterCategory.AudioInputDevice);

			ComboBox_VideoDeviceName.Items.Clear();
			if (videoDeviceNameList != null && videoDeviceNameList.Length > 0)
			{
				// デバイスリストをコンボボックスにセット
				ComboBox_VideoDeviceName.Items.AddRange(videoDeviceNameList);
			}
			//ComboBox_AudioDeviceName.Items.Clear();
			//if (audioDeviceNameList != null && audioDeviceNameList.Length > 0)
			//{
			//	// デバイスリストをコンボボックスにセット
			//	ComboBox_AudioDeviceName.Items.AddRange(audioDeviceNameList);
			//}

			// 設定ファイルの内容を画面に反映
			Preferences option = Preferences.Instance;
			if (!string.IsNullOrEmpty(option.mVideoDeviceName))
			{
				foreach (string deviceName in ComboBox_VideoDeviceName.Items)
				{
					string defaultDeviceName = option.mVideoDeviceName;
					if (deviceName == defaultDeviceName)
					{
						ComboBox_VideoDeviceName.SelectedItem = deviceName;
						break;
					}
				}
			}
			//if (!string.IsNullOrEmpty(option.mAudioDeviceName))
			//{
			//	foreach (string deviceName in ComboBox_AudioDeviceName.Items)
			//	{
			//		string defaultDeviceName = option.mAudioDeviceName;
			//		if (deviceName == defaultDeviceName)
			//		{
			//			ComboBox_AudioDeviceName.SelectedItem = deviceName;
			//			break;
			//		}
			//	}
			//}
		}

		private void Button_Save_Click(object sender, EventArgs e)
		{
			// 画面の内容を設定ファイルに保存
			Preferences prefs = Preferences.Instance;
			prefs.mVideoDeviceName = ComboBox_VideoDeviceName.Text.ToString();
			//prefs.mAudioDeviceName = ComboBox_AudioDeviceName.Text.ToString();
			prefs.SaveToFile();
			MyMessageBox.Show("保存しました。", MyMessageBox.MyMessageBoxIcon.Information);
			this.DialogResult = DialogResult.OK;

			Close();
		}

		private void Button_Cancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

	}
}
