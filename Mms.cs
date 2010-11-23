/*******************************************************************************

    NbuExplorer - Nokia backup file parser, viewer and extractor
    Copyright (C) 2010 Petr Vilem

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

    Project homepage: http://sourceforge.net/projects/nbuexplorer/
    Author: Petr Vilem, petrusek@seznam.cz

********************************************************************************
External references for MMS parser:
http://heyman.info/mmsdecoder.php
http://www.openmobilealliance.org/tech/affiliates/LicenseAgreement.asp?DocName=/wap/wap-209-mmsencapsulation-20020105-a.pdf
http://www.openmobilealliance.org/tech/affiliates/LicenseAgreement.asp?DocName=/wap/wap-230-wsp-20010705-a.pdf
http://www.wapforum.org/wina/wsp-content-type.htm
http://www.nowsms.com/discus/messages/board-topics.html
http://www.nowsms.com/discus/messages/12/554.html
http://www.nowsms.com/discus/messages/12/470.html
http://www.nowsms.com/discus/messages/12/522.html
*******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NbuExplorer
{
	public class Mms
	{
		private StringBuilder log = new StringBuilder();
		public string ParseLog
		{
			get { return log.ToString(); }
		}

		private string subject = "";
		public string Subject
		{
			get { return subject; }
		}

		private List<FileInfo> files = new List<FileInfo>();
		public List<FileInfo> Files
		{
			get { return files; }
		}

		private DateTime time = DateTime.MinValue;
		public DateTime Time
		{
			get { return time; }
		}

		public Mms(Stream s, long end)
		{
			bool stop = false;
			bool parseParts = false;

			// header
			while (s.Position < end)
			{
				int type = s.ReadByte();
				switch (type)
				{
					case 0x8C: // MsgType
						MessageType messageType = (MessageType)s.ReadByte();
						log.AppendFormat("MessageType = {0}\r\n", messageType);
						break;
					case 0x8F: // Priority
						MmsPriority prio = (MmsPriority)s.ReadByte();
						log.AppendFormat("Priority = {0}\r\n", prio);
						break;
					case 0x98: // TransId
						log.AppendFormat("TransId = {0}\r\n", ReadTextString(s));
						break;
					case 0x8B:
						log.AppendFormat("MessageId = {0}\r\n", ReadTextString(s));
						break;
					case 0x8D: // MMS version
						int verByte = s.ReadByte();
						int verMajor = (verByte & 0x70) >> 4;
						int verMinor = (verByte & 0x0F);
						log.AppendFormat("MMS version  = {0}.{1}\r\n", verMajor, verMinor);
						break;
					case 0x89: // From
						log.AppendFormat("From = {0}\r\n", ReadEncodedString(s));
						break;
					case 0x97: // To
						log.AppendFormat("To = {0}\r\n", ReadEncodedString(s));
						break;
					case 0x96: // Subject
						this.subject = ReadEncodedString(s);
						log.AppendFormat("Subject = {0}\r\n", this.subject);
						break;
					case 0x86: // Delivery report
						log.AppendFormat("Delivery report = {0}\r\n", ReadYesNo(s));
						break;
					case 0x90: // Read reply
						log.AppendFormat("Read reply = {0}\r\n", ReadYesNo(s));
						break;
					case 0x91: // Report allowed
						log.AppendFormat("Report allowed = {0}\r\n", ReadYesNo(s));
						break;
					case 0x94: // Sender visibility
						log.AppendFormat("Sender visibility = {0}\r\n", ReadYesNo(s));
						break;
					case 0x88: // Expiry
						long expiry = ReadLongInteger(s);
						log.AppendFormat("Expiry = {0}\r\n", expiry);
						break;
					case 0x85: // Date
						this.time = ReadDateTime(s);
						log.AppendFormat("Date = {0}\r\n", this.time);
						break;
					case 0x8A: // MessageClass
						log.AppendFormat("Message class = {0}\r\n", ReadMessageClass(s));
						break;
					case 0x84:
						int contentType = s.ReadByte();

						if (contentType <= 31)
						{ /* Content-general-form */
							s.Seek(-1, SeekOrigin.Current);
							ReadValueLength(s);
							contentType = s.ReadByte();
						}

						string cts = "";
						if (contentType > 31 && contentType < 128)
						{ /* Constrained-media - Extension-media*/
							s.Seek(-1, SeekOrigin.Current);
							cts = ReadTextString(s);
						}
						else
						{ /* Constrained-media - Short Integer*/
							contentType = contentType & 0x7F;
							/******************************************************************
							 * A list of content-types of a MMS message can be found here:    *
							 * http://www.wapforum.org/wina/wsp-content-type.htm              *
							 ******************************************************************/
							switch (contentType)
							{
								case 0x23:
									cts = "multipart.mixed";
									parseParts = true;
									break;
								case 0x33:
									cts = "multipart.related";
									parseParts = true;
									break;
								default:
									cts = "unknown";
									break;
							}
						}

						log.AppendFormat("Content type = {0}\r\n", cts);

						bool noparams = false;
						while (!noparams)
						{
							int testParam = s.ReadByte();
							switch (testParam)
							{
								case 0x89:
									log.AppendFormat("Start = {0}\r\n", ReadTextString(s));
									break;
								case 0x8A:
									testParam = s.ReadByte();
									if (testParam < 128)
									{
										log.AppendFormat("Param = {0}\r\n", ReadTextString(s));
									}
									else
									{
										testParam = testParam & 0x7F;
										log.AppendFormat("Param = {0}\r\n", testParam.ToString("X"));
									}
									break;
								default:
									s.Seek(-1, SeekOrigin.Current);
									noparams = true;
									break;
							}
						}

						stop = true;
						break;
					case 0x99:
					case 0x9A:
						int test = s.ReadByte();
						if (test < 0x80)
						{
							s.Seek(-1, SeekOrigin.Current);
							log.AppendLine("0x" + type.ToString("X") + " = " + ReadTextString(s));
						}
						else
						{
							log.AppendLine("0x" + type.ToString("X") + " = " + test.ToString("X"));
						}
						break;
					default:
						throw new ApplicationException(string.Format("Unknown field type: 0x{0}", type.ToString("X")));
				}
				if (stop) break;
			}

			// body
			if (parseParts)
			{
				int partCount = (int)ReadUint(s);
				log.AppendFormat("PartCnt = {0}\r\n", partCount);

				for (int i = 0; i < partCount; i++)
				{
					long headlen = (long)ReadUint(s);
					long datalen = (long)ReadUint(s);

					long ctypepos = s.Position;

					string ctype;

					int type = s.ReadByte();
					s.Seek(-1, SeekOrigin.Current);

					if (type <= 31)
					{
						ReadValueLength(s);
						type = s.ReadByte();
						s.Seek(-1, SeekOrigin.Current);
					}

					if (type > 31 && type < 128)
					{
						ctype = ReadTextString(s);
					}
					else
					{
						switch (type)
						{
							case 0x9E:
								ctype = "image/jpeg";
								break;
							case 0x83:
								ctype = "text/plain";
								break;
							case 0x87:
								ctype = "text/x-vCard";
								break;
							default: ctype = "unknown";
								break;
						}
					}

					string filename;

					if (headlen > 4)
					{
						filename = ReadTextString(s).TrimStart('?');
					}
					else // no filename present
					{
						string ext = "";
						switch (ctype)
						{
							case "text/plain":
								ext = ".txt";
								break;
						}
						filename = string.Format("part_{0}{1}", i + 1, ext);
					}

					if (ctype == "application/smil" && !filename.ToLower().EndsWith(".smil"))
					{
						filename += ".smil";
					}

					files.Add(new FileInfo(filename, ctypepos + headlen, datalen, this.time));

					log.AppendFormat("Part {0}: headLength = {1}, dataLength = {2}, ctype = {3}, filename = {4}\r\n", i + 1, headlen, datalen, ctype, filename);

					s.Seek(ctypepos + headlen + datalen, SeekOrigin.Begin);
				}
			}
		}

		private static string ReadMessageClass(Stream s)
		{
			int cls = s.ReadByte();
			string clss = "";
			switch (cls)
			{
				case 128:
					clss = "Personal";
					break;
				case 129:
					clss = "Advertisement";
					break;
				case 130:
					clss = "Info";
					break;
				case 131:
					clss = "Auto";
					break;
				default:
					s.Seek(-1, SeekOrigin.Current);
					clss = ReadTextString(s);
					break;
			}
			return clss;
		}

		private static string ReadYesNo(Stream s)
		{
			int b = s.ReadByte();
			switch (b)
			{
				case 128:
					return "Yes";
				case 129:
					return "No";
				default:
					throw new ApplicationException(string.Format("Invalid Yes/No value: {0}", b));
			}
		}

		/*--------------------------------------------------------------------------*
		 * Parse Long-integer                                                       *
		 * Long-integer = Short-length<Octet 0-30> Multi-octet-integer<1*30 Octets> *
		 *--------------------------------------------------------------------------*/
		private static long ReadLongInteger(Stream s)
		{
			// Get the number of octets which the long-integer is stored in
			int octetCount = s.ReadByte();

			// Error checking
			if (octetCount > 30) throw new ApplicationException("Long Integer Parse error");

			long longint = 0;

			// Get the long-integer
			for (int i = 0; i < octetCount; i++)
			{
				longint = longint << 8;
				longint += s.ReadByte();
			}

			return longint;
		}

		private static DateTime ReadDateTime(Stream s)
		{
			DateTime result = new DateTime(1970, 1, 1);
			result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
			long add = ReadLongInteger(s);
			try
			{
				result = result.AddSeconds(add).ToLocalTime();
			}
			catch
			{
				result = DateTime.MinValue;
			}
			return result;
		}

		/*------------------------------------------------------------------------*
		 * Parse Encoded-string-value                                             *
		 * Encoded-string-value = Text-string | Value-length Char-set Text-string *
		 *------------------------------------------------------------------------*/
		private static string ReadEncodedString(Stream s)
		{
			int valLength = s.ReadByte();

			if (valLength == 01)
			{
				s.ReadByte();
				return "";
			}
			else if (valLength <= 31)
			{
				int encoding = s.ReadByte();

				List<byte> data = new List<byte>();
				int i;
				while ((i = s.ReadByte()) > 0) data.Add((byte)i);

				switch (encoding)
				{
					case 0xEA:
						return Encoding.UTF8.GetString(data.ToArray());
					case 0x84: // 'iso-8859-1'
						return Encoding.GetEncoding("iso-8859-1").GetString(data.ToArray());
					case 0x85: // 'iso-8859-2'
						return Encoding.GetEncoding("iso-8859-2").GetString(data.ToArray());
					case 0x86: // 'iso-8859-3'
						return Encoding.GetEncoding("iso-8859-3").GetString(data.ToArray());
					case 0x87: // 'iso-8859-4'
						return Encoding.GetEncoding("iso-8859-4").GetString(data.ToArray());
					default:
						//case 0x83: // ASCII
						return Encoding.ASCII.GetString(data.ToArray());
				}
			}
			else
			{
				s.Seek(-1, SeekOrigin.Current);
				return ReadTextString(s);
			}
		}

		/*----------------------------------------------------------------*
		 * Parse Text-string                                              *
		 * text-string = [Quote <Octet 127>] text [End-string <Octet 00>] *
		 *----------------------------------------------------------------*/
		private static string ReadTextString(Stream s)
		{
			List<byte> data = new List<byte>();
			int i;
			while ((i = s.ReadByte()) > 0) data.Add((byte)i);
			if (data[0] == 0x7F) data.RemoveAt(0); // remove quote
			return Encoding.ASCII.GetString(data.ToArray());
		}

		/*------------------------------------------------------------------*
		 * Parse Unsigned-integer                                           *
		 * The value is stored in the 7 last bits. If the first bit is set, *
		 * then the value continues into the next byte.                     *
		 * http://www.nowsms.com/discus/messages/12/522.html                *
		 *------------------------------------------------------------------*/
		private static UInt64 ReadUint(Stream s)
		{
			UInt64 result = 0;
			UInt64 i = 0;

			do
			{
				i = (UInt64)s.ReadByte();
				// Shift the current value 7 steps
				result = result << 7;
				// Remove the first bit of the byte and add it to the current value
				result |= (i & 0x7F);
			}
			while ((i & 0x80) > 0);

			return result;
		}

		/*--------------------------------------------------------------------------------*
		 * Parse Value-length                                                             *
		 * Value-length = Short-length<Octet 0-30> | Length-quote<Octet 31> Length<Uint>  *
		 *--------------------------------------------------------------------------------*/
		private static UInt64 ReadValueLength(Stream s)
		{
			int i = s.ReadByte();
			if (i < 31)
			{
				// it's a short-length
				return (uint)i;
			}
			else if (i == 31)
			{
				// got the quote, length is an Uint
				return ReadUint(s);
			}
			else
			{
				throw new ApplicationException("Parse value length error");
			}
		}

	}

	public enum MessageType
	{
		Unknown = 0,
		SendReq = 128,
		SendConf = 129,
		NotificationInd = 130,
		NotifyrespInd = 131,
		RetrieveConf = 132,
		AcknowledgeInd = 133,
		DeliveryInd = 134
	}

	public enum MmsPriority
	{
		Unknown = 0,
		Low = 128,
		Normal = 129,
		High = 130
	}

}
