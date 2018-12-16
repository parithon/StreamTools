using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace StreamTools.Web.TwitchPlugin
{
	[Export(typeof(IPluginService))]
	public class Service : IPluginService
	{
		public IServiceCollection AddService(IServiceCollection services)
		{
			services.AddSingleton<ITwitchClient, TwitchClient>();

			return services;
		}
	}
}
