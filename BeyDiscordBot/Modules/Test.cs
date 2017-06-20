using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace BeyDiscordBot.Modules
{
	public class Test : ModuleBase<SocketCommandContext>
	{

		[Command("help"), Summary("I will tell you about all of my commands.")]
		public async Task Help()
		{
            EmbedBuilder eb = new EmbedBuilder();

            MethodInfo[] mi = GetType().GetMethods();
            for (int o = 0; o < mi.Length - 5; o++)
            {
                CommandAttribute myAttribute1 = mi[o].GetCustomAttributes(true).OfType<CommandAttribute>().FirstOrDefault();
                SummaryAttribute myAttribute2 = mi[o].GetCustomAttributes(true).OfType<SummaryAttribute>().FirstOrDefault();
                eb.AddField(myAttribute1.Text, myAttribute2.Text);
            }

            await ReplyAsync("", false, eb);
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

				await ReplyAsync(string.Join(", ", d) + "\nTotal roll: " + d.Sum());
			}
		}

		[Command("science"), Summary("This is BeyJohn's explanation for pretty much anything.")]
		public async Task Science()
		{
			Bitmap b = Properties.Resources.science;
			Stream s = new MemoryStream();

			b.Save(s, System.Drawing.Imaging.ImageFormat.Jpeg);
			s.Seek(0, SeekOrigin.Begin);

			await Context.Channel.SendFileAsync(s, "science.jpg");
		}

		[Command("listReminders"), Summary("I will list the reminders I have set.")]
		public async Task ListReminders()
		{
			DataSet ds = await Program.FillDataSet($"SELECT * FROM Reminders", "Reminders");
			if (ds.Tables[0].Rows.Count > 0)
			{
				await ReplyAsync(string.Join(", ", ds.Tables[0].Rows.OfType<DataRow>().Select(r => r[0].ToString().Replace("###", "'"))));
			}
		}

		[Command("remind"), Summary("I will remind you of something you told me before.")]
		public async Task Remind(string key)
		{
			DataSet ds = await Program.FillDataSet($"SELECT * FROM Reminders WHERE key = '{key.Replace("'", "###")}'", "Reminders");
			if (ds.Tables[0].Rows.Count > 0)
			{
				DataRow dr = ds.Tables[0].Rows[0];
				await ReplyAsync(dr["message"].ToString().Replace("###", "'"));
			}
		}

		[Command("addReminder"), Summary("I will remember something for you.")]
		public async Task AddReminder(string key, string message)
		{
			DataSet ds = await Program.FillDataSet($"SELECT * FROM Reminders WHERE key = '{key.Replace("'", "###")}'", "Reminders");
			if (ds.Tables[0].Rows.Count > 0)
			{
				SQLiteCommand cmd = Program._con.CreateCommand();
				cmd.CommandText = "UPDATE Reminders SET [message] = @message WHERE [key] = @key;";
				cmd.Parameters.AddWithValue("@message", message.Replace("'", "###"));
				cmd.Parameters.AddWithValue("@key", key.Replace("'", "###"));

				await Program.ExecuteCommand(cmd);
			}
			else
			{
				SQLiteCommand cmd = Program._con.CreateCommand();
				cmd.CommandText = $"INSERT INTO Reminders([key],[message])Values('{key.Replace("'", "###")}','{message.Replace("###", "'")}')";
				await Program.ExecuteCommand(cmd);
			}
		}

		[Command("removeReminder"), Summary("I will forget something I was told.")]
		public async Task RemoveReminder(string key)
		{
			SQLiteCommand cmd = Program._con.CreateCommand();

			cmd.CommandText = "DELETE FROM Reminders WHERE [key] = @key";
			cmd.Parameters.AddWithValue("@key", key.Replace("'", "###"));

			await Program.ExecuteCommand(cmd);
		}

        [Command("shutdown"), Summary("Only BeyJohn may use this command.")]
        public async Task Shutdown()
        {
            if (Context.User.Id == 120706726042402816)
            {
                Program.IsExit = true;
                await ReplyAsync("Shutting Down ...");
            }
            else
            {
                await ReplyAsync("You can't shut me down!");
            }
        }

	}
}