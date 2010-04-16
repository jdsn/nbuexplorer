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

namespace NbuExplorer
{
	public enum ProcessType { None, FileSystem, Vcards, Memos, GeneralFolders, Groups };

	public struct Section
	{
		public byte[] id;
		public ProcessType type;
		public string name;
		public string name2;
		public int imageIndex;

		public Section(byte[] id, ProcessType type, string name, int imageIndex)
			: this(id, type, name, "", imageIndex)
		{ }

		public Section(byte[] id, ProcessType type, string name, string name2, int imageIndex)
		{
			this.id = id;
			this.type = type;
			this.name = name;
			this.name2 = name2;
			this.imageIndex = imageIndex;
		}
	}

	public static class NokiaConstants
	{
		public static Section FindSectById(byte[] id)
		{
			foreach (Section sct in KnownSections)
			{
				if (CompareByteArr(id, sct.id))
				{
					return sct;
				}
			}
			throw new ApplicationException("Unknown section type");
		}

		public static bool CompareByteArr(byte[] arr1, byte[] arr2)
		{
			if (arr1.Length != arr2.Length) return false;
			for (int i = 0; i < arr1.Length; i++)
			{
				if (arr1[i] != arr2[i]) return false;
			}
			return true;
		}

		public const string ptContacts = "Contacts";
		public const string ptGroups = "Groups";
		public const string ptBookmarks = "Bookmarks";
		public const string ptMessages = "Messages";
		public const string ptMms = "MMS";
		public const string ptSettings = "Settings";

		public static Section[] KnownSections = new Section[]
		{
			new Section(new byte[] {
			0x08, 0x29, 0x4B, 0x2B, 0x0E, 0x89, 0x17, 0x4B,
			0x97, 0x73, 0x17, 0xC2, 0x4C, 0x1A, 0xDB, 0xC8 },
			ProcessType.FileSystem, "Internal files", 9),

			new Section(new byte[] {
			0xEF, 0xD4, 0x2E, 0xD0, 0xA3, 0x51, 0x38, 0x47,
			0x9D, 0xD7, 0x30, 0x5C, 0x7A, 0xF0, 0x68, 0xD3 },
			ProcessType.Vcards, ptContacts, 6),

			new Section(new byte[] {
			0x1F, 0x0E, 0x58, 0x65, 0xA1, 0x9F, 0x3C, 0x49,
			0x9E, 0x23, 0x0E, 0x25, 0xEB, 0x24, 0x0F, 0xE1 },
			ProcessType.Groups, ptGroups, 7),

			new Section(new byte[] {
			0x16, 0xCD, 0xF8, 0xE8, 0x23, 0x5E, 0x5A, 0x4E,
			0xB7, 0x35, 0xDD, 0xDF, 0xF1, 0x48, 0x12, 0x22 },
			ProcessType.Vcards, "Calendar", 4),

			new Section(new byte[] {
			0x5C, 0x62, 0x97, 0x3B, 0xDC, 0xA7, 0x54, 0x41,
			0xA1, 0xC3, 0x05, 0x9D, 0xE3, 0x24, 0x68, 0x08 },
			ProcessType.Memos, "Memo", 8),

			new Section(new byte[] {
			0x61, 0x7A, 0xEF, 0xD1, 0xAA, 0xBE, 0xA1, 0x49,
			0x9D, 0x9D, 0x15, 0x5A, 0xBB, 0x4C, 0xEB, 0x8E },
			ProcessType.GeneralFolders, ptMessages, 10),

			new Section(new byte[] {
			0x47, 0x1D, 0xD4, 0x65, 0xEF, 0xE3, 0x32, 0x40,
			0x8C, 0x77, 0x64, 0xCA, 0xA3, 0x83, 0xAA, 0x33 },
			ProcessType.GeneralFolders, ptMms, 10),

			new Section(new byte[] {
			0x7F, 0x77, 0x90, 0x56, 0x31, 0xF9, 0x57, 0x49,
			0x8D, 0x96, 0xEE, 0x44, 0x5D, 0xBE, 0xBC, 0x5A },
			ProcessType.Vcards, ptBookmarks, 3),

			new Section(new byte[] {
			0x60, 0xC2, 0xCB, 0x9C, 0x7E, 0x73, 0x24, 0x41,
			0x8D, 0x90, 0x2E, 0xC0, 0xD9, 0xB0, 0xB6, 0x8C },
			ProcessType.GeneralFolders, ptSettings, "Contacts", 11),

			new Section(new byte[] {
			0x2D, 0xED, 0xC7, 0x29, 0x57, 0x68, 0x22, 0x45,
			0xAE, 0xD4, 0xEB, 0x21, 0x02, 0x96, 0xA1, 0xEE },
			ProcessType.GeneralFolders, ptSettings, "Calendar", 11),

			new Section(new byte[] {
			0x0A, 0xDF, 0x77, 0x94, 0xF7, 0x82, 0xBC, 0x48,
			0xAB, 0xA3, 0x78, 0x91, 0xDB, 0xDB, 0xD0, 0xCF },
			ProcessType.GeneralFolders, ptSettings, "Messages", 11),

			new Section(new byte[] {
			0x79, 0x00, 0x47, 0xC6, 0x6E, 0xCE, 0x7A, 0x44,
			0x81, 0xFB, 0x30, 0x7C, 0xD9, 0x56, 0xAB, 0x10 },
			ProcessType.GeneralFolders, ptSettings, "Basic", 11),

			new Section(new byte[] {
			0x77, 0xA4, 0x23, 0x6A, 0x99, 0xBA, 0x9A, 0x4B,
			0xAB, 0x16, 0xD5, 0x7B, 0x76, 0x0F, 0x16, 0xD9 },
			ProcessType.GeneralFolders, ptSettings, "Bookmarks", 11),

			new Section(new byte[] {
			0x2D, 0xF5, 0x68, 0x6B, 0x1F, 0x4B, 0x22, 0x4A,
			0x92, 0x83, 0x1B, 0x06, 0xC3, 0xC3, 0x9A, 0x35 },
			ProcessType.GeneralFolders, ptSettings, "Advanced", 11),

			new Section(new byte[] {
			0xAD, 0x3A, 0x1B, 0xEC, 0x97, 0x71, 0xB7, 0x42,
			0xA5, 0x67, 0x54, 0xE2, 0xD3, 0x19, 0xF4, 0x89 },
			ProcessType.FileSystem, "Memorycard files", 5),

			new Section(new byte[] {
			0x0E, 0x3D, 0x5F, 0x65, 0xAF, 0x22, 0x78, 0x48,
			0x93, 0x9E, 0xCD, 0x59, 0xA4, 0x5D, 0xF1, 0x29 },
			ProcessType.FileSystem, "Files", 5)
		};

		public static byte[] compHead = new byte[] {
			0x78, 0xDA 
		};

	}
}
