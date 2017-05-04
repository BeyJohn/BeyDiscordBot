using System;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Discord.Commands;

namespace BeyDiscordBot
{
	public class CommandHandler
	{
		private DiscordSocketClient _client;
		private CommandService _service;

		public CommandHandler(DiscordSocketClient client)
		{
			_client = client;
			_service = new CommandService();
			
			_service.AddModulesAsync(Assembly.GetEntryAssembly());

			_client.MessageReceived += HandleCommandAsync;
		}

		private async Task HandleCommandAsync(SocketMessage s)
		{
			SocketUserMessage message = s as SocketUserMessage;
			if (message == null)
				return;

			SocketCommandContext context = new SocketCommandContext(_client, message);
			int argPos = 0;

			if(message.HasCharPrefix('^', ref argPos))
			{
				IResult result = await _service.ExecuteAsync(context, argPos);

				if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
				{

				}
			}
		}
	}
}
