using Discord;
using Discord.Commands;
using DotNetTools.SharpGrabber.Internal.Grabbers;
using keeganstudios.possebot.Services;
using keeganstudios.possebot.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.CommandModules
{
    public class Theme : ModuleBase<SocketCommandContext>
    {        
        private readonly IAudioService _audioService;
        private readonly IOptionsService _optionsService;        
        private readonly ICommandUtils _commandUtils;
        private readonly IFileUtils _fileUtils;
        private readonly YouTubeGrabber _grabber;

        private string[] _acceptedAudioFileExtensions = { ".mp3", ".m4a" };

        public Theme(IAudioService audioService, IOptionsService optionsService, ICommandUtils commandUtils, IFileUtils fileUtils, YouTubeGrabber grabber)
        {     
            _audioService = audioService;
            _optionsService = optionsService;            
            _commandUtils = commandUtils;
            _fileUtils = fileUtils;
            _grabber = grabber;
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
                var theme = await _optionsService.ReadUserThemeDetailsAsync(Context.Guild.Id, Context.User.Id);

                if(theme == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, you don't have a theme. Use {await _commandUtils.BuildCommandAsync("theme-attach", true)} to learn how to set a theme.");
                    return;
                }

                if (!theme.Enabled)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, you're theme isn't enabled. Use {await _commandUtils.BuildCommandAsync("theme-enable", true)} to learn how to enable your theme.");
                    return;
                }

                await _audioService.ConnectToVoiceAndPlayTheme((Context.User as IVoiceState).VoiceChannel, theme);
            }
            catch(Exception ex)
            {
                await ReplyAsync($"Hey { Context.User.Mention}, I ran into a problem and couldn't announce you 😢.");
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
                var theme = await _optionsService.ReadUserThemeDetailsAsync(Context.Guild.Id, Context.User.Id);

                if (theme == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, I don't see a theme for you. Please use {await _commandUtils.BuildCommandAsync("theme-attach", true)} to learn how to set a theme.");
                    return;
                }

                theme.Enabled = enabled;

                await _optionsService.WriteThemeAsync(theme);
                
                var emoji = new Emoji("👍");
                await Context.Message.AddReactionAsync(emoji);
            }
            catch(Exception ex)
            {
                await ReplyAsync($"Hey { Context.User.Mention}, I ran into a problem and couldn't enable/disable your theme 😢.");
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
                    await ReplyAsync($"Hey {Context.User.Mention}, \"{fileExtension}\" is not an accepted file type. I can accept {string.Join(", " ,_acceptedAudioFileExtensions)} files.");
                    return;
                }

                var filePath = Path.Combine(_fileUtils.BuildAudioFilePath(Context.Guild.Id) , attachment.Filename);
                await _fileUtils.SaveAudioFile(filePath, attachment.Url);

                var theme = _optionsService.CreateTheme(Context.User.Id, Context.Guild.Id, filePath, start, duration, true);                

                await _optionsService.WriteThemeAsync(theme);
                await ReplyAsync($"Hey {Context.User.Mention}, you're theme has been updated!");

            }
            catch(Exception ex)
            {
                await ReplyAsync($"Hey { Context.User.Mention}, I ran into a problem and couldn't set your theme 😢.");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }

        [Command("theme-grab", RunMode = RunMode.Async)]
        [Alias("tg")]
        [Summary("Updates a user theme with audio from youtube url")]
        public async Task ThemeGrabAsync([Summary("Youtube Url")] string url, [Summary("Position to start in seconds")] int start, [Summary("Length of time to play in seconds")] int duration)
        {
            try
            {
                var result = await _grabber.GrabAsync(new Uri(url));

                await ReplyAsync($"Hey {Context.User.Mention}, I'm going to grab {result.Title} and set that as your theme. This can take a few minutes so hang tight. I'll let you know when I'm done.");

                var resourceToSave = _commandUtils.GetGrabbedMediaToSave(result.Resources);
               
                if (resourceToSave == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, I didn't find any usable audio streams for {result.Title}. Please try a different url.");
                    return;
                }

                var fileName = _fileUtils.CleanFileName(result.Title);
                var filePath = Path.Combine(_fileUtils.BuildAudioFilePath(Context.Guild.Id), $"{fileName}.{resourceToSave.Format.Extension}");

                if (!File.Exists(filePath))
                {   
                    await _fileUtils.SaveAudioFile(filePath, resourceToSave.ResourceUri.ToString());                    
                }

                var theme = _optionsService.CreateTheme(Context.User.Id, Context.Guild.Id, filePath, start, duration, true);

                await _optionsService.WriteThemeAsync(theme);
                await ReplyAsync($"Hey {Context.User.Mention}, you're theme has been updated to {result.Title}! Use the {await _commandUtils.BuildCommandAsyc("announce-me")} to hear it.");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Hey { Context.User.Mention}, I ran into a problem and couldn't set your theme 😢.");
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
                var theme = await _optionsService.ReadUserThemeDetailsAsync(Context.Guild.Id, Context.User.Id);
                
                if(theme == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, you don't have a theme set. Please use {await _commandUtils.BuildCommandAsync("theme-attach", true)} to learn how to set you're own theme.");
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
                    builder.WithFooter($"To hear your current theme use {await _commandUtils.BuildCommandAsyc("announce-me")}.");
                }

                await ReplyAsync(string.Empty, false, builder.Build());
            }
            catch(Exception ex)
            {
                await ReplyAsync($"Hey { Context.User.Mention}, I ran into a problem and couldn't get your theme info 😢.");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }
    }
}
