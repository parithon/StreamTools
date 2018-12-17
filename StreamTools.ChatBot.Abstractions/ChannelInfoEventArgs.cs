using System;
using System.Collections.Generic;
using System.Text;

namespace StreamTools.Chatbot
{
	public class ChannelInfoEventArgs : ChatEventArgs
	{		
		public ChannelInfoType Type { get; set; }
	}
}
