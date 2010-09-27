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
using System.Text.RegularExpressions;

namespace NbuExplorer
{
	public class Vcard
	{
		Dictionary<string, string> attrs = new Dictionary<string, string>();
		List<string> phoneNumbers = new List<string>();

		private static Regex rexEnc = new Regex("ENCODING=([^;]+)");
		private static Regex rexChs = new Regex("CHARSET=([^;]+)");
		private static Regex rexQuoteLineBreak = new Regex(@"=\r\n[\s]*");
		private static Regex rexBase64photo = new Regex(@"PHOTO((;ENCODING=BASE64)|(;TYPE=(?<type>.*?))){1,2}:(?<data>.*?((\r\n\r\n)|(=\r\n)))", RegexOptions.Singleline);

		byte[] photo = null;
		public byte[] Photo
		{
			get { return photo; }
		}

		string photoextension = "photo";
		public string PhotoExtension
		{
			get { return photoextension; }
		}

		public string[] Keys
		{
			get
			{
				List<string> result = new List<string>();
				foreach (string key in attrs.Keys)
				{
					result.Add(key);
				}
				return result.ToArray();
			}
		}

		public string this[string key]
		{
			get
			{
				if (attrs.ContainsKey(key)) return attrs[key];
				return "";
			}
		}

		public List<string> PhoneNumbers
		{
			get { return phoneNumbers; }
		}

		const string parseDateFormat = "yyyyMMddTHHmmss";

		public DateTime GetDateTime(string key)
		{
			if (!attrs.ContainsKey(key)) return DateTime.MinValue;
			try
			{
				return DateTime.ParseExact(this[key].Substring(0, parseDateFormat.Length), parseDateFormat, System.Globalization.CultureInfo.InvariantCulture);
			}
			catch
			{
				return DateTime.MinValue;
			}
		}

		public string Name
		{
			get
			{
				string result = this["N"];
				if (string.IsNullOrEmpty(result)) result = this["ORG"];
				if (string.IsNullOrEmpty(result)) result = this["EMAIL"];
				return result;
			}
		}

		public Vcard(string data)
		{
			Match photoMatch = rexBase64photo.Match(data);
			if (photoMatch.Success)
			{
				Group g = photoMatch.Groups["data"];
				try
				{
					photo = Convert.FromBase64String(g.Value);
					if (photoMatch.Groups["type"].Success)
					{
						photoextension = photoMatch.Groups["type"].Value.ToLower();
					}
					else if (photo[0] == 0xff && photo[1] == 0xd8 && photo[2] == 0xff)
					{
						photoextension = "jpg";
					}
				}
				catch { }
				data = data.Substring(0, photoMatch.Index) + data.Substring(photoMatch.Index + photoMatch.Length);
			}

			string[] lines = rexQuoteLineBreak.Replace(data, "").Replace("\r\n", "\n").Split('\n');

			foreach (string line in lines)
			{
				int index = line.IndexOf(':');
				if (index < 1) continue;
				string key = line.Substring(0, index);
				if (key == "BEGIN" || key == "END") continue;
				string value = line.Substring(index + 1);
				if (string.IsNullOrEmpty(value)) continue;

				if (key.StartsWith("TEL"))
				{
					phoneNumbers.Add(value);
					continue;
				}

				Match me = rexEnc.Match(key);
				Match mc = rexChs.Match(key);
				if (me.Success)
				{
					if (me.Groups[1].Value == "QUOTED-PRINTABLE")
					{
						if (mc.Success)
						{
							value = QutedTextDecoder.Decode(mc.Groups[1].Value, value);
						}
						else
						{
							value = QutedTextDecoder.Decode("UTF-8", value);
						}
					}
				}

				index = key.IndexOf(';');
				if (index > 0) key = key.Substring(0, index);

				if (attrs.ContainsKey(key)) continue;

				if (key == "N") // specialni preskladani jmena
				{
					string tmp = "";
					foreach (string part in value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
					{
						if (tmp.Length > 0) tmp += " ";
						tmp += part;
					}
					value = tmp;
				}

				attrs.Add(key, value);
			}
		}
	}
}