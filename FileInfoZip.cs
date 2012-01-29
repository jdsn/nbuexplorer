using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace NbuExplorer
{
	public class FileInfoZip : FileInfo
	{
		private long offset;
		private long fileSize;
		private int itemIndex;

		public FileInfoZip(ZipEntry ze, int itemIndex, long offset)
		{
			this.offset = offset;
			this.itemIndex = itemIndex;
			this.Filename = Path.GetFileName(ze.Name);
			this.FileTime = ze.DateTime;
			this.fileSize = ze.Size;
		}

		public override long FileSize
		{
			get { return fileSize; }
		}

		public override void CopyToStream(string sourceNbuFile, Stream fstgt)
		{
			using (FileStream fs = File.OpenRead(sourceNbuFile))
			{
				fs.Seek(offset, SeekOrigin.Begin);
				ZipInputStream zs = new ZipInputStream(fs);
				ZipEntry ze;
				int index = 0;
				while ((ze = zs.GetNextEntry()) != null)
				{
					if (itemIndex == index)
					{
						StreamUtils.CopyFromStreamToStream(zs, fstgt, ze.Size);
						return;
					}
					index++;
				}
			}
		}
	}
}
