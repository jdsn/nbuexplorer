using System;
using System.Collections.Generic;
using System.Text;

namespace NbuExplorer.cdbParsing
{
	public class Contact
	{
		public static Encoding VcardEncoding = Encoding.UTF8;
		private const string dateFormat = "yyyyMMddTHHmmss";

		public DateTime RevDate { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Company { get; set; }
		public string Address { get; set; }
		public string Note { get; set; }
		public List<string> Phones { get; private set; }
		public List<string> Emails { get; private set; }
		public List<string> Urls { get; private set; }
		public List<string> ParseMessages { get; private set; }
		public bool Confirmed { get; set; }

		public Contact()
		{
			RevDate = DateTime.MinValue;
			Phones = new List<string>();
			Emails = new List<string>();
			Urls = new List<string>();
			ParseMessages = new List<string>();
			Confirmed = false;
		}

		public string ToVcard()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("BEGIN:VCARD");
			sb.AppendLine("VERSION:2.1");
			if (RevDate != DateTime.MinValue)
			{
				AddItem(sb, "REV", RevDate.ToString(dateFormat) + "Z");
			}
			AddItem(sb, "N", LastName, FirstName);
			if (!string.IsNullOrEmpty(Company))
			{
				AddItem(sb, "ORG", Company);
			}
			foreach (string phone in Phones)
			{
				AddItem(sb, "TEL;VOICE", phone);
			}
			foreach (string email in Emails)
			{
				AddItem(sb, "EMAIL;INTERNET", email);
			}
			if (!string.IsNullOrEmpty(Address))
			{
				AddItem(sb, "ADR", Address);
			}
			foreach (string url in Urls)
			{
				AddItem(sb, "URL", url);
			}
			if (!string.IsNullOrEmpty(Note))
			{
				AddItem(sb, "NOTE", Note);
			}
			sb.AppendLine("END:VCARD");
			return sb.ToString();
		}

		private static void AddItem(StringBuilder sb, string key, params string[] values)
		{
			bool encNeeded = false;
			StringBuilder tmp = new StringBuilder();
			foreach (string value in values)
			{
				if (tmp.Length > 0)
				{
					tmp.Append(";");
				}

				if (value == null)
					continue;

				foreach (char c in value)
				{
					if (c < ' ' || c == '=' || c == ';' || c > '~')
					{
						encNeeded = true;
						foreach (byte b in VcardEncoding.GetBytes(new[] { c }))
						{
							tmp.Append("=");
							tmp.Append(((int)b).ToString("X").PadLeft(2, '0'));
						}
					}
					else
					{
						tmp.Append(c);
					}
				}
			}

			sb.Append(key);

			if (encNeeded)
			{
				sb.Append(";ENCODING=QUOTED-PRINTABLE;CHARSET=");
				sb.Append(VcardEncoding.BodyName.ToUpper());
			}

			sb.Append(":");
			sb.AppendLine(tmp.ToString());
		}

		public string DisplayName
		{
			get
			{
				string result = string.Format("{0} {1}", LastName, FirstName).Trim();
				if (string.IsNullOrEmpty(result))
				{
					result = Company;
				}
				if (string.IsNullOrEmpty(result))
				{
					result = "Unknown";
				}
				return result;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(DisplayName);

			sb.Append("|");

			for (int i = 0; i < Phones.Count; i++)
			{
				if (i > 0) sb.Append(",");
				sb.Append(Phones[i]);
			}

			if (Emails.Count > 0)
			{
				sb.Append("|");
				for (int i = 0; i < Emails.Count; i++)
				{
					if (i > 0) sb.Append(",");
					sb.Append(Emails[i]);
				}
			}

			return sb.ToString();
		}

		public static Contact ReadNfbNfcContact(string line)
		{
			Contact c = new Contact();

			string[] arr = line.Split('\x9');

			for (int i = 0; i < arr.Length; i += 2)
			{
				int type;
				string raw;
				if (int.TryParse(arr[i], out type) && arr.Length > i)
				{
					raw = arr[i + 1];
					switch (type)
					{
						case 208:
						case 209:
						case 210:
						case 211:
						case 212:
						case 213:
						case 219:
							if (!c.Phones.Contains(raw))
							{
								c.Phones.Add(raw);
							}
							break;
						case 202:
							c.LastName = raw;
							c.FirstName = "";
							break;
						case 204: // note
							c.Note = raw.Replace("\\n", "\r\n");
							break;
						case 205:
							c.Emails.Add(raw);
							break;
						case 206: // address
							c.Address = raw.Replace("\\n", "\r\n");
							break;
						case 226: // url
							c.Urls.Add(raw);
							break;
						/*case 200: // "PIT_CONTACT"
						case 216: // group membership?
						case 229: // PTT address?
						case 300: // "PIT_CALLERGROUP"
						case 308: // group name?
							break;
						default:
							break;*/
					}
				}
			}

			return c;
		}

		public bool IsEmpty
		{
			get
			{
				return
					LastName == null &&
					FirstName == null &&
					Phones.Count == 0;
			}
		}
	}
}
