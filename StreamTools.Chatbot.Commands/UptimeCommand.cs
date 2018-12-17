using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Logging;
using StreamTools.Core;

namespace StreamTools.Chatbot.Commands
{
	public class UptimeCommand : IBasicCommand
	{
		private readonly ILogger<UptimeCommand> _logger;

		public UptimeCommand(ILogger<UptimeCommand> logger)
		{
			_logger = logger;
		}

		public string Trigger => "uptime";

		public string Description => "Report how long the stream has been on the air.";

		public TimeSpan? Cooldown => TimeSpan.FromMinutes(1);

		public async Task ExecuteAsync(IChatService chatService, string username, ReadOnlyMemory<char> rhs, bool IsOwner = false, bool IsModerator = false)
		{
			if (!(chatService is IStreamService svc))
			{
				_logger.LogInformation($"The current chat service is not a stream service. Aborting.");
				return;
			}

			var uptime = await svc.Uptime();
			var message = uptime.HasValue ? $"The stream has been up for {uptime.Value.Humanize()}." : "Stream is offline.";
			await chatService.SendMessageAsync(message);
		}
	}
}
