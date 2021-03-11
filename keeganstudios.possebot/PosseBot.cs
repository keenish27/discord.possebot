using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using keeganstudios.possebot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot
{
    public class PosseBot
    {
        private DiscordSocketClient _client;
        private ConfigurationOptions _configOptions;
        private ThemeOptions _themeOptions;
        const ulong CHANNEL_MUSIC_ID = 817619794530533386;

        public async Task Run()
        {
           
            await LoadOptions(); ;
            _client = new DiscordSocketClient();
            _client.Log += Log;

            _client.UserVoiceStateUpdated += OnVoiceStateUpdated;

            await _client.LoginAsync(TokenType.Bot, _configOptions.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
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
                var theme = _themeOptions.Themes.Where(x => x.userId == user.Id).FirstOrDefault();

                if(theme != null)
                {
                    await ConnectToVoice(state2.VoiceChannel, theme);
                }
                
                Console.WriteLine($"User (Name: {user.Username} ID: {user.Id}) joined to a VoiceChannel (Name: {state2.VoiceChannel.Name} ID: {state2.VoiceChannel.Id})");
            }
            if (state1.VoiceChannel != null && state2.VoiceChannel == null)
            {
                //User left
                Console.WriteLine($"User (Name: {user.Username} ID: {user.Id}) left from a VoiceChannel (Name: {state1.VoiceChannel.Name} ID: {state1.VoiceChannel.Id})");
            }
        }

        private async Task LoadOptions()
        {
            _configOptions = await OptionsReader.ReadConfigurationOptions();
            _themeOptions = await OptionsReader.ReadThemeOptions();
        }        

        [Command("join", RunMode = RunMode.Async)]
        private Task ConnectToVoice(IVoiceChannel voiceChannel, ThemeDetails theme)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (voiceChannel == null)
                    {
                        return;
                    }

                    Console.WriteLine($"Connecting to channel {voiceChannel.Id}");
                    var audioClient = await voiceChannel.ConnectAsync();
                    Console.WriteLine($"Connected to channel {voiceChannel.Id}");

                    await Task.Delay(1000);

                    if (File.Exists(theme.AudioPath))
                    {
                        Console.WriteLine($"Sending {theme.AudioPath}");

                        await SendAsync(audioClient, theme.AudioPath);

                        Console.WriteLine($"Sent {theme.AudioPath}");
                    }                    

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private ProcessStartInfo CreatePsi(string path)
        {
            return new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            try
            {
                await client.SetSpeakingAsync(true);
                var psi = CreatePsi(path);
                // Create FFmpeg using the previous example
                using (var ffmpeg = Process.Start(psi))
                using (var output = ffmpeg.StandardOutput.BaseStream)
                using (var discord = client.CreatePCMStream(AudioApplication.Voice))
                {
                    try
                    {
                        await output.CopyToAsync(discord);
                    }
                    finally
                    {
                        await discord.FlushAsync();
                    }
                }

                await client.SetSpeakingAsync(false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        private async Task DisconnectFromVoice(SocketVoiceChannel voiceChannel)
        {
            if (voiceChannel == null)
            {
                return;
            }

            Console.WriteLine($"Disconnecting from channel {voiceChannel.Id}");
            await voiceChannel.DisconnectAsync();
            Console.WriteLine($"Disconnected from channel {voiceChannel.Id}");
        }

        private async Task SendMessage(ulong id, string message)
        {
            var channel = _client.GetChannel(id) as IMessageChannel;
            await channel.SendMessageAsync(message);
        }
    }
}
