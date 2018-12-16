using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace StreamTools.Web
{
	public interface IPluginConfigure
	{
		IApplicationBuilder UseConfigure(IApplicationBuilder app, IHostingEnvironment env);
	}
}
