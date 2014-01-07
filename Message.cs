using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace NbuExplorer
{
	public enum MessageDirection
	{
		Unknown,
		Incoming,
		Outgoing
	}

	public class Message
	{
		public const string MultipartFormat = "[{0}/{1}]:{2}";

		private string messageText = "";
		public string MessageText
		{
			get { return messageText; }
		}

		private string phoneNumber = "";
		public string PhoneNumber
		{
			get { return phoneNumber; }
		}

		private string name = "";
		public string Name
		{
			get { return name; }
		}

		private string smscNumber = "";
		public string SmscNumber
		{
			get { return smscNumber; }
		}

		private DateTime messageTime = DateTime.MinValue;
		public DateTime MessageTime
		{
			get { return messageTime; }
		}

		private MessageDirection direction = MessageDirection.Unknown;
		public MessageDirection Direction
		{
			get { return direction; }
		}

		public string DirectionString
		{
			get
			{
				switch (direction)
				{
					case MessageDirection.Incoming: return "from";
					case MessageDirection.Outgoing: return "to";
					default: return "";
				}
			}
		}

		public string DirectionBox
		{
			get
			{
				switch (direction)
				{
					case MessageDirection.Incoming: return "I";
					case MessageDirection.Outgoing: return "O";
					default: return "U";
				}
			}
		}

		/*private StringBuilder parseLog = new StringBuilder();
		public string ParseLog
		{
			get { return parseLog.ToString(); }
		}*/

		public static Message ReadNfbNfcMessage(string line)
		{
			Message m = new Message();

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
						case 1031: // folder index (2 = inbox, 3 = outbox, 4 = archive, 5 = template, >5 custom folder)
							if (raw == "2") m.direction = MessageDirection.Incoming;
							else if (raw == "3") m.direction = MessageDirection.Outgoing;
							break;
						case 1033: // message text
							m.messageText = raw.Replace("\\n","\r\n");
							break;
						case 1040: // sms center
							m.smscNumber = raw;
							break;
						case 1041: // delivery time
						case 1060: // ???
							try
							{ m.messageTime = DateTime.ParseExact(raw, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture); }
							catch { }
							break;
						case 1049: // multipart info
							string[] mparr = raw.Split(',');
							if (raw.StartsWith("6,5,0,3,") && mparr.Length == 7)
							{
								m.messageText = string.Format(MultipartFormat, mparr[6], mparr[5], m.messageText);
							}
							else if (raw.StartsWith("7,6,8,4,0,") && mparr.Length == 8)
							{
								m.messageText = string.Format(MultipartFormat, mparr[7], mparr[6], m.messageText);
							}
							/*else
							{ }*/
							break;
						case 1080: // sender number
							m.phoneNumber = raw;
							if (m.direction == MessageDirection.Unknown)
								m.direction = MessageDirection.Incoming;
							break;
						case 1081: // recipient number
							if (raw != "@") // used by templates
							{
								m.phoneNumber = raw;
							}
							break;
						/*case 1006: // "PIT_FOLDER_OWNFOLDER"
						case 1011: // custom folder index
						case 1012: // custom folder name
						case 1020: // "PIT_MESSAGE_INBOX"
						case 1021: // "PIT_MESSAGE_OUTBOX"
						case 1022: // "PIT_MESSAGE_ARCHIVE"
						case 1023: // "PIT_MESSAGE_TEMPLATE"
						case 1025: // "PIT_MESSAGE_OWNFOLDER"
						case 1032: // ?
						case 1034: // something like subject??
						case 1035: // 0
						case 1036: // 3
						case 1038: // 0
						case 1039: // 0
						case 1045: // 255
						case 1050: // 255
						case 1055: // 0
						case 1058: // 10
						case 1059: // 0
						case 1063: // some byte array ...maybe image binary data?
						case 1064: // e.g. 28 ...maybe image height?
						case 1065: // e.g. 72 ...maybe image width?
						case 1066: // 1
						default:
							break;*/
					}
				}
			}

			if (m.messageText == "")
				return null;
			else
				return m;
		}

		public static Message ReadSymbianMessage(Stream ms)
		{
			int test;

			if (StreamUtils.SeekTo(UnicodeExpander.msgBodyStartSeq, ms))
			{
				Message m = new Message();

				m.messageText = UnicodeExpander.Expand(ms);

				try
				{
					test = StreamUtils.ReadUInt16(ms);
					if (test != 0x2920) throw new ApplicationException("Not sms");
					test = StreamUtils.ReadUInt16(ms);
					if (test != 0x1834) throw new ApplicationException("Not sms");

					ms.Seek(3, SeekOrigin.Current);
					//parseLog.AppendLine(FormMain.buffToString(StreamUtils.ReadBuff(ms, 6)));

					int dirByte = ms.ReadByte();
					if (dirByte == 1 || dirByte == 0)
					{
						m.direction = MessageDirection.Unknown;
						ms.Seek(21 + dirByte, SeekOrigin.Current);
						m.messageTime = StreamUtils.ReadNokiaDateTime3(ms);
					}
					else if (dirByte == 2)
					{
						m.direction = MessageDirection.Unknown;
						ms.Seek(23, SeekOrigin.Current);

						m.messageTime = StreamUtils.ReadNokiaDateTime3(ms);
						ms.Seek(2, SeekOrigin.Current);
						m.smscNumber = StreamUtils.ReadPhoneNumber(ms);
						test = StreamUtils.ReadUInt16(ms);
						if (test == 0x15)
						{
							return m;
						}
						m.phoneNumber = StreamUtils.ReadPhoneNumber(ms);
						test = StreamUtils.ReadUInt16(ms);
						if (test == 0x0400)
						{
							return m;
						}
						m.messageTime = StreamUtils.ReadNokiaDateTime3(ms);
					}
					else if (dirByte == 5 || dirByte == 6)
					{
						m.direction = MessageDirection.Outgoing;
						ms.Seek(20, SeekOrigin.Current);
						m.messageTime = StreamUtils.ReadNokiaDateTime3(ms);
						m.phoneNumber = StreamUtils.ReadPhoneNumber(ms);

						if (StreamUtils.SeekTo(new byte[] { 0x02, 0x03, 0xC2, 0x01 }, ms))
						{
							ms.Seek(9, SeekOrigin.Current);
							StreamUtils.ReadNokiaDateTime3(ms);
							ms.Seek(2, SeekOrigin.Current);
							m.smscNumber = StreamUtils.ReadPhoneNumber(ms);
						}
					}
					else if (dirByte == 3 || dirByte == 4)
					{
						ms.Seek(2, SeekOrigin.Current);

						test = ms.ReadByte();
						if (test == 0)
						{
							m.direction = MessageDirection.Incoming;
							ms.Seek(20, SeekOrigin.Current);
							m.messageTime = StreamUtils.ReadNokiaDateTime3(ms);
							ms.Seek(2, SeekOrigin.Current);
							m.smscNumber = StreamUtils.ReadPhoneNumber(ms);
							if (test == 1) ms.ReadByte();
							ms.Seek(2, SeekOrigin.Current);
							m.phoneNumber = StreamUtils.ReadPhoneNumber(ms);
						}
						else if (test == 1)
						{
							m.direction = MessageDirection.Outgoing;
							ms.Seek(25, SeekOrigin.Current);
							m.phoneNumber = StreamUtils.ReadPhoneNumber(ms);
							m.name = UnicodeExpander.ReadShortString(ms);

							test = ms.ReadByte();
							if (test == 1)
							{
								ms.Seek(2, SeekOrigin.Current);
							}

							ms.Seek(21, SeekOrigin.Current);
							// TODO: check these values
							m.messageTime = StreamUtils.ReadNokiaDateTime3(ms);
						}
						else
						{
							throw new ApplicationException(string.Format("Unexpected test value ({0})", test));
						}
					}
					else
					{
						throw new ApplicationException(string.Format("Unknown direction ({0})", dirByte));
					}
				}
				catch //(Exception ex)
				{
					if (m.messageText.Length == 0)
					{
						throw new ApplicationException(string.Format("No message found. File size: ({0})", ms.Length));
					}
					else
					{
						// TODO:
						/*throw new ApplicationException(string.Format("Parsing error at position [{0}/{1}]: {2}\r\nMessage text found: {3}",
							ms.Position, ms.Length, ex.Message, messageText), ex);*/
					}
				}

				return m;
			}
			else
			{
				throw new ApplicationException("Starting sequence not found");
			}
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2}:\r\n{3}", messageTime, DirectionString, phoneNumber, messageText);
		}
	}
}
