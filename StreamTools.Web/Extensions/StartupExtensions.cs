﻿using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StreamTools.Composition;

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

		internal static IServiceCollection AddPluginServices(this IServiceCollection services, IConfiguration config)
		{
			Plugins = Container.GetExports<IPluginService>();

			foreach (var plugin in Plugins)
			{
				var pluginName = plugin.GetName();
				var configSection = config.GetSection(pluginName);

				services = plugin.AddService(services, configSection);
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
