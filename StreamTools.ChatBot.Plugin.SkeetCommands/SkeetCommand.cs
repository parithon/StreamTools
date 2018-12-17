using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using StreamTools.ChatBot.Abstractions;
using StreamTools.ChatBot.Composition.Abstractions;

namespace StreamTools.ChatBot.Plugin.SkeetCommands
{
	[Export(typeof(IChatBotPlugin))]
	class SkeetCommand : IBasicCommand, IChatBotPlugin
	{

	}
}
