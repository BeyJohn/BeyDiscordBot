using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;
using System.Data;
using System.Data.SQLite;

namespace BeyDiscordBot
{
	internal class Program
	{

        public static SQLiteConnection _con = new SQLiteConnection($"Data Source = dbBot.sqlite;Version=3;PRAGMA foreign_keys = ON");
        private string _botToken = Properties.Resources.Token;
		private DiscordSocketClient _client;
		private CommandHandler _handler;
        public static bool IsExit = false;

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

            while(!IsExit)
			    await Task.Delay(1000);

            await _client.LogoutAsync();
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

        public static async Task<DataSet> FillDataSet(string sql, string tableName)
        {
            DataSet ds = new DataSet();

            await Task.Run(() =>
            {
                try
                {
                    SQLiteDataAdapter da = new SQLiteDataAdapter(sql, _con);
                    da.Fill(ds, tableName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally { _con.Close(); }
            });
            return ds;
        }

        public static async Task<bool> ExecuteCommand(params SQLiteCommand[] commands)
        {
            bool success = false;
            await Task.Run(() =>
            {
                try
                {
                    _con.Open();
                    foreach (SQLiteCommand command in commands)
                    {
                        command.Connection = _con;
                        command.Prepare();
                        command.ExecuteNonQuery();
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally { _con.Close(); }
            });
            return success;
        }

    }
}