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
using System.Text;

namespace NbuExplorer
{
	public class FileInfo
	{
		protected string sourcePath;
		public string SourcePath
		{
			get { return sourcePath; }
			set { sourcePath = value; }
		}

		protected string filename;
		public string Filename
		{
			get { return filename; }
			set { filename = value; }
		}

		private static List<char> invalidFilenameChars = new List<char>(Path.GetInvalidFileNameChars());

		public string SafeFilename
		{
			get { return MakeFileOrDirNameSafe(filename); }
		}

		public static string MakeFileOrDirNameSafe(string name)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < name.Length; i++)
			{
				if (invalidFilenameChars.Contains(name[i])) sb.Append("_");
				else sb.Append(name[i]);
			}
			return sb.ToString();
		}

		protected long start;
		public long Start { get { return start; } }

		protected long length;
		public virtual long FileSize { get { return length; } }
		public long RawSize { get { return length; } }

		protected DateTime fileTime;
		public DateTime FileTime
		{
			get { return fileTime; }
			set { fileTime = value; }
		}

		protected bool compressed;
		public bool Compressed { get { return compressed; } }

		public bool FileTimeIsValid
		{
			get { return (fileTime > DateTime.MinValue); }
		}

		protected FileInfo()
		{
		}

		public FileInfo(string sourcePath, string filename, long start, long length)
			: this(sourcePath, filename, start, length, DateTime.MinValue, false)
		{
		}

		public FileInfo(string sourcePath, string filename, long start, long length, DateTime fileTime)
			: this(sourcePath, filename, start, length, fileTime, false)
		{
		}

		protected FileInfo(string sourcePath, string filename, long start, long length, DateTime fileTime, bool compressed)
		{
			this.sourcePath = sourcePath;
			this.filename = filename;
			this.start = start;
			this.length = length;
			this.fileTime = fileTime;
			this.compressed = compressed;
		}

		public virtual void CopyToStream(Stream fstgt)
		{
			FileStream fssrc = File.OpenRead(sourcePath);
			try
			{
				fssrc.Seek(this.Start, SeekOrigin.Begin);
				if (compressed)
				{
					fstgt = new ComponentAce.Compression.Libs.zlib.ZOutputStream(fstgt);
				}
				StreamUtils.CopyFromStreamToStream(fssrc, fstgt, this.length);
			}
			finally
			{
				fssrc.Close();
			}
		}

		public MemoryStream GetAsMemoryStream()
		{
			MemoryStream result = new MemoryStream();
			CopyToStream(result);
			result.Seek(0, SeekOrigin.Begin);
			return result;
		}
	}

	public class FileinfoCf : FileInfo
	{
		long uncompLength = 0;

		public FileinfoCf(string sourcePath, string filename, long start, long compLength, long uncompLength, DateTime fileTime)
			: base(sourcePath, filename, start, compLength, fileTime, true)
		{
			this.uncompLength = uncompLength;
		}

		public override long FileSize
		{
			get
			{
				return this.uncompLength;
			}
		}
	}

	public class FileInfoCfPart : FileInfo
	{
		protected long subPartStart = 0;
		protected long subPartLength = 0;

		public FileInfoCfPart(string sourcePath, string filename, long startOfFragment, long compressedLengthOfFragment, DateTime fileTime, long subPartStart, long subPartLength)
			: base(sourcePath, filename, startOfFragment, compressedLengthOfFragment, fileTime, true)
		{
			this.subPartStart = subPartStart;
			this.subPartLength = subPartLength;
		}

		public override void CopyToStream(Stream fstgt)
		{
			MemoryStream ms = new MemoryStream();
			base.CopyToStream(ms);

			ms.Seek(subPartStart, SeekOrigin.Begin);
			byte[] buff = new byte[1024];
			long rest = subPartLength;
			int readCnt;
			while ((readCnt = ms.Read(buff, 0, (int)Math.Min(buff.Length, rest))) > 0)
			{
				fstgt.Write(buff, 0, readCnt);
				rest -= readCnt;
				if (rest == 0) break;
			}

			ms.Dispose();
		}

		public override long FileSize
		{
			get { return this.subPartLength; }
		}
	}

	public class FileInfoCfMultiPart : FileInfo
	{
		private bool shortened = false;
		public bool Shortened
		{
			get { return shortened; }
		}

		private string root;
		public string Root
		{
			get { return root; }
		}

		private string dir;
		public string Dir
		{
			get { return dir; }
		}

		private List<FileInfoCfPart> parts = new List<FileInfoCfPart>();
		public List<FileInfoCfPart> Parts
		{
			get { return parts; }
		}

		protected long totalLength;

		public override long FileSize
		{
			get { return totalLength; }
		}

		public void Shorten(long cutLength)
		{
			shortened = true;
			totalLength -= cutLength;
		}

		public long MissingLength
		{
			get
			{
				long result = totalLength;
				foreach (FileInfoCfPart part in parts)
				{
					result -= part.FileSize;
				}
				return result;
			}
		}

		public FileInfoCfMultiPart(string sourcePath, string filename, DateTime fileTime, long totalLength, string root, string dir)
			: base(sourcePath, filename, 0, 0, fileTime, true)
		{
			this.totalLength = totalLength;
			this.root = root;
			this.dir = dir;
		}

		public override void CopyToStream(Stream fstgt)
		{
			foreach (FileInfoCfPart part in parts)
			{
				part.CopyToStream(fstgt);
			}
		}
	}

	public class FileInfoMemory : FileInfo
	{
		private byte[] content;
		public byte[] Content
		{
			get { return content; }
		}

		public override void CopyToStream(Stream fstgt)
		{
			fstgt.Write(content, 0, content.Length);
		}

		public FileInfoMemory(string sourcePath, string filename, byte[] content, DateTime fileTime)
			: base(sourcePath, filename, 0, content.Length, fileTime)
		{
			this.content = content;
		}
	}

	public enum FileInfoSortType { name, extension, size, time };

	public class FileInfoComparer : System.Collections.IComparer
	{
		public FileInfoSortType SortType;
		public bool desc;

		public int Compare(object x, object y)
		{
			int result = 0;

			try
			{
				FileInfo xfi = (FileInfo)((System.Windows.Forms.ListViewItem)x).Tag;
				FileInfo yfi = (FileInfo)((System.Windows.Forms.ListViewItem)y).Tag;

				switch (SortType)
				{
					case FileInfoSortType.size:
						result = xfi.FileSize.CompareTo(yfi.FileSize);
						break;
					case FileInfoSortType.extension:
						result = Path.GetExtension(xfi.Filename).CompareTo(Path.GetExtension(yfi.Filename));
						if (result == 0) goto default;
						break;
					case FileInfoSortType.time:
						result = xfi.FileTime.CompareTo(yfi.FileTime);
						break;
					default:
						result = xfi.Filename.CompareTo(yfi.Filename);
						break;
				}
				if (desc) result *= -1;
			}
			catch { }

			return result;
		}
	}
}
