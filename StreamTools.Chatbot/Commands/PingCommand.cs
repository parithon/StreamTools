using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StreamTools.ChatBot.Abstractions;
using StreamTools.Core;

namespace StreamTools.Chatbot.Commands
{
	public class PingCommand : IBasicCommand
	{
		public string Trigger => "ping";

		public string Description => "Receive a quick acknowledgement from the bot through a whisper.";

		public TimeSpan? Cooldown => null;

		public async Task ExecuteAsync(IChatService chatService, string username, ReadOnlyMemory<char> rhs, bool IsOwner = false, bool IsModerator = false)
		{
			await chatService.SendWhisperAsync(username, "pong");
		}
	}
}
