using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Net.WebSockets;
using System.IO;

namespace BeyDiscordBot
{
	class Program
	{
		private string _botToken = "MzA3Mjk5MTI0NjM2ODExMjY0.C-vblA.Gnt6IYLu1aIspygOdyyR0acSEA0";
		private DiscordSocketClient _client;
		private CommandHandler _handler;

		static void Main(string[] args)
		=> new Program().StartAsync().GetAwaiter().GetResult();

		public async Task StartAsync()
		{
			VerifyDatabase();

			_client = new DiscordSocketClient();
			_client.Log += Log;

			_handler = new CommandHandler(_client);

			await _client.LoginAsync(TokenType.Bot, _botToken);
			await _client.StartAsync();

			await Task.Delay(-1);
		}

		private void VerifyDatabase()
		{
			if(!File.Exists("dbBot.sqlite"))
			{
				File.WriteAllBytes("dbBot.sqlite", Properties.Resources.dbBot);
			}
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
