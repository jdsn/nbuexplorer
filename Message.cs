using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NbuExplorer
{
	public enum MessageDirection
	{
		Unknown,
		Incoming,
		Outgoing
	}

	public class MultipartData
	{
		private const string MultipartFormat = "[{0}/{1}]:{2}";

		public int PartNumber;
		public int TotalParts;
		public byte MessageNumber;

		public string AddMultipartPrefix(string s)
		{
			return string.Format(MultipartFormat, this.PartNumber, this.TotalParts, s);
		}
	}

	public class Message
	{
		private static byte[] UnicodeZero = new byte[] { 0, 0 };
		
		public static readonly Regex MsgFileNameRegex = new Regex("[0-9A-F]{47,80}");

		private string messageText = "";
		public string MessageText
		{
			get { return messageText; }
			private set { messageText = value; }
		}

		private string phoneNumber = "";
		public string PhoneNumber
		{
			get { return phoneNumber; }
			private set { phoneNumber = value; }
		}

		private string name = "";
		public string Name
		{
			get { return name; }
			private set { name = value; }
		}

		private string smscNumber = "";
		public string SmscNumber
		{
			get { return smscNumber; }
			private set { smscNumber = value; }
		}

		private DateTime messageTime = DateTime.MinValue;
		public DateTime MessageTime
		{
			get { return messageTime; }
			private set { messageTime = value; }
		}

		public bool IsDelivery { get; private set; }

		private MessageDirection direction = MessageDirection.Unknown;
		public MessageDirection Direction
		{
			get { return direction; }
			private set { direction = value; }
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

		public Mms Mms { get; private set; }

		public MultipartData MultipartInfo { get; private set; }

		public string MultipartKey
		{
			get
			{
				if (MultipartInfo == null)
					return "";

				// properties that are expected to be the same for all parts of one multipart message
				return string.Format("{0}_{1}_{2}_{3}_{4}_{5}",
					this.PhoneNumber,
					this.DirectionBox,
					this.MultipartInfo.MessageNumber,
					this.MultipartInfo.TotalParts,
					this.MessageTime.Year,
					this.MessageTime.DayOfYear);
			}
		}

		public void UpdateTextFromMultipart(string text)
		{
			this.MultipartInfo = null;
			this.MessageText = text;
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
							{
								m.messageTime = DateTime.ParseExact(raw, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
							}
							catch { }
							break;
						case 1049: // multipart info
							string[] mparr = raw.Split(',');
							try
							{
								if (raw.StartsWith("6,5,0,3,") && mparr.Length == 7)
								{
									m.MultipartInfo = new MultipartData
									{
										MessageNumber = byte.Parse(mparr[4]),
										TotalParts = int.Parse(mparr[5]),
										PartNumber = int.Parse(mparr[6])
									};
								}
								else if (raw.StartsWith("7,6,8,4,0,") && mparr.Length == 8)
								{
									m.MultipartInfo = new MultipartData
									{
										MessageNumber = byte.Parse(mparr[5]),
										TotalParts = int.Parse(mparr[6]),
										PartNumber = int.Parse(mparr[7])
									};
								}
							}
							catch
							{ }

							if (m.MultipartInfo != null)
							{
								m.MessageText = m.MultipartInfo.AddMultipartPrefix(m.MessageText);
							}
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

		public static Message ReadSymbianMessage(Stream s)
		{
			int test;

			if (StreamUtils.SeekTo(UnicodeExpander.msgBodyStartSeq, s))
			{
				Message m = new Message();

				m.messageText = UnicodeExpander.Expand(s);

				try
				{
					test = StreamUtils.ReadUInt16(s);
					if (test != 0x2920) throw new ApplicationException("Not sms");
					test = StreamUtils.ReadUInt16(s);
					if (test != 0x1834) throw new ApplicationException("Not sms");

					s.Seek(3, SeekOrigin.Current);
					//parseLog.AppendLine(FormMain.buffToString(StreamUtils.ReadBuff(ms, 6)));

					int dirByte = s.ReadByte();
					if (dirByte == 1 || dirByte == 0)
					{
						m.direction = MessageDirection.Unknown;
						s.Seek(21 + dirByte, SeekOrigin.Current);
						m.messageTime = StreamUtils.ReadNokiaDateTime3(s);
					}
					else if (dirByte == 2)
					{
						m.direction = MessageDirection.Unknown;
						s.Seek(23, SeekOrigin.Current);

						m.messageTime = StreamUtils.ReadNokiaDateTime3(s);
						s.Seek(2, SeekOrigin.Current);
						m.smscNumber = StreamUtils.ReadPhoneNumber(s);
						test = StreamUtils.ReadUInt16(s);
						if (test == 0x15)
						{
							return m;
						}
						m.phoneNumber = StreamUtils.ReadPhoneNumber(s);
						test = StreamUtils.ReadUInt16(s);
						if (test == 0x0400)
						{
							return m;
						}
						m.messageTime = StreamUtils.ReadNokiaDateTime3(s);
					}
					else if (dirByte == 5 || dirByte == 6)
					{
						m.direction = MessageDirection.Outgoing;
						s.Seek(20, SeekOrigin.Current);
						m.messageTime = StreamUtils.ReadNokiaDateTime3(s);
						m.phoneNumber = StreamUtils.ReadPhoneNumber(s);

						if (StreamUtils.SeekTo(new byte[] { 0x02, 0x03, 0xC2, 0x01 }, s))
						{
							s.Seek(9, SeekOrigin.Current);
							StreamUtils.ReadNokiaDateTime3(s);
							s.Seek(2, SeekOrigin.Current);
							m.smscNumber = StreamUtils.ReadPhoneNumber(s);
						}
					}
					else if (dirByte == 3 || dirByte == 4)
					{
						s.Seek(2, SeekOrigin.Current);

						test = s.ReadByte();
						if (test == 0)
						{
							m.direction = MessageDirection.Incoming;
							s.Seek(20, SeekOrigin.Current);
							m.messageTime = StreamUtils.ReadNokiaDateTime3(s);
							s.Seek(2, SeekOrigin.Current);
							m.smscNumber = StreamUtils.ReadPhoneNumber(s);
							if (test == 1) s.ReadByte();
							s.Seek(2, SeekOrigin.Current);
							m.phoneNumber = StreamUtils.ReadPhoneNumber(s);
						}
						else if (test == 1)
						{
							m.direction = MessageDirection.Outgoing;
							s.Seek(25, SeekOrigin.Current);
							m.phoneNumber = StreamUtils.ReadPhoneNumber(s);
							m.name = UnicodeExpander.ReadShortString(s);

							test = s.ReadByte();
							if (test == 1)
							{
								s.Seek(2, SeekOrigin.Current);
							}

							s.Seek(21, SeekOrigin.Current);
							// TODO: check these values
							m.messageTime = StreamUtils.ReadNokiaDateTime3(s);
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
				catch
				{
					if (m.messageText.Length == 0)
					{
						throw new ApplicationException(string.Format("No message found. File size: ({0})", s.Length));
					}
				}

				return m;
			}
			else
			{
				throw new ApplicationException("Starting sequence not found");
			}
		}

		public static Message ReadPredefBinMessage(Stream s, string filename)
		{
			Message m = new Message();

			s.Seek(0xB0, SeekOrigin.Begin);
			int MsgHeaderFlags = s.ReadByte();
			bool udhi = (MsgHeaderFlags & 0x40) > 0;
			int MsgType = MsgHeaderFlags & 0x0F;
			bool ucs2;

			switch (MsgType)
			{
				case 0:
				case 4:
					m.Direction = MessageDirection.Incoming;
					m.PhoneNumber = StreamUtilsPdu.ReadPhoneNumber(s);

					s.ReadByte(); // 00 expected
					ucs2 = (s.ReadByte() == 8);

					try
					{
						m.MessageTime = StreamUtilsPdu.ReadDateTime(s);
					}
					catch { }

					s.ReadByte();

					m.DecodeMessageBody(udhi, ucs2, s);
					break;
				case 1:
					m.Direction = MessageDirection.Outgoing;

					s.ReadByte();

					m.PhoneNumber = StreamUtilsPdu.ReadPhoneNumber(s);
					if (m.PhoneNumber == "+35856789123456789123") m.PhoneNumber = "";

					s.ReadByte();

					ucs2 = (s.ReadByte() == 8);

					s.ReadByte();

					m.DecodeMessageBody(udhi, ucs2, s);

					if (string.IsNullOrEmpty(m.PhoneNumber))
					{
						s.Seek(0x5F, SeekOrigin.Begin);
						StreamUtils.SeekTo(UnicodeZero, s);
						if (s.Position > 0x61)
						{
							int len = (int)(s.Position - 0x5F) / 2;
							s.Seek(0x5F, SeekOrigin.Begin);
							m.PhoneNumber = StreamUtils.ReadString(s, len);
						}
					}

					break;
				case 0x0C:
					s.Seek(-1, SeekOrigin.Current);
					try
					{
						m.Mms = new Mms(s, s.Length);
						m.MessageTime = m.Mms.Time;
						m.MessageText = m.Mms.ParseLog;
					}
					catch { }
					break;
				default:
					throw new ApplicationException("Unknown message type");
			}

			if (m.messageTime == DateTime.MinValue && filename.Length > 16)
			{
				try
				{
					string tmp = filename.Substring(8, 8);
					Int64 sec = Int64.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
					m.MessageTime = new DateTime(1980, 1, 1).AddSeconds(sec);
				}
				catch { }
			}

			return m;
		}

		public static Message ReadBinMessage(Stream s)
		{
			Message m = new Message();

			s.Seek(4, SeekOrigin.Current);
			int MsgHeaderFlags = s.ReadByte();
			bool udhi = ((MsgHeaderFlags & 0x40) > 0);
			int MsgType = MsgHeaderFlags & 0x0F;

			s.ReadByte();
			bool ucs2 = (s.ReadByte() == 8);
			s.ReadByte();

			int test, len, len2;
			byte[] buff;

			switch (MsgType)
			{
				case 0x00:
				case 0x04: // incoming message
					m.Direction = MessageDirection.Incoming;

					s.ReadByte();
					m.MessageTime = StreamUtilsPdu.ReadDateTime(s);
					s.Seek(8, SeekOrigin.Current);
					m.PhoneNumber = StreamUtilsPdu.ReadPhoneNumber(s);
					s.Seek(5, SeekOrigin.Current);
					m.SmscNumber = StreamUtilsPdu.ReadPhoneNumber(s, true);

					s.Seek(4, SeekOrigin.Current);
					len = StreamUtils.ReadUInt16(s);
					len2 = StreamUtils.ReadUInt16(s);
					s.Seek(2, SeekOrigin.Current);
					buff = StreamUtils.ReadBuff(s, len2);
					m.DecodeMessageText(udhi, ucs2, len, buff);
					StreamUtils.Counter += len2;

					break;
				case 0x01: // outgoing message
					m.Direction = MessageDirection.Outgoing;

					s.Seek(2, SeekOrigin.Current);

					test = s.ReadByte();
					if (test == 1)
					{
						m.MessageTime = StreamUtils.ReadNokiaDateTime2(s).ToLocalTime();
						s.Seek(7, SeekOrigin.Current);
					}

					test = s.ReadByte();
					if (test == 1)
					{
						s.Seek(4, SeekOrigin.Current);
						m.PhoneNumber = StreamUtilsPdu.ReadPhoneNumber(s);
						s.Seek(5, SeekOrigin.Current);
						m.SmscNumber = StreamUtilsPdu.ReadPhoneNumber(s, true);
					}
					else
					{
						s.Seek(9, SeekOrigin.Current);
					}

					s.Seek(4, SeekOrigin.Current);
					len = StreamUtils.ReadUInt16(s);
					len2 = StreamUtils.ReadUInt16(s);
					s.Seek(2, SeekOrigin.Current);
					buff = StreamUtils.ReadBuff(s, len2);
					m.DecodeMessageText(udhi, ucs2, len, buff);
					StreamUtils.Counter += len2;

					break;
				case 0x06: // delivery report message
					m.Direction = MessageDirection.Incoming;

					s.ReadByte();
					m.MessageTime = StreamUtilsPdu.ReadDateTime(s);
					s.Seek(15, SeekOrigin.Current);
					m.PhoneNumber = StreamUtilsPdu.ReadPhoneNumber(s);
					s.Seek(5, SeekOrigin.Current);
					m.SmscNumber = StreamUtilsPdu.ReadPhoneNumber(s, true);
					s.Seek(10, SeekOrigin.Current);

					m.MessageText = "Delivery report message";
					m.IsDelivery = true;
					break;
				default:
					throw new ApplicationException("Unknown message type");
			}
			return m;
		}

		private void DecodeMessageBody(bool udhi, bool ucs2, Stream s)
		{
			int len = s.ReadByte();
			byte[] buff;
			if (ucs2)
			{
				buff = StreamUtils.ReadBuff(s, len);
			}
			else
			{
				int len7 = (int)Math.Ceiling(7.0 * len / 8);
				buff = StreamUtils.ReadBuff(s, len7);
			}
			this.DecodeMessageText(udhi, ucs2, len, buff);
		}

		private void DecodeMessageText(bool udhi, bool ucs2, int len, byte[] buff)
		{
			if (udhi)
			{
				int headLength = buff[0];
				int skipChars = (headLength + 1);

				// multipart
				if (ucs2)
				{
					this.MessageText = System.Text.Encoding.BigEndianUnicode.GetString(buff, skipChars, buff.Length - skipChars);
				}
				else
				{
					skipChars = (int)Math.Ceiling(skipChars * 8.0 / 7);
					this.MessageText = StreamUtilsPdu.Decode7bit(buff, len).Substring(skipChars);
				}

				if (headLength > 4 && buff[2] == 3 && buff[4] > 1)
				{
					this.MultipartInfo = new MultipartData { MessageNumber = buff[3], TotalParts = buff[4], PartNumber = buff[5] };
				}
				else if (headLength > 5 && buff[2] == 4)
				{
					this.MultipartInfo = new MultipartData { MessageNumber = buff[4], TotalParts = buff[5], PartNumber = buff[6] };
				}

				if (this.MultipartInfo != null)
				{
					this.MessageText = this.MultipartInfo.AddMultipartPrefix(this.MessageText);
				}
			}
			else
			{
				if (ucs2)
				{
					this.MessageText = System.Text.Encoding.BigEndianUnicode.GetString(buff);
				}
				else
				{
					this.MessageText = StreamUtilsPdu.Decode7bit(buff, len);
				}
			}
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2}:\r\n{3}", messageTime, DirectionString, phoneNumber, messageText);
		}
	}
}
