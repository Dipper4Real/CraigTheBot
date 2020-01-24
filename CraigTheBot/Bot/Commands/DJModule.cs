using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using CraigTheBot.Bot.Commands.Preconditions;

namespace CraigTheBot.Bot.Commands
{
    public class DJModule : ModuleBase
    {
        SocketVoiceChannel voiceChannel;

        [DebugOnly]
        [Command("join")]
        public async Task JoinVoice(ulong id)
        {
            var channel = Craig.Instance.Client.GetChannel(id);

            //Gets the guid... then the voice channel.... then the users in 
            //that voice channel
            //Can just go straight from any point depending on what you 
            //have access to at your part of the code.
            var user = Context.Message.Author;

            var y = Craig.Instance.Client.Guilds.FirstOrDefault(x =>
            x.Id.Equals(Context.Guild.Id))
                        .VoiceChannels.FirstOrDefault(x =>
                        x.Id.Equals(channel.Id));

            if (channel == null)
            {
                Craig.Instance.Say("Sorry, you need to be in a voice channel to do that!", Context.Channel);
                return;
            }

            await Craig.Instance.ConnectToVoice(voiceChannel);
        }

        [DebugOnly]
        [Command("leave")]
        public async Task LeaveVoice()
        {
            await Craig.Instance.DisconnectFromVoice(voiceChannel);
        }
    }
}
