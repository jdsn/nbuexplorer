using System;
using System.IO;
using System.Text;

namespace NbuExplorer
{
	public enum MessageDirection
	{
		Unknown,
		Incoming,
		Outgoing
	}

	public class SymbianMessage
	{
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

		public SymbianMessage(Stream ms)
		{
			int test;

			if (StreamUtils.SeekTo(UnicodeExpander.msgBodyStartSeq, ms))
			{
				messageText = UnicodeExpander.Expand(ms);

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
						direction = MessageDirection.Unknown;
						ms.Seek(21 + dirByte, SeekOrigin.Current);
						messageTime = StreamUtils.ReadNokiaDateTime3(ms);
					}
					else if (dirByte == 2)
					{
						direction = MessageDirection.Unknown;
						ms.Seek(23, SeekOrigin.Current);

						messageTime = StreamUtils.ReadNokiaDateTime3(ms);
						ms.Seek(2, SeekOrigin.Current);
						smscNumber = StreamUtils.ReadPhoneNumber(ms);
						test = StreamUtils.ReadUInt16(ms);
						if (test == 0x15)
						{
							return;
						}
						phoneNumber = StreamUtils.ReadPhoneNumber(ms);
						test = StreamUtils.ReadUInt16(ms);
						if (test == 0x0400)
						{
							return;
						}
						messageTime = StreamUtils.ReadNokiaDateTime3(ms);
					}
					else if (dirByte == 5 || dirByte == 6)
					{
						direction = MessageDirection.Outgoing;
						ms.Seek(20, SeekOrigin.Current);
						messageTime = StreamUtils.ReadNokiaDateTime3(ms);
						phoneNumber = StreamUtils.ReadPhoneNumber(ms);

						if (StreamUtils.SeekTo(new byte[] { 0x02, 0x03, 0xC2, 0x01 }, ms))
						{
							ms.Seek(9, SeekOrigin.Current);
							StreamUtils.ReadNokiaDateTime3(ms);
							ms.Seek(2, SeekOrigin.Current);
							smscNumber = StreamUtils.ReadPhoneNumber(ms);
						}
					}
					else if (dirByte == 3 || dirByte == 4)
					{
						ms.Seek(2, SeekOrigin.Current);

						test = ms.ReadByte();
						if (test == 0)
						{
							direction = MessageDirection.Incoming;
							ms.Seek(20, SeekOrigin.Current);
							messageTime = StreamUtils.ReadNokiaDateTime3(ms);
							ms.Seek(2, SeekOrigin.Current);
							smscNumber = StreamUtils.ReadPhoneNumber(ms);
							if (test == 1) ms.ReadByte();
							ms.Seek(2, SeekOrigin.Current);
							phoneNumber = StreamUtils.ReadPhoneNumber(ms);
						}
						else if (test == 1)
						{
							direction = MessageDirection.Outgoing;
							ms.Seek(25, SeekOrigin.Current);
							phoneNumber = StreamUtils.ReadPhoneNumber(ms);
							name = UnicodeExpander.ReadShortString(ms);

							test = ms.ReadByte();
							if (test == 1)
							{
								ms.Seek(2, SeekOrigin.Current);
							}

							ms.Seek(21, SeekOrigin.Current);
							// TODO: check these values
							messageTime = StreamUtils.ReadNokiaDateTime3(ms);
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
					if (messageText.Length == 0)
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
