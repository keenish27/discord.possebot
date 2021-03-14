using Discord;
using Discord.Commands;
using keeganstudios.possebot.Models;
using keeganstudios.possebot.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace keeganstudios.possebot.CommandModules
{
    public class Theme : ModuleBase<SocketCommandContext>
    {        
        private readonly IAudioService _audioService;
        private readonly IOptionsService _optionsService;
        private readonly HttpClient _httpClient;
        private readonly ICommandUtils _commandUtils;

        private string[] _acceptedAudioFileExtensions = { ".mp3" };

        public Theme(IAudioService audioService, IOptionsService optionsService, HttpClient httpClient, ICommandUtils commandUtils)
        {     
            _audioService = audioService;
            _optionsService = optionsService;
            _httpClient = httpClient;
            _commandUtils = commandUtils;
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

        [Command("announce-me", RunMode = RunMode.Async)]
        [Alias("am")]
        [Summary("Announces the user in their current voice channel using their enabled theme.")]
        public async Task ThemeAnnounceAsync()
        {
            try
            {
                var configurationOptions = await _optionsService.ReadConfigurationOptionsAsync();
                var voiceChannel = (Context.User as IVoiceState).VoiceChannel;

                if (voiceChannel == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, you must be in a voice channel!");
                    return;
                }
                var theme = await _optionsService.ReadUserThemeDetailsAsync(Context.User.Id);

                if(theme == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, you don't have a theme. Use {await _commandUtils.BuildCommand("theme-attach", true)} to learn how to set a theme.");
                    return;
                }

                if (!theme.Enabled)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, you're theme isn't enabled. Use {await _commandUtils.BuildCommand("theme-enable", true)} to learn how to enable your theme.");
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

        [Command("theme-enable", RunMode = RunMode.Async)]
        [Alias("te")]
        [Summary("Enables or Disables the user's theme.")]
        public async Task ThemeEnableAsync([Summary("true or false")] bool enabled)
        {
            try
            {
                var theme = await _optionsService.ReadUserThemeDetailsAsync(Context.User.Id);

                if (theme == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, I don't see a theme for you. Please use {await _commandUtils.BuildCommand("theme-attach", true)} to learn how to set a theme.");
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

        [Command("theme-attach", RunMode=RunMode.Async)]
        [Alias("ta")]
        [Summary("Updates a user theme with the audio file attached to the message")]
        public async Task ThemeAttachAsync([Summary("Position to start in seconds")]int start, [Summary("Length of time to play in seconds")]int duration)
        {
            try
            {
                var attachment = Context.Message.Attachments.FirstOrDefault();               
                if(attachment == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, you didn't attach a file.");
                    return;
                }

                var fileExtension = Path.GetExtension(attachment.Filename);
                if (!_acceptedAudioFileExtensions.Any(x => x == fileExtension))
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, \"{fileExtension}\" is not an accepted file type");
                    return;
                }

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "files", attachment.Filename);

                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using (var file = await _httpClient.GetStreamAsync(attachment.Url))
                using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
                {
                    await file.CopyToAsync(fileStream);
                }

                var theme = new ThemeDetails
                {
                    UserId = Context.User.Id,
                    AudioPath = filePath,
                    Start = start,
                    Duration = duration,
                    Enabled = true
                };

                await _optionsService.WriteThemeAsync(theme);
                await ReplyAsync($"Hey {Context.User.Mention}, you're theme has been updated!");

            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }

        [Command("theme-info",RunMode = RunMode.Async)]
        [Alias("ti")]
        [Summary("Provides the user with the current information for their theme.")]
        public async Task ThemeInformationAsync()
        {
            try
            {                
                var theme = await _optionsService.ReadUserThemeDetailsAsync(Context.User.Id);
                
                if(theme == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, you don't have a theme set. Please use {await _commandUtils.BuildCommand("theme-attach", true)} to learn how to set you're own theme.");
                    return;
                }

                var builder = new EmbedBuilder();
                {
                    builder.WithColor(new Color(87, 222, 127));
                    builder.WithTitle($"Hey {Context.User.Username}, here is info about your current theme.");
                    builder.AddField(x => {
                        x.Name = "Theme Info";
                        x.Value = $"**File Name**: {Path.GetFileName(theme.AudioPath)}";
                        x.Value += $"\n**Start**: {theme.Start} second(s)";
                        x.Value += $"\n**Duration**: {theme.Duration} second(s)";
                        x.Value += $"\n**Enabled**: {theme.Enabled}";
                        });
                    builder.WithFooter($"To hear your current theme use {await _commandUtils.BuildCommand("announce-me")}.");
                }

                await ReplyAsync(string.Empty, false, builder.Build());
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }
    }
}
