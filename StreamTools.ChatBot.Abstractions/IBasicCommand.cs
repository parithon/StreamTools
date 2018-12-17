using System;
using System.Runtime;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StreamTools.Chatbot
{
	public interface IBasicCommand
	{
		/// <summary>
		/// The command keyword
		/// </summary>
		string Trigger { get; }

		/// <summary>
		/// The description used by the !help command
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Cooldown for this command, or null
		/// </summary>
		TimeSpan? Cooldown { get; }

		Task ExecuteAsync(IChatService chatService, string username, ReadOnlyMemory<char> rhs, bool IsOwner = false, bool IsModerator = false);
	}
}
