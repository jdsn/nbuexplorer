using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NbuExplorer
{
	public class BinMessage
	{
		private static byte[] UnicodeZero = new byte[] { 0, 0 };

		public static readonly Regex MsgFileNameRegex = new Regex("[0-9A-F]{47,80}");

		public string BoxLetter { get; private set; }

		public string Number { get; private set; }

		public DateTime Time { get; private set; }

		public string Text { get; private set; }

		public Mms Mms { get; private set; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (!string.IsNullOrEmpty(Number))
			{
				if (BoxLetter == "I") sb.AppendFormat("From {0}", Number);
				else if (BoxLetter == "O") sb.AppendFormat("To {0}", Number);
			}
			if (Time > DateTime.MinValue) sb.AppendFormat("; {0} ", Time);
			if (sb.Length > 0) sb.AppendLine(":");
			sb.Append(Text);
			return sb.ToString();
		}

		public BinMessage(Stream s, string filename)
		{
			StringBuilder sb = new StringBuilder();
			int test, len, len7;
			bool ucs2;
			byte[] buff;

			s.Seek(0xB0, SeekOrigin.Begin);
			test = s.ReadByte();

			switch (test & 0x0F)
			{
				case 0:
				case 4:
					BoxLetter = "I";
					Number = StreamUtilsPdu.ReadPhoneNumber(s);

					s.ReadByte(); // 00 expected
					ucs2 = (s.ReadByte() == 8);

					try
					{
						Time = StreamUtilsPdu.ReadDateTime(s);
					}
					catch
					{
						Time = DateTime.MinValue;
					}

					s.ReadByte();

					len = s.ReadByte();
					if (ucs2)
					{
						buff = StreamUtils.ReadBuff(s, len);
					}
					else
					{
						len7 = (int)Math.Ceiling(7.0 * len / 8);
						buff = StreamUtils.ReadBuff(s, len7);
					}
					Text = StreamUtilsPdu.DecodeMessageText(ucs2, len, buff);
					break;
				case 1:
					BoxLetter = "O";

					s.ReadByte();

					Number = StreamUtilsPdu.ReadPhoneNumber(s);
					if (Number == "+35856789123456789123") Number = "";

					s.ReadByte();

					ucs2 = (s.ReadByte() == 8);

					s.ReadByte();

					if (ucs2)
					{
						len = s.ReadByte();
						buff = StreamUtils.ReadBuff(s, len);
					}
					else
					{
						len = s.ReadByte();
						len7 = (int)Math.Ceiling(7.0 * len / 8);
						buff = StreamUtils.ReadBuff(s, len7);
					}
					Text = StreamUtilsPdu.DecodeMessageText(ucs2, len, buff);

					if (string.IsNullOrEmpty(Number))
					{
						s.Seek(0x5F, SeekOrigin.Begin);
						StreamUtils.SeekTo(UnicodeZero, s);
						if (s.Position > 0x61)
						{
							len = (int)(s.Position - 0x5F) / 2;
							s.Seek(0x5F, SeekOrigin.Begin);
							Number = StreamUtils.ReadString(s, len);
						}
					}

					break;
				case 0x0C:
					BoxLetter = "U";
					s.Seek(-1, SeekOrigin.Current);
					try
					{
						this.Mms = new Mms(s, s.Length);
						Text = this.Mms.ParseLog;
					}
					catch { }
					break;
				default:
					throw new ApplicationException("Unknown message type");
			}

			if (filename.Length == 55 || filename.Length == 63)
			{
				try
				{
					string tmp = filename.Substring(8, 8);
					Int64 sec = Int64.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
					Time = new DateTime(1980, 1, 1).AddSeconds(sec);
				}
				catch { }
			}
		}

	}
}
