using Discord;
using Discord.Commands;
using Discord.WebSocket;
using keeganstudios.possebot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.CommandModules
{
    public class Theme : ModuleBase<SocketCommandContext>
    {        
        private readonly IAudioService _audioService;
        private readonly IOptionsService _optionsService;

        public Theme(IAudioService audioService, IOptionsService optionsService)
        {     
            _audioService = audioService;
            _optionsService = optionsService;
        }

        [Command("ping")]
        [Summary("Pings the bot and he pongs you back.")]
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

        [Command("announce me", RunMode = RunMode.Async)]
        [Alias("am")]
        [Summary("Announces the user in their current voice channel using their enabled theme.")]
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
                var theme = await _optionsService.ReadUserThemeDetailsAsync(Context.User.Id);

                if(theme == null)
                {
                    await ReplyAsync($"{Context.User.Mention} doesn't have a theme.");
                    return;
                }

                if (!theme.Enabled)
                {
                    await ReplyAsync($"{Context.User.Mention} doesn't have a theme enabled.");
                    return;
                }

                await _audioService.ConnectToVoiceAndPlayTheme((Context.User as IVoiceState).VoiceChannel, theme);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }

        [Command("theme enable", RunMode = RunMode.Async)]
        [Alias("te")]
        [Summary("Enables or Disables the user's theme.")]
        public async Task ThemeEnable([Summary("true or false")] bool enabled)
        {
            try
            {
                var theme = await _optionsService.ReadUserThemeDetailsAsync(Context.User.Id);

                if (theme == null)
                {
                    await ReplyAsync($"{Context.User.Mention} doesn't have a theme");
                    return;
                }

                theme.Enabled = enabled;

                await _optionsService.WriteThemeAsync(theme);
                
                var emoji = new Emoji("👍");
                await Context.Message.AddReactionAsync(emoji);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }
    }
}
