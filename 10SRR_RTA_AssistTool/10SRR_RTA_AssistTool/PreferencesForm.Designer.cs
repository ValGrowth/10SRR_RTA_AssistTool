namespace TenSRR_RTA_AssistTool {
	partial class PreferencesForm {
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
			this.Button_Save = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.ComboBox_VideoDeviceName = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.ComboBox_AudioDeviceName = new System.Windows.Forms.ComboBox();
			this.Button_Cancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// Button_Save
			// 
			this.Button_Save.Location = new System.Drawing.Point(270, 121);
			this.Button_Save.Name = "Button_Save";
			this.Button_Save.Size = new System.Drawing.Size(75, 23);
			this.Button_Save.TabIndex = 5;
			this.Button_Save.Text = "Save";
			this.Button_Save.UseVisualStyleBackColor = true;
			this.Button_Save.Click += new System.EventHandler(this.Button_Save_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(15, 78);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(69, 12);
			this.label1.TabIndex = 4;
			this.label1.Text = "AudioDevice";
			this.label1.Visible = false;
			// 
			// ComboBox_VideoDeviceName
			// 
			this.ComboBox_VideoDeviceName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboBox_VideoDeviceName.FormattingEnabled = true;
			this.ComboBox_VideoDeviceName.Location = new System.Drawing.Point(90, 47);
			this.ComboBox_VideoDeviceName.Name = "ComboBox_VideoDeviceName";
			this.ComboBox_VideoDeviceName.Size = new System.Drawing.Size(336, 20);
			this.ComboBox_VideoDeviceName.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(15, 50);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(69, 12);
			this.label2.TabIndex = 6;
			this.label2.Text = "VideoDevice";
			// 
			// ComboBox_AudioDeviceName
			// 
			this.ComboBox_AudioDeviceName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboBox_AudioDeviceName.FormattingEnabled = true;
			this.ComboBox_AudioDeviceName.Location = new System.Drawing.Point(90, 75);
			this.ComboBox_AudioDeviceName.Name = "ComboBox_AudioDeviceName";
			this.ComboBox_AudioDeviceName.Size = new System.Drawing.Size(336, 20);
			this.ComboBox_AudioDeviceName.TabIndex = 9;
			this.ComboBox_AudioDeviceName.Visible = false;
			// 
			// Button_Cancel
			// 
			this.Button_Cancel.Location = new System.Drawing.Point(351, 121);
			this.Button_Cancel.Name = "Button_Cancel";
			this.Button_Cancel.Size = new System.Drawing.Size(75, 23);
			this.Button_Cancel.TabIndex = 16;
			this.Button_Cancel.Text = "Cancel";
			this.Button_Cancel.UseVisualStyleBackColor = true;
			this.Button_Cancel.Click += new System.EventHandler(this.Button_Cancel_Click);
			// 
			// PreferencesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(438, 156);
			this.Controls.Add(this.Button_Cancel);
			this.Controls.Add(this.ComboBox_AudioDeviceName);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.Button_Save);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ComboBox_VideoDeviceName);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "PreferencesForm";
			this.Text = "Preferences - SMM2_RTA_AssistTool";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button Button_Save;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox ComboBox_VideoDeviceName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox ComboBox_AudioDeviceName;
		private System.Windows.Forms.Button Button_Cancel;
	}
}