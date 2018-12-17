using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StreamTools.Chatbot.Commands
{
	public class EchoCommand : IBasicCommand
	{
		public string Trigger => "echo";

		public string Description => "Repeat the text that was requested by the echo command";

		public TimeSpan? Cooldown => null;

		public async Task ExecuteAsync(IChatService chatService, string username, ReadOnlyMemory<char> rhs, bool IsOwner = false, bool IsModerator = false)
		{
			if (rhs.IsEmpty)
				return;

			await chatService.SendWhisperAsync(username, $"Echo reply: {rhs}");
		}
	}
}
