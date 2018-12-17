using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StreamTools.Chatbot
{
	public interface IChatService
	{
		string Name { get; }
		bool IsAuthenticated { get; }

		event EventHandler<ChatMessageEventArgs> ChatMessage;
		event EventHandler<ChannelInfoEventArgs> ChannelMessage;

		Task<bool> SendMessageAsync(string message);
		Task<bool> SendWhisperAsync(string username, string message);
		Task<bool> TimeoutUserAsync(string username, TimeSpan time);
		Task<bool> BanUserAsync(string username);
		Task<bool> UnbanUserAsync(string username);
	}
}
