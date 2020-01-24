using CraigTheBot.Bot.Commands.Preconditions;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraigTheBot.Bot.Commands
{
    [DebugOnly]
    public class DebugModule : ModuleBase
    {
        [Command("Test"), Summary("It's a test!")]
        public async Task Test()
        {
            var craig = Craig.Instance;

            craig.Say("Test passed!", Context.Channel);
        }
    }
}
