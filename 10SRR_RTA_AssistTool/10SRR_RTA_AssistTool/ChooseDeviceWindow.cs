using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TenSRR_RTA_AssistTool {
	public partial class ChooseDeviceWindow : Form {

		private GraphFactory.DEVICE_TYPE mDeviceType;

		public string mDeviceName { get; set; } = "";

		public ChooseDeviceWindow(string[] deviceNames, GraphFactory.DEVICE_TYPE deviceType) {
			InitializeComponent();

			mDeviceType = deviceType;
			
			if (deviceType == GraphFactory.DEVICE_TYPE.VIDEO) {
				Label_ChooseDevice.Text = "Choose Video Device";
			}
			if (deviceType == GraphFactory.DEVICE_TYPE.AUDIO) {
				Label_ChooseDevice.Text = "Choose Audio Device";
			}
			
			ComboBox_DeviceName.Items.AddRange(deviceNames);
			ComboBox_DeviceName.SelectedIndex = 0;
		}

		private void Button_OK_Click(object sender, EventArgs e) {
			if (ComboBox_DeviceName.SelectedItem != null) {
				mDeviceName = ComboBox_DeviceName.SelectedItem.ToString();

				Preferences prefs = Preferences.Instance;
				if (mDeviceType == GraphFactory.DEVICE_TYPE.VIDEO) {
					prefs.mVideoDeviceName = mDeviceName;
				}
				if (mDeviceType == GraphFactory.DEVICE_TYPE.AUDIO) {
					prefs.mAudioDeviceName = mDeviceName;
				}
				prefs.SaveToFile();
			}
			Close();
		}
	}
}
