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
		public string Note { get; set; }
		public List<string> Phones { get; private set; }
		public List<string> Emails { get; private set; }
		public List<string> ParseMessages { get; private set; }
		public bool Confirmed { get; set; }

		public Contact()
		{
			RevDate = DateTime.MinValue;
			Phones = new List<string>();
			Emails = new List<string>();
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
	}
}
