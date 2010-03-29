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
	public static class StreamUtils
	{

		public static long Counter = 0;

		public static long ReadUInt64asLong(Stream s)
		{
			return (long)ReadUInt64(s);
		}

		public static UInt64 ReadUInt64(Stream s)
		{
			byte[] buff = new byte[8];
			s.Read(buff, 0, buff.Length);
			Counter += buff.Length;
			return BitConverter.ToUInt64(buff, 0);
		}

		public static int ReadUInt32asInt(Stream s)
		{
			return (int)ReadUInt32(s);
		}

		public static UInt32 ReadUInt32(Stream s)
		{
			byte[] buff = new byte[4];
			s.Read(buff, 0, buff.Length);
			Counter += buff.Length;
			return BitConverter.ToUInt32(buff, 0);
		}

		public static UInt16 ReadUInt16(Stream s)
		{
			byte[] buff = new byte[2];
			s.Read(buff, 0, buff.Length);
			Counter += buff.Length;
			return BitConverter.ToUInt16(buff, 0);
		}

		public static string ReadString(Stream s)
		{
			byte[] buff = new byte[ReadUInt16(s) * 2];
			s.Read(buff, 0, buff.Length);
			Counter += buff.Length;
			return Encoding.Unicode.GetString(buff);
		}

		public static string ReadString2(Stream s)
		{
			byte[] buff = new byte[ReadUInt32asInt(s) * 2];
			s.Read(buff, 0, buff.Length);
			Counter += buff.Length;
			return Encoding.Unicode.GetString(buff);
		}

		public static string ReadShortString(Stream s)
		{
			byte[] buff = new byte[s.ReadByte()];
			s.Read(buff, 0, buff.Length);
			Counter += 1 + buff.Length;
			return Encoding.ASCII.GetString(buff);
		}

		public static DateTime ReadNokiaDateTime(Stream s)
		{
			byte[] buff = new byte[8];
			s.Read(buff, 4, 4);
			s.Read(buff, 0, 4);
			Counter += buff.Length;
			DateTime dt;
			try
			{
				dt = DateTime.FromFileTimeUtc(BitConverter.ToInt64(buff, 0)).ToLocalTime();
			}
			catch
			{
				dt = DateTime.MinValue;
			}
			return dt;
		}

		public static DateTime ReadNokiaDateTime2(Stream s)
		{
			byte[] buff = new byte[8];
			s.Read(buff, 0, buff.Length);
			UInt16 year = BitConverter.ToUInt16(buff, 0);
			DateTime dt = new DateTime((int)year, (int)buff[2], (int)buff[3], (int)buff[4], (int)buff[5], (int)buff[6]);
			return dt;
		}

		public static bool SeekTo(byte[] sequence, Stream s)
		{
			int index = 0;
			while (true)
			{
				int b = s.ReadByte();
				if (b == -1) return false;
				else if (b == sequence[index])
				{
					index++;
					if (index == sequence.Length) return true;
				}
				else
				{
					index = 0;
				}
			}
		}

		public static string ReadStringTo(Stream s, params byte[] termination)
		{
			List<byte> termList = new List<byte>(termination);
			StringBuilder sb = new StringBuilder();
			while (true)
			{
				int b = s.ReadByte();
				if (b == -1) throw new EndOfStreamException();
				else if (termList.Contains((byte)b))
				{
					Counter += sb.Length;
					return sb.ToString();
				}
				else
				{
					sb.Append((char)b);
				}
			}
		}

	}
}
