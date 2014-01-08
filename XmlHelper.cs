using System.Collections.Generic;
using System.Text;

namespace NbuExplorer
{
	public class XmlHelper
	{
		private static readonly List<int> Illegals = new List<int> { 0xB, 0xC, 0xFFFE, 0xFFFF };

		public static string CleanStringForXml(string s)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char c in s)
			{
				if (!(0x0 <= c && c <= 0x8) &&
					!Illegals.Contains((int)c) &&
					!(0xE <= c && c <= 0x1F) &&
					!(0x7F <= c && c <= 0x84) &&
					!(0x86 <= c && c <= 0x9F) &&
					!(0xD800 <= c && c <= 0xDFFF))
					sb.Append(c);
			}
			return sb.ToString();
		}
	}
}
