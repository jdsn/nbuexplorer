using System;
using System.Collections.Generic;
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

		public static Dictionary<string, string> DumpTables(string cdbFilePath, params string[] tableNames)
		{
			if (!File.Exists(pathDbShell))
			{
				throw new FileNotFoundException("DbShell executable not found", pathDbShell);
			}

			string originalWorkDir = Directory.GetCurrentDirectory();

			try
			{
				Dictionary<string, string> result = new Dictionary<string, string>();

				PrepareWorkDir();

				string target = Path.Combine(workDir, workFile);
				if (string.Compare(cdbFilePath, target, true) != 0)
				{
					File.Copy(cdbFilePath, target, true);
				}
				Directory.SetCurrentDirectory(workDir);
				var proc = Process.Start(pathDbShell, workFile);
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
					try
					{
						using (var fs = File.OpenRead(tableName))
						using (var sr = new StreamReader(fs, Encoding.Unicode))
						{
							result[tableName] = sr.ReadToEnd();
						}
					}
					catch { }
				}

				return result;
			}
			finally
			{
				if (Directory.Exists(originalWorkDir))
				{
					Directory.SetCurrentDirectory(originalWorkDir);
				}
				/*try { Directory.Delete(workDir, true); }
				catch { }*/
			}
		}

	}
}
