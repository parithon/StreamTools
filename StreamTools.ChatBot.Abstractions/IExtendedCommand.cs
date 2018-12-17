using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StreamTools.Chatbot
{
	public interface IExtendedCommand
	{
		string Name { get; }
		string Description { get; }

		/// <summary>
		/// CanExecute order, higher the later
		/// </summary>
		int Order { get; }

		/// <summary>
		/// If true, don't run other commands after this one
		/// </summary>
		bool Handled { get; }

		/// <summary>
		/// Cooldown for this command, or null
		/// </summary>
		TimeSpan? Cooldown { get; }

		bool CanExecute(string username, string fullCommandText);

		Task Execute(IChatService chatService, string username, string fullCommandText);
	}
}
