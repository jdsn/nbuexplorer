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
using ICSharpCode.SharpZipLib.Zip;
using NbuExplorer.cdbParsing;

namespace NbuExplorer
{
	public partial class FormMain : Form
	{
		private bool analyzeRequest = false;
		private bool dbShellRequest = false;

		public void OpenFiles(bool bruteForceScan, bool clearOnStart, params string[] filenames)
		{
			try
			{
				if (clearOnStart)
				{
					PrepareForNewFileset();
				}

				foreach (string filename in filenames)
				{
					AddFile(filename, bruteForceScan);
				}
			}
			catch (Exception exc)
			{
				MessageBox.Show(string.Format("Following error occured during parse:\r\n{0}\r\nPlease consider providing this backup to the author of application for analyzing and improving application.", exc.Message), this.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				FinalizeFileset();
			}
		}

		public void PrepareForNewFileset()
		{
			textBoxLog.Clear();

			DataSetNbuExplorer.Init();
			treeViewMsgFilter.Nodes[0].Nodes.Clear();
			treeViewMsgFilter.Nodes[1].Nodes.Clear();
			treeViewMsgFilter.Nodes[2].Nodes.Clear();

			treeViewDirs.Nodes.Clear();
			listViewFiles.Items.Clear();
			listViewFiles_SelectedIndexChanged(this, EventArgs.Empty);

			recountTotal();
			statusLabelSelected.Text = "-";

			this.analyzeRequest = false;
			this.dbShellRequest = false;

			this.menuStripMain.Enabled = false;
			this.treeViewDirs.Enabled = false;
			this.Cursor = Cursors.AppStarting;
		}

		public void FinalizeFileset()
		{
			addLine("");
			recursiveRenameDuplicates(treeViewDirs.Nodes);
			DataSetNbuExplorer.FinalizeMultiparts();

			#region Prepare message filtering by numbers
			treeViewMsgFilter.AfterCheck -= new TreeViewEventHandler(treeViewMsgFilter_AfterCheck);
			buildFilterSubNodes(treeViewMsgFilter.Nodes[0], "I");
			buildFilterSubNodes(treeViewMsgFilter.Nodes[1], "O");
			buildFilterSubNodes(treeViewMsgFilter.Nodes[2], "U");
			treeViewMsgFilter_AfterCheck(this, new TreeViewEventArgs(null));
			#endregion

			menuStripMain.Enabled = true;
			treeViewDirs.Enabled = true;

			saveParsingLogToolStripMenuItem.Enabled = (textBoxLog.Text.Trim().Length > 0);
			exportAllToolStripMenuItem.Enabled = (treeViewDirs.Nodes.Count > 0);
			exportToolStripMenuItem.Enabled = exportSelectedFolderToolStripMenuItem.Enabled = (treeViewDirs.SelectedNode != null);

			recountTotal();

			this.Cursor = Cursors.Default;

			if (analyzeRequest)
			{
				MessageBox.Show("Unknown structure type found. Please consider providing this backup to the author of application for analyzing and improving application.", this.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			if (dbShellRequest)
			{
				MessageBox.Show("Symbian contact database detected but dbshell was not found. If you are interested in contacts from this database see readme, section dbshell integration for instructions.", this.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		public void AddFile(string currentFileName, bool bruteForceScan)
		{
			StreamUtils.Counter = 0;

			using (FileStream fs = File.OpenRead(currentFileName))
			{
				this.Text = Path.GetFileName(currentFileName) + " - " + appTitle;
				addLine(Path.GetFileName(currentFileName));
				addLine("");

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
						FileInfo fiPhonebook = null;
						FileInfo fiMessages = null;

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
												l2.Add(new FileInfo(currentFileName, Path.GetFileName(fileNames[index]), list[j].Start, list[j].FileSize));
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
										var fi = new FileInfo(currentFileName, Path.GetFileName(name), fs.Position, len);
										list.Add(fi);
										fs.Seek(len + 4, SeekOrigin.Current);

										if (name.EndsWith("\\PHONEBOOK.csv", StringComparison.InvariantCultureIgnoreCase))
										{
											fiPhonebook = fi;
										}
										else if (name.EndsWith("\\MESSAGES.csv", StringComparison.InvariantCultureIgnoreCase))
										{
											fiMessages = fi;
										}
									}
									StreamUtils.Counter += len;
									break;
								default:
									throw new ApplicationException(string.Format("Unknown item type {0}", type));
							}
						}

						if (fiPhonebook != null)
							parseNfbNfcPhonebook(fiPhonebook);

						if (fiMessages != null)
							parseNfbNfcMessages(fiMessages);
					}
					catch (Exception exc)
					{
						analyzeRequest = true;
						addLine(exc.Message);
					}

					if (fs.Length - fs.Position > 4) addLine("End of file not reached");
				}
				#endregion
				#region nbu format
				else if (!bruteForceScan && fileext == ".nbu")
				{
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

					List<FileInfo> contactList = new List<FileInfo>();

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
								AddFile(currentFileName, true);
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
									partFiles.Add(new FileInfo(currentFileName, filename, fileStartAddr, fileSize, fileTime));
									fileStartAddr += fileSize;
									StreamUtils.Counter += fileSize;
								}
								fs.Seek(partPos, SeekOrigin.Begin);
								break;
							#endregion
							#region vcards
							case ProcessType.Vcards:
								count = StreamUtils.ReadUInt32asInt(fs); // files
								count2 = StreamUtils.ReadUInt32asInt(fs); // folders

								if (count2 > 0)
								{
									addLine(count2.ToString() + " folders found");

									for (int i = 0; i < count2; i++)
									{
										fs.Seek(4, SeekOrigin.Current); //folder id
										long folderAddr = StreamUtils.ReadUInt64asLong(fs);

										partPos = fs.Position;
										try
										{
											fs.Seek(folderAddr + 4, SeekOrigin.Begin);
											foldername = StreamUtils.ReadString(fs);
											addLine(string.Format("\r\nFolder BEGIN {0}, name: '{1}'", numToAddr(folderAddr), foldername));
											parseFolderVcard(currentFileName, fs, contactList, sect.name, sect.name + "\\" + foldername);
										}
										catch (Exception exc)
										{
											addLine(exc.Message);
											analyzeRequest = true;
										}
										fs.Seek(partPos, SeekOrigin.Begin);
									}
								}
								else
								{
									partPos = fs.Position;
									try
									{
										fs.Seek(partStartAddr + 0x2C, SeekOrigin.Begin);
										parseFolderVcard(currentFileName, fs, contactList, sect.name, sect.name);
									}
									catch (Exception exc)
									{
										addLine(exc.Message);
										analyzeRequest = true;
									}
									fs.Seek(partPos, SeekOrigin.Begin);
								}
								break;
							#endregion
							#region memos
							case ProcessType.Memos:
								count = StreamUtils.ReadUInt32asInt(fs);
								addLine(count.ToString() + " memos found");
								partPos = fs.Position + 4;
								fs.Seek(partStartAddr + 0x30, SeekOrigin.Begin);
								partFiles = findOrCreateFileInfoList(sect.name);
								for (int i = 0; i < count; i++)
								{
									fs.Seek(4, SeekOrigin.Current);
									int len = StreamUtils.ReadUInt16(fs) * 2;
									partFiles.Add(new FileInfo(currentFileName, string.Format("{0}.txt", numToName(i + 1)), fs.Position, len));
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

								partPos = fs.Position;
								string zipTest = null;

								long tmpCnt = StreamUtils.Counter;
								try
								{
									fs.Seek(partStartAddr + 73, SeekOrigin.Begin);
									zipTest = StreamUtils.ReadString(fs);
								}
								catch { }
								StreamUtils.Counter = tmpCnt;

								if (zipTest == "application/vnd.nokia-backup")
								{
									long zipOffset = fs.Position;
									ZipInputStream zi = new ZipInputStream(fs);
									zi.IsStreamOwner = false;
									parseFolderZip(currentFileName, zi, zipOffset, sect.name);
									fs.Seek(partPos, SeekOrigin.Begin);
								}
								else if (sect.name2 == "Messages" && count == 0)
								{
									fs.Seek(partStartAddr, SeekOrigin.Begin);
									parseBinaryMessages(currentFileName, fs);
									fs.Seek(partPos, SeekOrigin.Begin);
								}
								else
								{
									fs.Seek(partPos, SeekOrigin.Begin);
									addLine(string.Format("{0} folders found", count));
									for (int i = 0; i < count; i++)
									{
										fs.Seek(4, SeekOrigin.Current);

										long start = StreamUtils.ReadUInt64asLong(fs);

										addLine("");
										addLine("Folder BEGIN " + numToAddr(start));

										partPos = fs.Position;

										try
										{
											parseFolder(currentFileName, fs, start, sect.name);
										}
										catch (Exception ex)
										{
											addLine(ex.Message);
											analyzeRequest = true;
										}

										fs.Seek(partPos, SeekOrigin.Begin);
									}
								}
								break;
							#endregion
							case ProcessType.Sbackup:
								goto case ProcessType.GeneralFolders;
						}

						partcount--;
					}
				}
				#endregion
				#region arc
				else if (!bruteForceScan && fileext == ".arc")
				{
					UInt32 test0 = StreamUtils.ReadUInt32(fs);
					if (test0 == 0x101F4667)
					{
						List<FileInfo> compFr = findOrCreateFileInfoList("compressed fragments");

						fs.Seek(0x1C, SeekOrigin.Begin);
						UInt32 test1 = StreamUtils.ReadUInt32(fs);
						fs.Seek(4, SeekOrigin.Current);
						UInt32 test2 = StreamUtils.ReadUInt32(fs);
						bool startsWithFiles =
							(test1 == 0 && test2 == 0) ||                   // Backup.arc
							(test1 == 0x1ea367a4 && test2 == 0xb00d58ae) || // UserFiles.arc
							(test1 == 0x53fb0d19 && test2 == 0xef7ac531);   // Settings.arc

						fs.Seek(0x3C, SeekOrigin.Begin);
						addLine("Phone model: " + StreamUtils.ReadShortString(fs));
						addLine("");

						long startAddr = fs.Position;
						long lenComp;
						long lenUncomp;

						byte[] seq = new byte[] { 0, 0, 0, 1, 0, 0, 0, 0 };
						byte[] buff = new byte[8];

						if (startsWithFiles && StreamUtils.SeekTo(seq, fs))
						{
							string filename = "";
							string dir = "";
							long recoveryAddr;

							do
							{
								recoveryAddr = fs.Position;

								fs.Seek(12, SeekOrigin.Current);
								filename = StreamUtils.ReadStringTo(fs, 0, 0x80);
								fs.Seek(11, SeekOrigin.Current);
								lenUncomp = StreamUtils.ReadUInt32(fs);
								fs.Seek(12, SeekOrigin.Current);
								lenComp = StreamUtils.ReadUInt64asLong(fs);

								addLine(filename + " - size: " + lenComp + " / " + lenUncomp);
								try
								{
									dir = Path.GetDirectoryName(filename);
									filename = Path.GetFileName(filename);
								}
								catch (Exception exc)
								{
									addLine(exc.Message);
									analyzeRequest = true;

									fs.Seek(recoveryAddr, SeekOrigin.Begin);
									if (StreamUtils.SeekTo(NokiaConstants.compHead, fs))
									{
										fs.Seek(-22, SeekOrigin.Current);
									}
									break;
								}

								List<FileInfo> list = findOrCreateFileInfoList(dir);
								if (filename.Length > 0 && lenComp > 8)
								{
									list.Add(new FileinfoCf(currentFileName, filename, startAddr, lenComp, lenUncomp, DateTime.MinValue));
								}

								StreamUtils.Counter += lenComp;
								startAddr += lenComp;

								fs.Seek(1, SeekOrigin.Current);
								fs.Read(buff, 0, buff.Length);
							}
							while (NokiaConstants.CompareByteArr(seq, buff));

							addLine(""); // end of first section
						}

						seq[3] = 0;

						long lastRecoveryPosition = 0;

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

									FileinfoCf fi = new FileinfoCf(currentFileName, numToAddr(fs.Position), fs.Position, lenComp, lenUncomp, DateTime.MinValue);
									compFr.Add(fi);

									addLine(fi.Filename + " - compressed fragment");
									parseCompressedFragment("", fi, lenUncomp);

									fs.Seek(lenComp, SeekOrigin.Current);
								}
								while (lenUncomp == 65536);
							}
							else
							{
								if (StreamUtils.SeekTo(NokiaConstants.compHead, fs))
								{
									if (lastRecoveryPosition != fs.Position)
									{
										lastRecoveryPosition = fs.Position;
										fs.Seek(-22, SeekOrigin.Current);
									}
								}
								else break;
							}
						}
					}
					else
					{
						byte[] pathStartSequence = new byte[] { 0x00, 0x5c, 0x00, 0x53, 0x00 };

						if (StreamUtils.SeekTo(pathStartSequence, fs))
						{
							fs.Seek(-9, SeekOrigin.Current);
							while (true)
							{
								try
								{
									int len = fs.ReadByte() / 2;

									string filename = StreamUtils.ReadString(fs, len);
									string dir = Path.GetDirectoryName(filename);

									fs.Seek(10, SeekOrigin.Current); // ?? datetime?
									UInt32 lenUncomp = StreamUtils.ReadUInt32(fs);
									UInt32 lenComp = StreamUtils.ReadUInt32(fs);
									StreamUtils.Counter += lenComp;

									addLine(filename + " - size: " + lenComp + " / " + lenUncomp);
									filename = Path.GetFileName(filename);

									FileinfoCf fic = new FileinfoCf(currentFileName, filename, fs.Position, lenComp, lenUncomp, DateTime.MinValue);

									addSymbianFile("", dir, fic);

									fs.Seek(lenComp, SeekOrigin.Current);
								}
								catch
								{
									if (StreamUtils.SeekTo(pathStartSequence, fs))
									{
										fs.Seek(-9, SeekOrigin.Current);
									}
									else
									{
										break;
									}
								}
							}
						}

					}


