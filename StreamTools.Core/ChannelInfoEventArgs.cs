using System;
using System.Collections.Generic;
using System.Text;

namespace StreamTools.Core
{
	public class ChannelInfoEventArgs : EventArgs
	{		
		public ChannelInfoType Type { get; set; }
	}
}
