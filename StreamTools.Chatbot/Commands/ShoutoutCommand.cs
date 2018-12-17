using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StreamTools.Chatbot.Commands
{
	public class ShoutoutCommand : IBasicCommand
	{
		private readonly ILogger<ShoutoutCommand> _logger;

		public ShoutoutCommand(IHttpClientFactory clientFactory, ILogger<ShoutoutCommand> logger)
		{
			_logger = logger;
			Client = clientFactory.CreateClient(nameof(ShoutoutCommand));
		}

		protected HttpClient Client { get; }

		public string Trigger => "so";

		public string Description => "Issue a shout out to another streamer, promoting them on stream";

		public TimeSpan? Cooldown => TimeSpan.FromSeconds(5);

		public async Task ExecuteAsync(IChatService chatService, string username, ReadOnlyMemory<char> rhs, bool owner = false, bool moderator = false)
		{
			if (!owner && !moderator)
			{
				await chatService.SendWhisperAsync(username, $"Only a moderator or the broadcaster can use this command.");
				return;
			}

			var rhsTest = rhs.ToString();
			if (rhsTest.StartsWith("@")) rhsTest = rhsTest.Substring(1);
			if (rhsTest.StartsWith("http")) return;
			if (rhsTest.Contains(" ")) return;

			rhsTest = WebUtility.UrlEncode(rhsTest);

			var result = await Client.GetAsync(rhsTest);
			if (result.StatusCode != HttpStatusCode.OK)
			{
				_logger.LogWarning("Verifying the shoutout was unsuccessful.");
				return;
			}

			await chatService.SendMessageAsync($"Please follow @{rhsTest} at: https://twitch.tv/{rhsTest}");
		}
	}
}
