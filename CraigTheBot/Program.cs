using CraigTheBot.Bot;
using CraigTheBot.Bot.Database;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CraigTheBot
{
    internal class Program
    {
        private CommandService commands;
        private IServiceProvider services;

        private Craig craig;
        private DBConnector database = DBConnector.Instance;

        private DateTime time1;

        public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            time1 = DateTime.Now;

            craig = Craig.Instance;

            craig.Client = new DiscordSocketClient();

            commands = new CommandService();

            var token = craig.Token;

            services = new ServiceCollection()
                .BuildServiceProvider();
            //Events
            craig.Client.Log += Log;

            craig.Client.MessageUpdated += MessageUpdated;

            craig.Client.Ready += WhenReady;

            craig.Client.JoinedGuild += OnServerJoined;
            craig.Client.LeftGuild += OnServerLeft;

            craig.Client.UserJoined += OnUserJoin;
            craig.Client.UserLeft += OnUserLeave;

            craig.Client.Disconnected += OnDisconnected;

            await InstallCommands();

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await craig.Client.LoginAsync(TokenType.Bot, token);
            await craig.Client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            craig.Client.MessageReceived += MessageReceived;
            // Discover all of the commands in this assembly and load them.
            var assembly= Assembly.GetEntryAssembly();
            var x = await commands.AddModulesAsync(assembly, services);
        }

        private Task OnDisconnected(Exception arg)
        {
            return null;
        }

        private async Task OnUserJoin(SocketGuildUser user)
        {
            string welcomeChannel;

            try
            {
                welcomeChannel = database.GetDBData($"SELECT WelcomeChannel FROM Servers WHERE ServerID = {user.Guild.Id}")[0];
            }
            catch (Exception)
            {
                welcomeChannel = null;
            }

            SocketGuildChannel WelcomeChannel = null;

            if (welcomeChannel == null)
            {
                WelcomeChannel = user.Guild.DefaultChannel;
            }
            else
            {
                WelcomeChannel = user.Guild.GetChannel(Convert.ToUInt64(welcomeChannel));
            }

            var messageChannel = WelcomeChannel as IMessageChannel;

            database.ExecuteCommand($"INSERT INTO Users (UserID, UserName, ServerID) VALUES ({user.Id}, \'{user.Username}\', {user.Guild.Id})");

            craig.Say($"Welcome {craig.GetUserName(user)}!", messageChannel);
        }

        private async Task OnUserLeave(SocketGuildUser user)
        {
            string leaveChannel;

            try
            {
                leaveChannel = database.GetDBData($"SELECT WelcomeChannel FROM Servers WHERE ServerID = {user.Guild.Id}")[0];
            }
            catch (Exception)
            {
                leaveChannel = null;
            }

            SocketGuildChannel LeaveChannel = null;

            if (leaveChannel == null)
            {
                LeaveChannel = user.Guild.DefaultChannel;
            }
            else
            {
                LeaveChannel = user.Guild.GetChannel(Convert.ToUInt64(leaveChannel));
            }

            var messageChannel = LeaveChannel as IMessageChannel;

            database.ExecuteCommand($"DELETE FROM Users WHERE UserID = {user.Id} AND ServerID = {user.Guild.Id}");

            craig.Say($"That's sad. {craig.GetUserName(user, true)} left us", messageChannel);
        }

        private async Task OnServerLeft(SocketGuild server)
        {
            database.ExecuteCommand($"DELETE FROM Servers WHERE ServerID = {server.Id}");
        }

        private async Task OnServerJoined(SocketGuild server)
        {
            string roles = ConvertRoleArrayToString(server.Roles);

            database.ExecuteCommand($"INSERT INTO Servers (ServerID, ServerName, RoleIDs) VALUES ({server.Id}, \"{server.Name}\", \"{roles}\")");
        }

        private string ConvertRoleArrayToString(IReadOnlyCollection<SocketRole> collection)
        {
            string convString = "";

            string[] roles = new string[collection.Count];

            int j = 0;

            foreach (var item in collection)
            {
                roles[j] = item.Id.ToString();
                j++;
            }

            for (int i = 0; i < roles.Count(); i++)
            {
                convString += $"{roles[i]} ";
            }

            return convString;
        }

        private async Task WhenReady()
        {
            DateTime insertingPlayersStarted = DateTime.Now;
            DateTime time2 = DateTime.Now;

            var bootTime = time2 - time1;

            var channel = craig.Client.GetGuild(630417781191475214).GetChannel(646306131727220746);
            craig.BotLogChannel = channel as IMessageChannel;
            craig.Client = craig.Client;

            //craig.Say($"Booted up!\nIt took {bootTime.Seconds} seconds", botLogChannel);
        }

        public IDiscordClient Client { get => craig.Client; }

        private async Task OnServerRemoved()
        {
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            // If the message was not in the cache, downloading it will result in getting a copy of `after`.
            /*var message = await before.GetOrDownloadAsync();

            craig.Say($"{message} -> {after}", channel);*/
        }

        private async Task MessageReceived(SocketMessage messageParam)
        {
            if (messageParam.Author.IsBot)
            {
                var craig = Craig.Instance;
                if (messageParam.Author.Id != craig.ClientID)
                {
                    return;
                }
            }

            var message = messageParam as SocketUserMessage;
            SocketGuild Guild = null;
            var chnl = message.Channel as SocketGuildChannel;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            string prefix = "!!";


            if (chnl != null)
            {
                Guild = chnl.Guild;
                prefix = database.GetDBData($"SELECT CommandPrefix FROM Servers WHERE ServerID = {Guild.Id}")[0];
            }

            bool isCommand = message.HasStringPrefix(prefix, ref argPos);



            if (!message.Author.IsBot && !isCommand)
            {
                try
                {
                    var databaseInfo = database.GetDBData($"SELECT MessageMoney FROM ServerSettings WHERE ServerID = {chnl.Guild.Id}")[0];
                    bool messageMoney = false;
                    if (databaseInfo == "1")
                    {
                        messageMoney = true;
                    }

                    if (messageMoney)
                    {
                        int minMoney = Convert.ToInt32(database.GetDBData($"SELECT MinMoneyPerMessage FROM ServerSettings WHERE ServerID = {chnl.Guild.Id}")[0]);

                        int maxMoney = Convert.ToInt32(database.GetDBData($"SELECT MaxMoneyPerMessage FROM ServerSettings WHERE ServerID = {chnl.Guild.Id}")[0]);

                        int messageLength = message.Content.Length;

                        int money = messageLength * (minMoney / 100);

                        database.ExecuteCommand($"UPDATE Users SET Money=Money + {money} WHERE UserID = {messageParam.Author.Id} AND ServerID = {chnl.Guild.Id}");
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Unable to add money");
                }
            }

            //if (!messageParam.Author.IsBot)
            //{
            //    if (messageParam.Channel.GetType() != typeof(IPrivateChannel))
            //    {
            //        Random ran = new Random();

            //        if (ran.Next(1, 1500) == 666)
            //        {
            //            int money = ran.Next(1, 10);
            //            var cnl = messageParam.Channel as SocketGuildChannel;
            //            DBConnector.Instance.ExecuteCommand($"UPDATE Users SET Money={money} WHERE UserID = {messageParam.Author.Id} AND ServerID = {cnl.Guild.Id}");
            //        }
            //    }
            //}

            // Don't process the command if it was a System Message

            if (message == null) return;
            // Determine if the message is a command, based on if it starts with the Server based prefix or a mention prefix
            if (!isCommand || message.HasMentionPrefix(craig.Client.CurrentUser, ref argPos)) return;

            // Create a Command Context
            var context = new CommandContext(craig.Client, message);
            // Execute the command. (result does not indicate a return value,
            // rather an object stating if the command executed successfully)

            IResult result;

            try
            {
                result = await commands.ExecuteAsync(context, argPos, services);
            }
            catch (Exception)
            {
                return;
            }

            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        private void Log(LogMessage msg, IMessageChannel channel = null)
        {
            if (channel == null)
            {
                channel = craig.BotLogChannel;
            }

            Console.WriteLine(msg.ToString());

            craig.Say(msg.ToString(), channel);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}