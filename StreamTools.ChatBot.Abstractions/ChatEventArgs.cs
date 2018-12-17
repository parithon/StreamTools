using System;
using System.Collections.Generic;
using System.Text;

namespace StreamTools.Chatbot
{
	public class ChatEventArgs : EventArgs
	{
		/// <summary>
		/// The name of the service that raised the event.
		/// </summary>
		public string ServiceName { get; set; }

		/// <summary>
		/// Holds the service specific properties (e.g. AvatarUrl)
		/// </summary>
		public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

		public uint ChannelId { get; set; }
		public uint UserId { get; set; }
		public string Username { get; set; }
	}
}
