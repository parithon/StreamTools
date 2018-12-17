using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace StreamTools.Chatbot
{
	public class ChatUserInfo
	{
		public Instant LastCommandInstant { get; set; }
	}
}
