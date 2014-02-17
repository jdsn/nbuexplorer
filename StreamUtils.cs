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
			int len = ReadUInt16(s);
			return ReadString(s, len);
		}

		public static string ReadString2(Stream s)
		{
			int len = ReadUInt32asInt(s);
			return ReadString(s, len);
		}

		public static string ReadString(Stream s, int len)
		{
			byte[] buff = ReadBuff(s, len * 2);
			Counter += buff.Length;
			return Encoding.Unicode.GetString(buff);
		}

		public static string ReadShortString(Stream s)
		{
			Counter += 1;
			return ReadShortString(s, s.ReadByte());
		}

		public static string ReadShortString(Stream s, int len)
		{
			byte[] buff = new byte[len];
			s.Read(buff, 0, buff.Length);
			for (int i = 0; i < len; i++)
			{
				if (buff[i] < 32 && buff[i] != 10 && buff[i] != 13)
					buff[i] = (byte)'?';
			}
			Counter += buff.Length;
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

		public static DateTime ReadNokiaDateTime3(Stream s)
		{
			UInt64 sec = StreamUtils.ReadUInt64(s) / 1000000;
			try
			{
				DateTime dt = DateTime.MinValue.AddSeconds(sec).AddDays(-378);
				return dt;
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Invalid datetime value", ex);
			}
		}

		public static string ReadPhoneNumber(Stream s)
		{
			int len = s.ReadByte() / 4;
			return ReadShortString(s, len);
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
					if (index > 1)
						s.Seek(1 - index, SeekOrigin.Current);
					index = 0;
				}
			}
		}

		public static bool MatchSequence(byte[] sequence, Stream s)
		{
			byte[] buff = new byte[sequence.Length];
			s.Read(buff, 0, sequence.Length);
			return (NokiaConstants.CompareByteArr(sequence, buff));
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

		public static byte[] ReadBuff(Stream s, int length)
		{
			if (s.Position + length > s.Length) throw new InvalidOperationException(string.Format("Invalid read attempt behind end of stream, position {0}, bytes required {1}", s.Position, length));
			byte[] result = new byte[length];
			s.Read(result, 0, length);
			return result;
		}

		public static void CopyFromStreamToStream(Stream src, Stream dst, long bytesCount)
		{
			byte[] buff = new byte[1024];
			long rest = bytesCount;
			while (true)
			{
				if (src.Read(buff, 0, buff.Length) == 0)
				{
					throw new EndOfStreamException();
				}
				if (rest > buff.Length)
				{
					dst.Write(buff, 0, buff.Length);
					rest -= buff.Length;
				}
				else
				{
					dst.Write(buff, 0, (int)rest);
					break;
				}
			}
		}

	}

	public static class StreamUtilsPdu
	{
		public static UInt16[] GSM2Unicode = {
			0x0040,0x00A3,0x0024,0x00A5,0x00E8,0x00E9,0x00F9,0x00EC, // @ $
			0x00F2,0x00C7,0x000A,0x00D8,0x00F8,0x000D,0x00C5,0x00E5, //  _
			0x0394,0x005F,0x03A6,0x0393,0x039B,0x03A9,0x03A0,0x03A8, //
			0x03A3,0x0398,0x039E,0x001B,0x00C6,0x00E6,0x00DF,0x00C9, //
			32,33,34,35,0x00A4,37,38,39,     //  !"#¤%&'
			40,41,42,43,44,45,46,47,         // ()*+,-./
			48,49,50,51,52,53,54,55,         // 01234567
			56,57,58,59,60,61,62,63,         // 89:;<=>?
			0x00A1,65,66,67,68,69,70,71,     // _ABCDEFG
			72,73,74,75,76,77,78,79,         // HIJKLMNO
			80,81,82,83,84,85,86,87,         // PQRSTUVW
			88,89,90,196,214,209,220,167,    // XYZÄÖ Ü§
			0x00BF,97,98,99,100,101,102,103, //  abcdefg
			104,105,106,107,108,109,110,111, // hijklmno
			112,113,114,115,116,117,118,119, // pqrstuvw
			120,121,122,0x00E4,0x00F6,0x00F1,0x00FC,0x00E0  // xyzäö ü
		};

		private static byte[] Convert8to7(byte[] raw, int len)
		{
			List<byte> result = new List<byte>();
			byte c;
			byte c2 = 0;
			for (int i = 0; i < raw.Length; i++)
			{
				int shift = i % 7;
				c = (byte)(c2 + ((raw[i] & (0x7F >> shift)) << shift));
				result.Add(c);

				if (result.Count >= len) break;

				shift = 7 - shift;
				c2 = (byte)((raw[i] & (0x7F << shift)) >> shift);
				if (shift == 1)
				{
					result.Add(c2);
					c2 = 0;
				}
			}
			return result.ToArray();
		}

		public static string Decode7bit(byte[] source, int length)
		{
			byte[] data = Convert8to7(source, length);
			StringBuilder sb = new StringBuilder();
			bool esc = false;
			for (int i = 0; i < data.Length; i++)
			{
				UInt16 c = GSM2Unicode[data[i]];
				if (c == 27)
				{
					esc = true;
				}
				else if (esc)
				{
					switch (c)
					{
						case 10: c = 12; break; // FORM FEED
						case 20: c = 94; break; // CIRCUMFLEX ACCENT ^
						case 40: c = 123; break; // LEFT CURLY BRACKET {
						case 41: c = 125; break; // RIGHT CURLY BRACKET }
						case 47: c = 92; break; // REVERSE SOLIDUS (BACKSLASH) \
						case 60: c = 91; break; // LEFT SQUARE BRACKET [
						case 61: c = 126; break; // TILDE ~
						case 62: c = 93; break; // RIGHT SQUARE BRACKET ]
						case 64: c = 124; break; // VERTICAL BAR |
						case 101: c = 8364; break;// EURO SIGN
					}
					sb.Append((char)c);
					esc = false;
				}
				else
				{
					sb.Append((char)c);
				}
			}
			return sb.ToString();
		}

		private static byte ReadInvertDecimalByte(Stream s)
		{
			int b = s.ReadByte();
			int result = ((b & 0xF) * 10) + ((b & 0xF0) >> 4);
			return (byte)result;
		}

		public static DateTime ReadDateTime(Stream s)
		{
			int year = ReadInvertDecimalByte(s);
			if (year < 80) year += 2000; else year += 1900;
			int month = ReadInvertDecimalByte(s);
			int day = ReadInvertDecimalByte(s);
			int hour = ReadInvertDecimalByte(s);
			int min = ReadInvertDecimalByte(s);
			int sec = ReadInvertDecimalByte(s);
			return new DateTime(year, month, day, hour, min, sec);
		}

		public static string ReadPhoneNumber(Stream s, bool lenInBytes = false)
		{
			StringBuilder sb = new StringBuilder();
			int len = s.ReadByte();
			if (len == 0) return "";
			if (lenInBytes)
			{
				len = (len - 1) * 2;
			}
			int type = (s.ReadByte() & 0xF0) >> 4;
			switch (type)
			{
				case 0x08: // 1000
					//sb.Append("*");
					goto case 0x0A;
				case 0x09: // 1001
					sb.Append("+");
					goto case 0x0A;
				case 0x0A: // 1010
				case 0x0B: // 1011
				case 0x0E: // 1110
					int b;
					while (len > 0)
					{
						b = s.ReadByte();
						sb.Append(((int)(b & 0xF)).ToString("X"));
						len--;
						if (len > 0)
						{
							sb.Append(((int)((b & 0xF0) >> 4)).ToString("X"));
							len--;
						}
					}
					break;
				case 0x0D: // 1101
					int len8 = (len + 1) / 2;
					int len7 = (len * 4 / 7);
					byte[] buff = new byte[len8];
					s.Read(buff, 0, buff.Length);
					sb.Append(Decode7bit(buff, len7));
					break;
				default:
					throw new ApplicationException("Unknown phone number format");
			}
			return sb.ToString();
		}
	}
}
