using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NbuExplorer
{

	public partial class DataSetNbuExplorer
	{
		private static System.Text.RegularExpressions.Regex newLineRex = new System.Text.RegularExpressions.Regex("[\r\n]+");
		private static System.Security.Cryptography.MD5 md5alg = System.Security.Cryptography.MD5.Create();

		static DataSetNbuExplorer()
		{
			_defaultInstance = new DataSetNbuExplorer();
			_defaultInstance.Message.DefaultView.Sort = "time desc";
		}

		private static DataSetNbuExplorer _defaultInstance;
		public static DataSetNbuExplorer DefaultInstance
		{
			get { return _defaultInstance; }
		}

		private static Dictionary<string, List<Message>> _multipartMessages = new Dictionary<string, List<Message>>();

		public static void Init()
		{
			_defaultInstance.Clear();
			_multipartMessages.Clear();
		}

		public static void FinalizeMultiparts()
		{
			foreach (var group in _multipartMessages.Values)
			{
				int expectedCount = group[0].MultipartInfo.TotalParts;

				if (group.Count == expectedCount)
				{
					// ideal situation - just sort and join
					group.Sort(CompareMessagesByPartNumber);
					ProcessSortedGroup(group);
				}
				else if (group.Count % expectedCount == 0)
				{
					// sent multipart messages are often having MessageNumber = 0
					// which may lead to this situation
					// we can still try to distinguish parts of one message by time
					int subgroups = group.Count / expectedCount;

					group.Sort(CompareMessagesByPartNumberAndTime);

					for (int i = 0; i < subgroups; i++)
					{
						var subgroup = new List<Message>();
						bool checkPassed = true;
						for (int j = 0; j < expectedCount; j++)
						{
							var part = group[subgroups * j + i];
							subgroup.Add(part);

							if (part.MultipartInfo.PartNumber != j + 1 || part.MessageTime == DateTime.MinValue)
							{
								// we cannot be sure
								checkPassed = false;
							}
						}

						if (checkPassed)
						{
							ProcessSortedGroup(subgroup);
						}
						else
						{
							ProcessUnjoinableMessages(subgroup);
						}
					}
				}
				else
				{
					// unable to join multipart message automatically
					// lets keep it as individual messages
					ProcessUnjoinableMessages(group);
				}
			}
			_multipartMessages.Clear();
		}

		private static void ProcessUnjoinableMessages(List<Message> group)
		{
			foreach (var m in group)
			{
				AddMessageInternal(m);
			}
		}

		private static void ProcessSortedGroup(List<Message> group)
		{
			var sb = new StringBuilder();
			foreach (var part in group)
			{
				sb.Append(part.MessageText.Substring(part.MultipartInfo.AddMultipartPrefix("").Length));
			}
			var first = group[0];
			first.UpdateTextFromMultipart(sb.ToString());
			AddMessageInternal(first);
		}

		private static int CompareMessagesByPartNumber(Message m1, Message m2)
		{
			return m1.MultipartInfo.PartNumber.CompareTo(m2.MultipartInfo.PartNumber);
		}

		private static int CompareMessagesByPartNumberAndTime(Message m1, Message m2)
		{
			int c = m1.MultipartInfo.PartNumber.CompareTo(m2.MultipartInfo.PartNumber);
			if (c != 0) return c;
			return m1.MessageTime.CompareTo(m2.MessageTime);
		}

		public static MessageDataTable DefaultMessageTable
		{
			get { return _defaultInstance.Message; }
		}

		public static DataView DefaultMessageView
		{
			get { return _defaultInstance.Message.DefaultView; }
		}

		private static string NormalizePhoneNumber(string number)
		{
			return number.Replace("+", "00");
		}

		public static void AddPhonebookEntry(string number, string name)
		{
			if (string.IsNullOrEmpty(number) || string.IsNullOrEmpty(name)) return;
			number = NormalizePhoneNumber(number);
			if (_defaultInstance.PhoneBook.FindBynumber(number) == null)
			{
				_defaultInstance.PhoneBook.AddPhoneBookRow(number, name);
			}
		}

		public static PhoneBookRow FindPhoneBookEntry(string number)
		{
			number = NormalizePhoneNumber(number);
			PhoneBookRow row = _defaultInstance.PhoneBook.FindBynumber(number);
			if (row == null && number.Length > 6)
			{
				DataRow[] rows = _defaultInstance.PhoneBook.Select("number LIKE '%" + number.Substring(number.Length - 6).Replace("'","''") + "'");
				foreach (PhoneBookRow r in rows)
				{
					if (r.number.EndsWith(number) || number.EndsWith(r.number))
					{
						AddPhonebookEntry(number, r.name);
						return r;
					}
				}
			}
			return row;
		}

		public static string NumToName(string number)
		{
			PhoneBookRow row = FindPhoneBookEntry(number);
			return (row == null) ? number : row.name;
		}

		public static void AddMessageFromVmg(Vcard vmg)
		{
			if (!vmg.MessageFound || string.IsNullOrEmpty(vmg.MessageBody))
				return;

			string number, name;
			if (vmg.PhoneNumbers.Count > 0)
			{
				number = vmg.PhoneNumbers[0];
				name = NumToName(number);
			}
			else
			{
				number = "";
				name = "";
			}

			string hash;
			if (FindExistingMessage(number, vmg.MessageBody, out hash))
				return;

			MessageRow row = _defaultInstance.Message.AddMessageRow(vmg.MessageBox, vmg.MessageTime, number, name, vmg.MessageBody, hash);
			if (row.time == DateTime.MinValue) row.SettimeNull();
		}

		public static bool FindExistingMessage(string number, string text, out string hash)
		{
			// normalize newlines for comparison
			text = newLineRex.Replace(text, "\n");
			if (number == null) number = "";

			var tmp = md5alg.ComputeHash(System.Text.Encoding.UTF8.GetBytes(number + "|" + text));
			hash = BitConverter.ToString(tmp).Replace("-", "");

			DataSetNbuExplorer.MessageRow[] dupl = (DataSetNbuExplorer.MessageRow[])_defaultInstance.Message.Select("hash = '" + hash + "'");
			return dupl.Length > 0;
		}

		public static void AddMessage(Message m)
		{
			if (string.IsNullOrEmpty(m.MessageText))
				return;

			if (m.MultipartInfo != null)
			{
				string key = m.MultipartKey;
				if (_multipartMessages.ContainsKey(key))
				{
					var existing = _multipartMessages[key];
					// filter duplicates
					foreach(var other in existing)
					{
						if (other.MessageText.Equals(m.MessageText))
							return;
					}
					existing.Add(m);
				}
				else
				{
					_multipartMessages[key] = new List<Message> { m };
				}
				return;
			}

			AddMessageInternal(m);
		}

		private static void AddMessageInternal(Message m)
		{
			string hash;
			if (FindExistingMessage(m.PhoneNumber, m.MessageText, out hash))
			{
				return;
			}

			MessageRow row = _defaultInstance.Message.AddMessageRow(m.DirectionBox,
						 m.MessageTime,
						 m.PhoneNumber,
						 string.IsNullOrEmpty(m.Name) ? NumToName(m.PhoneNumber) : m.Name,
						 m.MessageText,
						 hash);
			if (row.time == DateTime.MinValue) row.SettimeNull();
		}

		partial class MessageRow
		{
			public long SbrTime
			{
				get
				{
					if (IstimeNull())
						return 0;

					return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000;
				}
			}

			public int SbrType
			{
				get
				{
					switch (box)
					{
						case "I":
							return 1;
						case "O":
							return 2;
						default:
							return 3;
					}
				}
			}
		}


	}
}
