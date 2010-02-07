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
using System.IO;
using System.Text;

namespace NbuExplorer
{
	public class Pattern
	{

		public static Pattern Msg = new Pattern("BEGIN:VMSG", "END:VMSG", Encoding.Unicode);
		public static Pattern Contact = new Pattern("BEGIN:VCARD", "END:VCARD", Encoding.UTF8);
		public static Pattern Calendar = new Pattern("BEGIN:VCALENDAR", "END:VCALENDAR", Encoding.UTF8);
		public static Pattern Bookmark = new Pattern("BEGIN:VBKM", "END:VBKM", Encoding.UTF8);

		private Encoding enc;
		private byte[] startSeq;
		private byte[] endSeq;
		private int index = 0;

		public Pattern(string start, string end, Encoding encoding)
		{
			enc = encoding;
			startSeq = enc.GetBytes(start);
			endSeq = enc.GetBytes(end);
		}

		private long startIndex = 0;
		public long StartIndex
		{
			get { return startIndex; }
		}

		private long length;
		public long Length
		{
			get { return length; }
		}

		private bool active = false;
		public bool Active
		{
			get { return active; }
		}

		public bool Step(byte input, long pos)
		{
			if (active)
			{
				if (endSeq[index] == input)
				{
					index++;
					if (index == endSeq.Length)
					{
						length = pos - startIndex;
						index = 0;
						active = false;
						return true;
					}
				}
				else index = 0;
			}
			else
			{
				if (startSeq[index] == input)
				{
					index++;
					if (index == startSeq.Length)
					{
						index = 0;
						active = true;
						startIndex = pos - startSeq.Length;
					}
				}
				else index = 0;
			}

			return false;
		}

		public byte[] GetCapture(Stream s)
		{
			byte[] buff = new byte[(int)length];
			s.Seek(startIndex, SeekOrigin.Begin);
			s.Read(buff, 0, buff.Length);
			return buff;
		}

		public string GetCaptureAsString(Stream s)
		{
			return enc.GetString(GetCapture(s));
		}
	}
}
