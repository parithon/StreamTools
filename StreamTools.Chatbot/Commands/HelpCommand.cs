using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace StreamTools.Chatbot.Commands
{
	public class HelpCommand : IBasicCommand
	{
		private readonly IServiceProvider _services;
		private readonly ILogger<HelpCommand> _logger;

		public HelpCommand(IServiceProvider services, ILogger<HelpCommand> logger)
		{
			_services = services;
			_logger = logger;
		}

		public string Trigger => "help";

		public string Description => "Get information about the functionality available to the chatbot";

		public TimeSpan? Cooldown => null;

		public async Task ExecuteAsync(IChatService chatService, string username, ReadOnlyMemory<char> rhs, bool IsOwner = false, bool IsModerator = false)
		{
			var commands = _services.GetServices<IBasicCommand>();

			if (rhs.IsEmpty)
			{
				var availableCommands = string.Join(" ", commands.Where(command => !string.IsNullOrEmpty(command.Trigger)).Select(command => $"!{command.Trigger.ToLower()}"));
				await chatService.SendMessageAsync($"Supported commands: {availableCommands}");
				return;
			}

			var cmd = commands.FirstOrDefault(command => rhs.Span.Equals(command.Trigger.AsSpan(), StringComparison.OrdinalIgnoreCase));
			if (cmd == null)
			{
				await chatService.SendMessageAsync($"Unknown command to provide help for.");
				return;
			}

			await chatService.SendMessageAsync($"{rhs}: {cmd.Description}");
		}
	}
}
