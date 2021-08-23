using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static TenSRR_RTA_AssistTool.MyMessageBox;

namespace TenSRR_RTA_AssistTool {
	public partial class SettingForm : Form {
		public SettingForm() {
			InitializeComponent();
		}

		private void SettingForm_Load(object sender, EventArgs e) {
			DeathSetting s = DeathSetting.Instance;
			TextBox_PastTimeRange.Text = s.PastTimeRange.ToString();
			TextBox_DeathSpan.Text = s.DeadSpan.Seconds.ToString();
			TextBox_PixelValueThreshold.Text = s.DeadPixelValueThreshold.ToString();
			TextBox_AmplitudeThreshold.Text = s.DeadAmplitudeThreshold.ToString();
		}

		private void SaveButton_Click(object sender, EventArgs e) {
			
			string message = checkInput();
			if (message != "") {
				MyMessageBox.Show(message, MyMessageBoxIcon.Information);
				return;
			}

			DeathSetting s = DeathSetting.Instance;
			s.PastTimeRange = double.Parse(TextBox_PastTimeRange.Text);
			s.setDeadSpan(int.Parse(TextBox_DeathSpan.Text));
			s.DeadPixelValueThreshold = int.Parse(TextBox_PixelValueThreshold.Text);
			s.DeadAmplitudeThreshold = int.Parse(TextBox_AmplitudeThreshold.Text);

			s.SaveToFile();

			this.Close();
		}

		private string checkInput() {
			double tempDouble = 0.0;
			int tempInt = 0;
			bool ret = false;
			ret = double.TryParse(TextBox_PastTimeRange.Text, out tempDouble);
			if (!ret) {
				return "PastTimeRange is invalid.";
			}
			ret = int.TryParse(TextBox_DeathSpan.Text, out tempInt);
			if (!ret) {
				return "DeathSpan is invalid.";
			}
			ret = int.TryParse(TextBox_PixelValueThreshold.Text, out tempInt);
			if (!ret) {
				return "PixelValueThreshold is invalid.";
			}
			ret = int.TryParse(TextBox_AmplitudeThreshold.Text, out tempInt);
			if (!ret) {
				return "AmplitudeThreshold is invalid.";
			}

			if (double.Parse(TextBox_PastTimeRange.Text) < 0.0) {
				return "PastTimeRange must be 0 or greater.";
			}
			if (int.Parse(TextBox_DeathSpan.Text) < 0) {
				return "DeathSpan must be greater than 0";
			}
			if (int.Parse(TextBox_DeathSpan.Text) >= 60) {
				return "DeathSpan must be less than 60 seconds.";
			}
			if (int.Parse(TextBox_PixelValueThreshold.Text) < 0) {
				return "PixelValueThreshold must be 0 or greater.";
			}
			if (int.Parse(TextBox_AmplitudeThreshold.Text) < 0) {
				return "AmplitudeThreshold must be 0 or greater.";
			}
			return "";
		}

		private void CancelButton_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void ResetToDefaultButton_Click(object sender, EventArgs e) {
			TextBox_PastTimeRange.Text = DeathSetting.DEFAULT_PAST_TIME_RANGE.ToString();
			TextBox_DeathSpan.Text = DeathSetting.DEFAULT_DEATH_SPAN.ToString();
			TextBox_PixelValueThreshold.Text = DeathSetting.DEFAULT_DEAD_PIXEL_VALUE_THRESHOLD.ToString();
			TextBox_AmplitudeThreshold.Text = DeathSetting.DEFAULT_DEAD_AMPLITUDE_THRESHOLD.ToString();
		}

		private bool isValidIntKey(char KeyChar) {
			//押されたキーが 0～9か, バックスペースか
			return (KeyChar >= '0' && KeyChar <= '9') || KeyChar == '\b';
		}

		private bool isValidDoubleKey(char KeyChar) {
			//押されたキーが 0～9か, バックスペースか, ドットか
			return (KeyChar >= '0' && KeyChar <= '9') || KeyChar == '\b' || KeyChar == '.';
		}

		private void TextBox_PastTimeRange_KeyPress(object sender, KeyPressEventArgs e) {
			if (!isValidDoubleKey(e.KeyChar)) {
				e.Handled = true;
			}
		}

		private void TextBox_DeathSpan_KeyPress(object sender, KeyPressEventArgs e) {
			if (!isValidIntKey(e.KeyChar)) {
				e.Handled = true;
			}
		}

		private void TextBox_PixelValueThreshold_KeyPress(object sender, KeyPressEventArgs e) {
			if (!isValidIntKey(e.KeyChar)) {
				e.Handled = true;
			}
		}

		private void TextBox_AmplitudeThreshold_KeyPress(object sender, KeyPressEventArgs e) {
			if (!isValidIntKey(e.KeyChar)) {
				e.Handled = true;
			}
		}

		private void TextBox_PastTimeRange_Leave(object sender, EventArgs e) {
			if (string.IsNullOrEmpty(((TextBox)sender).Text)) {
				TextBox_PastTimeRange.Text = "0.0";
			}
		}

		private void TextBox_DeathSpan_Leave(object sender, EventArgs e) {
			if (string.IsNullOrEmpty(((TextBox)sender).Text)) {
				TextBox_DeathSpan.Text = "0";
			}
		}

		private void TextBox_PixelValueThreshold_Leave(object sender, EventArgs e) {
			if (string.IsNullOrEmpty(((TextBox)sender).Text)) {
				TextBox_PixelValueThreshold.Text = "0";
			}
		}

		private void TextBox_AmplitudeThreshold_Leave(object sender, EventArgs e) {
			if (string.IsNullOrEmpty(((TextBox)sender).Text)) {
				TextBox_AmplitudeThreshold.Text = "0";
			}
		}
	}
}
