using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StreamTools.Chatbot;

namespace StreamTools.ConsoleTools
{
	[Command(Name = "streamtools", Description = "")]
	class Program
	{
		private readonly IConsole _console;

		static async Task<int> Main(string[] args) => await CommandLineApplication.ExecuteAsync<Program>(args);

		public Program(IConsole console)
		{
			_console = console;
		}

		private Task OnExecuteAsync()
		{
			ShowMenu();

			var consoleService = new ConsoleChatService(_console);
			var consoleBot = CreateChatBot(consoleService);
			consoleBot.StartAsync(new CancellationToken());

			var loop = true;
			do
			{
				var prefix = consoleService.IsOwner ? "owner" : consoleService.IsModerator ? "mod" : "";
				var result = Prompt.GetString($"{prefix}> ");
				loop = ProcessResult(consoleService, loop, result);
				Task.Delay(1);
			} while (loop);

			return Task.CompletedTask;
		}

		private static bool ProcessResult(ConsoleChatService consoleService, bool loop, string result)
		{
			if (result != null)
			{
				if (result.Equals("#owner", StringComparison.OrdinalIgnoreCase))
				{
					consoleService.SetOwner(true);
					consoleService.SetModerator(false);
				}
				else if (result.Equals("#mod", StringComparison.OrdinalIgnoreCase))
				{
					consoleService.SetOwner(false);
					consoleService.SetModerator(true);
				}
				else if (result.Equals("#user", StringComparison.OrdinalIgnoreCase))
				{
					consoleService.SetOwner(false);
					consoleService.SetModerator(false);
				}
				else if (result.Equals("exit", StringComparison.OrdinalIgnoreCase))
				{
					loop = false;
				}
				consoleService.ConsoleMessageReceived(result);
			}

			return loop;
		}

		private ChatBot CreateChatBot(IChatService chatService)
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", true)
				.AddUserSecrets<Program>()
				.Build();

			var services = ChatBot.RegisterCommands(new ServiceCollection(), config)
				.AddLogging(configure =>
				{
					configure.AddConfiguration(config.GetSection("Logging"));
					configure.AddConsole(c => c.IncludeScopes = true);
					configure.AddDebug();
				})
				.AddSingleton(chatService)
				.BuildServiceProvider();

			var options = services.GetRequiredService<IOptionsSnapshot<ChatBotSettings>>();
			var loggerFactory = services.GetRequiredService<ILoggerFactory>();

			return new ChatBot(services, options, loggerFactory.CreateLogger<ChatBot>());
		}

		private void ShowMenu()
		{
			_console.WriteLine(StreamToolsAscii.CalvinS);
			_console.WriteLine($"Version: {Assembly.GetAssembly(typeof(Program)).GetName().Version}");
			_console.WriteLine();
			_console.WriteLine("StreamTools interactive console for testing.");
			_console.WriteLine("Elevate commands: #owner = owner, #mod = moderator, #user = user");
			_console.WriteLine("Press CTRL+C or type 'exit' to exit at any time.");
			_console.WriteLine();
		}
	}
}
