namespace TenSRR_RTA_AssistTool {
	partial class SettingForm {
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
            this.label2 = new System.Windows.Forms.Label();
            this.TextBox_PastTimeRange = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TextBox_DeathSpan = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TextBox_PixelValueThreshold = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TextBox_AmplitudeThreshold = new System.Windows.Forms.TextBox();
            this.Button_Cancel = new System.Windows.Forms.Button();
            this.Button_ResetToDefault = new System.Windows.Forms.Button();
            this.Button_Save = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.Location = new System.Drawing.Point(11, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 16);
            this.label2.TabIndex = 8;
            this.label2.Text = "PastTimeRange";
            // 
            // TextBox_PastTimeRange
            // 
            this.TextBox_PastTimeRange.Location = new System.Drawing.Point(220, 12);
            this.TextBox_PastTimeRange.MaxLength = 4;
            this.TextBox_PastTimeRange.Name = "TextBox_PastTimeRange";
            this.TextBox_PastTimeRange.Size = new System.Drawing.Size(52, 19);
            this.TextBox_PastTimeRange.TabIndex = 7;
            this.TextBox_PastTimeRange.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_PastTimeRange_KeyPress);
            this.TextBox_PastTimeRange.Leave += new System.EventHandler(this.TextBox_PastTimeRange_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(11, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 16);
            this.label1.TabIndex = 10;
            this.label1.Text = "DeathSpan";
            // 
            // TextBox_DeathSpan
            // 
            this.TextBox_DeathSpan.Location = new System.Drawing.Point(220, 45);
            this.TextBox_DeathSpan.MaxLength = 2;
            this.TextBox_DeathSpan.Name = "TextBox_DeathSpan";
            this.TextBox_DeathSpan.Size = new System.Drawing.Size(52, 19);
            this.TextBox_DeathSpan.TabIndex = 9;
            this.TextBox_DeathSpan.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_DeathSpan_KeyPress);
            this.TextBox_DeathSpan.Leave += new System.EventHandler(this.TextBox_DeathSpan_Leave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label3.Location = new System.Drawing.Point(12, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(141, 16);
            this.label3.TabIndex = 12;
            this.label3.Text = "PixelValueThreshold";
            // 
            // TextBox_PixelValueThreshold
            // 
            this.TextBox_PixelValueThreshold.Location = new System.Drawing.Point(220, 77);
            this.TextBox_PixelValueThreshold.MaxLength = 3;
            this.TextBox_PixelValueThreshold.Name = "TextBox_PixelValueThreshold";
            this.TextBox_PixelValueThreshold.Size = new System.Drawing.Size(52, 19);
            this.TextBox_PixelValueThreshold.TabIndex = 11;
            this.TextBox_PixelValueThreshold.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_PixelValueThreshold_KeyPress);
            this.TextBox_PixelValueThreshold.Leave += new System.EventHandler(this.TextBox_PixelValueThreshold_Leave);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label4.Location = new System.Drawing.Point(11, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(139, 16);
            this.label4.TabIndex = 14;
            this.label4.Text = "AmplitudeThreshold";
            // 
            // TextBox_AmplitudeThreshold
            // 
            this.TextBox_AmplitudeThreshold.Location = new System.Drawing.Point(220, 112);
            this.TextBox_AmplitudeThreshold.MaxLength = 3;
            this.TextBox_AmplitudeThreshold.Name = "TextBox_AmplitudeThreshold";
            this.TextBox_AmplitudeThreshold.Size = new System.Drawing.Size(52, 19);
            this.TextBox_AmplitudeThreshold.TabIndex = 13;
            this.TextBox_AmplitudeThreshold.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_AmplitudeThreshold_KeyPress);
            this.TextBox_AmplitudeThreshold.Leave += new System.EventHandler(this.TextBox_AmplitudeThreshold_Leave);
            // 
            // Button_Cancel
            // 
            this.Button_Cancel.Location = new System.Drawing.Point(220, 164);
            this.Button_Cancel.Name = "Button_Cancel";
            this.Button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.Button_Cancel.TabIndex = 16;
            this.Button_Cancel.Text = "Cancel";
            this.Button_Cancel.UseVisualStyleBackColor = true;
            this.Button_Cancel.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // Button_ResetToDefault
            // 
            this.Button_ResetToDefault.Location = new System.Drawing.Point(12, 164);
            this.Button_ResetToDefault.Name = "Button_ResetToDefault";
            this.Button_ResetToDefault.Size = new System.Drawing.Size(100, 23);
            this.Button_ResetToDefault.TabIndex = 14;
            this.Button_ResetToDefault.Text = "ResetToDefault";
            this.Button_ResetToDefault.UseVisualStyleBackColor = true;
            this.Button_ResetToDefault.Click += new System.EventHandler(this.ResetToDefaultButton_Click);
            // 
            // Button_Save
            // 
            this.Button_Save.Location = new System.Drawing.Point(139, 164);
            this.Button_Save.Name = "Button_Save";
            this.Button_Save.Size = new System.Drawing.Size(75, 23);
            this.Button_Save.TabIndex = 15;
            this.Button_Save.Text = "Save";
            this.Button_Save.UseVisualStyleBackColor = true;
            this.Button_Save.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 199);
            this.Controls.Add(this.Button_Save);
            this.Controls.Add(this.Button_ResetToDefault);
            this.Controls.Add(this.Button_Cancel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.TextBox_AmplitudeThreshold);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TextBox_PixelValueThreshold);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TextBox_DeathSpan);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TextBox_PastTimeRange);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingForm";
            this.Text = "Setting - SMM2_RTA_AssistTool";
            this.Load += new System.EventHandler(this.SettingForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox TextBox_PastTimeRange;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox TextBox_DeathSpan;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox TextBox_PixelValueThreshold;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox TextBox_AmplitudeThreshold;
		private System.Windows.Forms.Button Button_Cancel;
		private System.Windows.Forms.Button Button_ResetToDefault;
		private System.Windows.Forms.Button Button_Save;
	}
}