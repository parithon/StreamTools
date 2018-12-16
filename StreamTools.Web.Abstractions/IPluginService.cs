using System;
using Microsoft.Extensions.DependencyInjection;

namespace StreamTools.Web
{
	public interface IPluginService
	{
		IServiceCollection AddService(IServiceCollection services);
	}
}
