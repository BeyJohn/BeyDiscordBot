using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyDiscordBot.Modules
{
	public class Test : ModuleBase<SocketCommandContext>
	{
		private SQLiteConnection _con = new SQLiteConnection($"Data Source = dbBot.sqlite;Version=3;PRAGMA foreign_keys = ON");

		[Command("help"), Summary("I will tell you about all of my commands.")]
		public async Task Help()
		{
			await Context.Channel.SendMessageAsync("help  - I will tell you about all of my commands.\nroll amount sides - I will roll dice for you.\nscience - This is BeyJohn's explanation for pretty much anything.\nlistReminders - I will list the reminders I have set.\nremind key - I will remind you of something you told me before.\naddReminder key message - I will remember something for you.\nremoveReminder key -I will forget some");
		}

		[Command("roll"), Summary("I will roll dice for you.")]
		public async Task Roll(int amount, int sides)
		{
			if (amount > 0 && sides > 1)
			{
				Random r = new Random();
				int[] d = new int[amount];
				for (int i = 0; i < d.Length; i++)
					d[i] = r.Next(sides) + 1;

				await Context.Channel.SendMessageAsync(string.Join(", ", d) + "\nTotal roll: " + d.Sum());
			}
		}

		[Command("science"), Summary("This is BeyJohn's explanation for pretty much anything.")]
		public async Task Science()
		{
			Bitmap b = Properties.Resources.science;
			Stream s = new MemoryStream();

			b.Save(s, ImageFormat.Jpeg);
			s.Seek(0, SeekOrigin.Begin);

			await Context.Channel.SendFileAsync(s, "science.jpg");
		}

		[Command("listReminders"), Summary("I will list the reminders I have set.")]
		public async Task ListReminders()
		{
			DataSet ds = await FillDataSet($"SELECT * FROM Reminders", "Reminders");
			if (ds.Tables[0].Rows.Count > 0)
			{
				await Context.Channel.SendMessageAsync(string.Join(", ", ds.Tables[0].Rows.OfType<DataRow>().Select(r => r[0].ToString().Replace("###", "'"))));
			}
		}

		[Command("remind"), Summary("I will remind you of something you told me before.")]
		public async Task Remind(string key)
		{
			DataSet ds = await FillDataSet($"SELECT * FROM Reminders WHERE key = '{key.Replace("'", "###")}'", "Reminders");
			if (ds.Tables[0].Rows.Count > 0)
			{
				DataRow dr = ds.Tables[0].Rows[0];
				await Context.Channel.SendMessageAsync(dr["message"].ToString().Replace("###", "'"));
			}
		}

		[Command("addReminder"), Summary("I will remember something for you.")]
		public async Task AddReminder(string key, string message)
		{
			DataSet ds = await FillDataSet($"SELECT * FROM Reminders WHERE key = '{key.Replace("'", "###")}'", "Reminders");
			if (ds.Tables[0].Rows.Count > 0)
			{
				SQLiteCommand cmd = _con.CreateCommand();
				cmd.CommandText = "UPDATE Reminders SET [message] = @message WHERE [key] = @key;";
				cmd.Parameters.AddWithValue("@message", message.Replace("'", "###"));
				cmd.Parameters.AddWithValue("@key", key.Replace("'", "###"));

				await ExecuteCommand(cmd);
			}
			else
			{
				SQLiteCommand cmd = _con.CreateCommand();
				cmd.CommandText = $"INSERT INTO Reminders([key],[message])Values('{key.Replace("'", "###")}','{message.Replace("###", "'")}')";
				await ExecuteCommand(cmd);
			}
		}

		[Command("removeReminder"), Summary("I will forget something I was told.")]
		public async Task RemoveReminder(string key)
		{
			SQLiteCommand cmd = _con.CreateCommand();

			cmd.CommandText = "DELETE FROM Reminders WHERE [key] = @key";
			cmd.Parameters.AddWithValue("@key", key.Replace("'", "###"));

			await ExecuteCommand(cmd);
		}

		public async Task<DataSet> FillDataSet(string sql, string tableName)
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

		private async Task<bool> ExecuteCommand(params SQLiteCommand[] commands)
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