namespace NbuExplorer
{
	partial class MessageBoxWithMemory
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
			this.butYes = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.butYesAll = new System.Windows.Forms.Button();
			this.butNo = new System.Windows.Forms.Button();
			this.butNoAll = new System.Windows.Forms.Button();
			this.butCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// butYes
			// 
			this.butYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.butYes.Location = new System.Drawing.Point(85, 53);
			this.butYes.Name = "butYes";
			this.butYes.Size = new System.Drawing.Size(75, 23);
			this.butYes.TabIndex = 1;
			this.butYes.Text = "Yes";
			this.butYes.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(85, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(399, 38);
			this.label1.TabIndex = 0;
			this.label1.Text = "Message";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(12, 12);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(64, 64);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox1.TabIndex = 2;
			this.pictureBox1.TabStop = false;
			// 
			// butYesAll
			// 
			this.butYesAll.Location = new System.Drawing.Point(166, 53);
			this.butYesAll.Name = "butYesAll";
			this.butYesAll.Size = new System.Drawing.Size(75, 23);
			this.butYesAll.TabIndex = 2;
			this.butYesAll.Text = "Yes to all";
			this.butYesAll.UseVisualStyleBackColor = true;
			this.butYesAll.Click += new System.EventHandler(this.butYesAll_Click);
			// 
			// butNo
			// 
			this.butNo.DialogResult = System.Windows.Forms.DialogResult.No;
			this.butNo.Location = new System.Drawing.Point(247, 53);
			this.butNo.Name = "butNo";
			this.butNo.Size = new System.Drawing.Size(75, 23);
			this.butNo.TabIndex = 3;
			this.butNo.Text = "No";
			this.butNo.UseVisualStyleBackColor = true;
			// 
			// butNoAll
			// 
			this.butNoAll.Location = new System.Drawing.Point(328, 53);
			this.butNoAll.Name = "butNoAll";
			this.butNoAll.Size = new System.Drawing.Size(75, 23);
			this.butNoAll.TabIndex = 4;
			this.butNoAll.Text = "No to all";
			this.butNoAll.UseVisualStyleBackColor = true;
			this.butNoAll.Click += new System.EventHandler(this.butNoAll_Click);
			// 
			// butCancel
			// 
			this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butCancel.Location = new System.Drawing.Point(409, 53);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 23);
			this.butCancel.TabIndex = 5;
			this.butCancel.Text = "Cancel";
			this.butCancel.UseVisualStyleBackColor = true;
			// 
			// MessageBoxWithMemory
			// 
			this.AcceptButton = this.butYes;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.butCancel;
			this.ClientSize = new System.Drawing.Size(496, 88);
			this.Controls.Add(this.butCancel);
			this.Controls.Add(this.butNoAll);
			this.Controls.Add(this.butNo);
			this.Controls.Add(this.butYesAll);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butYes);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MessageBoxWithMemory";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button butYes;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button butYesAll;
		private System.Windows.Forms.Button butNo;
		private System.Windows.Forms.Button butNoAll;
		private System.Windows.Forms.Button butCancel;
	}
}