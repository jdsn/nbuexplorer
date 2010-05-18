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
using System.IO;
using System.Text;

namespace NbuExplorer
{
	public static class UnicodeExpander
	{
		public static byte[] msgBodyStartSeq = new byte[] { 0x25, 0x3A, 0x00, 0x10 };

		public static int ReadInt(Stream s)
		{
			int i = s.ReadByte();

			if ((i & 1) == 0)
			{
				return (i >> 1);
			}
			else if ((i & 3) == 1)
			{
				return (i >> 2) + (s.ReadByte() << 6);
			}
			else
			{
				throw new ApplicationException("Unknown integer encoding");
			}
		}

		// Single-byte mode tag values
		private const byte SQ0 = 0x01;	// <byte>				quote from window 0
		private const byte SDX = 0x0B;	// <hbyte> <lbyte>		define window in expansion area
		private const byte SQU = 0x0E;	// <hbyte> <lbyte>		quote Unicode value
		private const byte SCU = 0x0F;	//						switch to Unicode mode
		private const byte SC0 = 0x10;	//						select dynamic window 0
		private const byte SD0 = 0x18;	// <byte>				set dynamic window 0 index to <byte> and select it

		private static UInt16[] iStaticWindow = new UInt16[]{
			0x0000,		// tags
			0x0080,		// Latin-1 supplement
			0x0100,		// Latin Extended-A
			0x0300,		// Combining Diacritics
			0x2000,		// General Punctuation
			0x2080,		// Currency Symbols
			0x2100,		// Letterlike Symbols and Number Forms
			0x3000		// CJK Symbols and Punctuation
		};

		private static UInt16[] iDynamicWindowDefault = new UInt16[]{
			0x0080,		// Latin-1 supplement
			0x00C0,		// parts of Latin-1 supplement and Latin Extended-A
			0x0400,		// Cyrillic
			0x0600,		// Arabic
			0x0900,		// Devanagari
			0x3040,		// Hiragana
			0x30A0,		// Katakana
			0xFF00		// Fullwidth ASCII
		};

		private static UInt16[] iSpecialBase = new UInt16[]{
			0x00C0,		// Latin 1 letters (not symbols) and some of Extended-A
			0x0250,		// IPA extensions
			0x0370,		// Greek
			0x0530,		// Armenian
			0x3040,		// Hiragana
			0x30A0,		// Katakana
			0xFF60		// Halfwidth katakana
		};

		public static string Expand(Stream s)
		{
			int len = ReadInt(s);
			return Expand(s, len);
		}

		public static string Expand(Stream s, int len)
		{
			// persistentstorage\store\pcstore\src\unicodecompression.cpp

			StringBuilder sb = new StringBuilder();

			int iActiveWindowBase = 0x80;
			UInt16[] iDynamicWindow = new UInt16[iDynamicWindowDefault.Length];
			Array.Copy(iDynamicWindowDefault, iDynamicWindow, iDynamicWindowDefault.Length);

			int aByte;

			while (len > 0)
			{
				aByte = s.ReadByte();
				if (EncodeAsIs(aByte)) // 'Pass-through' codes.
				{
					sb.Append((char)aByte);
				}
				else if (aByte >= 0x80) // Codes 0x80-0xFF select a character from the active window.
				{
					sb.Append((char)(aByte + iActiveWindowBase - 0x80));
				}
				else if (aByte == SQU) // SQU: quote a Unicode character.
				{
					if (len == 1) return sb.ToString();
					int u = (s.ReadByte() << 8) + (s.ReadByte());
					sb.Append((char)u);
				}
				else if (aByte >= SQ0 && aByte <= SQ0 + 7) // SQn: quote from window n.
				{
					int window = aByte - SQ0;
					int c = s.ReadByte();
					if (c <= 0x7F)
						c += iStaticWindow[window];
					else
						c += iDynamicWindow[window] - 0x80;
					sb.Append((char)c);
				}
				else if (aByte >= SC0 && aByte <= SC0 + 7) // SCn: switch to dynamic window n.
				{
					iActiveWindowBase = iDynamicWindow[aByte - SC0];
					len++;
				}
				else if (aByte >= SD0 && aByte <= SD0 + 7)
				{
					int aIndex = aByte - SD0;
					int window = s.ReadByte();
					iActiveWindowBase = DynamicWindowBase(window);
					iDynamicWindow[aIndex] = (ushort)iActiveWindowBase;
					len++;
				}
				else if (aByte == SCU)
				{
					throw new NotImplementedException("Unicode mode not implemented");
				}
				else if (aByte == SDX)
				{
					throw new NotImplementedException("Expansion area windows not implemented");
				}
				else throw new InvalidDataException("Unknown mode tag");
				len--;
			}
			return sb.ToString();
		}

		private static int DynamicWindowBase(int aOffsetIndex)
		{
			if (aOffsetIndex >= 0xF9 && aOffsetIndex <= 0xFF)
			{
				int special_base_index = aOffsetIndex - 0xF9;
				return iSpecialBase[special_base_index];
			}
			if (aOffsetIndex >= 0x01 && aOffsetIndex <= 0x67)
				return aOffsetIndex * 0x80;
			if (aOffsetIndex >= 0x68 && aOffsetIndex <= 0xA7)
				return aOffsetIndex * 0x80 + 0xAC00;
			return 0;
		}

		private static bool EncodeAsIs(int aCode)
		{
			return aCode == 0x0000 ||
				aCode == 0x0009 ||
				aCode == 0x000A ||
				aCode == 0x000D ||
			 (aCode >= 0x0020 && aCode <= 0x007F);
		}

	}
}
