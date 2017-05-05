using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;

namespace BeyDiscordBot
{
	internal class Program
	{
		private string _botToken = Properties.Resources.Token;
		private DiscordSocketClient _client;
		private CommandHandler _handler;

		private static void Main(string[] args)
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
			if (!File.Exists("dbBot.sqlite"))
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