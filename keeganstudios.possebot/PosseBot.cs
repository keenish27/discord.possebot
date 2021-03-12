using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using keeganstudios.possebot.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot
{
    public class PosseBot
    {
        private IServiceProvider _services;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IAudioService _audioService;
        private IOptionsReader _optionsReader;
        private Dictionary<ulong, IAudioClient> _audioClients = new Dictionary<ulong, IAudioClient>();

        public async Task Run()
        {            
            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commands = _services.GetRequiredService<CommandService>();
            _audioService = _services.GetRequiredService<IAudioService>();
            _optionsReader = _services.GetRequiredService<IOptionsReader>();            

            _client.Log += Log;
            _client.UserVoiceStateUpdated += OnVoiceStateUpdated;

            var configOptions = await _optionsReader.ReadConfigurationOptions();

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
            services.AddSingleton<IOptionsReader, OptionsReader>();
            services.AddSingleton<IAudioService, AudioService>();
            
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
                Console.WriteLine($"User (Name: {user.Username} ID: {user.Id}) joined to a VoiceChannel (Name: {state2.VoiceChannel.Name} ID: {state2.VoiceChannel.Id})");

                var theme = await _optionsReader.ReadUserThemeDetails(user.Id);

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
