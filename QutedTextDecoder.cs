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
using System.Text;
using System.Text.RegularExpressions;

namespace NbuExplorer
{
	public static class QutedTextDecoder
	{
		private static Encoding enc;

		private static Regex rexHexText = new Regex("(=([0-9A-F][0-9A-F]))+");

		private static MatchEvaluator eval = new MatchEvaluator(HexTextReplace);

		private static string HexTextReplace(Match m)
		{
			byte[] buff = new byte[m.Groups[2].Captures.Count];
			for (int i = 0; i < buff.Length; i++)
			{
				buff[i] = (byte)int.Parse(m.Groups[2].Captures[i].Value, System.Globalization.NumberStyles.HexNumber);
			}

			try
			{
				return enc.GetString(buff);
			}
			catch
			{
				return "?";
			}
		}

		public static string Decode(string encodingName, string input)
		{
			enc = Encoding.GetEncoding(encodingName);
			return rexHexText.Replace(input, eval);
		}
	}
}
