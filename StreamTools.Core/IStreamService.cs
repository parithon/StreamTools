using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StreamTools.Core
{
	public interface IStreamService
	{
		string Name { get; set; }
		int CurrentFollowerCount { get; set; }
		int CurrentViewCount { get; set; }
		ValueTask<TimeSpan?> Uptime();

		/// <summary>
		/// Event raised when the number of Followers or Viewers changes
		/// </summary>
		event EventHandler<ServiceUpdatedEventArgs> Updated;
	}
}
