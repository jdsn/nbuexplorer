using System;
using System.Collections.Generic;
using System.Data;

namespace NbuExplorer.cdbParsing
{
	public class CdbContactParser
	{
		private static readonly List<string> ignoredContactTypes = new List<string> { "268440331", "268455921", "268440330" };
		private static readonly List<string> ignoredSearchableItems = new List<string> { "private", "public" };

		private static string Reverse(string s)
		{
			var arr = s.ToCharArray();
			Array.Reverse(arr);
			return new string(arr);
		}

		public static Dictionary<string, Contact> ParseTableDumps(Dictionary<string, DataTable> tables)
		{
			Dictionary<string, Contact> phonebook = new Dictionary<string, Contact>();
			foreach (var arr in tables["identitytable"].Select())
			{
				// identitytable:
				//Parent_CMID|CM_FirstName|CM_LastName|CM_CompanyName|
				//CM_Type|CM_Attributes|CM_HintField|CM_ExtHintField|
				//CM_FirstNmPrn|CM_LastNmPrn|CM_CompanyNmPrn

				if (ignoredContactTypes.Contains((string)arr[4]))
					continue;

				string id = (string)arr[0];
				Contact c = new Contact
				{
					FirstName = (string)arr[1],
					LastName = (string)arr[2],
					Company = (string)arr[3],
				};
				phonebook[id] = c;
			}

			foreach (var arr in tables["phone"].Select())
			{
				// phone:
				//CM_Id|CM_PhoneMatching|CM_ExtendedPhoneMatching
				string id = (string)arr[0];
				if (!phonebook.ContainsKey(id))
					continue;

				Contact cont = phonebook[id];

				string number = Reverse(((string)arr[2]).PadLeft(8, '0')) + Reverse(((string)arr[1]).PadLeft(7, '0'));
				number = number.TrimStart('0');

				if (number.Length == 0)
				{
					cont.ParseMessages.Add("empty number found");
					continue;
				}
				else
				{
					cont.Phones.Add(number);
				}
			}

			if (tables.ContainsKey("emailtable"))
			{
				foreach (var arr in tables["emailtable"].Select())
				{
					// emailtable:
					//EMail_FieldID|EmailParent_CMID|EMailAddress
					string id = (string)arr[1];
					if (!phonebook.ContainsKey(id))
						continue;
					phonebook[id].Emails.Add((string)arr[2]);
				}
			}

			foreach (var arr in tables["contacts"].Select())
			{
				// contacts:
				//CM_Id|CM_Type|CM_PrefTemplateRefId|CM_UIDString|
				//CM_Last_modified|CM_ContactCreationDate|CM_Attributes|
				//CM_ReplicationCount|CM_Header|CM_TextBlob|CM_SearchableText

				//if (arr.Length == 6)
				{
					DateTime rev = DbShell.ParseDateTime((string)arr[0]);

					List<string> items = new List<string>();
					List<string> itemsPending = new List<string>();
					int test;
					foreach (string s in ((string)arr[5]).Split('\x00'))
					{
						var ltrim = s.Trim();
						if (string.IsNullOrEmpty(ltrim))
							continue;
						if (ignoredSearchableItems.Contains(ltrim.ToLower()))
							continue;
						if (int.TryParse(ltrim, out test) && test < 1000)
						{
							itemsPending.Add(ltrim);
							continue;
						}

						items.Add(ltrim);
					}

					if (items.Count == 0)
					{
						if (itemsPending.Count > 0)
						{
							// short number is better than nothing
							items.AddRange(itemsPending);
						}
						else
						{
							continue;
						}
					}

					Contact cont = FindContact(phonebook, items);

					if (cont == null)
						continue;

					cont.RevDate = rev;
					cont.Confirmed = true;

					for (int i = 0; i < cont.Phones.Count; i++)
					{
						var number = items.Find(n => n.Replace(" ", "").EndsWith(cont.Phones[i]));
						if (number != null)
						{
							cont.Phones[i] = number;
							items.Remove(number);
						}
					}

					if (items.Count > 0)
					{
						cont.Note = string.Join("\r\n", items.ToArray());
					}
				}
			}

			return phonebook;
		}

		private static Contact FindContact(Dictionary<string, Contact> phonebook, List<string> items)
		{
			var candidates = new List<ContactMatch>();
			foreach (var contact in phonebook.Values)
			{
				if (contact.Confirmed)
					continue;

				ContactMatch mtch = new ContactMatch { Contact = contact };

				foreach (var phone in contact.Phones)
				{
					mtch.Total += phone.Length;
					foreach (var number in items)
					{
						if (number.Replace(" ", "").EndsWith(phone))
						{
							mtch.Match += phone.Length;
							break;
						}
					}
				}

				if (mtch.Match > 0)
				{
					candidates.Add(mtch);
				}
			}

			if (candidates.Count == 0)
			{
				return null;
			}
			else if (candidates.Count > 1)
			{
				candidates.Sort(Compare);
			}

			return candidates[0].Contact;
		}

		private static int Compare(ContactMatch x1, ContactMatch x2)
		{
			int result = x2.Match.CompareTo(x1.Match);
			if (result == 0)
			{
				result = x1.Total.CompareTo(x2.Total);
			}
			return result;
		}

		private class ContactMatch
		{
			public Contact Contact { get; set; }
			public int Match { get; set; }
			public int Total { get; set; }
		}
	}
}
