using System.Data;
using System;

namespace NbuExplorer
{

	public partial class DataSetNbuExplorer
	{
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

		public static void AddMessageFromSymbianMessage(SymbianMessage sm)
		{
			if (sm.MessageText.Length == 0) return;

			DataSetNbuExplorer.MessageRow[] dupl = (DataSetNbuExplorer.MessageRow[])_defaultInstance.Message.Select("number = '" + sm.PhoneNumber.Replace("'", "''") + "'");
			foreach (DataSetNbuExplorer.MessageRow mr in dupl)
			{
				if (mr.messagetext == sm.MessageText) return;
			}

			MessageRow row = _defaultInstance.Message.AddMessageRow(sm.DirectionBox,
				sm.MessageTime,
				sm.PhoneNumber,
				string.IsNullOrEmpty(sm.Name) ? NumToName(sm.PhoneNumber) : sm.Name,
				sm.MessageText);
			if (row.time == DateTime.MinValue) row.SettimeNull();
		}

	}
}
