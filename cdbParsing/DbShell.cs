using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace NbuExplorer.cdbParsing
{
	public class DbShell
	{
		public static string pathDbShell = null;
		public static string workDir = Path.Combine(Path.GetTempPath(), "DbShell");
		public static readonly string workFile = "source.cdb";

		static DbShell()
		{
			pathDbShell = Path.Combine(Path.GetDirectoryName(
				Assembly.GetExecutingAssembly().Location), @"dbshell\dbshell.exe");
		}

		public static bool Ready
		{
			get { return File.Exists(pathDbShell); }
		}

		public static void PrepareWorkDir()
		{
			if (Directory.Exists(workDir))
			{
				foreach (string file in Directory.GetFiles(workDir, "*."))
				{
					File.Delete(file);
				}
			}
			else
			{
				Directory.CreateDirectory(workDir);
			}
		}

		[DllImport("user32.dll")]
		private static extern byte VkKeyScan(char ch);

		[DllImport("user32.dll", EntryPoint = "PostMessageA")]
		private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

		private const int WM_KEYDOWN = 0x100;

		private static void SendKeys(IntPtr handle, string text)
		{
			foreach (char ch in text)
			{
				int code;
				if (ch >= '0' && ch <= '9')
				{
					//'0' = 48, Keys.NumPad0 = 96
					code = (int)ch + 48;
				}
				else
				{
					code = VkKeyScan(ch);
				}
				PostMessage(handle, WM_KEYDOWN, code, 0);
			}
		}

		public static Dictionary<string, DataTable> DumpTables(string cdbFilePath, params string[] tableNames)
		{
			if (!File.Exists(pathDbShell))
			{
				throw new FileNotFoundException("DbShell executable not found", pathDbShell);
			}

			Dictionary<string, DataTable> result = new Dictionary<string, DataTable>();

			PrepareWorkDir();

			string target = Path.Combine(workDir, workFile);
			if (string.Compare(cdbFilePath, target, true) != 0)
			{
				File.Copy(cdbFilePath, target, true);
			}

			var pi = new ProcessStartInfo(pathDbShell);
			pi.WindowStyle = ProcessWindowStyle.Minimized;
			pi.WorkingDirectory = workDir;
			pi.Arguments = workFile;

			var proc = Process.Start(pi);
			try
			{
				DateTime dtstart = DateTime.Now;
				Thread.Sleep(1000);
				System.Diagnostics.Debug.WriteLine(DateTime.Now - dtstart);
				foreach (string tableName in tableNames)
				{
					SendKeys(proc.MainWindowHandle, string.Format("export {0} {0}\n", tableName));
				}
				Thread.Sleep(2000);
				System.Diagnostics.Debug.WriteLine(DateTime.Now - dtstart);
			}
			finally
			{
				proc.Kill();
			}

			foreach (string tableName in tableNames)
			{
				DataTable tbl = new DataTable(tableName);

				try
				{
					using (var fs = File.OpenRead(Path.Combine(workDir, tableName)))
					{
						int b1, b2;
						List<byte> cell = new List<byte>();
						List<string> row = new List<string>();

						while (true)
						{
							b1 = fs.ReadByte();
							b2 = fs.ReadByte();

							if (b1 == -1 || b2 == -1)
								break;

							if (b1 == 0x7C && b2 == 0) // | cell separator
							{
								AddCell(cell, row);
							}
							else if (b1 == 0x0D && b2 == 0) // CR
							{
								b1 = fs.ReadByte();
								b2 = fs.ReadByte();
								if (b1 == 0x0A && b2 == 0) // LF
								{
									AddCell(cell, row);

									if (tbl.Columns.Count == 0)
									{
										foreach (var s in row)
											tbl.Columns.Add();
									}

									if (tbl.Columns.Count == row.Count)
									{
										tbl.Rows.Add(row.ToArray());
									}
									else
									{
										// invalid cell count in row
									}
									row.Clear();
								}
								else
								{
									cell.Add(0x0D);
									cell.Add(0x00);
									fs.Seek(-2, SeekOrigin.Current);
								}
							}
							else if (b1 == 0x29 && b2 == 0x20)
							{
								cell.Add(0x0D);
								cell.Add(0x00);
								cell.Add(0x0A);
								cell.Add(0x00);
							}
							else
							{
								cell.Add((byte)b1);
								cell.Add((byte)b2);
							}
						}
					}
				}
				catch { }

				if (tbl.Rows.Count > 0)
				{
					result[tableName] = tbl;
				}
			}

			return result;
		}

		private static void AddCell(List<byte> cell, List<string> row)
		{
			row.Add(Encoding.Unicode.GetString(cell.ToArray()));
			cell.Clear();
		}

		public static DateTime ParseDateTime(string dateTime)
		{
			DateTime result = DateTime.MinValue;
			DateTime.TryParseExact(dateTime,
					"dd/MM/yyyy h:mm:ss tt",
					System.Globalization.CultureInfo.InvariantCulture,
					System.Globalization.DateTimeStyles.None,
					out result);
			return result;
		}

	}
}
