using CraigTheBot.Bot.Commands.Preconditions;
using CraigTheBot.Bot.Database;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CraigTheBot.Bot.Commands
{
    // Create a module with no prefix
    public class InfoModule : ModuleBase
    {
        // ~say hello world -> hello world
        [Command("say")]
        [Summary("Echoes a message.")]
        public async Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
        {
            var messageID = Context.Message.Id;
            await Context.Channel.DeleteMessageAsync(messageID);
            ReplyAsync(echo);
        }
    }

    public class UserModule : ModuleBase
    {
        [Command("note")]
        public async Task Note(string whatToDo, string note)
        {
            var prefix = DBConnector.Instance.GetDBData($"SELECT CommandPrefix FROM Servers WHERE ServerID = {Context.Guild.Id}")[0];

            var craig = Craig.Instance;
            if (whatToDo.ToLower() == "add")
            {
                var allNotesList = DBConnector.Instance.GetDBData($"SELECT Note FROM Notes WHERE UserID = {Context.Message.Author.Id}");

                var context = Context;
                DBConnector.Instance.ExecuteCommand($"INSERT INTO Notes (UserID, Note, NoteID) VALUES ({(Context.Message.Author).Id}, \"{note}\", {allNotesList.Count + 1})");

                Craig.Instance.Say("Note added!", Context.Channel);
            }
            else if (whatToDo.ToLower() == "remove")
            {
                var allNotesList = DBConnector.Instance.GetDBData($"SELECT Note FROM Notes WHERE UserID = {Context.Message.Author.Id}");

                try
                {
                    var x = Convert.ToUInt64(note);
                }
                catch (Exception)
                {
                    Craig.Instance.Say($"I'm sorry, you need to enter a valid note ID.\n" +
                        $"Use `{prefix}notes` to get a List of all your personal notes along with their ID", Context.Channel);
                    return;
                }

                var noteIDCount = DBConnector.Instance.GetDBData($"SELECT NoteID FROM Notes WHERE NoteID = {note}");

                if (noteIDCount.Count > 0)
                {
                    DBConnector.Instance.ExecuteCommand($"DELETE FROM Notes WHERE NoteID = {note}");
                }
                else
                {
                    Craig.Instance.Say($"I'm sorry, there is no Note with the ID {note}\n" +
                        $"Use `{prefix}notes` to get a List of all your personal notes along with their ID", Context.Channel);
                    return;
                }




                Craig.Instance.Say("Note removed!", Context.Channel);
            }
            else
            {
                craig.Say($"I'm sorry, there is no function {whatToDo} I can work with. Try \"add\" or \"remove\"", Context.Channel);
            }


        }

        [Command("notes")]
        public async Task Notes()
        {
            string allNotes = "";

            var prefix = DBConnector.Instance.GetDBData($"SELECT CommandPrefix FROM Servers WHERE ServerID = {Context.Guild.Id}")[0];

            var allNotesList = DBConnector.Instance.GetDBData($"SELECT Note FROM Notes WHERE UserID = {Context.Message.Author.Id}");
            var allNoteIDs = DBConnector.Instance.GetDBData($"SELECT NoteID FROM Notes WHERE UserID = {Context.Message.Author.Id}");

            for (int i = 0; i < allNotesList.Count; i++)
            {
                allNotes += $"\"{allNotesList[i]}\" | ID: {allNoteIDs[i]} \n";
            }

            if (allNotes == "")
            {
                Craig.Instance.Say($"You have no notes! Create your personal one with `{prefix}note add [message]`", Context.Channel);
                return;
            }

            await Context.User.SendMessageAsync($"Your notes:\n{allNotes}");
        }

        [Command("8ball")]
        public async Task EightBall(params string[] dump)
        {
            Random ran = new Random();

            var craig = Craig.Instance;

            switch (ran.Next(1, 5))
            {
                case 1:
                    craig.Say("Of course", Context.Channel);
                    break;

                case 2:
                    craig.Say("Not in a million years!", Context.Channel);
                    break;

                case 3:
                    craig.Say("Sure, why not?", Context.Channel);
                    break;

                case 4:
                    craig.Say("Maybe, I can't say that at the moment", Context.Channel);
                    break;

                default:
                    break;
            }
        }

        [Command("Strikes")]
        public async Task StrikeInfo()
        {
            var strikes = DBConnector.Instance.GetDBData($"SELECT Strikes FROM Strikes WHERE UserId = {Context.Message.Author.Id} AND ServerID = {Context.Guild.Id}");
            string strikeCount;

            if (strikes == null || strikes.Count == 0)
            {
                strikeCount = "0";
            }
            else
            {
                strikeCount = strikes[0];
            }
            Craig.Instance.Say($"{Craig.Instance.GetUserName(Context.Message.Author, true)}, you currently have {strikeCount} strikes.", Context.Channel);
        }

        [Command("Money")]
        public async Task MoneyInfo()
        {
            if (Context.Channel.GetType() == typeof(SocketDMChannel))
            {
                Craig.Instance.SendDM($"You need to execute this on a server!", Context.Message.Author);
                return;
            }

            var money = DBConnector.Instance.GetDBData($"SELECT Money FROM Users WHERE UserId = {Context.Message.Author.Id} AND ServerID = {Context.Guild.Id}");
            string moneyCount;

            if (money == null || money.Count == 0)
            {
                moneyCount = "0";
            }
            else
            {
                moneyCount = money[0];
            }

            Craig.Instance.Say($"{Craig.Instance.GetUserName(Context.Message.Author, true)}, you currently have `{moneyCount}` CraigCoins.", Context.Channel);
        }
    }
}