using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace StreamTools.Web
{
	public static class StartupExtensions
	{
		private static CompositionHost Container { get; set; }

		internal static void LoadPlugins(this Startup startup, string contentRootPath)
		{
			var pluginsPath = Path.Combine(contentRootPath, "plugins");
			var files = Directory.GetFiles(pluginsPath, "*.dll", SearchOption.AllDirectories)
				.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath);

			var configuration = new ContainerConfiguration()
				.WithAssemblies(files);

			Container = configuration.CreateContainer();
		}

		[ImportMany]
		private static IEnumerable<IPluginService> Plugins { get; set; }

		[ImportMany]
		private static IEnumerable<IPluginConfigure> Configurations { get; set; }

		internal static IServiceCollection AddPluginServices(this IServiceCollection services)
		{
			Plugins = Container.GetExports<IPluginService>();

			foreach (var plugin in Plugins)
			{
				services = plugin.AddService(services);
			}

			return services;
		}

		internal static IApplicationBuilder UsePlugins(this IApplicationBuilder app, IHostingEnvironment env)
		{
			Configurations = Container.GetExports<IPluginConfigure>();

			foreach (var plugin in Configurations)
			{
				app = plugin.UseConfigure(app, env);
			}

			return app;
		}
	}
}
