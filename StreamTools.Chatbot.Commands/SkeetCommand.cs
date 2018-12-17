using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StreamTools.Chatbot.Commands
{
	public class SkeetCommand : IBasicCommand
	{
		private readonly Random _random = new Random();
		const string QUOTE_FILENAME = "SkeetQuotes.txt";
		internal string[] _quotes;

		public SkeetCommand()
		{
			var assemblyPath = Assembly.GetAssembly(typeof(SkeetCommand)).Location;
			var path = $"{Path.GetDirectoryName(assemblyPath)}\\{QUOTE_FILENAME}";
			if (File.Exists(path))
			{
				_quotes = File.ReadLines(path).ToArray();
			}
		}

		public string Trigger => "skeet";

		public string Description => "Returns a random quote about Jon Skeet.";

		public TimeSpan? Cooldown => null;

		public async Task ExecuteAsync(IChatService chatService, string username, ReadOnlyMemory<char> rhs, bool IsOwner = false, bool IsModerator = false)
		{
			if (_quotes == null)
			{
				return;
			}

			await chatService.SendMessageAsync(_quotes[_random.Next(_quotes.Length)]);
		}
	}
}
