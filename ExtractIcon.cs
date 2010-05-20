using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace NbuExplorer
{
	/// <summary>
	/// icon associated to filetype (by file extension)
	/// </summary>
	public static class ExtractIcon
	{
		[DllImport("Shell32.dll")]
		private static extern int SHGetFileInfo
			(
			string pszPath,
			uint dwFileAttributes,
			out SHFILEINFO psfi,
			uint cbfileInfo,
			SHGFI uFlags
			);

		[StructLayout(LayoutKind.Sequential)]
		private struct SHFILEINFO
		{
			public SHFILEINFO(bool b)
			{
				hIcon = IntPtr.Zero; iIcon = 0; dwAttributes = 0; szDisplayName = ""; szTypeName = "";
			}
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.LPStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.LPStr, SizeConst = 80)]
			public string szTypeName;
		};

		private enum SHGFI
		{
			SmallIcon = 0x00000001,
			OpenIcon = 0x00000002,
			LargeIcon = 0x00000000,
			Icon = 0x00000100,
			DisplayName = 0x00000200,
			Typename = 0x00000400,
			SysIconIndex = 0x00004000,
			LinkOverlay = 0x00008000,
			UseFileAttributes = 0x00000010
		}

		/// <summary>
		/// Get the associated Icon for a file or application, this method always returns
		/// an icon. If the strPath is invalid or there is no idonc the defaulticon is returned
		/// </summary>
		/// <param name="strPath">full path to the file or directory</param>
		/// <param name="bSmall">if true, the 16x16 icon is returned otherwise the32x32</param>
		/// <param name="bOpen">if true, and strPath is a folder, returns the 'open' icon rather than the 'closed'</param>
		/// <returns></returns>
		public static Icon GetIcon(string strPath, bool bSmall, bool bOpen)
		{
			// Detect Operating System
			// From: http://www.mono-project.com/FAQ:_Technical
			int p = (int)Environment.OSVersion.Platform;
			if ((p == 4) || (p == 6) || (p == 128))
			{
				return GetIconFromResources(strPath);
			}
			else
			{
				try
				{
					SHFILEINFO info = new SHFILEINFO(true);
					int cbFileInfo = Marshal.SizeOf(info);
					SHGFI flags;
					if (bSmall)
						flags = SHGFI.Icon | SHGFI.SmallIcon | SHGFI.UseFileAttributes;
					else
						flags = SHGFI.Icon | SHGFI.LargeIcon | SHGFI.UseFileAttributes;
					if (bOpen) flags = flags | SHGFI.OpenIcon;

					SHGetFileInfo(strPath, 256, out info, (uint)cbFileInfo, flags);
					return Icon.FromHandle(info.hIcon);
				}
				catch
				{
					return GetIconFromResources(strPath);
				}
			}
		}

		private static Icon GetIconFromResources(string strPath)
		{
			switch (System.IO.Path.GetExtension(strPath).ToLower())
			{
				case ".jpg":
				case ".jpeg":
				case ".gif":
				case ".png":
				case ".bmp":
					return Properties.Resources.iconfile_image;
				case ".3gp":
				case ".avi":
				case ".amr":
				case ".mp3":
				case ".mid":
					return Properties.Resources.iconfile_multimedia;
				case ".vcf":
					return Properties.Resources.iconfile_contacts;
				case ".vcs":
					return Properties.Resources.iconfile_calendar;
				case ".vmg":
				case ".mms":
					return Properties.Resources.iconfile_messages;
				case ".bmk":
				case ".url":
				case ".htm":
				case ".html":
					return Properties.Resources.iconfile_bookmark;
				case ".txt":
				case ".csv":
				case ".xml":
					return Properties.Resources.iconfile_text;
				default:
					return Properties.Resources.iconfile;
			}
		}
	}
}
