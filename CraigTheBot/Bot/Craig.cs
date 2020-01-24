using CraigTheBot.Bot.Database;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CraigTheBot.Bot
{
    internal class Craig
    {
        private static readonly object LockObject = new object();

        public string Name { get; set; }
        public ulong ClientID { get; set; }
        public string Token { get; set; }
        public List<ulong> MaintenanceUser { get; set; }
        public List<ulong> DebugUser { get; set; }

        private static Craig _instance;

        private DiscordSocketClient _client;

        private Craig()
        {
        }

        public static Craig Instance
        {
            get
            {
                lock (LockObject)
                {
                    return _instance ?? (_instance = JsonConvert.DeserializeObject<Craig>(File.ReadAllText("C:/Config.json")));
                }
            }
        }

        public async void SetStatus(string status)
        {
        }

        public async Task ExecuteCommand(string command, IMessageChannel channel)
        {
            var guild = (channel as IGuildChannel).Guild;
            var prefix = DBConnector.Instance.GetDBData($"SELECT CommandPrefix FROM Servers WHERE ServerID = {guild.Id}")[0];
            Say(prefix + command, channel);
        }

        public void LoadAllPlayers()
        {
            var database = DBConnector.Instance;

            int serverCount = Craig.Instance.Client.Guilds.Count;

            for (int i = 0; i < serverCount; i++)
            {
                var guild = new List<SocketGuild>(Craig.Instance.Client.Guilds)[i];

                var playerCount = guild.MemberCount;
                var players = new List<SocketGuildUser>(guild.Users);

                for (int j = 0; j < playerCount; j++)
                {
                    var list = database.GetDBData($"SELECT * FROM Users WHERE ServerID = {guild.Id} AND Users.UserID = {players[j].Id}");
                    if (list == null || list.Count == 0)
                    {
                        database.ExecuteCommand($"INSERT INTO Users (UserID, UserName, ServerID) VALUES ({players[j].Id}, \"{players[j].Username}\", {guild.Id})");
                    }
                }
            }
        }

        public string GetUserName(SocketGuildUser user, bool showNickName = false)
        {
            if (showNickName)
            {
                if (user.Nickname != null)
                {
                    Console.WriteLine("User has no Nickname. Returning Username");
                    return user.Nickname;
                }
            }

            return user.Username;
        }

        public string GetUserName(IGuildUser user, bool showNickName)
        {
            if (showNickName)
            {
                if (user.Nickname != null)
                {
                    Console.WriteLine("User has no Nickname. Returning Username");
                    return user.Nickname;
                }
            }

            return user.Username;
        }

        public async void SendDM(string message, IUser user)
        {
            if (message == null)
            {
                return;
            }
            await user.SendMessageAsync(message);
        }

        public async Task ConnectToVoice(SocketVoiceChannel voiceChannel)
        {
            if (voiceChannel == null)
                return;

            Console.WriteLine($"Connecting to channel {voiceChannel.Id}");
            var connection = await voiceChannel.ConnectAsync();
            Console.WriteLine($"Connected to channel {voiceChannel.Id}");
        }

        public async Task DisconnectFromVoice(SocketVoiceChannel voiceChannel)
        {
            if (voiceChannel == null)
                return;

            Console.WriteLine($"Disconnecting from channel {voiceChannel.Id}");
            await voiceChannel.DisconnectAsync();
            Console.WriteLine($"Disconnected from channel {voiceChannel.Id}");
        }

        public async void Say(string message, IMessageChannel channel)
        {
            if (message == null)
            {
                return;
            }
            await channel.SendMessageAsync(message);
        }

        public IMessageChannel BotLogChannel
        {
            get; set;
        }

        public DiscordSocketClient Client { get; set; }

        public string GetUserName(IUser user, bool showNickName = false)
        {
            return GetUserName(user as IGuildUser, showNickName);
        }
    }
}