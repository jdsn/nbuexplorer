using System.Data;
using System;

namespace NbuExplorer
{

	public partial class DataSetNbuExplorer
	{
		private static System.Text.RegularExpressions.Regex newLineRex = new System.Text.RegularExpressions.Regex("[\r\n]+");

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

		public static MessageDataTable DefaultMessageTable
		{
			get { return _defaultInstance.Message; }
		}

		public static DataView DefaultMessageView
		{
			get { return _defaultInstance.Message.DefaultView; }
		}

		public static void AddPhonebookEntry(string number, string name)
		{
			if (string.IsNullOrEmpty(number) || string.IsNullOrEmpty(name)) return;
			if (_defaultInstance.PhoneBook.FindBynumber(number) == null)
			{
				_defaultInstance.PhoneBook.AddPhoneBookRow(number, name);
			}
		}

		public static PhoneBookRow FindPhoneBookEntry(string number)
		{
			PhoneBookRow row = _defaultInstance.PhoneBook.FindBynumber(number);
			return row;
		}

		public static string NumToName(string number)
		{
			PhoneBookRow row = _defaultInstance.PhoneBook.FindBynumber(number);
			return (row == null) ? number : row.name;
		}

		public static void AddMessageFromVmg(Vcard vmg)
		{
			if (!vmg.MessageFound) return;

			MessageRow row;
			if (vmg.PhoneNumbers.Count > 0)
			{
				row = _defaultInstance.Message.AddMessageRow(vmg.MessageBox, vmg.MessageTime, vmg.PhoneNumbers[0], NumToName(vmg.PhoneNumbers[0]), vmg.MessageBody);
			}
			else
			{
				row = _defaultInstance.Message.AddMessageRow(vmg.MessageBox, vmg.MessageTime, null, null, vmg.MessageBody);
			}

			if (row.time == DateTime.MinValue) row.SettimeNull();
		}

		public static bool FindExistingMessage(string number, string text)
		{
			// normalize newlines for comparison
			text = newLineRex.Replace(text, "\n");
			if (number == null) number = "";

			DataSetNbuExplorer.MessageRow[] dupl = (DataSetNbuExplorer.MessageRow[])_defaultInstance.Message.Select("number = '" + number.Replace("'", "''") + "'");
			foreach (DataSetNbuExplorer.MessageRow mr in dupl)
			{
				if (newLineRex.Replace(mr.messagetext, "\n") == text) return true;
			}
			return false;
		}

		public static void AddMessage(Message sm)
		{
			if (sm.MessageText.Length == 0) return;

			if (FindExistingMessage(sm.PhoneNumber, sm.MessageText)) return;

			MessageRow row = _defaultInstance.Message.AddMessageRow(sm.DirectionBox,
				sm.MessageTime,
				sm.PhoneNumber,
				string.IsNullOrEmpty(sm.Name) ? NumToName(sm.PhoneNumber) : sm.Name,
				sm.MessageText);
			if (row.time == DateTime.MinValue) row.SettimeNull();
		}

		public static void AddMessageFromBinMessage(BinMessage msg)
		{
			if (string.IsNullOrEmpty(msg.Text)) return;
			if (FindExistingMessage(msg.Number, msg.Text)) return;

			MessageRow row = _defaultInstance.Message.AddMessageRow(msg.BoxLetter,
				msg.Time,
				msg.Number,
				NumToName(msg.Number),
				msg.Text);
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
