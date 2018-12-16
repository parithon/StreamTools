using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StreamTools
{
	public interface IPluginService
	{
		string GetName();
		IServiceCollection AddService(IServiceCollection services, IConfiguration config = null);
	}
}
