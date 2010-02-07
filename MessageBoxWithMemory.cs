using System;
using System.Drawing;
using System.Windows.Forms;

namespace NbuExplorer
{
	public partial class MessageBoxWithMemory : Form
	{
		DialogResult memorizedDr = DialogResult.None;
		public DialogResult MemorizedDialogResult
		{
			get { return memorizedDr; }
		}

		public MessageBoxWithMemory()
		{
			InitializeComponent();
			pictureBox1.Image = SystemIcons.Question.ToBitmap();
		}

		private void butYesAll_Click(object sender, EventArgs e)
		{
			memorizedDr = DialogResult.Yes;
			this.DialogResult = memorizedDr;
		}

		private void butNoAll_Click(object sender, EventArgs e)
		{
			memorizedDr = DialogResult.No;
			this.DialogResult = memorizedDr;
		}

		public string MessageText
		{
			get { return label1.Text; }
			set { label1.Text = value; }
		}
	}
}
