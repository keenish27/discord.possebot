using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DotNetTools.SharpGrabber.Internal.Grabbers;
using keeganstudios.possebot.Services;
using keeganstudios.possebot.Utils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace keeganstudios.possebot
{
    public class PosseBot
    {
        private IServiceProvider _services;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private LoggingService _loggingService;
        private EventHandlerService _eventHandlerService;
        private IAudioService _audioService;
        private IOptionsService _optionsService;     

        public async Task Run()
        {            
            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commands = _services.GetRequiredService<CommandService>();
            _audioService = _services.GetRequiredService<IAudioService>();
            _optionsService = _services.GetRequiredService<IOptionsService>();
            _loggingService = _services.GetRequiredService<LoggingService>();
            _eventHandlerService = _services.GetRequiredService<EventHandlerService>();

            var configOptions = await _optionsService.ReadConfigurationOptionsAsync();

            await _client.LoginAsync(TokenType.Bot, configOptions.Token);
            await _client.StartAsync();
            await _services.GetRequiredService<CommandHandler>().InstallCommandsAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandler>();
            services.AddSingleton<LoggingService>();
            services.AddSingleton<EventHandlerService>();
            services.AddSingleton<HttpClient>();
            services.AddSingleton<IOptionsService, OptionsService>();
            services.AddSingleton<IAudioService, AudioService>();
            services.AddSingleton<ICommandUtils, CommandUtils>();
            services.AddSingleton<IFileUtils, FileUtils>();
            services.AddSingleton<IEmbedBuilderUtils, EmbedBuilderUtils>();
            services.AddSingleton<YouTubeGrabber>();
            services.AddLogging(logging => logging.AddSerilog());

            return services.BuildServiceProvider();
        }
    }
}
