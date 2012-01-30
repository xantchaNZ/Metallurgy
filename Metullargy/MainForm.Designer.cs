namespace Metallurgy
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ForceReportTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// ForceReportTextBox
			// 
			this.ForceReportTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ForceReportTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForceReportTextBox.Location = new System.Drawing.Point(0, 0);
			this.ForceReportTextBox.Multiline = true;
			this.ForceReportTextBox.Name = "ForceReportTextBox";
			this.ForceReportTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ForceReportTextBox.Size = new System.Drawing.Size(464, 701);
			this.ForceReportTextBox.TabIndex = 0;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 701);
			this.Controls.Add(this.ForceReportTextBox);
			this.Name = "MainForm";
			this.Text = "Force Report";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox ForceReportTextBox;
	}
}

