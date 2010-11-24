/*******************************************************************************

    NbuExplorer - Nokia backup file parser, viewer and extractor
    Copyright (C) 2010 Petr Vilem

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

    Project homepage: http://sourceforge.net/projects/nbuexplorer/
    Author: Petr Vilem, petrusek@seznam.cz

*******************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NbuExplorer
{
	public partial class FormMain : Form
	{
		string currentFileName = "";
		string appTitle = "";
		FileInfoComparer fic = new FileInfoComparer();
		System.Globalization.NumberFormatInfo fileSizeFormat;

		public FormMain()
		{
			InitializeComponent();

			textBoxMessage.Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 15);

			dataGridViewMessages.AutoGenerateColumns = false;
			dataGridViewMessages.DataSource = DataSetNbuExplorer.DefaultMessageView;

			fileSizeFormat = new System.Globalization.NumberFormatInfo();
			fileSizeFormat.NumberGroupSeparator = " ";
			fileSizeFormat.NumberDecimalDigits = 0;

			appTitle = this.Text;

			this.Icon = Properties.Resources.icon_phone;
			this.tsPreview.Image = Properties.Resources.icon_view_bottom.ToBitmap();
			this.tsLargeIcons.Image = Properties.Resources.icon_view_multicolumn.ToBitmap();
			this.tsDetails.Image = Properties.Resources.icon_view_detailed.ToBitmap();
			this.exportSelectedFilesToolStripMenuItem.Image = Properties.Resources.icon_document_save.ToBitmap();
			this.exportSelectedFilesToolStripMenuItem1.Image = Properties.Resources.icon_document_save.ToBitmap();
			this.exportSelectedFolderToolStripMenuItem.Image = Properties.Resources.icon_save_all.ToBitmap();
			this.exportToolStripMenuItem.Image = Properties.Resources.icon_save_all.ToBitmap();
			this.exportAllToolStripMenuItem.Image = Properties.Resources.icon_save_all.ToBitmap();
			this.tsExportMessages.Image = Properties.Resources.icon_save_all.ToBitmap();
			exportSelectedFilesToolStripMenuItem1.Font = new Font(exportSelectedFilesToolStripMenuItem1.Font, FontStyle.Bold);

			this.textBoxPreview.Dock = DockStyle.Fill;

			listViewFiles.ListViewItemSorter = fic;
			listViewFiles_Resize(this, EventArgs.Empty);
			listViewFiles_SelectedIndexChanged(this, EventArgs.Empty);

			recountTotal();
		}

		private void addLine(string line)
		{
			textBoxLog.AppendText(line);
			textBoxLog.AppendText("\r\n");
			Application.DoEvents();
		}

		private TreeNode findOrCreateDirNode(string path)
		{
			string[] pth = path.Split('\\', '/');
			TreeNodeCollection cc = treeViewDirs.Nodes;
			TreeNode cn = null;
			for (int i = 0; i < pth.Length; i++)
			{
				if (pth[i].Length == 0) continue;
				cn = null;
				foreach (TreeNode tn in cc)
				{
					if (tn.Text.ToLower() == pth[i].ToLower())
					{
						cn = tn;
						break;
					}
				}
				if (cn == null)
				{
					cn = cc.Add(pth[i]);
					cn.Tag = new List<FileInfo>();
					if (cn.Level == 0)
					{
						// section icon
						foreach (Section sect in NokiaConstants.KnownSections)
						{
							if (pth[i] == sect.name)
							{
								cn.ImageIndex = cn.SelectedImageIndex = sect.imageIndex;
								break;
							}
						}
					}
					else
					{
						cn.ImageIndex = 1;
						cn.SelectedImageIndex = 2;
					}
				}
				cc = cn.Nodes;
			}
			return cn;
		}

		private List<FileInfo> findOrCreateFileInfoList(string path)
		{
			TreeNode tn = findOrCreateDirNode(path);
			return (List<FileInfo>)tn.Tag;
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Nokia backup files|*.nbu;*.nfb;*.nfc;*.arc|All files (bruteforce scan)|*.*";
#if DEBUG
			ofd.Multiselect = true;
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				foreach (string filename in ofd.FileNames)
				{
					System.Diagnostics.Debug.WriteLine(filename);
					OpenFile(filename, (ofd.FilterIndex == 2));
				}
			}
#else
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				OpenFile(ofd.FileName, (ofd.FilterIndex == 2));
			}
#endif
		}

		private void treeViewDirs_AfterSelect(object sender, TreeViewEventArgs e)
		{
			exportToolStripMenuItem.Enabled = exportSelectedFolderToolStripMenuItem.Enabled = (treeViewDirs.SelectedNode != null);

			listViewFiles.Items.Clear();
			listViewFiles_SelectedIndexChanged(sender, EventArgs.Empty);
			listViewFiles.ListViewItemSorter = null;

			if (treeViewDirs.SelectedNode != null)
			{
				recountSelected(treeViewDirs.SelectedNode);
			}

			List<FileInfo> lfi = treeViewDirs.SelectedNode.Tag as List<FileInfo>;
			if (lfi == null || lfi.Count == 0) return;

			foreach (FileInfo fi in lfi)
			{
				ListViewItem li = listViewFiles.Items.Add(fi.Filename);
				li.Tag = fi;
				li.SubItems.Add(fi.FileSize.ToString("N", fileSizeFormat) + " Bytes");
				if (fi.FileTimeIsValid) li.SubItems.Add(fi.FileTime.ToString("dd.MM.yyyy HH:mm"));

				string safename = fi.SafeFilename;
				string key = "";
				try
				{
					key = Path.GetExtension(safename);
				}
				catch { }
				if (string.IsNullOrEmpty(key)) key = "_noextension_";
				if (!imageListFilesLarge.Images.ContainsKey(key))
				{
					imageListFilesLarge.Images.Add(key, ExtractIcon.GetIcon(safename, false, false));
					imageListFilesSmall.Images.Add(key, ExtractIcon.GetIcon(safename, true, false));
				}
				li.ImageIndex = imageListFilesLarge.Images.IndexOfKey(key);
			}

			listViewFiles.ListViewItemSorter = fic;

			if (listViewFiles.Items.Count > 0) listViewFiles.Items[0].Selected = true;
		}

		private void listViewFiles_DoubleClick(object sender, EventArgs e)
		{
			if (listViewFiles.SelectedItems.Count == 0) return;
			FileInfo fi = listViewFiles.SelectedItems[0].Tag as FileInfo;
			if (fi == null) return;

			SaveFileDialog sfd = new SaveFileDialog();
			sfd.FileName = fi.SafeFilename;
			sfd.Filter = string.Format("*{0}|*{0}", Path.GetExtension(sfd.FileName));
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				try
				{
					writeFile(fi, sfd.FileName);
				}
				catch (Exception exc)
				{
					MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void hidePreview()
		{
			textBoxPreview.Clear();
			pictureBoxPreview.Image = null;
		}

		private void listViewFiles_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewFiles.SelectedItems.Count == 0)
			{
				hidePreview();
				exportSelectedFilesToolStripMenuItem.Enabled = exportSelectedFilesToolStripMenuItem1.Enabled = false;
				return;
			}

			recountSelected(listViewFiles.SelectedItems);

			exportSelectedFilesToolStripMenuItem.Enabled = exportSelectedFilesToolStripMenuItem1.Enabled = true;

			FileInfo fi = listViewFiles.SelectedItems[0].Tag as FileInfo;
			if (fi == null || fi.FileSize > 5000000)
			{
				hidePreview();
				return;
			}

			try
			{
				MemoryStream ms = new MemoryStream();
				fi.CopyToStream(currentFileName, ms);
				ms.Seek(0, SeekOrigin.Begin);

				try
				{
					if (treeViewDirs.SelectedNode.Parent.Parent.Text.StartsWith("Mail"))
					{
						textBoxPreview.Visible = true;
						pictureBoxPreview.Visible = false;
						if (StreamUtils.SeekTo(UnicodeExpander.msgBodyStartSeq, ms))
						{
							string body = UnicodeExpander.Expand(ms);

							textBoxPreview.Text = body;
							return;
						}
						else
						{
							textBoxPreview.Text = "Unknown format";
							return;
						}
					}
				}
				catch { }

				System.Text.StringBuilder sb;
				StreamReader sr;

				switch (Path.GetExtension(fi.SafeFilename).ToLower())
				{
					case ".vcf":
					case ".vcs":
						// VCARD text preview
						sr = new StreamReader(ms, true);
						Vcard vcrd = new Vcard(sr.ReadToEnd());
						sb = new System.Text.StringBuilder();
						foreach (string key in vcrd.Keys)
						{
							if (key == "VERSION") continue;
							sb.AppendFormat("{0}: {1}\r\n", key, vcrd[key]);
						}
						if (vcrd.PhoneNumbers.Count > 0)
						{
							sb.Append("TEL: ");
							for (int i = 0; i < vcrd.PhoneNumbers.Count; i++)
							{
								if (i > 0) sb.Append(", ");
								sb.Append(vcrd.PhoneNumbers[i]);
							}
							sb.AppendLine();
						}
						sr.BaseStream.Seek(0, SeekOrigin.Begin);
						sb.AppendLine("\r\n--- Raw VCARD format ---\r\n");
						sb.Append(sr.ReadToEnd());
						sr.Close();

						textBoxPreview.Text = sb.ToString();
						textBoxPreview.Visible = true;
						pictureBoxPreview.Visible = false;
						break;
					case ".vmg":
					case ".url":
					case ".txt":
					case ".jad":
					case ".xml":
					case ".csv":
					case ".ini":
					case ".log":
					case ".xhtml":
					case ".html":
					case ".htm":
					case ".smil":
					case ".css":
					case ".wbxml":
						// general text preview
						int b;
						bool unicode = false;
						while ((b = ms.ReadByte()) != -1)
						{
							if (b == 0)
							{
								unicode = true;
								break;
							}
						}
						ms.Seek(0, SeekOrigin.Begin);

						if (unicode) sr = new StreamReader(ms, System.Text.Encoding.Unicode);
						else sr = new StreamReader(ms, true);

						sb = new System.Text.StringBuilder();
						while (!sr.EndOfStream)
						{
							sb.AppendLine(sr.ReadLine());
						}
						sr.Close();

						textBoxPreview.Text = sb.ToString();
						textBoxPreview.Visible = true;
						pictureBoxPreview.Visible = false;
						break;
					default:
						Image img = Image.FromStream(ms);
						pictureBoxPreview.Image = img;

						textBoxPreview.Visible = false;
						pictureBoxPreview.Visible = true;
						break;
				}
			}
			catch
			{
				hidePreview();
			}
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutBox ab = new AboutBox();
			ab.ShowDialog();
			ab.Dispose();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void tsLargeIcons_Click(object sender, EventArgs e)
		{
			listViewFiles.View = View.LargeIcon;
			tsLargeIcons.Checked = true;
			tsDetails.Checked = false;
		}

		private void tsDetails_Click(object sender, EventArgs e)
		{
			listViewFiles.View = View.Details;
			tsLargeIcons.Checked = false;
			tsDetails.Checked = true;
		}

		private void tsPreview_Click(object sender, EventArgs e)
		{
			splitContainer2.Panel2Collapsed = !tsPreview.Checked;
		}

		private void listViewFiles_Resize(object sender, EventArgs e)
		{
			colName.Width = Math.Max(200, listViewFiles.Width - colSize.Width - colTime.Width - 24);
		}

		private void listViewFiles_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			if (e.Column == 1)
			{
				if (fic.SortType == FileInfoSortType.size)
				{
					fic.desc = !fic.desc;
				}
				else
				{
					fic.SortType = FileInfoSortType.size;
					fic.desc = false;
				}
			}
			else if (e.Column == 2)
			{
				if (fic.SortType == FileInfoSortType.time)
				{
					fic.desc = !fic.desc;
				}
				else
				{
					fic.SortType = FileInfoSortType.time;
					fic.desc = false;
				}
			}
			else
			{
				if (fic.SortType == FileInfoSortType.name)
				{
					fic.desc = !fic.desc;
				}
				else
				{
					fic.SortType = FileInfoSortType.name;
					fic.desc = false;
				}
			}
			listViewFiles.Sort();
		}

		FolderBrowserDialog fbd = null;
		private FolderBrowserDialog Fbd
		{
			get
			{
				if (fbd == null)
				{
					fbd = new FolderBrowserDialog();
					fbd.ShowNewFolderButton = true;
					fbd.Description = "Select target folder for export";
				}
				return fbd;
			}
		}

		private void exportSelectedFilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (listViewFiles.SelectedItems.Count == 0)
			{
				return;
			}
			else if (listViewFiles.SelectedItems.Count == 1)
			{
				listViewFiles_DoubleClick(sender, e);
			}
			else
			{
				if (Fbd.ShowDialog() == DialogResult.OK)
				{
					overWriteDialog = null; // reset possible "yes to all / no to all" from previous operation
					this.Enabled = false;
					try
					{
						foreach (ListViewItem lvi in listViewFiles.SelectedItems)
						{
							FileInfo fi = lvi.Tag as FileInfo;
							if (!interactiveSaveFile(fbd.SelectedPath, fi)) break;
						}
					}
					catch (Exception exc)
					{
						MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					this.Enabled = true;
				}
			}
		}

		MessageBoxWithMemory overWriteDialog = null;

		private bool interactiveSaveFile(string targetPath, FileInfo fi)
		{
			string targetFilename = Path.Combine(targetPath, fi.SafeFilename);

			if (File.Exists(targetFilename))
			{
				DialogResult dr;

				if (overWriteDialog == null)
				{
					overWriteDialog = new MessageBoxWithMemory();
					overWriteDialog.Text = "File already exists";
				}

				if (overWriteDialog.MemorizedDialogResult == DialogResult.None)
				{
					overWriteDialog.MessageText = string.Format("Overwrite file '{0}'?", fi.Filename);
					dr = overWriteDialog.ShowDialog();
				}
				else
				{
					dr = overWriteDialog.MemorizedDialogResult;
				}

				if (dr == DialogResult.No) return true;
				if (dr == DialogResult.Cancel) return false;
			}

			writeFile(fi, targetFilename);

			return true;
		}

		private void writeFile(FileInfo fi, string targetFilename)
		{
			FileStream fs = File.Create(targetFilename);
			try
			{
				fi.CopyToStream(currentFileName, fs);
			}
			finally
			{
				fs.Close();
			}

			if (fi.FileTimeIsValid)
			{
				try
				{
					File.SetCreationTime(targetFilename, fi.FileTime);
					File.SetLastWriteTime(targetFilename, fi.FileTime);
				}
				catch { }
			}
		}

		private void exportAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeViewDirs.Nodes.Count == 0) return;

			if (Fbd.ShowDialog() == DialogResult.OK)
			{
				overWriteDialog = null; // reset possible "yes to all / no to all" from previous operation
				this.Enabled = false;
				try
				{
					exportFolder(Fbd.SelectedPath, treeViewDirs.Nodes);
				}
				catch (Exception exc)
				{
					MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				this.Enabled = true;
			}
		}

		private void exportFolder(string path, TreeNodeCollection tc)
		{
			foreach (TreeNode tn in tc)
			{
				string subpath = Path.Combine(path, FileInfo.MakeFileOrDirNameSafe(tn.Text));
				if (!Directory.Exists(subpath)) Directory.CreateDirectory(subpath);

				foreach (FileInfo fi in (List<FileInfo>)tn.Tag)
				{
					if (!interactiveSaveFile(subpath, fi)) return;
				}

				exportFolder(subpath, tn.Nodes);
			}
		}

		private void exportSelectedFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeViewDirs.SelectedNode != null)
			{
				if (Fbd.ShowDialog() == DialogResult.OK)
				{
					overWriteDialog = null; // reset possible "yes to all / no to all" from previous operation
					this.Enabled = false;
					try
					{
						string subpath = Fbd.SelectedPath;
						TreeNode tn = treeViewDirs.SelectedNode;

						foreach (FileInfo fi in (List<FileInfo>)tn.Tag)
						{
							if (!interactiveSaveFile(subpath, fi))
							{
								this.Enabled = true;
								return;
							}
						}
						exportFolder(subpath, tn.Nodes);
					}
					catch (Exception exc)
					{
						MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					this.Enabled = true;
				}
			}
		}

		private void sortToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (sender == byTimeAscendingToolStripMenuItem || sender == byTimeDescendingToolStripMenuItem)
			{
				fic.SortType = FileInfoSortType.time;
			}
			else if (sender == bySizeAscendingToolStripMenuItem || sender == bySizeDescendingToolStripMenuItem)
			{
				fic.SortType = FileInfoSortType.size;
			}
			else if (sender == byExtensionAscendingToolStripMenuItem || sender == byExtensionDescendingToolStripMenuItem)
			{
				fic.SortType = FileInfoSortType.extension;
			}
			else
			{
				fic.SortType = FileInfoSortType.name;
			}

			fic.desc = (sender == byNameDescendingToolStripMenuItem ||
				sender == bySizeDescendingToolStripMenuItem ||
				sender == byExtensionDescendingToolStripMenuItem ||
				sender == byTimeDescendingToolStripMenuItem);

			listViewFiles.Sort();
		}

		private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			treeViewDirs.ExpandAll();
		}

		private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			treeViewDirs.CollapseAll();
		}

		private void treeViewDirs_MouseDown(object sender, MouseEventArgs e)
		{
			TreeNode tn = treeViewDirs.GetNodeAt(e.X, e.Y);
			if (tn != null) treeViewDirs.SelectedNode = tn;
		}

		private void sortToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			foreach (ToolStripItem ti in sortToolStripMenuItem.DropDownItems)
			{
				ToolStripMenuItem mi = ti as ToolStripMenuItem;
				if (mi == null) continue;
				mi.Checked = false;
			}

			switch (fic.SortType)
			{
				case FileInfoSortType.extension:
					if (fic.desc) byExtensionDescendingToolStripMenuItem.Checked = true;
					else byExtensionAscendingToolStripMenuItem.Checked = true;
					break;
				case FileInfoSortType.size:
					if (fic.desc) bySizeDescendingToolStripMenuItem.Checked = true;
					else bySizeAscendingToolStripMenuItem.Checked = true;
					break;
				case FileInfoSortType.time:
					if (fic.desc) byTimeDescendingToolStripMenuItem.Checked = true;
					else byTimeAscendingToolStripMenuItem.Checked = true;
					break;
				case FileInfoSortType.name:
					if (fic.desc) byNameDescendingToolStripMenuItem.Checked = true;
					else byNameAscendingToolStripMenuItem.Checked = true;
					break;
			}
		}

		private bool compareFileByContent(FileInfo f1, FileInfo f2)
		{
			if (f1.FileSize != f2.FileSize) return false;

			MemoryStream ms1 = new MemoryStream();
			f1.CopyToStream(currentFileName, ms1);
			ms1.Seek(0, SeekOrigin.Begin);

			MemoryStream ms2 = new MemoryStream();
			f2.CopyToStream(currentFileName, ms2);
			ms2.Seek(0, SeekOrigin.Begin);

			while (ms1.Position < ms1.Length)
			{
				if (ms1.ReadByte() != ms2.ReadByte())
				{
					ms1.Dispose();
					ms2.Dispose();
					return false;
				}
			}

			return true;
		}

		private void recursiveRenameDuplicates(TreeNodeCollection col)
		{
			foreach (TreeNode tn in col)
			{
				renameDuplicates(tn);
				recursiveRenameDuplicates(tn.Nodes);
			}
		}

		private void renameDuplicates(TreeNode node)
		{
			List<FileInfo> list = node.Tag as List<FileInfo>;
			if (list == null || list.Count < 2) return;
			List<string> usednames = new List<string>();

			for (int i = list.Count - 1; i > -1; i--)
			{
				bool remove = false;
				FileInfo fi = list[i];
				string lowername = fi.Filename.ToLower();
				if (usednames.Contains(lowername))
				{
					for (int j = list.Count - 1; j > i; j--)
					{
						FileInfo fi2 = list[j];
						if (fi.Filename.ToLower() == fi2.Filename.ToLower() && compareFileByContent(fi, fi2))
						{
							remove = true;
							addLine(string.Format("Removing duplicated file '{1}\\{0}'", fi2.Filename, node.Text));
							list.RemoveAt(j);
						}
					}

					if (!remove)
					{
						int counter = 0;
						string filename = Path.GetFileNameWithoutExtension(fi.Filename);
						string ext = Path.GetExtension(fi.Filename);
						do
						{
							counter++;
							fi.Filename = string.Format("{0}[{1}]{2}", filename, counter, ext);
							lowername = fi.Filename.ToLower();
						}
						while (usednames.Contains(lowername));
					}
				}
				if (!remove) usednames.Add(lowername);
			}
		}

		private void FormMain_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				string[] dragfiles = (string[])e.Data.GetData("FileDrop");
				if (File.Exists(dragfiles[0])) e.Effect = DragDropEffects.Copy;
			}
			catch
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private void FormMain_DragDrop(object sender, DragEventArgs e)
		{
			try
			{
				string[] dragfiles = (string[])e.Data.GetData("FileDrop");
				OpenFile(dragfiles[0], false);
			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void saveParsingLogToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "*.txt|*.txt";
			sfd.FileName = Path.GetFileNameWithoutExtension(currentFileName) + "_log";
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				StreamWriter sw = new StreamWriter(sfd.FileName);
				sw.WriteLine(currentFileName);
				sw.WriteLine();
				sw.Write(textBoxLog.Text);
				sw.Close();
			}
			sfd.Dispose();
		}

		private void addCountNode(TreeNode tn, ref int fileCount, ref long totalSize)
		{
			foreach (FileInfo fi in (List<FileInfo>)tn.Tag)
			{
				fileCount++;
				totalSize += fi.FileSize;
			}

			foreach (TreeNode chtn in tn.Nodes)
			{
				addCountNode(chtn, ref fileCount, ref totalSize);
			}
		}

		private void recountTotal()
		{
			int fileCount = 0;
			long totalSize = 0;
			foreach (TreeNode tn in treeViewDirs.Nodes)
			{
				addCountNode(tn, ref fileCount, ref totalSize);
			}
			statusLabelTotal.Text = string.Format(fileSizeFormat, "Total: {0} files / {1:N} Bytes", fileCount, totalSize);
		}

		private void recountSelected(TreeNode selNode)
		{
			int fileCount = 0;
			long totalSize = 0;
			addCountNode(selNode, ref fileCount, ref totalSize);
			statusLabelSelected.Text = string.Format(fileSizeFormat, "Selected: {0} files, {1:N} Bytes", fileCount, totalSize);
		}

		private void recountSelected(System.Windows.Forms.ListView.SelectedListViewItemCollection selFiles)
		{
			int fileCount = selFiles.Count;
			long totalSize = 0;
			foreach (ListViewItem it in selFiles)
			{
				totalSize += ((FileInfo)it.Tag).FileSize;
			}
			statusLabelSelected.Text = string.Format(fileSizeFormat, "Selected: {0} files, {1:N} Bytes", fileCount, totalSize);
		}

		private void dataGridViewMessages_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
		{
			try
			{
				DataGridViewRow row = dataGridViewMessages.Rows[e.RowIndex];
				DataSetNbuExplorer.MessageRow mr = ((DataRowView)row.DataBoundItem).Row as DataSetNbuExplorer.MessageRow;

				if (mr.box == "O") row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 255);
				else if (mr.box == "I") row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 255);
			}
			catch { }
		}

		private void dataGridViewMessages_SelectionChanged(object sender, EventArgs e)
		{
			if (dataGridViewMessages.SelectedRows.Count > 1)
			{
				DataGridViewSelectedRowCollection sel = dataGridViewMessages.SelectedRows;
				int maxindex = int.MinValue;
				int minindex = int.MaxValue;
				foreach (DataGridViewRow dgr in sel)
				{
					maxindex = Math.Max(maxindex, dgr.Index + 1);
					minindex = Math.Min(minindex, dgr.Index + 1);
				}
				toolStripLabelPos.Text = string.Format("{0}...{1} ({2})/{3}", minindex, maxindex, sel.Count, dataGridViewMessages.Rows.Count);
			}
			else if (dataGridViewMessages.SelectedRows.Count == 1)
			{
				toolStripLabelPos.Text = string.Format("{0}/{1}", dataGridViewMessages.SelectedRows[0].Index + 1, dataGridViewMessages.Rows.Count);
			}
			else
			{
				toolStripLabelPos.Text = string.Format("{0}", dataGridViewMessages.Rows.Count);
			}

			try
			{
				DataSetNbuExplorer.MessageRow mr = ((DataRowView)dataGridViewMessages.SelectedRows[0].DataBoundItem).Row as DataSetNbuExplorer.MessageRow;
				textBoxMessage.Text = mr.messagetext.Replace("\n", "\r\n");
			}
			catch
			{
				textBoxMessage.Text = "";
			}
		}

		private void buildFilterSubNodes(TreeNode tn, string box)
		{
			tn.Nodes.Clear();
			List<string> nums = new List<string>();
			foreach (DataSetNbuExplorer.MessageRow mr in DataSetNbuExplorer.DefaultMessageTable.Select("box='" + box + "'", "name"))
			{
				if (mr.IsnumberNull()) continue;
				if (!nums.Contains(mr.number))
				{
					TreeNode subnode = tn.Nodes.Add(mr.name);
					subnode.Tag = mr.number;
					subnode.Checked = tn.Checked;
					nums.Add(mr.number);
					int cnt = Convert.ToInt32(DataSetNbuExplorer.DefaultMessageTable.Compute("COUNT(box)", "box='" + box + "' AND number='" + mr.number + "'"));
					subnode.Text += string.Format(" ({0})", cnt);
				}
			}
		}

		private void treeViewMsgFilter_AfterCheck(object sender, TreeViewEventArgs e)
		{
			treeViewMsgFilter.AfterCheck -= new TreeViewEventHandler(treeViewMsgFilter_AfterCheck);

			if (e.Node != null)
			{
				if (e.Node.Level == 1)
				{
					if (e.Node.Checked) e.Node.Parent.Checked = true;
				}
				else
				{
					bool ch = e.Node.Checked;
					foreach (TreeNode child in e.Node.Nodes) child.Checked = ch;
				}
			}

			System.Text.StringBuilder sbFilter = new System.Text.StringBuilder();
			appendFilter(sbFilter, treeViewMsgFilter.Nodes[0], "I");
			appendFilter(sbFilter, treeViewMsgFilter.Nodes[1], "O");
			appendFilter(sbFilter, treeViewMsgFilter.Nodes[2], "U");

			if (sbFilter.Length == 0)
			{
				DataSetNbuExplorer.DefaultMessageView.RowFilter = "1=0";
			}
			else
			{
				DataSetNbuExplorer.DefaultMessageView.RowFilter = sbFilter.ToString();
			}

			System.Diagnostics.Debug.WriteLine(DataSetNbuExplorer.DefaultMessageView.RowFilter);

			treeViewMsgFilter.AfterCheck += new TreeViewEventHandler(treeViewMsgFilter_AfterCheck);
		}

		private void appendFilter(System.Text.StringBuilder sbout, TreeNode tn, string box)
		{
			if (!tn.Checked) return;

			bool allChecked = true;
			bool noneChecked = (tn.Nodes.Count > 0);
			foreach (TreeNode ch in tn.Nodes)
			{
				if (!ch.Checked) allChecked = false;
				else noneChecked = false;
			}
			if (noneChecked) return;

			if (sbout.Length > 0) sbout.Append(" OR ");
			sbout.Append("(box = '" + box + "'");
			if (allChecked)
			{
				sbout.Append(")");
				return;
			}

			sbout.Append(" AND number IN(");

			bool first = true;
			foreach (TreeNode ch in tn.Nodes)
			{
				if (ch.Checked)
				{
					if (!first) sbout.Append(",");
					else first = false;
					sbout.Append("'" + ch.Tag.ToString() + "'");
				}
			}
			sbout.Append("))");
		}

		private void tsExportMessages_Click(object sender, EventArgs e)
		{
			if (dataGridViewMessages.Rows.Count == 0)
			{
				MessageBox.Show("No messages to export...", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "Plain text file (*.txt)|*.txt|Comma-separated values (*.csv)|*.csv|All messages to XML file (*.xml)|*.xml";
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				if (sfd.FilterIndex == 3) // xml format - all messages
				{
					DataSetNbuExplorer.DefaultMessageTable.boxColumn.ColumnMapping = MappingType.Attribute;
					DataSetNbuExplorer.DefaultMessageTable.WriteXml(sfd.FileName);
				}
				else
				{
					// text formats (txt and csv)
					StreamWriter sw = null;
					try
					{
						System.Text.Encoding enc = (sfd.FilterIndex == 2) ? System.Text.Encoding.Default : System.Text.Encoding.UTF8;
						sw = new StreamWriter(sfd.FileName, false, enc);
						if (dataGridViewMessages.SelectedRows.Count > 1)
						{
							foreach (DataGridViewRow dvr in dataGridViewMessages.SelectedRows)
							{
								writeMessageInTextFormat(sw, dvr, sfd.FilterIndex);
							}
						}
						else
						{
							foreach (DataGridViewRow dvr in dataGridViewMessages.Rows)
							{
								writeMessageInTextFormat(sw, dvr, sfd.FilterIndex);
							}
						}
					}
					catch (Exception exc)
					{
						MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					finally
					{
						if (sw != null) sw.Close();
					}
				}
			}
		}

		private static void writeMessageInTextFormat(StreamWriter sw, DataGridViewRow dvr, int format)
		{
			DataSetNbuExplorer.MessageRow mr = (DataSetNbuExplorer.MessageRow)((DataRowView)dvr.DataBoundItem).Row;

			string msgdirection = "";
			switch (mr.box)
			{
				case "I": msgdirection = "from"; break;
				case "O": msgdirection = "to"; break;
			}

			if (format == 2) // filterindex 2 (csv)
			{
				sw.WriteLine(string.Format("{0};{1};{2};\"{3}\";\"{4}\"",
					mr.IstimeNull() ? "" : mr.time.ToString(),
					msgdirection,
					mr.number,
					(mr.name == mr.number) ? "" : mr.name.Replace("\"", "\"\""),
					mr.messagetext.Replace("\"", "\"\"")
					));
			}
			else
			{
				if (!mr.IstimeNull()) sw.Write(mr.time.ToString() + " ");
				sw.Write(string.Format("{0} {1}", msgdirection, mr.number).TrimStart());
				if (mr.name != mr.number) sw.Write(" (" + mr.name + ")");
				if (!mr.IsnumberNull()) sw.WriteLine(":");
				sw.WriteLine(mr.messagetext.TrimEnd());
				sw.WriteLine();
			}
		}

	}
}
