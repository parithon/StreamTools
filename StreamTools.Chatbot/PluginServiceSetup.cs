using System;
using System.Composition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StreamTools.Chatbot.Commands;
using StreamTools.Composition;

namespace StreamTools.Chatbot
{
	[Export(typeof(IPluginService))]
	public class PluginServiceSetup : IPluginService
	{
		public IServiceCollection AddService(IServiceCollection services, IConfiguration config = null)
		{
			services = ChatBot.RegisterCommands(services, config);
			return services;
		}

		public string GetName()
		{
			return "Chatbot";
		}
	}
}
