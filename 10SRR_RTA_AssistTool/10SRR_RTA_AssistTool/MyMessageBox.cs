using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TenSRR_RTA_AssistTool {
	public class MyMessageBox {

		public enum MyMessageBoxIcon {
			Information = MessageBoxIcon.Information,
			Question = MessageBoxIcon.Question,
			Warning = MessageBoxIcon.Warning,
			Error = MessageBoxIcon.Error,
		}

		public static void Show(string text, MyMessageBoxIcon icon) {
			MessageBox.Show(text, "SMMDeathCounter", MessageBoxButtons.OK, (MessageBoxIcon)icon);
		}

		public static void Show(string text, MessageBoxButtons buttons, MyMessageBoxIcon icon) {
			MessageBox.Show(text, "SMMDeathCounter", buttons, (MessageBoxIcon)icon);
		}

	}
}
