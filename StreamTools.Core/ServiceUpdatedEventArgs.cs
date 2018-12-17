using System;

namespace StreamTools.Core
{
	public class ServiceUpdatedEventArgs : EventArgs
	{
		public string ServiceName { get; set; }
		public uint ChannelId { get; set; }
		public int? NewFollowers { get; set; }
		public int? NewViewers { get; set; }
		public bool? IsOnline { get; set; }
	}
}
