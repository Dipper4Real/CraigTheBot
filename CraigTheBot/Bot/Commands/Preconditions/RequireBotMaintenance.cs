using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CraigTheBot.Bot.Commands.Preconditions
{
    internal class RequiresBotMaintenance : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var craig = Craig.Instance;
            // Check if this user is a Guild User, which is the only context where roles exist
            if (context.User is SocketGuildUser gUser)
            {
                // If this command was executed by a user with the appropriate role, return a success
                if (!craig.MaintenanceUser.Contains(gUser.Id))
                {
                    return Task.FromResult(PreconditionResult.FromError("I'm sorry, you are not permitted to do this!"));
                }
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
                return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
        }
    }
}