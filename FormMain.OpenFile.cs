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
using System.IO;
using System.Windows.Forms;

namespace NbuExplorer
{
	public partial class FormMain : Form
	{
		public void OpenFile(string nbufilename, bool bruteForceScan)
		{
			this.Text = Path.GetFileName(nbufilename) + " - " + appTitle;
			this.currentFileName = nbufilename;

			textBoxLog.Clear();
			StreamUtils.Counter = 0;

			treeViewDirs.Nodes.Clear();
			listViewFiles.Items.Clear();
			listViewFiles_SelectedIndexChanged(this, EventArgs.Empty);

			this.menuStripMain.Enabled = false;
			this.treeViewDirs.Enabled = false;
			this.Cursor = Cursors.AppStarting;

			try
			{
				FileStream fs = File.OpenRead(currentFileName);

				string fileext = Path.GetExtension(currentFileName).ToLower();

				#region nfb & nfc format
				if (!bruteForceScan && (fileext == ".nfb" || fileext == ".nfc"))
				{
					fs.Seek(4, SeekOrigin.Begin);
					addLine("Phone model:\t" + StreamUtils.ReadString2(fs)); // RM
					addLine("Phone name:\t" + StreamUtils.ReadString2(fs)); // nazev
					addLine("");

					int count = StreamUtils.ReadUInt32asInt(fs);
					addLine(string.Format("{0} items found", count));
					addLine("");

					string name;

					try
					{
						for (int i = 0; i < count; i++)
						{
							uint type = StreamUtils.ReadUInt32(fs);
							switch (type)
							{
								case 2: // folder
									name = StreamUtils.ReadString2(fs);
									addLine(string.Format("Folder '{0}' found", name));
									break;
								case 1: // file
									name = StreamUtils.ReadString2(fs).TrimStart('\\');
									UInt32 len = StreamUtils.ReadUInt32(fs);

									if (name.EndsWith("FolderIndex"))
									{
										fs.Seek(len + 4, SeekOrigin.Current);
									}
									else if (name.EndsWith("InfoIndex"))
									{
										long indexEndAddr = fs.Position + len + 4;
										fs.Seek(5, SeekOrigin.Current);

										StreamReader sr = new StreamReader(fs, System.Text.Encoding.Unicode, false, 32);
										List<string> fileNames = new List<string>();

										string homeDir = Path.GetDirectoryName(name);
										List<FileInfo> list = findOrCreateFileInfoList(homeDir);

										for (int j = list.Count - 1; j > -1; j--)
										{
											try
											{
												int index = int.Parse(list[j].Filename);

												while (index > fileNames.Count - 1 && !sr.EndOfStream)
												{
													fileNames.Add(sr.ReadLine().Replace("\\v", "\\"));
													while (!string.IsNullOrEmpty(sr.ReadLine())) ; // move to next filename
												}

												List<FileInfo> l2 = findOrCreateFileInfoList(homeDir + "\\" + Path.GetDirectoryName(fileNames[index]));
												l2.Add(new FileInfo(Path.GetFileName(fileNames[index]), list[j].Start, list[j].FileSize));
												list.RemoveAt(j);

												addLine(fileNames[index]);
											}
											catch { }
										}

										fs.Seek(indexEndAddr, SeekOrigin.Begin);
									}
									else
									{
										if (name.StartsWith("MPAPI")) name += ".csv";
										if (name.IndexOf('\\') == -1) name = "root\\" + name + ".csv";

										List<FileInfo> list = findOrCreateFileInfoList(Path.GetDirectoryName(name));
										list.Add(new FileInfo(Path.GetFileName(name), fs.Position, len));
										fs.Seek(len + 4, SeekOrigin.Current);
									}
									StreamUtils.Counter += len;
									break;
								default:
									throw new ApplicationException(string.Format("Unknown item type {0}", type));
							}
						}
					}
					catch (Exception exc)
					{
						addLine(exc.Message);
					}

					if (fs.Length - fs.Position > 4) addLine("End of file not reached");
				}
				#endregion
				#region nbu format
				else if (!bruteForceScan && fileext == ".nbu")
				{
					bool analyzeRequest = false;

					fs.Seek(0x14, SeekOrigin.Begin);
					fs.Seek(StreamUtils.ReadUInt64asLong(fs), SeekOrigin.Begin);
					fs.Seek(0x14, SeekOrigin.Current);

					addLine("Backup time:\t" + StreamUtils.ReadNokiaDateTime(fs).ToString()); // datetime
					addLine("Phone IMEI:\t" + StreamUtils.ReadString(fs));
					addLine("Phone model:\t" + StreamUtils.ReadString(fs));
					addLine("Phone name:\t" + StreamUtils.ReadString(fs));
					addLine("Phone firmware:\t" + StreamUtils.ReadString(fs));
					addLine("Phone language:\t" + StreamUtils.ReadString(fs));

					fs.Seek(0x14, SeekOrigin.Current); // ?

					int partcount = StreamUtils.ReadUInt32asInt(fs);
					long partPos, partStartAddr;//, partLength;
					int count;

					List<FileInfo> contactList = null;
					Dictionary<string, string> phNumToName = new Dictionary<string, string>();

					while (partcount > 0)
					{
						byte[] partGuid = new byte[16];
						fs.Read(partGuid, 0, partGuid.Length);

						partStartAddr = StreamUtils.ReadUInt64asLong(fs);
						fs.Seek(8, SeekOrigin.Current);
						//partLength = StreamUtils.ReadUInt64asLong(fs);

						addLine("");
						addLine("BEGIN " + numToAddr(partStartAddr));

						Section sect;

						try
						{
							sect = NokiaConstants.FindSectById(partGuid);
						}
						catch (ApplicationException)
						{
							addLine("Unknown section type - BREAK !!!");
							addLine("TOC addr: " + numToAddr(fs.Position - 32));

							if (MessageBox.Show("Unknown structure type found, process cannot continue. Please consider providing this backup to the author of application for analyzing and improving application.\r\n\r\nIf you need to get your contacts, messages and calendar items, you can try brute force scan mode. Would you like to do it now?", this.appTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
							{
								OpenFile(nbufilename, true);
								return;
							}

							break;
						}

#if DEBUG
						System.Diagnostics.Debug.WriteLine(sect.name + " " + sect.name2);
#endif

						if (string.IsNullOrEmpty(sect.name2))
						{
							addLine(sect.name);
						}
						else
						{
							addLine(string.Format("{0} - {1}", sect.name, sect.name2));
						}
						addLine("=================");

						List<FileInfo> partFiles;
						string foldername, filename;
						int count2;

						switch (sect.type)
						{
							#region filesystem
							case ProcessType.FileSystem:
								count = StreamUtils.ReadUInt32asInt(fs);
								addLine(count.ToString() + " files found");
								fs.Seek(0x14, SeekOrigin.Current); // ?
								long dirStartAddr = StreamUtils.ReadUInt64asLong(fs);
								long fileStartAddr = partStartAddr + 0x30;
								partPos = fs.Position; // zapamatovani pro dalsi process
								fs.Seek(dirStartAddr + 20, SeekOrigin.Begin);
								for (int i = 0; i < count; i++)
								{
									foldername = StreamUtils.ReadString(fs).TrimStart('\\');
									filename = StreamUtils.ReadString(fs);
									fs.Seek(4, SeekOrigin.Current); // ???
									long fileSize = StreamUtils.ReadUInt32(fs);
									DateTime fileTime = StreamUtils.ReadNokiaDateTime(fs);

									addLine(foldername + filename + " - " + fileSize + " Bytes, " + fileTime.ToString());

									fs.Seek(12, SeekOrigin.Current); // ???

									partFiles = findOrCreateFileInfoList(sect.name + "/" + foldername);
									partFiles.Add(new FileInfo(filename, fileStartAddr, fileSize, fileTime));
									fileStartAddr += fileSize;
									StreamUtils.Counter += fileSize;
								}
								fs.Seek(partPos, SeekOrigin.Begin);
								break;
							#endregion
							#region vcards
							case ProcessType.Vcards:
								count = StreamUtils.ReadUInt32asInt(fs);
								partPos = fs.Position;
								fs.Seek(partStartAddr + 0x30, SeekOrigin.Begin);

								partFiles = findOrCreateFileInfoList(sect.name);

								string filenameTemplate;
								if (sect.name == NokiaConstants.ptContacts)
								{
									contactList = partFiles;
									filenameTemplate = "{0}.vcf";
									addLine(count.ToString() + " contacts found");
								}
								else if (sect.name == NokiaConstants.ptBookmarks)
								{
									fs.Seek(8, SeekOrigin.Current);
									filenameTemplate = "{0}.url";
									addLine(count.ToString() + " bookmarks found");
								}
								else
								{
									filenameTemplate = "{0}.vcs";
									addLine(count.ToString() + " calendar items found");
								}

								for (int i = 0; i < count; i++)
								{
									uint test = StreamUtils.ReadUInt32(fs);
									if (test < 0x10)
									{
										foldername = StreamUtils.ReadString(fs);
										partFiles = findOrCreateFileInfoList(sect.name + "\\" + foldername);
										addLine("Folder '" + foldername + "' found");
										fs.Seek(8, SeekOrigin.Current);
									}
									fs.Seek(4, SeekOrigin.Current);
									long start = fs.Position + 4;
									int vclen = StreamUtils.ReadUInt32asInt(fs);
									byte[] buff = new byte[vclen];
									fs.Read(buff, 0, buff.Length);
									StreamUtils.Counter += buff.Length;

									Vcard crd = new Vcard(System.Text.Encoding.UTF8.GetString(buff));

									string name;
									DateTime time = DateTime.MinValue;

									if (sect.name == NokiaConstants.ptContacts)
									{
										name = crd.Name;
										foreach (string number in crd.PhoneNumbers)
										{
											if (phNumToName.ContainsKey(number)) continue;
											phNumToName.Add(number, name);
										}
									}
									else if (sect.name == NokiaConstants.ptBookmarks)
									{
										name = crd["TITLE"];
									}
									else
									{
										partFiles = findOrCreateFileInfoList(sect.name + "\\" + crd["X-EPOCAGENDAENTRYTYPE"]);
										name = crd["SUMMARY"];
										time = crd.GetDateTime("DTSTART");
									}

									if (string.IsNullOrEmpty(name)) name = numToName(i + 1);

									partFiles.Add(new FileInfo(string.Format(filenameTemplate, name), start, vclen, time));
								}

								fs.Seek(partPos, SeekOrigin.Begin);
								count2 = StreamUtils.ReadUInt32asInt(fs);
								for (int i = 0; i < count2; i++)
								{
									fs.Seek(12, SeekOrigin.Current); // poradove cislo adresare + start adresa adresare
								}

								break;
							#endregion
							#region memos
							case ProcessType.Memos:
								count = StreamUtils.ReadUInt32asInt(fs);
								addLine(count.ToString() + " memos found");
								partPos = fs.Position + 4; // zapamatovani konce pro pokracovani
								fs.Seek(partStartAddr + 0x30, SeekOrigin.Begin);
								partFiles = findOrCreateFileInfoList(sect.name);
								for (int i = 0; i < count; i++)
								{
									fs.Seek(4, SeekOrigin.Current);
									int len = StreamUtils.ReadUInt16(fs) * 2;
									partFiles.Add(new FileInfo(string.Format("{0}.txt", numToName(i + 1)), fs.Position, len));
									fs.Seek(len, SeekOrigin.Current);
									StreamUtils.Counter += len;
								}
								fs.Seek(partPos, SeekOrigin.Begin);
								break;
							#endregion
							#region groups
							case ProcessType.Groups:
								count2 = StreamUtils.ReadUInt32asInt(fs);
								count = StreamUtils.ReadUInt32asInt(fs);
								addLine(string.Format("{0} groups found.", count));
								for (int i = 0; i < count; i++)
								{
									fs.Seek(4, SeekOrigin.Current);

									long start = StreamUtils.ReadUInt64asLong(fs);
									addLine("Folder BEGIN " + numToAddr(start));

									partPos = fs.Position;

									fs.Seek(start + 4, SeekOrigin.Begin);
									foldername = StreamUtils.ReadString(fs);
									count2 = StreamUtils.ReadUInt32asInt(fs);
									addLine(string.Format("{0} - {1} contacts", foldername, count2));

									partFiles = findOrCreateFileInfoList(sect.name + "\\" + foldername);
									if (contactList != null)
									{
										for (int j = 0; j < count2; j++)
										{
											int ix = StreamUtils.ReadUInt32asInt(fs);
											if (contactList.Count >= ix) partFiles.Add(contactList[ix - 1]);
											else addLine("Invalid index: " + ix);
										}
									}

									fs.Seek(partPos, SeekOrigin.Begin);
								}
								break;
							#endregion
							#region folders
							case ProcessType.GeneralFolders:
								count2 = StreamUtils.ReadUInt32asInt(fs);
								count = StreamUtils.ReadUInt32asInt(fs);
								addLine(string.Format("{0} folders found.", count));
								for (int i = 0; i < count; i++)
								{
									fs.Seek(4, SeekOrigin.Current);

									long start = StreamUtils.ReadUInt64asLong(fs);

									addLine("");
									addLine("Folder BEGIN " + numToAddr(start));

									partPos = fs.Position;
									foldername = null;

									parseFolder(fs, start, sect.name, phNumToName, ref analyzeRequest);

									fs.Seek(partPos, SeekOrigin.Begin);
								}
								break;
							#endregion
						}

						partcount--;
					}

					if (analyzeRequest)
					{
						MessageBox.Show("Unknown structure type found. Please consider providing this backup to the author of application for analyzing and improving application.", this.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
				#endregion
				#region arc
				else if (!bruteForceScan && fileext == ".arc")
				{
					byte[] seq = new byte[] { 0, 0, 0, 1, 0, 0, 0, 0 };
					byte[] buff = new byte[8];

					fs.Seek(0x3C, SeekOrigin.Begin);
					addLine("Phone model: " + StreamUtils.ReadShortString(fs));
					addLine("");

					long startAddr = fs.Position;

					if (StreamUtils.SeekTo(seq, fs))
					{
						do
						{
							fs.Seek(12, SeekOrigin.Current);
							string filename = StreamUtils.ReadStringTo(fs, 0, 0x80);
							fs.Seek(27, SeekOrigin.Current);
							long compLength = StreamUtils.ReadUInt64asLong(fs);

							addLine(filename + " - compressed size: " + compLength);

							string dir = Path.GetDirectoryName(filename);
							filename = Path.GetFileName(filename);
							List<FileInfo> list = findOrCreateFileInfoList(dir);
							list.Add(new FileInfo(filename, startAddr, compLength, DateTime.MinValue, true));

							StreamUtils.Counter += compLength;
							startAddr += compLength;

							fs.Seek(1, SeekOrigin.Current);
							fs.Read(buff, 0, buff.Length);
						}
						while (NokiaConstants.CompareByteArr(seq, buff));

						addLine(""); // end of first section

						seq[3] = 0;

						List<FileInfo> compFr = findOrCreateFileInfoList("compressed fragments");

						UInt32 lenComp;
						UInt32 lenUncomp;

						while (true)
						{
							fs.Read(buff, 0, buff.Length);
							if (NokiaConstants.CompareByteArr(seq, buff))
							{
								fs.Seek(4, SeekOrigin.Current);
								do
								{
									lenComp = StreamUtils.ReadUInt32(fs);
									lenUncomp = StreamUtils.ReadUInt32(fs);

									FileInfo fi = new FileInfo(numToAddr(fs.Position), fs.Position, lenComp, DateTime.MinValue, true);
									compFr.Add(fi);

									if (lenUncomp == 16)
									{
										addLine(fi.Filename + " - empty compressed fragment");
									}
									else
									{
										addLine(fi.Filename + " - compressed fragment");
									}

									parseCompressedFragment("", fi);

									fs.Seek(lenComp, SeekOrigin.Current);
								}
								while (lenUncomp == 65536);
							}
							else if (StreamUtils.SeekTo(NokiaConstants.compHead, fs))
							{
								fs.Seek(-22, SeekOrigin.Current);
							}
							else break;
						}
					}

					foreach (TreeNode tn in treeViewDirs.Nodes)
					{
						tn.Expand();
					}
				}
				#endregion
				#region bruteforce
				else
				{
					addLine("Scanning for vcards (contacts, messages, calendar items, bookmarks)...");
					addLine("");

					List<FileInfo> files;
					string filename;
					int current;
					Dictionary<string, string> phNumToName = new Dictionary<string, string>();

					int pr = 0;
					int pr2 = 0;

					while ((current = fs.ReadByte()) > -1)
					{
						if (fs.Position % 2048 == 0)
						{
							pr = (int)(20 * fs.Position / fs.Length);
							if (pr != pr2)
							{
								addLine(string.Format("{0:00}%", pr * 5));
								pr2 = pr;
							}
							Application.DoEvents();
						}

						if (Pattern.Msg.Step((byte)current, fs.Position))
						{
							Vcard msg = new Vcard(Pattern.Msg.GetCaptureAsString(fs));
							string box = msg["X-MESSAGE-TYPE"];
							if (box.Length == 0) box = msg["X-IRMC-BOX"];
							files = findOrCreateFileInfoList(NokiaConstants.ptMessages + "\\" + box);
							filename = numToName(files.Count + 1);
							try
							{
								filename = msg.PhoneNumbers[0];
								filename = phNumToName[msg.PhoneNumbers[0]];
							}
							catch { }
							files.Add(new FileInfo(filename + ".vmg", Pattern.Msg.StartIndex, Pattern.Msg.Length, getMsgTime(msg)));
							StreamUtils.Counter += Pattern.Msg.Length;
							addLine(numToProgressAndAddr(Pattern.Msg.StartIndex, fs.Length) + "\tmessage: " + filename);
						}

						if (Pattern.Msg.Active) continue;

						if (Pattern.Contact.Step((byte)current, fs.Position))
						{
							Vcard contact = new Vcard(Pattern.Contact.GetCaptureAsString(fs));
							string name = contact.Name;
							foreach (string number in contact.PhoneNumbers)
							{
								if (!phNumToName.ContainsKey(number)) phNumToName.Add(number, name);
							}
							files = findOrCreateFileInfoList(NokiaConstants.ptContacts);
							filename = name;
							if (filename.Length == 0) filename = numToName(files.Count + 1);
							files.Add(new FileInfo(filename + ".vcf", Pattern.Contact.StartIndex, Pattern.Contact.Length));
							StreamUtils.Counter += Pattern.Contact.Length;
							addLine(numToProgressAndAddr(Pattern.Contact.StartIndex, fs.Length) + "\tcontact: " + filename);
						}

						if (Pattern.Contact.Active) continue;

						if (Pattern.Calendar.Step((byte)current, fs.Position))
						{
							Vcard calendar = new Vcard(Pattern.Calendar.GetCaptureAsString(fs));
							files = findOrCreateFileInfoList("Calendar\\" + calendar["X-EPOCAGENDAENTRYTYPE"]);
							filename = calendar["SUMMARY"];
							if (filename.Length == 0) filename = numToName(files.Count + 1);
							files.Add(new FileInfo(filename + ".vcs", Pattern.Calendar.StartIndex, Pattern.Calendar.Length, calendar.GetDateTime("DTSTART")));
							StreamUtils.Counter += Pattern.Calendar.Length;
							addLine(numToProgressAndAddr(Pattern.Calendar.StartIndex, fs.Length) + "\tcalendar: " + filename);
						}

						if (Pattern.Calendar.Active) continue;

						if (Pattern.Bookmark.Step((byte)current, fs.Position))
						{
							Vcard calendar = new Vcard(Pattern.Bookmark.GetCaptureAsString(fs));
							files = findOrCreateFileInfoList(NokiaConstants.ptBookmarks);
							filename = calendar["TITLE"];
							if (filename.Length == 0) filename = numToName(files.Count + 1);
							files.Add(new FileInfo(filename + ".url", Pattern.Bookmark.StartIndex, Pattern.Bookmark.Length));
							StreamUtils.Counter += Pattern.Bookmark.Length;
							addLine(numToProgressAndAddr(Pattern.Bookmark.StartIndex, fs.Length) + "\tbookmark: " + filename);
						}
					}
				}
				#endregion

				addLine("");
				addLine(string.Format("Done, file coverage: {0:0.##}%", 100 * (float)StreamUtils.Counter / fs.Length));

				fs.Close();

				recursiveRenameDuplicates(treeViewDirs.Nodes);

				exportAllToolStripMenuItem.Enabled = (treeViewDirs.Nodes.Count > 0);
				exportToolStripMenuItem.Enabled = exportSelectedFolderToolStripMenuItem.Enabled = (treeViewDirs.SelectedNode != null);
			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				menuStripMain.Enabled = true;
				treeViewDirs.Enabled = true;
				this.Cursor = Cursors.Default;
			}
		}

		private FileInfoCfMultiPart currentIncompleteMultipartFile = null;

		private void parseCompressedFragment(string rootFolder, FileInfo fi)
		{
			long initCounter = StreamUtils.Counter;

			MemoryStream ms = new MemoryStream();
			fi.CopyToStream(this.currentFileName, ms);

			if (currentIncompleteMultipartFile != null)
			{
				long missingLength = currentIncompleteMultipartFile.MissingLength;
				if (missingLength > ms.Length)
				{
					currentIncompleteMultipartFile.Parts.Add(new FileInfoCfPart("", fi.Start, fi.FileSize, fi.FileTime, 0, ms.Length));
					StreamUtils.Counter += ms.Length;
					addLine("still incomplete file, continue on next fragment...");
				}
				else
				{
					currentIncompleteMultipartFile.Parts.Add(new FileInfoCfPart("", fi.Start, fi.FileSize, fi.FileTime, 0, missingLength));
					StreamUtils.Counter += missingLength;
					ms.Seek(missingLength, SeekOrigin.Current);
					currentIncompleteMultipartFile.Finish();
					addLine(string.Format("multipart file '{0}' complete.", currentIncompleteMultipartFile.Filename));
					currentIncompleteMultipartFile = null;

					if (ms.Length > missingLength)
					{
						addLine("looking for next files in rest of fragment...");
						parseCompressedFragmentFiles(rootFolder, fi, ms);
					}
				}
			}
			else
			{
				ms.Seek(9, SeekOrigin.Begin);
				if (StreamUtils.MatchSequence(NokiaConstants.cfType1Header, ms))
				{
					ms.Seek(0, SeekOrigin.Begin);
					parseCompressedFragmentFiles(rootFolder, fi, ms);
				}
				else
				{
					ms.Seek(5, SeekOrigin.Begin);
					if (StreamUtils.MatchSequence(NokiaConstants.cfType2Header, ms))
					{
						/*if (StreamUtils.SeekTo(NokiaConstants.cfHeader, ms))
						{
							int fileCnt = StreamUtils.ReadUInt32asInt(ms);
							for (int i = 0; i < fileCnt; i++)
							{
								int len = ms.ReadByte();
								if ((len & 3) > 0)
								{
									len = len >> 2;
									len += ms.ReadByte() << 6;
									len = len >> 1;
								}
								else
								{
									len = len >> 2;
								}
								byte[] buffName = new byte[len];
								ms.Read(buffName, 0, len);
								string fname = System.Text.Encoding.Default.GetString(buffName);
								addLine(fname);
							}
						}*/

						ms.Seek(0, SeekOrigin.Begin);
						UInt32 headerLength = StreamUtils.ReadUInt32(ms);
						ms.Seek(headerLength, SeekOrigin.Current);
						parseCompressedFragmentFiles(rootFolder, fi, ms);
					}
					else
					{
						addLine("unknown fragment type");
					}
				}
			}

			long uncompressedCoverage = StreamUtils.Counter - initCounter;
			double ratio = (double)uncompressedCoverage / ms.Length;

			addLine(string.Format("Compressed fragment coverage {0:0.##}%", 100 * ratio));
			addLine("");

			StreamUtils.Counter = initCounter + (long)(fi.FileSize * ratio);
		}

		private void parseCompressedFragmentFiles(string rootFolder, FileInfo fi, MemoryStream ms)
		{
			List<FileInfo> compFr2;
			while (ms.Position < ms.Length)
			{
				int fnameLen = StreamUtils.ReadUInt32asInt(ms);
				long fsize = StreamUtils.ReadUInt32(ms);
				ms.Seek(16, SeekOrigin.Current);
				string fname = StreamUtils.ReadString(ms, fnameLen);
				addLine(fname);

				compFr2 = findOrCreateFileInfoList(rootFolder + "\\" + Path.GetDirectoryName(fname));
				fname = Path.GetFileName(fname).Trim();
				if (fname.Length > 0)
				{
					if (ms.Length >= ms.Position + fsize)
					{
						compFr2.Add(new FileInfoCfPart(fname, fi.Start, fi.FileSize, DateTime.MinValue, ms.Position, fsize));
						StreamUtils.Counter += fsize;
					}
					else
					{
						currentIncompleteMultipartFile = new FileInfoCfMultiPart(fname, DateTime.MinValue, fsize, compFr2);
						currentIncompleteMultipartFile.Parts.Add(new FileInfoCfPart("", fi.Start, fi.FileSize, DateTime.MinValue, ms.Position, ms.Length - ms.Position));
						StreamUtils.Counter += (ms.Length - ms.Position);
						addLine("incomplete file, continue on next fragment...");
					}
				}

				ms.Seek(fsize, SeekOrigin.Current);
			}
		}

		private void parseFolder(FileStream fs, long start, string sectName, Dictionary<string, string> phNumToName, ref bool analyzeRequest)
		{
			int count;
			List<FileInfo> partFiles;
			string foldername;
			string filename;

			switch (sectName)
			{
				#region messages
				case NokiaConstants.ptMessages:
				case NokiaConstants.ptMms:
					fs.Seek(start + 4, SeekOrigin.Begin);
					foldername = StreamUtils.ReadString(fs);
					count = StreamUtils.ReadUInt32asInt(fs);
					addLine(string.Format("{0} - {1} messages", foldername, count));
					partFiles = findOrCreateFileInfoList(sectName + "\\" + foldername);
					for (int j = 0; j < count; j++)
					{
						fs.Seek(8, SeekOrigin.Current);
						long len;
						if (sectName == NokiaConstants.ptMms)
						{
							int test = fs.ReadByte();
							for (int k = 0; k < test; k++)
							{
								fs.Seek(8, SeekOrigin.Current);
								StreamUtils.ReadString(fs); // cislo adresata ?
							}
							fs.Seek(20, SeekOrigin.Current); // 4 + 16

							len = StreamUtils.ReadUInt32(fs);

							long fileStart = fs.Position;
							filename = numToName(j + 1);

							addLine("");

							try
							{
								Mms m = new Mms(fs, fileStart + len - 1);

								addLine(filename + ":");
								addLine(m.ParseLog.TrimEnd());

								if (!string.IsNullOrEmpty(m.Subject))
								{
									filename += " - " + m.Subject;
								}

								if (m.Files.Count > 0)
								{
									TreeNode mmsNode = findOrCreateDirNode(sectName + "\\" + foldername + "\\" + filename);
									mmsNode.Tag = m.Files;
								}
							}
							catch (Exception exc)
							{
								addLine(string.Format("Error parsing mms {0}: {1}", filename, exc.Message));
							}

							partFiles.Add(new FileInfo(string.Format("{0}.mms", filename), fileStart, len));

							fs.Seek(fileStart + len, SeekOrigin.Begin);
						}
						else
						{
							len = StreamUtils.ReadUInt32(fs);
							long mstart = fs.Position;

							byte[] buff = new byte[len];
							fs.Read(buff, 0, buff.Length);

							Vcard crd = new Vcard(System.Text.Encoding.Unicode.GetString(buff));
							DateTime time = getMsgTime(crd);

							string name = numToName(j + 1);
							if (crd.PhoneNumbers.Count > 0)
							{
								string num = crd.PhoneNumbers[0];
								name = name + " " + ((phNumToName.ContainsKey(num)) ? phNumToName[num] : num);
							}

							partFiles.Add(new FileInfo(string.Format("{0}.vmg", name), mstart, len, time));
						}

						StreamUtils.Counter += len;
					}
					break;
				#endregion
				default:
					fs.Seek(start, SeekOrigin.Begin);
					UInt32 tst = StreamUtils.ReadUInt32(fs);
					bool procAsDefault = false;

					switch (tst)
					{
						case 0x0301: // contacts
							parseContacts(fs, ref analyzeRequest);
							break;
						case 0x0302: // groups
							parseGroups(fs);
							break;
						case 0x0312: // calendar - events
						case 0x0313: // calendar - todos
							parseCalendar(fs, ref analyzeRequest);
							break;
						case 0x0314: // memo
							#region memo
							fs.ReadByte(); // ?
							count = StreamUtils.ReadUInt16(fs);
							fs.Seek(2, SeekOrigin.Current); // ?
							for (int j = 0; j < count; j++)
							{
								fs.Seek(0x13, SeekOrigin.Current);
								string s = StreamUtils.ReadString(fs);
								addLine(string.Format("Memo {0}: {1}", j, s));
								fs.Seek(6, SeekOrigin.Current);
							}
							break;
							#endregion
						case 0x0010: // S40 settings
						case 0x0308: // Menu settings
						case 0x0309: // User notes
						case 0x030A: // Playlists
						case 0x030B: // predefcalendar
						case 0x030C: // predefinfofolder
						case 0x030F: // predefgames (java games)
						case 0x0310: // predefcollections (java apps)
						case 0x0311: // Installed Java games/apps
						case 0x0315: // predefemail (java email client)
							int test = fs.ReadByte();
							if (test == 2)
							{
								procAsDefault = true;
							}
							else
							{
								addLine(string.Format("Unimplemented folder srtucture (0x{0}/0x{1})", tst.ToString("X"), test.ToString("X")));
							}
							break;
						case 0x0001: // S60 settings
						case 0x0002: // S60 settings
						case 0x1008: // S60 settings - picture previews?
						case 0x0303: // messages
						case 0x0304: // messages
							procAsDefault = true;
							break;
						case 0x0020: // fm radio stations
							addLine(string.Format("Unimplemented folder structure (0x{0}) - FM radio stations", tst.ToString("X")));
							break;
						case 0x0040: // phone profiles
							addLine(string.Format("Unimplemented folder structure (0x{0}) - Phone profiles", tst.ToString("X")));
							break;
						case 0x1001: // S60 compressed files
							addLine(string.Format("Unimplemented S60 folder stucture (0x{0})", tst.ToString("X")));
							break;
						case 0x1002: // S60 compressed fragments
						case 0x1004: // S60 compressed fragments
						case 0x1006: // S60 compressed fragments
							#region compressed fragments
							addLine(string.Format("S60 compressed folder structure (0x{0})", tst.ToString("X")));

							partFiles = findOrCreateFileInfoList(sectName + "\\compressed fragments\\" + tst.ToString("X"));

							while (fs.Position < fs.Length)
							{
								UInt16 x = StreamUtils.ReadUInt16(fs);
								if (x == 0xFFFF)
								{
									break; // correct end of folder structure
								}
								else if (x != 0)
								{
									addLine("Unexpected folder structure at " + numToAddr(fs.Position) + ": " + x.ToString("X"));
									analyzeRequest = true;
									break;
								}

								filename = StreamUtils.ReadString(fs);

								if (filename.Length > 0)
								{
									fs.Seek(12, SeekOrigin.Current);
									x = StreamUtils.ReadUInt16(fs);
									if (x == 0)
									{
										// empty fragment
										fs.Seek(6, SeekOrigin.Current);
										continue;
									}
									fs.Seek(18, SeekOrigin.Current);
								}
								else
								{
									//filename = "unnamed";
									fs.Seek(8, SeekOrigin.Current);
								}

								while (true)
								{
									long len = StreamUtils.ReadUInt32(fs);

									if (fs.Position + len > fs.Length)
									{
										addLine(numToAddr(fs.Position) + " invalid fragment length (" + len + ") out of stream");
										analyzeRequest = true;
										break;
									}

									fs.Seek(2, SeekOrigin.Current);
									x = StreamUtils.ReadUInt16(fs);

									if (filename.Length > 0) filename = numToAddr(fs.Position) + " - " + filename;
									else filename = numToAddr(fs.Position);

									addLine("fragment: " + filename);
									FileInfo fi = new FileInfo(filename, fs.Position, len, DateTime.MinValue, true);
									parseCompressedFragment(sectName, fi);
									partFiles.Add(fi);
									fs.Seek(len, SeekOrigin.Current);
									//StreamUtils.Counter += len;

									if (x == 0) break;
									else if (x == 1)
									{
										filename = "cont";
									}
									else
									{
										addLine("Unexpected folder structure at " + numToAddr(fs.Position) + ": " + x.ToString("X"));
										analyzeRequest = true;
										break;
									}
								}
							}
							break;
							#endregion
						default:
							addLine(string.Format("Unknown folder structure (0x{0})", tst.ToString("X")));
							analyzeRequest = true;
							break;
					}

					if (procAsDefault)
					{
						addLine(string.Format("Folder structure (0x{0})", tst.ToString("X")));

						count = StreamUtils.ReadUInt32asInt(fs);
						for (int j = 0; j < count; j++)
						{
							if (tst == 0x1008)
							{
								UInt32 x = StreamUtils.ReadUInt32(fs);
								if (x != 0)
								{
									if ((x & 0x80000000) == 0x80000000)
									{
										// just empty item to skip
										continue;
									}
									else
									{
										addLine("Folder structure reading error");
										analyzeRequest = true;
										break;
									}
								}
							}

							foldername = StreamUtils.ReadString(fs).TrimStart('\\');
							filename = StreamUtils.ReadString(fs);

							if (string.IsNullOrEmpty(foldername) || string.IsNullOrEmpty(filename) || foldername.IndexOfAny(Path.GetInvalidPathChars()) > 0)
							{
								addLine("Folder structure reading error");
								analyzeRequest = true;
								break;
							}

							addLine(foldername + filename);

							partFiles = findOrCreateFileInfoList(sectName + "\\" + foldername);

							fs.Seek(12, SeekOrigin.Current);
							long len = StreamUtils.ReadUInt32(fs);
							partFiles.Add(new FileInfo(filename, fs.Position + 2, len));
							fs.Seek(len + 2, SeekOrigin.Current);
							StreamUtils.Counter += len;
						}
					}
					break;
			}
		}

		private bool parseContacts(FileStream fs, ref bool analyzeRequest)
		{
			int count = StreamUtils.ReadUInt32asInt(fs);
			for (int j = 0; j < count; j++)
			{
				int c3 = fs.ReadByte(); // element count
				string s = "";
				for (int k = 0; k < c3; k++)
				{
					int x = fs.ReadByte(); // field
					switch (x)
					{
						case 0x0B: // number
							x = fs.ReadByte(); // number type
							s += StreamUtils.ReadString(fs) + ";";
							break;
						case 0x1E: // group ???
						case 0x43: // group
							x = StreamUtils.ReadUInt32asInt(fs); // group number
							s += "G" + x + ";";
							break;
						case 0x57: // ???
							fs.Seek(6, SeekOrigin.Current);
							break;
						case 0x33: // image
						case 0x37: // ringtone
							string foldername = StreamUtils.ReadString(fs);
							string filename = StreamUtils.ReadString(fs);
							s += foldername + "\\" + filename + ";";
							fs.Seek(12, SeekOrigin.Current);
							int size = StreamUtils.ReadUInt32asInt(fs);
							fs.Seek(2, SeekOrigin.Current);
							List<FileInfo> partFiles = findOrCreateFileInfoList(NokiaConstants.ptContacts + "\\" + Path.GetFileName(foldername));
							partFiles.Add(new FileInfo(filename, fs.Position, size));
							StreamUtils.Counter += size;
							fs.Seek(size, SeekOrigin.Current);
							break;
						case 0xFE: // image link
							fs.ReadByte(); // ??
							s += StreamUtils.ReadString(fs) + ";"; // filename
							break;
						case 0x07: // name
						case 0x46: // first name
						case 0x47: // surname
						case 0x56: // nick
						case 0x52: // nick
						case 0x08: // email
						case 0x0A: // note
						case 0x54: // position / job
						case 0x55: // company
						case 0x4B: // address
						case 0x4F: // address 2
						case 0x50: // state
						case 0x2C: // url
						case 0x09: // address 3
						case 0x4C: // address 4
						case 0x4D: // address 5
						case 0x4E: // address 6
						case 0x3F: // X-SIP
							s += StreamUtils.ReadString(fs) + ";";
							break;
						default:
							s += StreamUtils.ReadString(fs) + ";";
							analyzeRequest = true;
							break;
					}
				}
				addLine(s);
			}
			return analyzeRequest;
		}

		private void parseGroups(FileStream fs)
		{
			int count = StreamUtils.ReadUInt16(fs);
			for (int j = 0; j < count; j++)
			{
				UInt16 grNum = StreamUtils.ReadUInt16(fs);
				string grName = "";

				int c3 = fs.ReadByte(); // element count
				string s = "";

				for (int k = 0; k < c3; k++)
				{
					int x = fs.ReadByte(); // datetype
					switch (x)
					{
						case 0x07: // name
							grName = StreamUtils.ReadString(fs);
							s = string.Format("G{0} = {1}", grNum, grName);
							break;
						case 0x33: // image
						case 0x37: // ringtone
							string foldername = StreamUtils.ReadString(fs);
							if (foldername.Length == 0) break;
							string filename = StreamUtils.ReadString(fs);
							s += ";" + foldername + "\\" + filename + ";";
							fs.Seek(12, SeekOrigin.Current);
							int size = StreamUtils.ReadUInt32asInt(fs);
							fs.Seek(2, SeekOrigin.Current);

							List<FileInfo> partFiles = findOrCreateFileInfoList(NokiaConstants.ptGroups + "\\" + grName);
							partFiles.Add(new FileInfo(filename, fs.Position, size));
							StreamUtils.Counter += size;

							fs.Seek(size, SeekOrigin.Current);
							break;
						default:
							s += ";" + StreamUtils.ReadString(fs);
							break;
					}
				}
				addLine(s);
			}
		}

		private void parseCalendar(FileStream fs, ref bool analyzeRequest)
		{
			fs.ReadByte(); // ?
			int count = StreamUtils.ReadUInt16(fs);
			addLine(string.Format("{0} calendar items found", count));
			fs.Seek(6, SeekOrigin.Current);
			for (int j = 0; j < count; j++)
			{
				int c3 = fs.ReadByte(); // element count

				string s = "";
				int x = 0;
				bool todo = false;

				for (int k = 0; k < c3; k++)
				{
					x = StreamUtils.ReadUInt16(fs); // field
					switch (x)
					{
						case 0x102: // 1 EVENT, 2 TODO
							switch (StreamUtils.ReadUInt32(fs))
							{
								//case 1: s += "EVENT"; break;
								case 2:
									s += "TODO";
									todo = true;
									break;
							}
							break;
						case 0x103: // 1 appointment, 2 call, 3 anniversary, 4 event, 5 reminder
							if (todo) fs.Seek(4, SeekOrigin.Current);
							else
							{
								switch (StreamUtils.ReadUInt32(fs))
								{
									case 1: s += "APPOINTMENT"; break;
									case 2: s += "CALL"; break;
									case 3: s += "ANNIVERSARY"; break;
									case 4: s += "EVENT"; break;
									case 5: s += "REMINDER"; break;
								}
							}
							break;
						case 0x10B: // anniversary year
							s += ";" + StreamUtils.ReadUInt32(fs);
							break;
						case 0x101: // item number ?
						case 0x104:
						case 0x105:
						case 0x107:
						case 0x108:
						case 0x10C:
							//UInt32 type = StreamUtils.ReadUInt32(fs);
							//s += string.Format("{0}={1};", x.ToString("X"), type);
							fs.Seek(4, SeekOrigin.Current);
							break;
						case 0x201: // summary
						case 0x202: // ??
						case 0x203: // location
						case 0x204: // call number
							s += ";" + StreamUtils.ReadString(fs);
							break;
						case 0x301: // dtstart
							s += ";START " + dttmToString(StreamUtils.ReadNokiaDateTime2(fs));
							fs.Seek(7, SeekOrigin.Current);
							break;
						case 0x302: // dtend
							s += ";END " + dttmToString(StreamUtils.ReadNokiaDateTime2(fs));
							fs.Seek(7, SeekOrigin.Current);
							break;
						case 0x303: // alarm
							s += ";ALARM " + dttmToString(StreamUtils.ReadNokiaDateTime2(fs));
							fs.Seek(7, SeekOrigin.Current);
							break;
						case 0x304:
						case 0x305:
						case 0x306:
							fs.Seek(15, SeekOrigin.Current);
							break;
						default:
							addLine("Unknown calendar field: " + x);
							k = c3; // break inner cycle
							j = count; // break outer cycle
							analyzeRequest = true;
							break;
					}
				}

				if (x == 0x302 || x == 0x304 || x == 0x305 || x == 0x306) // ???
				{
					fs.Seek(4, SeekOrigin.Current);
				}

				addLine(s);
			}
		}

		#region Format functions
		static string numToName(int i)
		{
			return i.ToString("0000");
		}

		static string numToAddr(long i)
		{
			return i.ToString("X").PadLeft(8, '0');
		}

		static string numToProgressAndAddr(long i, long len)
		{
			return string.Format("{0:00}% {1}", 100 * i / len, numToAddr(i));
		}

		static string dttmToString(DateTime dt)
		{
			if (dt.Hour == 0 && dt.Minute == 0)
			{
				return dt.ToString("dd.MM.yyyy");
			}
			else
			{
				return dt.ToString("dd.MM.yyyy HH:mm");
			}
		}

		/*static string buffToString(byte[] buff)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for (int i = 0; i < buff.Length; i++)
			{
				if (i >= 0) sb.Append(" ");
				sb.Append(buff[i].ToString("X").PadLeft(2, '0'));
			}
			return sb.ToString();
		}*/

		private static DateTime getMsgTime(Vcard crd)
		{
			DateTime time = crd.GetDateTime("X-NOK-DT");
			if (time == DateTime.MinValue)
			{
				try
				{
					time = DateTime.Parse(crd["Date"]);
				}
				catch { }
			}
			return time;
		}
		#endregion
	}
}
