using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraigTheBot.Bot.Commands.Preconditions
{
    class DebugOnly : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var craig = Craig.Instance;
            // Check if this user is a Guild User, which is the only context where roles exist
            if (context.User is SocketGuildUser gUser)
            {
                // If this command was executed by a user with the appropriate role, return a success
                if (craig.MaintenanceUser.Contains(gUser.Id) || craig.DebugUser.Contains(gUser.Id))
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
            }
            return Task.FromResult(PreconditionResult.FromError("Unknown command."));
        }
    }
}
