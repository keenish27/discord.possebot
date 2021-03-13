using Discord;
using Discord.Commands;
using keeganstudios.possebot.Models;
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
        private string[] _acceptedAudioFileExtensions = { ".mp3" };

        public Theme(IAudioService audioService, IOptionsService optionsService, HttpClient httpClient)
        {     
            _audioService = audioService;
            _optionsService = optionsService;
            _httpClient = httpClient;
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
        public async Task ThemeAnnounceAsync()
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
        public async Task ThemeEnableAsync([Summary("true or false")] bool enabled)
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

        [Command("theme attach", RunMode=RunMode.Async)]
        [Alias("ta")]
        [Summary("Updates a user theme with the audio file attached to the message")]
        public async Task ThemeAttachAsync([Summary("Position to start in seconds")]int start, [Summary("Length of time to play in seconds")]int duration)
        {
            try
            {
                var attachment = Context.Message.Attachments.FirstOrDefault();               
                if(attachment == null)
                {
                    await ReplyAsync($"{Context.User.Mention} no file was attached.");
                    return;
                }

                var fileExtension = Path.GetExtension(attachment.Filename);
                if (!_acceptedAudioFileExtensions.Any(x => x == fileExtension))
                {
                    await ReplyAsync($"{Context.User.Mention}, \"{fileExtension}\" is not an accepted file type");
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
                await ReplyAsync($"{Context.User.Mention}, you're theme has been updated!");

            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }
    }
}
