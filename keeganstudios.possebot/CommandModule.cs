using Discord;
using Discord.Commands;
using Discord.WebSocket;
using keeganstudios.possebot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {        
        private readonly IAudioService _audioService;
        private readonly IOptionsReader _optionsReader;

        public CommandModule(IAudioService audioService, IOptionsReader optionsReader)
        {     
            _audioService = audioService;
            _optionsReader = optionsReader;
        }

        [Command("ping")]
        public async Task Ping()
        {
            try
            {
                await ReplyAsync($"Pong!, {Context.User.Mention}!");
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }

        [Command("announceme", RunMode = RunMode.Async)]
        public async Task JoinAndPlay()
        {
            try
            {
                var voiceChannel = (Context.User as IVoiceState).VoiceChannel;

                if (voiceChannel == null)
                {
                    await ReplyAsync($"{Context.User.Mention} must be in a voice channel!");
                    return;
                }
                var theme = await _optionsReader.ReadUserThemeDetails(Context.User.Id);

                if(theme == null)
                {
                    await ReplyAsync($"{Context.User.Mention} doesn't have a theme");
                    return;
                }

                if (theme.Enabled)
                {
                    await _audioService.ConnectToVoiceAndPlayTheme((Context.User as IVoiceState).VoiceChannel, theme);
                }
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }
    }
}
