using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DotNetTools.SharpGrabber.Internal.Grabbers;
using keeganstudios.possebot.Services;
using keeganstudios.possebot.Utils;
using Microsoft.Extensions.DependencyInjection;
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
        private IAudioService _audioService;
        private IOptionsService _optionsService;        

        public async Task Run()
        {            
            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commands = _services.GetRequiredService<CommandService>();
            _audioService = _services.GetRequiredService<IAudioService>();
            _optionsService = _services.GetRequiredService<IOptionsService>();            

            _client.Log += Log;
            _client.UserVoiceStateUpdated += OnVoiceStateUpdated;

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
            services.AddSingleton<HttpClient>();
            services.AddSingleton<IOptionsService, OptionsService>();
            services.AddSingleton<IAudioService, AudioService>();
            services.AddSingleton<ICommandUtils, CommandUtils>();
            services.AddSingleton<IFileUtils, FileUtils>();
            services.AddSingleton<YouTubeGrabber>();
            
            return services.BuildServiceProvider();
        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        private async Task OnVoiceStateUpdated(SocketUser user, SocketVoiceState state1, SocketVoiceState state2)
        {
            if (user.IsBot)
            {
                return;
            }
            
            if (state1.VoiceChannel == null && state2.VoiceChannel != null)
            {
                Console.WriteLine($"User (Name: {user.Username} ID: {user.Id}) joined to a VoiceChannel (Name: {state2.VoiceChannel.Name} Id: {state2.VoiceChannel.Id}) Guild Id: {state2.VoiceChannel.Guild.Id}");

                var theme = await _optionsService.ReadUserThemeDetailsAsync(state2.VoiceChannel.Guild.Id, user.Id);

                if(theme != null && theme.Enabled)
                {
                    Console.WriteLine($"Theme found for User (Name: {user.Username} ID: {user.Id}) at path: {theme.AudioPath}");
                    await _audioService.ConnectToVoiceAndPlayTheme(state2.VoiceChannel, theme);                          
                }               
                
            }
            if (state1.VoiceChannel != null && state2.VoiceChannel == null)
            {
                //User left
                Console.WriteLine($"User (Name: {user.Username} ID: {user.Id}) left from a VoiceChannel (Name: {state1.VoiceChannel.Name} ID: {state1.VoiceChannel.Id})");
            }
        }    

        private async Task SendMessage(ulong id, string message)
        {
            var channel = _client.GetChannel(id) as IMessageChannel;
            await channel.SendMessageAsync(message);
        }
    }
}
