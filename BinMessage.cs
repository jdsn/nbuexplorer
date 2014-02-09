using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NbuExplorer
{
	public class BinMessage
	{
		public class MultipartData
		{
			public int PartNumber;
			public int TotalParts;
			public byte MessageNumber;
		}

		private static byte[] UnicodeZero = new byte[] { 0, 0 };

		public static readonly Regex MsgFileNameRegex = new Regex("[0-9A-F]{47,80}");

		public string BoxLetter { get; private set; }

		public string Number { get; private set; }

		public string SmsCenter { get; private set; }

		public DateTime Time { get; private set; }

		public string Text { get; private set; }

		public MultipartData MultipartInfo { get; private set; }

		public bool IsDelivery { get; private set; }

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
			bool udhi = (test & 0x40) > 0;

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
					this.DecodeMessageText(ucs2, udhi, len, buff);
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

					this.DecodeMessageText(ucs2, udhi, len, buff);

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

		public BinMessage(string boxLetter, Stream fs)
		{
			this.Time = DateTime.MinValue;
			this.BoxLetter = boxLetter;

			fs.Seek(4, SeekOrigin.Current);
			int MsgHeaderFlags = fs.ReadByte();
			bool udhi = ((MsgHeaderFlags & 0x40) > 0);

			fs.ReadByte();
			//fs.Seek(6, SeekOrigin.Current);
			bool ucs2 = (fs.ReadByte() == 8);
			fs.ReadByte();

			int test = fs.ReadByte();

			if (test == 7) // time is present
			{
				this.Time = StreamUtilsPdu.ReadDateTime(fs);
				fs.ReadByte();
				test = fs.ReadByte();
				if (test == 7)
				{
					// delivery message???
					fs.Seek(13, SeekOrigin.Current);
					this.Number = StreamUtilsPdu.ReadPhoneNumber(fs);
					fs.Seek(5, SeekOrigin.Current);
					this.SmsCenter = StreamUtilsPdu.ReadPhoneNumber(fs);
					fs.Seek(12, SeekOrigin.Current);

					this.Text = "Delivery message";
					this.IsDelivery = true;

					return;
				}
				else
				{
					//addLine(buffToString(StreamUtils.ReadBuff(fs, 6)));
					fs.Seek(6, SeekOrigin.Current);
					this.Number = StreamUtilsPdu.ReadPhoneNumber(fs);
					//addLine(buffToString(StreamUtils.ReadBuff(fs, 5)));
					fs.Seek(5, SeekOrigin.Current);
					this.SmsCenter = StreamUtilsPdu.ReadPhoneNumber(fs);
				}
			}
			else
			{
				if (fs.ReadByte() != 0)
					throw new Exception("00 expected here");

				test = fs.ReadByte();
				if (test == 1)
				{
					this.Time = StreamUtils.ReadNokiaDateTime2(fs).ToLocalTime();
					fs.Seek(7, SeekOrigin.Current);
				}

				test = fs.ReadByte();
				if (test == 1)
				{
					fs.Seek(4, SeekOrigin.Current);
					this.Number = StreamUtilsPdu.ReadPhoneNumber(fs);
					fs.Seek(5, SeekOrigin.Current);
					this.SmsCenter = StreamUtilsPdu.ReadPhoneNumber(fs);
				}
				else
				{
					fs.Seek(7, SeekOrigin.Current);
				}
			}

			//addLine(buffToString(StreamUtils.ReadBuff(fs, 6)));
			fs.Seek(6, SeekOrigin.Current);

			int len1 = StreamUtils.ReadUInt16(fs);
			int len2 = StreamUtils.ReadUInt16(fs);

			//addLine(StreamUtils.ReadUInt16(fs).ToString());
			fs.Seek(2, SeekOrigin.Current);

			byte[] buff = StreamUtils.ReadBuff(fs, len2);
			StreamUtils.Counter += len2;

			this.DecodeMessageText(ucs2, udhi, len1, buff);
		}

		public void DecodeMessageText(bool ucs2, bool udhi, int len1, byte[] buff)
		{
			if (udhi)
			{
				if (buff.Length > 2)
				{
					int headLength = buff[0];
					int skipChars = (headLength + 1);

					// multipart
					if (ucs2)
					{
						this.Text = System.Text.Encoding.BigEndianUnicode.GetString(buff, skipChars, buff.Length - skipChars);
					}
					else
					{
						skipChars = (int)Math.Ceiling(skipChars * 8.0 / 7);
						this.Text = StreamUtilsPdu.Decode7bit(buff, len1).Substring(skipChars);
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
						this.Text = string.Format(Message.MultipartFormat, MultipartInfo.PartNumber, MultipartInfo.TotalParts, this.Text);
					}
				}
				else
				{
					this.Text = "Unsupported message format";
				}
			}
			else
			{
				if (ucs2)
				{
					this.Text = System.Text.Encoding.BigEndianUnicode.GetString(buff);
				}
				else
				{
					this.Text = StreamUtilsPdu.Decode7bit(buff, len1);
				}
			}
		}

	}
}
