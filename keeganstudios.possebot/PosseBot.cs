using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DotNetTools.SharpGrabber;
using DotNetTools.SharpGrabber.YouTube;
using keeganstudios.possebot.DataAccessLayer;
using keeganstudios.possebot.Entities;
using keeganstudios.possebot.Models;
using keeganstudios.possebot.Services;
using keeganstudios.possebot.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace keeganstudios.possebot
{
    public class PosseBot
    {
        private IConfiguration _configuration;
        private IServiceProvider _services;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private LoggingService _loggingService;
        private EventHandlerService _eventHandlerService;
        private IAudioService _audioService;
        private IOptionsService _optionsService;
        private IDbUtils _dbUtils;

        public PosseBot(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Run()
        {            
            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commands = _services.GetRequiredService<CommandService>();
            _audioService = _services.GetRequiredService<IAudioService>();
            _optionsService = _services.GetRequiredService<IOptionsService>();
            _loggingService = _services.GetRequiredService<LoggingService>();
            _eventHandlerService = _services.GetRequiredService<EventHandlerService>();
            _dbUtils = _services.GetRequiredService<IDbUtils>();

            var configOptions = await _optionsService.ReadConfigurationOptionsAsync();

            await _dbUtils.EnsureDatabaseCreated();

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
            services.AddSingleton<IModelService, ModelService>();
            services.AddSingleton<IAudioService, AudioService>();
            services.AddSingleton<ICommandUtils, CommandUtils>();
            services.AddSingleton<IDbUtils, DbUtils>();
            services.AddSingleton<IFileUtils, FileUtils>();
            services.AddSingleton<IEmbedBuilderUtils, EmbedBuilderUtils>();
            services.AddSingleton<IGrabberServices, GrabberServices>();
            services.AddSingleton<IGrabber, YouTubeGrabber>();
            services.AddSingleton<IThemeDal, ThemeDal>();
            services.AddLogging(logging => logging.AddSerilog());            

            var dbPath = BuildDbPath();

            services.AddDbContext<SqliteContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            return services.BuildServiceProvider();
        }

        private string BuildDbPath()
        {
            var pbFolder = _configuration["configuration:dbFolder"];
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Path.Combine(Environment.GetFolderPath(folder), pbFolder);
            var dbPath = $"{path}{System.IO.Path.DirectorySeparatorChar}possebot.db";

            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return dbPath;
        }
    }
}
