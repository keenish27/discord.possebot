using Discord;
using Discord.Commands;
using DotNetTools.SharpGrabber.Internal.Grabbers;
using keeganstudios.possebot.Services;
using keeganstudios.possebot.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.CommandModules
{
    public class Theme : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<Theme> _logger;
        private readonly IAudioService _audioService;
        private readonly IOptionsService _optionsService;        
        private readonly ICommandUtils _commandUtils;
        private readonly IFileUtils _fileUtils;
        private readonly IEmbedBuilderUtils _embedBuilderUtils;
        private readonly YouTubeGrabber _grabber;

        private string[] _acceptedAudioFileExtensions = { ".mp3", ".m4a" };

        public Theme(ILogger<Theme> logger, IAudioService audioService, IOptionsService optionsService, ICommandUtils commandUtils, IFileUtils fileUtils,IEmbedBuilderUtils embedBuilderUtils,  YouTubeGrabber grabber)
        {
            _logger = logger;
            _audioService = audioService;
            _optionsService = optionsService;            
            _commandUtils = commandUtils;
            _fileUtils = fileUtils;
            _embedBuilderUtils = embedBuilderUtils;
            _grabber = grabber;
        }               

        [Command("announce-me", RunMode = RunMode.Async)]
        [Alias("announce me", "am")]
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

                var emoji = new Emoji("🎺");
                await Context.Message.AddReactionAsync(emoji);

                await _audioService.ConnectToVoiceAndPlayTheme((Context.User as IVoiceState).VoiceChannel, theme);
            }
            catch(Exception ex)
            {
                await ReplyAsync($"Hey {Context.User.Mention}, I ran into a problem and couldn't announce you 😢.");
                _logger.LogError(ex, "Unable to announce theme for user: userId} in guild: guildId}", Context.User.Id, Context.Guild.Id);
            }
        }

        [Command("theme-enable", RunMode = RunMode.Async)]
        [Alias("theme enable", "te")]
        [Summary("Enables or Disables your theme.")]
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
                _logger.LogError(ex, "Unable to enabe/disable theme for user: {userId} in guild: {guildId}", Context.User.Id, Context.Guild.Id);
            }
        }

        [Command("theme-attach", RunMode=RunMode.Async)]
        [Alias("theme attach", "ta")]
        [Summary("Updates a user theme with the audio file attached to the message")]
        public async Task ThemeAttachAsync([Summary("Position to start in seconds")]int start, [Summary("Length of time to play in seconds")]int duration)
        {
            try
            {
                var attachment = Context.Message.Attachments.FirstOrDefault();               
                if(attachment == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, you didn't attach a file.");
                    _logger.LogInformation("User: (Name: {userName} Id: {userId}) didn't attach a file.", Context.User.Username, Context.User.Id);
                    return;
                }

                var fileExtension = Path.GetExtension(attachment.Filename);
                if (!_acceptedAudioFileExtensions.Any(x => x == fileExtension))
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, \"{fileExtension}\" is not an accepted file type. I can accept {string.Join(", " ,_acceptedAudioFileExtensions)} files.");
                    _logger.LogInformation("User: (Name: {userName} Id: {userId}) tried to attach file type [{fileExtension}] isn't isn't supported.", Context.User.Username, Context.User.Id, fileExtension);
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
                _logger.LogError(ex, $"Unable to attach theme for user: {Context.User.Id} in guild: {Context.Guild.Id}");
            }
        }

        [Command("theme-grab", RunMode = RunMode.Async)]
        [Alias("theme grab", "tg")]
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
                    _logger.LogInformation("Unable to find any usable audio streams for [{title}] at url: {url}", result.Title, url);
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
                _logger.LogError(ex, "Unable to grab theme for user: {userId} in guild: {guildId}", Context.User.Id, Context.Guild.Id);
            }
        }

        [Command("theme-info",RunMode = RunMode.Async)]
        [Alias("theme info", "ti")]
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
                    
                    var fieldValue = $"**File Name**: {Path.GetFileName(theme.AudioPath)}";
                    fieldValue += $"\n**Start**: {theme.Start} second(s)";
                    fieldValue += $"\n**Duration**: {theme.Duration} second(s)";
                    fieldValue += $"\n**Enabled**: {theme.Enabled}";
                    
                    builder.AddField(_embedBuilderUtils.BuildEmbedField("Theme Info", fieldValue, false));
                    builder.WithFooter($"To hear your current theme use {await _commandUtils.BuildCommandAsyc("announce-me")}.");
                }

                await ReplyAsync(string.Empty, false, builder.Build());
            }
            catch(Exception ex)
            {
                await ReplyAsync($"Hey { Context.User.Mention}, I ran into a problem and couldn't get your theme info 😢.");
                _logger.LogError(ex, "Unable to get theme info for user: {userId} in guild: {guildId}", Context.User.Id, Context.Guild.Id);
            }
        }

        [Command("announce", RunMode = RunMode.Async)]
        [Alias("a")]
        [Summary("Plays a user's theme")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ThemeAnnounceUserAsync([Summary("Username")] string userName)
        {
            try
            {
                var user = Context.Guild.Users.Where(x => x.Username.Equals(userName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (user == null)
                {
                    await ReplyAsync($"Hey, {Context.User.Mention}, I can't find user {userName} so I am unable to announce that user.");
                    return;
                }

                var configurationOptions = await _optionsService.ReadConfigurationOptionsAsync();
                var voiceChannel = (Context.User as IVoiceState).VoiceChannel;

                if (voiceChannel == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, you must be in a voice channel!");
                    return;
                }

                var theme = await _optionsService.ReadUserThemeDetailsAsync(Context.Guild.Id, user.Id);

                if (theme == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, {user.Username} doesn't have a theme. Use {await _commandUtils.BuildCommandAsync("theme-attach", true)} to learn how to set a theme.");
                    return;
                }

                if (!theme.Enabled)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, {user.Username} theme isn't enabled. Use {await _commandUtils.BuildCommandAsync("theme-enable", true)} to learn how to enable your theme.");
                    return;
                }

                var emoji = new Emoji("🎺");
                await Context.Message.AddReactionAsync(emoji);

                await _audioService.ConnectToVoiceAndPlayTheme((Context.User as IVoiceState).VoiceChannel, theme);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Hey { Context.User.Mention}, I ran into a problem and couldn't announce {userName} 😢.");
                _logger.LogError(ex, "Unable to announce theme for user: {userName} in guild: {guildId}", userName, Context.Guild.Id);
            }
        }

        [Command("theme-grab-user", RunMode = RunMode.Async)]
        [Alias("theme grab user", "tgu")]
        [Summary("Sets a theme for the given user")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ThemeGrabForUserAsync([Summary("Username")] string userName, [Summary("Youtube Url")] string url, [Summary("Position to start in seconds")] int start, [Summary("Length of time to play in seconds")] int duration)
        {
            try
            {
                var user = Context.Guild.Users.Where(x => x.Username.Equals(userName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if(user == null)
                {
                    await ReplyAsync($"Hey, {Context.User.Mention}, I can't find user {userName} so I am unable to set that user's theme.");
                    _logger.LogInformation("Unable to find user {userName} to update their theme", userName);
                    return;
                }

                var result = await _grabber.GrabAsync(new Uri(url));

                await ReplyAsync($"Hey {Context.User.Mention}, I'm going to grab {result.Title} and set that as {user.Username}'s theme. This can take a few minutes so hang tight. I'll let you know when I'm done.");

                var resourceToSave = _commandUtils.GetGrabbedMediaToSave(result.Resources);

                if (resourceToSave == null)
                {
                    await ReplyAsync($"Hey {Context.User.Mention}, I didn't find any usable audio streams for {result.Title}. Please try a different url.");
                    _logger.LogInformation("Unable to find any usable audio streams for [{title}] at url: {url}", result.Title, url);
                    return;
                }

                var fileName = _fileUtils.CleanFileName(result.Title);
                var filePath = Path.Combine(_fileUtils.BuildAudioFilePath(Context.Guild.Id), $"{fileName}.{resourceToSave.Format.Extension}");

                if (!File.Exists(filePath))
                {
                    await _fileUtils.SaveAudioFile(filePath, resourceToSave.ResourceUri.ToString());
                }

                var theme = _optionsService.CreateTheme(user.Id, Context.Guild.Id, filePath, start, duration, true);

                await _optionsService.WriteThemeAsync(theme);
                await ReplyAsync($"Hey {Context.User.Mention}, you're theme has been updated to {result.Title}! Use the {await _commandUtils.BuildCommandAsyc("announce-me")} to hear it.");
            }
            catch(Exception ex)
            {
                await ReplyAsync($"Hey { Context.User.Mention}, I ran into a problem and couldn't set the theme for {userName} 😢.");
                _logger.LogError(ex, "Unable to set theme for user: {userId} in guild: {guildId}", Context.User.Id, Context.Guild.Id);
            }
        }
    }
}