					foreach (TreeNode tn in treeViewDirs.Nodes)
					{
						tn.Expand();
					}
				}
				#endregion
				#region nbf & zip
				else if (!bruteForceScan && (fileext == ".nbf" || fileext == ".zip"))
				{
					ZipInputStream zi = new ZipInputStream(fs);
					parseFolderZip(currentFileName, zi, 0, "");
				}
				#endregion
				#region mms
				else if (!bruteForceScan && (fileext == ".mms"))
				{
					Mms m = new Mms(currentFileName, fs, fs.Length);
					addLine(m.ParseLog);
					var list = findOrCreateFileInfoList(m.Subject);
					list.AddRange(m.Files);
					StreamUtils.Counter = fs.Position;
				}
				#endregion
				#region cdb
				else if (!bruteForceScan && (fileext == ".cdb"))
				{
					addLine("Parsing contacts from " + currentFileName + ":\r\n");

					if (DbShell.Ready)
					{
						try
						{
							DbShell.PrepareWorkDir();

							var tables = DbShell.DumpTables(currentFileName, "identitytable", "phone", "emailtable", "contacts");

							var phonebook = CdbContactParser.ParseTableDumps(tables);

							string dir = Path.GetFileNameWithoutExtension(currentFileName);

							if (phonebook.Count > 0)
							{
								var node = findOrCreateFileInfoList(dir);
								foreach (KeyValuePair<string, Contact> pair in phonebook)
								{
									FileInfoMemory vcf = new FileInfoMemory(currentFileName, pair.Value.DisplayName + ".vcf",
										System.Text.Encoding.UTF8.GetBytes(pair.Value.ToVcard()),
										pair.Value.RevDate);
									node.Add(vcf);
								}

							}
							addLine(string.Format("{0} contacts found\r\n", phonebook.Count));

							StreamUtils.Counter = fs.Length;
						}
						catch (Exception exc)
						{
							addLine(string.Format("Error parsing contacts from cdb file: {0}", exc.Message));
						}
					}
					else
					{
						dbShellRequest = true;
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
								filename = DataSetNbuExplorer.FindPhoneBookEntry(filename).name;
							}
							catch { }
							files.Add(new FileInfo(currentFileName, filename + ".vmg", Pattern.Msg.StartIndex, Pattern.Msg.Length, msg.MessageTime));
							StreamUtils.Counter += Pattern.Msg.Length;
							addLine(numToProgressAndAddr(Pattern.Msg.StartIndex, fs.Length) + "\tmessage: " + filename);

							DataSetNbuExplorer.AddMessageFromVmg(msg);
						}

						if (Pattern.Msg.Active) continue;

						if (Pattern.Contact.Step((byte)current, fs.Position))
						{
							Vcard contact = new Vcard(Pattern.Contact.GetCaptureAsString(fs));
							string name = contact.Name;
							DateTime time = contact.GetDateTime("REV");
							foreach (string number in contact.PhoneNumbers)
							{
								DataSetNbuExplorer.AddPhonebookEntry(number, name);
							}
							files = findOrCreateFileInfoList(NokiaConstants.ptContacts);
							filename = name;
							if (filename.Length == 0) filename = numToName(files.Count + 1);
							files.Add(new FileInfo(currentFileName, filename + ".vcf", Pattern.Contact.StartIndex, Pattern.Contact.Length, time));
							if (contact.Photo != null)
							{
								files.Add(new FileInfoMemory(currentFileName, filename + "." + contact.PhotoExtension, contact.Photo, time));
							}
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
							files.Add(new FileInfo(currentFileName, filename + ".vcs", Pattern.Calendar.StartIndex, Pattern.Calendar.Length, calendar.GetDateTime("DTSTART")));
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
							files.Add(new FileInfo(currentFileName, filename + ".url", Pattern.Bookmark.StartIndex, Pattern.Bookmark.Length));
							StreamUtils.Counter += Pattern.Bookmark.Length;
							addLine(numToProgressAndAddr(Pattern.Bookmark.StartIndex, fs.Length) + "\tbookmark: " + filename);
						}
					}
				}
				#endregion

				addLine("");
				addLine(string.Format("Done, file coverage: {0:0.##}%", 100 * (float)StreamUtils.Counter / fs.Length));
				addLine("");
			}
		}

		private void parseFolderZip(string currentFileName, ZipInputStream zi, long start, string sectName)
		{
			ZipEntry ze;
			int index = 0;
			while ((ze = zi.GetNextEntry()) != null)
			{
				addLine(ze.Name);
				StreamUtils.Counter += ze.CompressedSize;
				if (ze.IsDirectory)
				{
					findOrCreateDirNode(ze.Name);
				}
				else
				{
					if (ze.Size == 0 && dirExists(ze.Name))
					{
						addLine("^ redundant directory ignored");
					}
					else
					{
						string dir = Path.GetDirectoryName(sectName + "\\" + ze.Name);
						FileInfoZip item = new FileInfoZip(currentFileName, ze, index, start);
						findOrCreateFileInfoList(dir).Add(item);

						if (parseMsgPredefToolStripMenuItem.Checked && Message.MsgFileNameRegex.IsMatch(item.Filename))
						{
							long CntBackup = StreamUtils.Counter;
							try
							{
								using (MemoryStream ms = new MemoryStream())
								{
									StreamUtils.CopyFromStreamToStream(zi, ms, ze.Size);
									Message msg = Message.ReadPredefBinMessage(currentFileName, ms, item.Filename);
									if (msg.Mms == null)
									{
										addLine(msg.ToString());
										DataSetNbuExplorer.AddMessage(msg);
									}
									else
									{
										addLine(msg.Mms.ParseLog);
									}
								}
							}
							catch (Exception ex)
							{
								addLine(ex.Message);
							}
							StreamUtils.Counter = CntBackup;
						}
						else if (item.Filename.ToLower().EndsWith(".vcf"))
						{
							try
							{
								byte[] buff = new byte[item.FileSize];
								zi.Read(buff, 0, buff.Length);
								Vcard crd = new Vcard(System.Text.Encoding.UTF8.GetString(buff));
								string name = crd.Name;
								foreach (string number in crd.PhoneNumbers)
								{
									DataSetNbuExplorer.AddPhonebookEntry(number, name);
								}

								if (crd.Photo != null)
								{
									string filename = Path.ChangeExtension(item.Filename, "." + crd.PhotoExtension);
									findOrCreateFileInfoList(dir).Add(new FileInfoMemory(currentFileName, filename, crd.Photo, item.FileTime));
								}
							}
							catch (Exception ex)
							{
								addLine(ex.Message);
							}
						}
					}
				}
				index++;
			}
		}

		private void addSymbianFile(string root, string dir, FileInfo fi)
		{
			var list = findOrCreateFileInfoList(root + "\\" + dir);
			list.Add(fi);

			if (fi.Filename.ToLower().EndsWith("contacts.cdb"))
			{
				parseSymbianContactDatabase(fi);
			}
			else if (fi.Filename.ToLower().EndsWith("notepad.dat"))
			{
				parseSymbianNotes(fi);
			}
			else if (fi.Filename.ToLower() == "logdbu.dat")
			{
				parseSymbianLog(fi);
			}
			else if (parseMsgSymbianToolStripMenuItem.Checked && ExpectMessage(dir))
			{
				parseSymbianMessage(fi);
			}
		}

		private void parseSymbianLog(FileInfo fi)
		{
			if (!DbShell.Ready)
			{
				dbShellRequest = true;
				return;
			}

			try
			{
				addLine("\r\nParsing log from " + fi.Filename + ":\r\n");

				var tempFile = PrepareDbshellTempFile(fi);

				var tables = DbShell.DumpTables(tempFile, "event");

				var sb = new System.Text.StringBuilder();

				foreach (var row in tables["event"].Select())
				{
					if (!"-1".Equals(row[1]))
						continue;

					DateTime rev = DbShell.ParseDateTime((string)row[0]);

					sb.Append(rev);
					if ("2".Equals(row[8]))
						sb.Append(" to ");
					else
						sb.Append(" from ");

					sb.Append(row[12]);
					sb.Append(": ");
					sb.Append(row[11]);
					sb.AppendLine();
				}

				if (sb.Length > 0)
				{
					var node = findOrCreateFileInfoList("Messages\\" + fi.Filename);
					FileInfoMemory fi2 = new FileInfoMemory(fi.SourcePath, "log.txt",
						System.Text.Encoding.UTF8.GetBytes(sb.ToString()),
						DateTime.MinValue);
					node.Add(fi2);
				}
			}
			catch (Exception exc)
			{
				addLine(string.Format("Error parsing log: {0}", exc.Message));
			}
		}

		private void parseSymbianNotes(FileInfo fi)
		{
			if (!DbShell.Ready)
			{
				dbShellRequest = true;
				return;
			}

			try
			{
				addLine("\r\nParsing notes from " + fi.Filename + ":\r\n");

				var tempFile = PrepareDbshellTempFile(fi);

				var tables = DbShell.DumpTables(tempFile, "table1");

				int cnt = 1;
				foreach (var row in tables["table1"].Select())
				{
					try
					{
						DateTime rev = DbShell.ParseDateTime((string)row[0]);
						var node = findOrCreateFileInfoList("Memo\\" + fi.Filename);
						FileInfoMemory fi2 = new FileInfoMemory(fi.SourcePath, string.Format("{0:0000}.txt", cnt++),
							System.Text.Encoding.UTF8.GetBytes((string)row[1]),
							rev);
						node.Add(fi2);
					}
					catch (Exception ex)
					{
						addLine(ex.Message);
					}
				}
			}
			catch (Exception exc)
			{
				addLine(string.Format("Error parsing notes: {0}", exc.Message));
			}
		}

		private void parseSymbianContactDatabase(FileInfo fi)
		{
			if (!DbShell.Ready)
			{
				dbShellRequest = true;
				return;
			}

			try
			{
				addLine("\r\nParsing contacts from " + fi.Filename + ":\r\n");

				var tempFile = PrepareDbshellTempFile(fi);

				var tables = DbShell.DumpTables(tempFile, "identitytable", "phone", "emailtable", "contacts");

				var phonebook = CdbContactParser.ParseTableDumps(tables);

				if (phonebook.Count > 0)
				{
					var node = findOrCreateFileInfoList("Contacts\\" + fi.Filename);
					foreach (KeyValuePair<string, Contact> pair in phonebook)
					{
						FileInfoMemory vcf = new FileInfoMemory(fi.SourcePath, pair.Value.DisplayName + ".vcf",
							System.Text.Encoding.UTF8.GetBytes(pair.Value.ToVcard()),
							pair.Value.RevDate);
						node.Add(vcf);
					}
					addLine(string.Format("{0} contacts found\r\n", phonebook.Count));
				}
			}
			catch (Exception exc)
			{
				addLine(string.Format("Error parsing contacts from cdb file: {0}", exc.Message));
			}
		}

		private string PrepareDbshellTempFile(FileInfo fi)
		{
			DbShell.PrepareWorkDir();
			var tempFile = Path.Combine(DbShell.workDir, DbShell.workFile);
			var fs = File.Create(tempFile);
			fi.CopyToStream(fs);
			fs.Close();
			return tempFile;
		}

		private static bool ExpectMessage(string dir)
		{
			if (dir.Contains("Mail"))
			{
				string[] tmp = dir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
				return (tmp.Length > 4
					&& tmp[tmp.Length - 3].StartsWith("Mail")
					);
			}
			return false;
		}

		private FileInfoCfMultiPart currentIncompleteMultipartFile = null;
		private long currentHeaderLengthToSkip = 0;
		private MemoryStream previousMs = null;

		private void parseCompressedFragment(string rootFolder, FileinfoCf fi, long lenUncomp)
		{
			long initCounter = StreamUtils.Counter;

			MemoryStream ms = new MemoryStream();
			try
			{
				fi.CopyToStream(ms);
				if (ms.Length != lenUncomp)
				{
					addLine(string.Format("WARNING: uncompressed size ({0}) differs from expected ({1})", ms.Length, lenUncomp));
				}
			}
			catch (Exception exc)
			{
				previousMs = null; // reset recovery mode
				addLine("ERROR decompressing fragment: " + exc.Message);
				if (currentIncompleteMultipartFile != null)
				{
					if (currentIncompleteMultipartFile.MissingLength >= lenUncomp)
					{
						addLine(string.Format("Incomplete multipart file '{0}' shortened", currentIncompleteMultipartFile.Filename));
						currentIncompleteMultipartFile.Shorten(lenUncomp);
						return;
					}
					else
					{
						addLine(string.Format("Incomplete multipart file '{0}' corrupted and lost", currentIncompleteMultipartFile.Filename));
						currentIncompleteMultipartFile = null;
						return;
					}
				}
			}

			ms.Seek(0, SeekOrigin.Begin);

			if (previousMs != null)
			{
				// continue recovery mode started by previous fragment
				parseCompressedFragmentFiles(rootFolder, fi, ms);
			}
			else if (currentHeaderLengthToSkip > 0)
			{
				// continnue skipping useless header
				if (currentHeaderLengthToSkip > ms.Length)
				{
					currentHeaderLengthToSkip -= ms.Length;
					addLine("still incomplete header, continue on next fragment...");
				}
				else
				{
					ms.Seek(currentHeaderLengthToSkip, SeekOrigin.Begin);
					currentHeaderLengthToSkip = 0;
					addLine("end of header reached, looking for files in rest of fragment...");
					parseCompressedFragmentFiles(rootFolder, fi, ms);
				}
			}
			else if (currentIncompleteMultipartFile != null)
			{
				// continue collecting parts of multifragment file
				long missingLength = currentIncompleteMultipartFile.MissingLength;
				if (missingLength > ms.Length)
				{
					currentIncompleteMultipartFile.Parts.Add(new FileInfoCfPart(fi.SourcePath, "", fi.Start, fi.RawSize, fi.FileTime, 0, ms.Length));
					StreamUtils.Counter += ms.Length;
					addLine("still incomplete file, continue on next fragment...");
				}
				else
				{
					if (currentIncompleteMultipartFile.Shortened)
					{
						addLine(string.Format("multipart file '{0}' complete, but corrupted (shortened) - skipping", currentIncompleteMultipartFile.Filename));
					}
					else
					{
						addLine(string.Format("multipart file '{0}' complete.", currentIncompleteMultipartFile.Filename));
						currentIncompleteMultipartFile.Parts.Add(new FileInfoCfPart(fi.SourcePath, "", fi.Start, fi.RawSize, fi.FileTime, 0, missingLength));
						addSymbianFile(currentIncompleteMultipartFile.Root, currentIncompleteMultipartFile.Dir, currentIncompleteMultipartFile);
					}
					currentIncompleteMultipartFile = null;

					StreamUtils.Counter += missingLength;
					ms.Seek(missingLength, SeekOrigin.Begin);

					if (ms.Length > missingLength)
					{
						addLine("looking for next files in rest of fragment...");
						parseCompressedFragmentFiles(rootFolder, fi, ms);
					}
				}
			}
			else
			{
				if (ms.Length <= 16)
				{
					addLine("empty fragment\r\n");
					return;
				}

				// detection of fragment type
				ms.Seek(0, SeekOrigin.Begin);
				UInt32 test1 = StreamUtils.ReadUInt32(ms);
				UInt32 test2 = StreamUtils.ReadUInt32(ms);
				UInt32 test3 = StreamUtils.ReadUInt32(ms);
				UInt32 test4 = StreamUtils.ReadUInt32(ms);

				if (test3 == 0x20 || test3 == 0 || (test3 == 1 && test4 == 8))
				{
					ms.Seek(0, SeekOrigin.Begin);
					parseCompressedFragmentFiles(rootFolder, fi, ms);
				}
				else if (test3 == 1 || test3 == 2)
				{
					ms.Seek(0, SeekOrigin.Begin);
					UInt32 headerLength = StreamUtils.ReadUInt32(ms) + 4;
					if (headerLength > ms.Length)
					{
						currentHeaderLengthToSkip = headerLength - ms.Length;
						addLine("header fragment start, continue on next fragment...");
					}
					else
					{
						ms.Seek(headerLength, SeekOrigin.Begin);
						parseCompressedFragmentFiles(rootFolder, fi, ms);
					}
				}
				else if (test1 == 1 && test2 == 1 && test4 == 0x10202be9)
				{
					addLine("phone settings fragment type\r\n");
					return;
				}
				else if (test1 == 0 && (test2 & 0xFFF00F) == 4) // test2 == 0x0224 || 0x24 || 0x0464 || 0x0444 || 0x0434
				{
					addLine("ignored fragment type\r\n");
					return;
				}
				else
				{
					addLine(string.Format("{0} - unknown fragment type\r\n", fi.Filename));
				}
			}

			long uncompressedCoverage = StreamUtils.Counter - initCounter;

			if (uncompressedCoverage > 0)
			{
				double ratio = (double)uncompressedCoverage / ms.Length;
				StreamUtils.Counter = initCounter + (long)(fi.RawSize * ratio);
				addLine(string.Format("fragment coverage {0:0.##}%", 100 * ratio));
			}

			addLine("");
		}

		private void parseCompressedFragmentFiles(string rootFolder, FileInfo fi, MemoryStream ms)
		{
			try
			{
				int fnameLen;
				long fsize;
				string fname;

				while (ms.Position < ms.Length)
				{
					if (previousMs != null) // recovery mode
					{
						byte[] buff = new byte[24];
						int a = previousMs.Read(buff, 0, buff.Length);
						if (a < buff.Length) ms.Read(buff, a, buff.Length - a);

						fnameLen = BitConverter.ToInt32(buff, 0);

						if (fnameLen > 2048) // out of memory prevention
						{
							throw new ApplicationException(string.Format("Invalid filename length: {0}", fnameLen));
						}

						fsize = BitConverter.ToUInt32(buff, 4);

						buff = new byte[fnameLen * 2];
						a = previousMs.Read(buff, 0, buff.Length);
						if (a < buff.Length) ms.Read(buff, a, buff.Length - a);

						fname = System.Text.Encoding.Unicode.GetString(buff);
						previousMs.Dispose();
						previousMs = null; // leave recovery mode
						addLine("leaving recovery mode with filename: " + fname);
					}
					else
					{

						if ((ms.Position + 24) > ms.Length) // 24 = sizeof(fnameLen + fsize) + 16
						{
							previousMs = ms;
							addLine("file header behind end of fragment - entering recovery mode");
							return;
						}

						fnameLen = StreamUtils.ReadUInt32asInt(ms);

						if (fnameLen > 2048) // out of memory prevention
						{
							throw new ApplicationException(string.Format("Invalid filename length: {0}", fnameLen));
						}

						fsize = StreamUtils.ReadUInt32(ms);
						ms.Seek(16, SeekOrigin.Current); // 8 bytes?? + 8bytes date and time??

						if ((ms.Position + 2 * fnameLen) > ms.Length)
						{
							ms.Seek(-24, SeekOrigin.Current);
							previousMs = ms; // enter recovery mode
							addLine("file name behind end of fragment - entering recovery mode");
							return;
						}

						fname = StreamUtils.ReadString(ms, fnameLen);
					}

					addLine(fname);

					string dir = Path.GetDirectoryName(fname);

					fname = Path.GetFileName(fname).Trim();
					if (fname.Length > 0)
					{
						if (ms.Length >= ms.Position + fsize)
						{
							FileInfoCfPart ficp = new FileInfoCfPart(fi.SourcePath, fname, fi.Start, fi.RawSize, DateTime.MinValue, ms.Position, fsize);
							addSymbianFile(rootFolder, dir, ficp);
							StreamUtils.Counter += fsize;
						}
						else
						{
							currentIncompleteMultipartFile = new FileInfoCfMultiPart(fi.SourcePath, fname, DateTime.MinValue, fsize, rootFolder, dir);
							currentIncompleteMultipartFile.Parts.Add(new FileInfoCfPart(fi.SourcePath, "", fi.Start, fi.RawSize, DateTime.MinValue, ms.Position, ms.Length - ms.Position));
							StreamUtils.Counter += (ms.Length - ms.Position);
							addLine("incomplete file, continue on next fragment...");
						}
					}

					ms.Seek(fsize, SeekOrigin.Current);
				}
			}
			catch (Exception exc)
			{
				previousMs = null; // reset recovery
				addLine(string.Format("Parsing ERROR at position {0}: {1}", ms.Position.ToString("X"), exc.Message));
			}
		}

		private void parseNfbNfcPhonebook(FileInfo fi)
		{
			try
			{
				List<Contact> contacts =new List<Contact>();

				using (MemoryStream ms = fi.GetAsMemoryStream())
				{
					var sr = new StreamReader(ms, System.Text.Encoding.Unicode);
					while (!sr.EndOfStream)
					{
						Contact c = Contact.ReadNfbNfcContact(sr.ReadLine());
						if (c != null)
						{
							foreach (string number in c.Phones)
							{
								DataSetNbuExplorer.AddPhonebookEntry(number, c.DisplayName);
							}
							contacts.Add(c);
						}
					}
				}

				if (contacts.Count > 0)
				{
					var node = findOrCreateFileInfoList("Contacts");
					foreach (var c in contacts)
					{
						FileInfoMemory vcf = new FileInfoMemory(fi.SourcePath, c.DisplayName + ".vcf",
							System.Text.Encoding.UTF8.GetBytes(c.ToVcard()),
							c.RevDate);
						node.Add(vcf);
					}
				}
			}
			catch (Exception ex)
			{
				addLine(ex.Message);
			}
		}

		private void parseNfbNfcMessages(FileInfo fi)
		{
			try
			{
				using (MemoryStream ms = fi.GetAsMemoryStream())
				{
					var sr = new StreamReader(ms, System.Text.Encoding.Unicode);
					while (!sr.EndOfStream)
					{
						Message m = Message.ReadNfbNfcMessage(sr.ReadLine());
						if (m != null)
						{
							DataSetNbuExplorer.AddMessage(m);
						}
					}
				}
			}
			catch (Exception ex)
			{
				addLine(ex.Message);
			}
		}

		private void parseSymbianMessage(FileInfo fi)
		{
			long cnt = StreamUtils.Counter;
			try
			{
				using (MemoryStream ms = fi.GetAsMemoryStream())
				{
					Message sm = Message.ReadSymbianMessage(ms);
					fi.FileTime = sm.MessageTime;
					DataSetNbuExplorer.AddMessage(sm);
				}
			}
			catch (Exception ex)
			{
				addLine(ex.Message);
			}
			finally
			{
				StreamUtils.Counter = cnt;
			}
		}

		private void parseFolder(string currentFileName, FileStream fs, long start, string sectName)
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
							if (test == -1)
							{
								throw new EndOfStreamException();
							}

							for (int k = 0; k < test; k++)
							{
								fs.Seek(8, SeekOrigin.Current);
								StreamUtils.ReadString(fs); // cislo adresata ?
							}
							fs.Seek(20, SeekOrigin.Current); // 4 + 16

							len = StreamUtils.ReadUInt32(fs);

							long fileStart = fs.Position;
							filename = numToName(j + 1);
							DateTime fileTime;

							addLine("");

							try
							{
								Mms m = new Mms(currentFileName, fs, fileStart + len - 1);
								fileTime = m.Time;

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
								fileTime = DateTime.MinValue;
							}

							partFiles.Add(new FileInfo(currentFileName, string.Format("{0}.mms", filename), fileStart, len, fileTime));

							fs.Seek(fileStart + len, SeekOrigin.Begin);
						}
						else
						{
							len = StreamUtils.ReadUInt32(fs);
							long mstart = fs.Position;

							byte[] buff = new byte[len];
							fs.Read(buff, 0, buff.Length);

							Vcard crd = new Vcard(System.Text.Encoding.Unicode.GetString(buff));
							DateTime time = crd.MessageTime;

							string name = numToName(j + 1);
							if (crd.PhoneNumbers.Count > 0)
							{
								string num = crd.PhoneNumbers[0];
								var phe = DataSetNbuExplorer.FindPhoneBookEntry(num);
								name = name + " " + ((phe != null) ? phe.name : num);
							}

							partFiles.Add(new FileInfo(currentFileName, string.Format("{0}.vmg", name), mstart, len, time));

							if (parseMsgVMGToolStripMenuItem.Checked)
							{
								DataSetNbuExplorer.AddMessageFromVmg(crd);
							}
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
							parseContacts(currentFileName, fs);
							break;
						case 0x0302: // groups
							parseGroups(currentFileName, fs);
							break;
						case 0x0312: // calendar - events
						case 0x0313: // calendar - todos
							parseCalendar(fs);
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
						case 0x0305: // predefvisualradio
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

							partFiles = findOrCreateFileInfoList("compressed fragments\\" + tst.ToString("X"));

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
									long lenComp = StreamUtils.ReadUInt32(fs);
									long lenUncomp = StreamUtils.ReadUInt32(fs);

									if (fs.Position + lenComp > fs.Length)
									{
										addLine(numToAddr(fs.Position) + " invalid fragment length (" + lenComp + ") out of stream");
										analyzeRequest = true;
										break;
									}

									if (filename.Length > 0) filename = numToAddr(fs.Position) + " - " + filename;
									else filename = numToAddr(fs.Position);

									addLine("fragment: " + filename);

									FileinfoCf fi = new FileinfoCf(currentFileName, filename, fs.Position, lenComp, lenUncomp, DateTime.MinValue);
									parseCompressedFragment(sectName, fi, lenUncomp);
									partFiles.Add(fi);
									fs.Seek(lenComp, SeekOrigin.Current);

									if (lenUncomp < 65536) break;
									else if (lenUncomp == 65536)
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

							fs.Seek(12, SeekOrigin.Current);
							long len = StreamUtils.ReadUInt32(fs);
							fs.Seek(2, SeekOrigin.Current);

							FileInfo fi = new FileInfo(currentFileName, filename, fs.Position, len);
							addSymbianFile(sectName, foldername, fi);

							if (parseMsgPredefToolStripMenuItem.Checked && Message.MsgFileNameRegex.IsMatch(filename))
							{
								try
								{
									using (MemoryStream ms = fi.GetAsMemoryStream())
									{
										Message msg = Message.ReadPredefBinMessage(currentFileName, ms, filename);
										if (msg.Mms == null)
										{
											addLine(msg.ToString());
											DataSetNbuExplorer.AddMessage(msg);
										}
										else
										{
											addLine(msg.Mms.ParseLog);
										}
										fi.FileTime = msg.MessageTime;
									}
								}
								catch (Exception ex)
								{
									addLine(ex.Message);
								}
							}

							fs.Seek(len, SeekOrigin.Current);
							StreamUtils.Counter += len;
						}
					}
					break;
			}
		}

		private void parseFolderVcard(string currentFileName, FileStream fs, List<FileInfo> contactList, string sectName, string rootFolderPath)
		{
			int count = StreamUtils.ReadUInt32asInt(fs);

			List<FileInfo> partFiles = findOrCreateFileInfoList(rootFolderPath);

			string filenameTemplate;
			if (sectName == NokiaConstants.ptContacts)
			{
				filenameTemplate = "{0}.vcf";
				addLine(count.ToString() + " contacts found");
			}
			else if (sectName == NokiaConstants.ptBookmarks)
			{
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
				if (test == 0x10)
				{
					test = StreamUtils.ReadUInt32(fs);
					if (test > 1)
					{
						addLine("test 2 greater than 0x01:" + test.ToString("X"));
					}
				}
				else
				{
					addLine("test 1 different than 0x10:" + test.ToString("X"));
				}

				int vclen = StreamUtils.ReadUInt32asInt(fs);
				long start = fs.Position;

				byte[] buff = StreamUtils.ReadBuff(fs, vclen);
				StreamUtils.Counter += buff.Length;

				Vcard crd = new Vcard(System.Text.Encoding.UTF8.GetString(buff));

				string name;
				DateTime time = DateTime.MinValue;

				if (sectName == NokiaConstants.ptContacts)
				{
					name = crd.Name;
					foreach (string number in crd.PhoneNumbers)
					{
						DataSetNbuExplorer.AddPhonebookEntry(number, name);
					}
					time = crd.GetDateTime("REV");
				}
				else if (sectName == NokiaConstants.ptBookmarks)
				{
					name = crd["TITLE"];
				}
				else
				{
					partFiles = findOrCreateFileInfoList(rootFolderPath + "\\" + crd["X-EPOCAGENDAENTRYTYPE"]);
					name = crd["SUMMARY"];
					time = crd.GetDateTime("DTSTART");
				}

				if (string.IsNullOrEmpty(name)) name = numToName(i + 1);

				FileInfo fi = new FileInfo(currentFileName, string.Format(filenameTemplate, name), start, vclen, time);
				partFiles.Add(fi);
				if (sectName == NokiaConstants.ptContacts)
				{
					contactList.Add(fi);
				}

				if (crd.Photo != null)
				{
					partFiles.Add(new FileInfoMemory(currentFileName, name + "." + crd.PhotoExtension, crd.Photo, time));
				}
			}
		}

		private void parseContacts(string currentFileName, FileStream fs)
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
							partFiles.Add(new FileInfo(currentFileName, filename, fs.Position, size));
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
		}

		private void parseGroups(string currentFileName, FileStream fs)
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
							partFiles.Add(new FileInfo(currentFileName, filename, fs.Position, size));
							StreamUtils.Counter += size;

							fs.Seek(size, SeekOrigin.Current);
							break;
						case 0x1C: // some number - ???
						case 0x1E: // some number - same as group number
							if (s.Length > 0) s += ";";
							s += StreamUtils.ReadUInt32(fs).ToString();
							break;
						default:
							s += ";" + StreamUtils.ReadString(fs);
							break;
					}
				}
				if (grName == "") addLine(string.Format("G{0} = {1}", grNum, s));
				else addLine(s);
			}
		}

		private void parseCalendar(FileStream fs)
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
						case 0:
							fs.ReadByte(); // skip one byte
							k = c3; // break inner cycle
							j -= 1; // decrease outer cycle counter and retry
							break;
						default:
							addLine("Unknown calendar field: " + x);
							k = c3; // break inner cycle
							j = count; // break outer cycle
							analyzeRequest = true;
							break;
					}
				}

				if (x == 0x302 || x == 0x304 || x == 0x305 || x == 0x306 || (todo && x == 0x103)) // ???
				{
					fs.Seek(4, SeekOrigin.Current);
				}

				if (!string.IsNullOrEmpty(s))
				{
					addLine(string.Format("{0:000}: {1}", j + 1, s));
				}
			}
		}

		private void parseBinaryMessages(string currentFileName, FileStream fs)
		{
			long smsBegin = 0;
			try
			{
				addLine("binary encoded messages...");

				fs.Seek(45, SeekOrigin.Current);

				int count = fs.ReadByte();
				if (count > 0)
				{
					fs.ReadByte();
					addLine("Custom folder name: " + StreamUtils.ReadString(fs));
				}

				count = fs.ReadByte();

				for (int i = 1; i <= count; i++)
				{
					int boxnumber = fs.ReadByte();

					string boxname;
					switch (boxnumber)
					{
						case 2: boxname = "Inbox"; break;
						case 3: boxname = "Sent"; break;
						case 4: boxname = "Archive"; break;
						case 5: boxname = "Templates"; break;
						default: boxname = string.Format("box{0}", boxnumber); break;
					}

					int countSms = StreamUtils.ReadUInt16(fs);
					if (countSms > 0)
					{
						List<FileInfo> partFilesTxt = findOrCreateFileInfoList("Messages/" + boxname);
						List<FileInfo> partFilesBin = findOrCreateFileInfoList("Settings/Messages/" + boxname);

						addLine(string.Format("\r\n{0}, {1} messages found:", boxname, countSms));
						for (int j = 1; j <= countSms; j++)
						{
							smsBegin = fs.Position;

							var ms = Message.ReadBinMessage(fs);

							string dateAsString = (ms.MessageTime > DateTime.MinValue) ? ms.MessageTime.ToString() : "";
							addLine(string.Format("{0:000} [{1}] {2}; {3}; {4}", j, numToAddr(smsBegin), dateAsString, ms.PhoneNumber, ms.MessageText));

							string filename = string.Format("{0:0000} {1}", j, ms.PhoneNumber).TrimEnd();
							partFilesBin.Add(new FileInfo(currentFileName, filename + ".sms", smsBegin, fs.Position - smsBegin, ms.MessageTime));

							if (!ms.IsDelivery)
							{
								byte[] data = System.Text.Encoding.UTF8.GetBytes(string.Format("{0}\r\n{1}\r\n{2}", ms.PhoneNumber, dateAsString, ms.MessageText));
								partFilesTxt.Add(new FileInfoMemory(currentFileName, filename + ".txt", data, ms.MessageTime));

								if (parseMsgBinToolStripMenuItem.Checked)
								{
									DataSetNbuExplorer.AddMessage(ms);
								}
							}
						}
					}
				}
			}
			catch (Exception exc)
			{
				analyzeRequest = true;
				addLine(string.Format("Error in message starting at [{0}]: {1}", numToAddr(smsBegin), exc.Message));
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

		public static string buffToString(byte[] buff)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for (int i = 0; i < buff.Length; i++)
			{
				if (i >= 0) sb.Append(" ");
				sb.Append(buff[i].ToString("X").PadLeft(2, '0'));
			}
			return sb.ToString();
		}

		#endregion
	}
}