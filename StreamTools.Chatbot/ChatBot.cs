using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using StreamTools.Chatbot.Commands;

namespace StreamTools.Chatbot
{
	public class ChatBot : IHostedService
	{
		private readonly ILogger<ChatBot> _logger;

		public ChatBot(IServiceProvider services, IOptionsSnapshot<ChatBotSettings> options, ILogger<ChatBot> logger)
		{
			_logger = logger;
			ChatServices = services.GetServices<IChatService>();
			Commands = services.GetServices<IBasicCommand>();
			ExtendedCommands = services.GetServices<IExtendedCommand>().OrderBy(service => service.Order);

			ConfigureCommandCooldown(options.Value.CooldownTime);
		}

		protected ConcurrentDictionary<string, ChatUserInfo> ActiveUsers { get; } = new ConcurrentDictionary<string, ChatUserInfo>();
		protected ConcurrentDictionary<string, Instant> CommandExecutedTimeMap = new ConcurrentDictionary<string, Instant>();
		protected IEnumerable<IChatService> ChatServices { get; }
		protected IEnumerable<IBasicCommand> Commands { get; }
		protected IOrderedEnumerable<IExtendedCommand> ExtendedCommands { get; }
		protected TimeSpan CooldownTimeSpan { get; private set; }

		private Instant GetInstant
		{
			get { return SystemClock.Instance.GetCurrentInstant(); }
		}

		protected void ConfigureCommandCooldown(TimeSpan? cooldownTimeSpan)
		{
			CooldownTimeSpan = cooldownTimeSpan ?? TimeSpan.FromSeconds(30);
			_logger.LogInformation($"Cooldown set to {CooldownTimeSpan}");
		}

		private async Task ProcessChatMessageAsync(IChatService chatService, ChatMessageEventArgs e)
		{
			var userkey = $"{e.ServiceName}:{e.UserName}";
			var user = ActiveUsers.AddOrUpdate(userkey, new ChatUserInfo(), (_, u) => u);

			if (e.Message.FirstOrDefault() == '!')
			{
				if (await HandleExtendedCommands())
					return;

				if (await HandleBasicCommands())
					return;

				await chatService.SendWhisperAsync(e.UserName, "Unknown command. Try !help for a list of available commands.");
			}

			async Task<bool> HandleBasicCommands()
			{
				var trigger = e.Message.AsMemory(1);
				var rhs = ReadOnlyMemory<char>.Empty;
				var n = trigger.Span.IndexOf(' ');
				if (n != -1)
				{
					rhs = trigger.Slice(n + 1);
					trigger = trigger.Slice(0, n);
				}

				foreach (var cmd in Commands)
				{
					if (!trigger.Span.Equals(cmd.Trigger.AsSpan(), StringComparison.OrdinalIgnoreCase) || await CommandsTooFast(cmd.Trigger, cmd.Cooldown))
						return true;

					await cmd.ExecuteAsync(chatService, e.UserName, rhs, e.IsOwner, e.IsModerator);

					AfterExecute(cmd.Trigger);
					return true;
				}

				return false;
			}

			async Task<bool> HandleExtendedCommands()
			{
				foreach (var cmd in ExtendedCommands)
				{
					if (!cmd.CanExecute(e.UserName, e.Message) || await CommandsTooFast(cmd.Name, cmd.Cooldown))
						return false;

					await cmd.Execute(chatService, e.UserName, e.Message);

					AfterExecute(cmd.Name);
					return cmd.Handled;
				}

				return false;
			}

			async Task<bool> CommandsTooFast(string commandName, TimeSpan? cooldownTimeSpan)
			{
				if (e.IsModerator || e.IsOwner)
					return false;

				if (GetInstant.Minus(user.LastCommandInstant).ToTimeSpan() < CooldownTimeSpan)
				{
					_logger.LogInformation($"Ignoring command {commandName} from {e.UserName} on {e.ServiceName}. Cooldown active.");
					await ProcessCommandsTooFastWhisperAsync(chatService, e.UserName);
					return true;
				}

				if (CommandExecutedTimeMap.TryGetValue(commandName, out var dt))
				{
					var now = GetInstant;
					if (now.Minus(dt).ToTimeSpan() < cooldownTimeSpan.GetValueOrDefault())
					{
						var remain = cooldownTimeSpan.GetValueOrDefault() - now.Minus(dt).ToTimeSpan();
						_logger.LogWarning($"Ignoring command {commandName} from {e.UserName} on {e.ServiceName}. Cooldown active for another {remain.TotalSeconds} second(s).");
						await ProcessCommandsTooFastWhisperAsync(chatService, e.UserName, remain.TotalSeconds);
						return true;
					}
				}

				return false;
			}

			// Remember the last command instant
			void AfterExecute(string commandName)
			{
				var instant = GetInstant;
				user.LastCommandInstant = instant;
				CommandExecutedTimeMap[commandName] = instant;
			}
		}

