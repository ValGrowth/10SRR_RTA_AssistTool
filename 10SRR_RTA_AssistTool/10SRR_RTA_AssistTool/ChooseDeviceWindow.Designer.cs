namespace TenSRR_RTA_AssistTool {
	partial class ChooseDeviceWindow {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.ComboBox_DeviceName = new System.Windows.Forms.ComboBox();
            this.Label_ChooseDevice = new System.Windows.Forms.Label();
            this.Button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ComboBox_DeviceName
            // 
            this.ComboBox_DeviceName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboBox_DeviceName.FormattingEnabled = true;
            this.ComboBox_DeviceName.Location = new System.Drawing.Point(12, 37);
            this.ComboBox_DeviceName.Name = "ComboBox_DeviceName";
            this.ComboBox_DeviceName.Size = new System.Drawing.Size(299, 20);
            this.ComboBox_DeviceName.TabIndex = 0;
            // 
            // Label_ChooseDevice
            // 
            this.Label_ChooseDevice.AutoSize = true;
            this.Label_ChooseDevice.Location = new System.Drawing.Point(12, 20);
            this.Label_ChooseDevice.Name = "Label_ChooseDevice";
            this.Label_ChooseDevice.Size = new System.Drawing.Size(78, 12);
            this.Label_ChooseDevice.TabIndex = 1;
            this.Label_ChooseDevice.Text = "ChooseDevice";
            // 
            // Button_OK
            // 
            this.Button_OK.Location = new System.Drawing.Point(236, 70);
            this.Button_OK.Name = "Button_OK";
            this.Button_OK.Size = new System.Drawing.Size(75, 23);
            this.Button_OK.TabIndex = 2;
            this.Button_OK.Text = "OK";
            this.Button_OK.UseVisualStyleBackColor = true;
            this.Button_OK.Click += new System.EventHandler(this.Button_OK_Click);
            // 
            // ChooseDeviceWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 105);
            this.Controls.Add(this.Button_OK);
            this.Controls.Add(this.Label_ChooseDevice);
            this.Controls.Add(this.ComboBox_DeviceName);
            this.Name = "ChooseDeviceWindow";
            this.Text = "ChooseDevice - SMM2_RTA_AssistTool";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox ComboBox_DeviceName;
		private System.Windows.Forms.Label Label_ChooseDevice;
		private System.Windows.Forms.Button Button_OK;
	}
}