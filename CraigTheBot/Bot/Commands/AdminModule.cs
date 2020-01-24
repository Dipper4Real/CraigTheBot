using CraigTheBot.Bot;
using CraigTheBot.Bot.Commands.Preconditions;
using CraigTheBot.Bot.Database;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CraigTheBot.Bot.Commands
{
    [RequiresBotMaintenance]
    public class MaintenanceModule : ModuleBase
    {
        [Command("ExecCommand")]
        public async Task ExecCommand(string command)
        {
            await Craig.Instance.ExecuteCommand(command, Context.Channel);
        }

        [Command("load"), Summary("Loading things of the database")]
        public async Task Load(string s)
        {
            if (s.ToLower() == "players")
            {
                var dateTimeStart = DateTime.Now;
                Craig.Instance.LoadAllPlayers();
                var dateTimeEnd = DateTime.Now;

                var timeSpan = dateTimeEnd - dateTimeStart;
                Craig.Instance.Say($"Finished in {timeSpan.Seconds} seconds!", Context.Channel);
            }
            else
            {
                Craig.Instance.Say("I can't load this!", Context.Channel);
            }
        }

        [Command("serverinfo"), Summary("Server Information")]
        public async Task ServerInfo(ulong id)
        {
            var craig = Craig.Instance;
            var guild = craig.Client.GetGuild(id);

            var channelList = guild.Channels;
            var userList = guild.Users;
            string channels = "\n";
            string users = "\n";

            foreach (var channel in channelList)
            {
                channels += $"{channel.Name} : {channel.Id}\n";
            }

            foreach (var user in userList)
            {
                users += $"{user.Username} : {user.Id}\n";
            }

            await Context.Message.Author.SendMessageAsync($"Server Info:\n" +
                $"```Name: {guild.Name}\n" +
                $"ID: {guild.Id}\n" +
                $"UserCount: {guild.MemberCount}\n\n" +
                $"Channels: {channels}\n" +
                $"Users: {users}```");
        }

        [Command("shutdown"), Summary("Shutting down Craig.")]
        public async Task Shutdown()
        {
            var craig = Craig.Instance;

            craig.Say("Shutting down...", craig.BotLogChannel);

            Thread.Sleep(1000);

            Environment.Exit(0);
        }

        [Command("update"), Summary("Update Database Infos.")]
        public async Task Update()
        {
            var craig = Craig.Instance;
            craig.Say("This can take some time.", Context.Channel);

            var guilds = craig.Client.Guilds;
            var database = DBConnector.Instance;

            foreach (var guild in guilds)
            {
                string serverString = null;
                try
                {
                    serverString = database.GetDBData($"SELECT ServerID FROM Servers WHERE ServerID = {guild.Id}")[0];
                }
                catch (Exception)
                {
                    serverString = null;
                }

                if (serverString == null)
                    database.ExecuteCommand($"INSERT INTO Servers (ServerID, ServerName, WelcomeChannel) VALUES ({guild.Id}, \"{guild.Name}\", {guild.DefaultChannel.Id})");

                try
                {
                    serverString = database.GetDBData($"SELECT ServerID FROM ServerSettings WHERE ServerID = {guild.Id}")[0];
                }
                catch (Exception)
                {
                    serverString = null;
                }

                if (serverString == null)
                    database.ExecuteCommand($"INSERT INTO ServerSettings (ServerID) VALUES ({guild.Id})");

                try
                {
                    serverString = database.GetDBData($"SELECT ServerID FROM StrikeInfo WHERE ServerID = {guild.Id}")[0];
                }
                catch (Exception)
                {
                    serverString = null;
                }

                if (serverString == null)
                    database.ExecuteCommand($"INSERT INTO StrikeInfo (ServerID) VALUES ({guild.Id})");
            }
            craig.Say("Done!", Context.Channel);
        }

        [Group("Member")]
        public class Member : ModuleBase
        {
            [Command("add")]
            public async Task Add(IGuildUser user, string group)
            {
                var craig = Craig.Instance;
                if (group.ToLower() == "debug")
                {
                    craig.DebugUser.Add(user.Id);
                    craig.Say($"Member {craig.GetUserName(user, true)} added to group \"Debug\"!", Context.Channel);
                }
                else if (group.ToLower() == "maintenance")
                {
                    craig.MaintenanceUser.Add(user.Id);
                    craig.Say($"Member {craig.GetUserName(user, true)} added to group \"Maintenance\"!", Context.Channel);
                }
                else
                {
                    craig.Say($"{group} is not a valid group!", Context.Channel);
                    return;
                }
            }

            [Command("remove")]
            public async Task Remove(IGuildUser user, string group)
            {
                var craig = Craig.Instance;
                if (group.ToLower() == "debug")
                {
                    craig.DebugUser.Remove(user.Id);
                    craig.Say($"Member {craig.GetUserName(user, true)} removed from group \"Debug\"!", Context.Channel);
                }
                else if (group.ToLower() == "maintenance")
                {
                    craig.MaintenanceUser.Remove(user.Id);
                    craig.Say($"Member {craig.GetUserName(user, true)} removed from group \"Maintenance\"!", Context.Channel);
                }
                else
                {
                    craig.Say($"{group} is not a valid group!", Context.Channel);
                    return;
                }
            }
        }
    }

    [RequireUserPermission(GuildPermission.Administrator)]
    public class AdminModule : ModuleBase
    {
        [Command("Money add")]
        public async Task AddMoney(IGuildUser user, int amount)
        {
            int money = Convert.ToInt32(DBConnector.Instance.GetDBData
                ($"SELECT Money FROM Users WHERE UserID = {user.Id} AND ServerID = {Context.Guild.Id}")[0]);

            //This will be read from the database in the future
            bool capped = false;

            if (capped)
            {
                int maxAmount = Convert.ToInt32(DBConnector.Instance.GetDBData
                ($"SELECT Money FROM Users WHERE UserID = {user.Id} AND ServerID = {Context.Guild.Id}")[0]);

                if (money + amount > maxAmount)
                {
                    amount = maxAmount - money;
                }
            }

            DBConnector.Instance.ExecuteCommand($"UPDATE Users SET Money=Money+{amount} WHERE UserID = {user.Id} AND ServerID = {Context.Guild.Id}");

            Craig.Instance.Say($"Added {amount} Craig-Coins to {Craig.Instance.GetUserName(user, true)}s' account.", Context.Message.Channel);
        }

        [Command("Money remove")]
        public async Task RemoveMoney(IGuildUser user, int amount)
        {
            int money = Convert.ToInt32(DBConnector.Instance.GetDBData
                ($"SELECT Money FROM Users WHERE UserID = {user.Id} AND ServerID = {Context.Guild.Id}")[0]);

            if (money - amount < 0)
            {
                amount = money;
            }

            DBConnector.Instance.ExecuteCommand($"UPDATE Users SET Money=Money-{amount} WHERE UserID = {user.Id} AND ServerID = {Context.Guild.Id}");

            Craig.Instance.Say($"Removed {amount} Craig-Coins from {Craig.Instance.GetUserName(user, true)}s' account. I am sorry.", Context.Message.Channel);
        }

        [Command("Money info")]
        public async Task MoneyInfo(IGuildUser user)
        {
            if (Context.Channel.GetType() == typeof(SocketDMChannel))
            {
                Craig.Instance.SendDM($"You need to execute this on a server!", Context.Message.Author);
                return;
            }

            var money = DBConnector.Instance.GetDBData($"SELECT Money FROM Users WHERE UserId = {user.Id} AND ServerID = {Context.Guild.Id}");
            string moneyCount;

            if (money == null || money.Count == 0)
            {
                moneyCount = "0";
            }
            else
            {
                moneyCount = money[0];
            }
            if (user == Context.Message.Author)
            {
                Craig.Instance.Say($"{Craig.Instance.GetUserName(Context.Message.Author, true)}, you currently have `{moneyCount}` CraigCoins.", Context.Channel);
            }
            else
            {
                Craig.Instance.Say($"{Craig.Instance.GetUserName(user, true)} currently has `{moneyCount}` CraigCoins.", Context.Channel);
            }
        }

        [Group("prefix")]
        public class CleanModule : ModuleBase
        {
            [Command("set")]
            public async Task SetPrefix(string prefix)
            {
                var database = DBConnector.Instance;

                if (database.ExecuteCommand($"UPDATE Servers SET CommandPrefix = \'{prefix}\' WHERE ServerID = {Context.Guild.Id}"))
                {
                    var craig = Craig.Instance;
                    craig.Say($"My prefix is now {prefix}", Context.Channel);
                }
            }
        }

        [Command("Permaban")]
        public async Task Ban(IGuildUser user)
        {
            await user.BanAsync();
        }

        [Command("Strike")]
        public async Task Strike(IGuildUser user)
        {
            if (user.IsBot)
            {
                Craig.Instance.Say("You cannot strike a Bot!", Context.Channel);
                return;
            }

            var database = DBConnector.Instance;
            var strikeList = database.GetDBData($"SELECT Strikes FROM Strikes WHERE UserID = {user.Id} AND ServerID = {user.Guild.Id}");

            int userStrikes;

            if (strikeList == null || strikeList.Count == 0)
            {
                userStrikes = 0;
            }
            else
            {
                userStrikes = Convert.ToInt32(strikeList[0]);
            }

            var commandList = database.GetDBData($"SELECT Penalty FROM StrikeInfo, Strikes WHERE StrikeInfo.ServerID = Strikes.ServerID AND StrikeInfo.ServerID = {user.Guild.Id} AND StrikeInfo.StrikeNumber = {userStrikes + 1}");

            string command;

            if (commandList == null || commandList.Count == 0)
            {
                command = "";
            }
            else
            {
                command = commandList[0];
            }

            if (command == null || command == "")
            {
                Craig.Instance.Say($"There is no penalty set for the {userStrikes + 1}. strike yet!", Context.Channel);
                return;
            }

            if (userStrikes == 0)
            {
                database.ExecuteCommand($"INSERT INTO Strikes (UserID, Strikes, ServerID) VALUES ({user.Id}, {userStrikes + 1}, {user.Guild.Id})");

            }
            else
            {
                database.ExecuteCommand($"UPDATE Strikes SET Strikes = {userStrikes + 1} WHERE UserID = {user.Id} AND ServerID = {user.Guild.Id}");
            }

            Craig.Instance.Say($"{user.Mention}, you've earned your {userStrikes + 1} Strike! Be prepared for how I'm punishing you!", Context.Channel);
            await Craig.Instance.ExecuteCommand(command, Context.Channel);
        }

        [Command("Strike penalty add")]
        public async Task Add(string penalty, int cooldown = 168)
        {
            var strikeNumberList = DBConnector.Instance.GetDBData($"SELECT CoolDown FROM StrikeInfo WHERE ServerID = {Context.Guild.Id}");

            var strikeNumber = strikeNumberList.Count;

            DBConnector.Instance.ExecuteCommand($"INSERT INTO StrikeInfo (ServerID, StrikeNumber, Penalty, CoolDown, StartDate) VALUES " +
                $"({Context.Guild.Id}, {strikeNumber + 1}, \"{penalty}\", {cooldown}, \"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\")");

            Craig.Instance.Say("Penalty added!", Context.Channel);
        }

        [Command("Strike remove")]
        public async Task Remove(IGuildUser user, int quantity = 1)
        {

        }

        [Command("Strike penalty remove")]
        public async Task Remove(int StrikeNumber)
        {

        }

    }
}