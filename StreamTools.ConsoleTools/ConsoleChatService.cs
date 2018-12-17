using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NodaTime;
using StreamTools.Chatbot;
using StreamTools.Core;

namespace StreamTools.ConsoleTools
{
	internal class ConsoleChatService : IChatService, IStreamService
	{
		private readonly IConsole _console;
		private readonly Instant _started;

		public ConsoleChatService(IConsole console)
		{
			_console = console;
			_started = SystemClock.Instance.GetCurrentInstant();
		}

		public bool IsOwner { get; private set; }
		public bool IsModerator { get; private set; }

		public void SetModerator(bool value = true)
		{			
			IsModerator = value;
		}

		public void SetOwner(bool value = true)
		{
			IsOwner = value;
		}

		public void ConsoleMessageReceived(string message)
		{
			ChatMessage?.Invoke(this, new ChatMessageEventArgs()
			{
				ServiceName = "Console",
				Message = message,
				Username = "ConsoleUser",
				IsOwner = IsOwner,
				IsModerator = IsModerator
			});
		}

		private void WriteLine(string message)
		{
			_console.ForegroundColor = ConsoleColor.DarkYellow;
			_console.WriteLine($"{message}");
			_console.ResetColor();
		}

		public string Name => "Console";

		public bool IsAuthenticated => true;

		string IStreamService.Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int CurrentFollowerCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int CurrentViewCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public event EventHandler<ChatMessageEventArgs> ChatMessage;
		public event EventHandler<ChannelInfoEventArgs> ChannelMessage;
		public event EventHandler<ServiceUpdatedEventArgs> Updated;

		public Task<bool> BanUserAsync(string username)
		{
			throw new NotImplementedException();
		}

		public Task<bool> SendMessageAsync(string message)
		{
			WriteLine(message);
			return Task.FromResult(true);
		}

		public Task<bool> SendWhisperAsync(string username, string message)
		{
			WriteLine($"<<{username}>> {message}");
			return Task.FromResult(true);
		}

		public Task<bool> TimeoutUserAsync(string username, TimeSpan time)
		{
			throw new NotImplementedException();
		}

		public Task<bool> UnbanUserAsync(string username)
		{
			throw new NotImplementedException();
		}

		public ValueTask<TimeSpan?> Uptime()
		{
			TimeSpan? ts = SystemClock.Instance.GetCurrentInstant().Minus(_started).ToTimeSpan();
			return new ValueTask<TimeSpan?>(Task.FromResult(ts));
		}
	}
}
