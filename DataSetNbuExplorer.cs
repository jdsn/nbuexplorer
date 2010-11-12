using System.Data;
using System;

namespace NbuExplorer
{

	public partial class DataSetNbuExplorer
	{

		private static DataSetNbuExplorer _defaultInstance = new DataSetNbuExplorer();
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

	}
}