		private async Task ProcessCommandsTooFastWhisperAsync(IChatService chatService, string username, double secondsRemaining = 0)
		{
			var message = secondsRemaining > 0 ?
				$"That command is currently in a cooldown period. Please wait another {secondsRemaining} second(s) before trying again." :
				$"Your command is currently in an active cooldown period. Please wait and try again later.";

			await chatService.SendWhisperAsync(username, message);
		}

		private Task ProcessUserJoinedAsync(IChatService chatService, ChatEventArgs e)
		{
			_logger.LogTrace($"[{GetInstant}] {e.UserName} joined {e.ServiceName} chat.");

			return Task.CompletedTask;
		}

		private Task ProcessUserLeftAsync(IChatService chatService, ChatEventArgs e)
		{
			_logger.LogTrace($"[{GetInstant}] {e.UserName} has left {e.ServiceName} chat.");

			return Task.CompletedTask;
		}

		public static IServiceCollection RegisterCommands(IServiceCollection services, IConfiguration config)
		{
			services.AddHttpClient(nameof(ShoutoutCommand), configuration =>
			{
				configuration.BaseAddress = new Uri("https://api.twitch.tv/kraken/channels/");
				configuration.DefaultRequestHeaders.Add("client-id", config["StreamServices:Twitch:ClientId"]);
			});

			services.AddSingleton<IBasicCommand, PingCommand>();
			services.AddSingleton<IBasicCommand, EchoCommand>();
			services.AddSingleton<IBasicCommand, HelpCommand>();
			services.AddSingleton<IBasicCommand, ShoutoutCommand>();

			#region Load Commands from Plugins

			var pluginsPath = $"{AppDomain.CurrentDomain.BaseDirectory}/Plugins";
			var assemblies = Directory.GetFiles(pluginsPath, "*.dll", SearchOption.AllDirectories)
				.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath);

			foreach (var assembly in assemblies)
			{
				foreach (var type in assembly.GetTypes().Where(t => typeof(IBasicCommand).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass))
					services.AddSingleton(typeof(IBasicCommand), type);

				foreach (var type in assembly.GetTypes().Where(t => typeof(IExtendedCommand).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass))
					services.AddSingleton(typeof(IExtendedCommand), type);
			}

			#endregion

			return services;
		}

		#region IHostedService Implementation

		public Task StartAsync(CancellationToken cancellationToken)
		{
			foreach (var chat in ChatServices)
			{
				chat.ChatMessage += OnChat_ChatMessage;
				chat.ChannelMessage += OnChat_ChannelMessage;
			}

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			foreach (var chat in ChatServices)
			{
				chat.ChatMessage -= OnChat_ChatMessage;
				chat.ChannelMessage -= OnChat_ChannelMessage;
			}

			return Task.CompletedTask;
		}

		#endregion

		#region Events

		public async void OnChat_ChatMessage(object sender, ChatMessageEventArgs e)
		{
			try
			{
				await ProcessChatMessageAsync(sender as IChatService, e);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[{GetInstant}] An unhandled exception occured while processing the chat message.");
			}
		}

		public async void OnChat_ChannelMessage(object sender, ChannelInfoEventArgs e)
		{
			try
			{
				switch (e.Type)
				{
					case ChannelInfoType.UserJoined:
						await ProcessUserJoinedAsync(sender as IChatService, e);
						break;
					case ChannelInfoType.UserLeft:
						await ProcessUserLeftAsync(sender as IChatService, e);
						break;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[{GetInstant}] An unhandled exception occured while processing the channel message.");
			}
		}

		#endregion
	}
}
