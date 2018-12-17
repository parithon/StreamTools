using System;
using System.Collections.Generic;
using System.Text;

namespace StreamTools.Chatbot
{
	public class ChatMessageEventArgs : ChatEventArgs
	{
		public bool IsWhisper { get; set; }
		public bool IsOwner { get; set; }
		public bool IsModerator { get; set; }
		public string Message { get; set; }
	}
}
