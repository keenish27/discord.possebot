using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using keeganstudios.possebot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace keeganstudios.possebot
{
    public class PosseBot
    {
        private DiscordSocketClient _client;
        private ConfigurationOptions _configOptions;
        private ThemeOptions _themeOptions;
        private Dictionary<ulong, IAudioClient> _audioClients = new Dictionary<ulong, IAudioClient>();

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
                var theme = _themeOptions.Themes.Where(x => x.UserId == user.Id).FirstOrDefault();

                if(theme != null)
                {                    
                    await ConnectToVoiceAndPlayTheme(state2.VoiceChannel, theme);                    
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

        [Command("jp", RunMode = RunMode.Async)]
        private Task ConnectToVoiceAndPlayTheme(IVoiceChannel voiceChannel, ThemeDetails theme)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (voiceChannel == null)
                    {
                        return;
                    }

                    _audioClients.TryGetValue(voiceChannel.Id, out var audioClient);                    

                    if(audioClient == null || audioClient.ConnectionState == ConnectionState.Disconnected)
                    {
                        Console.WriteLine($"Connecting to channel {voiceChannel.Id}");

                        audioClient = await voiceChannel.ConnectAsync();

                        if (!_audioClients.ContainsKey(voiceChannel.Id))
                        {
                            Console.WriteLine($"Adding audio client {voiceChannel.Id}");
                            _audioClients.Add(voiceChannel.Id, audioClient);
                            Console.WriteLine($"Added audio client {voiceChannel.Id}");
                        }

                        Console.WriteLine($"Connected to channel {voiceChannel.Id}");

                        await Task.Delay(1000);
                        await PlayAudioFile(audioClient, theme);                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }

                await DisconnectFromVoice(voiceChannel);
            });
            return Task.CompletedTask;
        }

        private ProcessStartInfo CreatePsi(ThemeDetails theme)
        {
            var args = BuildFfmegArguments(theme.AudioPath, theme.Start, theme.Duration);
            Console.WriteLine(args);
            return new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
        }

        public async Task PlayAudioFile(IAudioClient audioClient, ThemeDetails theme)
        {
            if (File.Exists(theme.AudioPath))
            {
                Console.WriteLine($"Sending {theme.AudioPath}");

                await SendAudioAsync(audioClient, theme);

                Console.WriteLine($"Sent {theme.AudioPath}");
            }
        }

        private string BuildFfmegArguments(string path, int start, int duration)
        {
            var args = new StringBuilder();
            try
            {
                args.Append($"-hide_banner -loglevel panic -i \"{path}\"");

                if (start > 0)
                {
                    args.Append($" -ss {start}");
                }

                if (duration < 5)
                {
                    duration = 15;
                }

                args.Append($" -t {duration} -ac 2 -f s16le -ar 48000 pipe:1");
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
            return args.ToString();
        }

        private async Task SendAudioAsync(IAudioClient client, ThemeDetails theme)
        {
            try
            {                
                await client.SetSpeakingAsync(true);
                var psi = CreatePsi(theme);
                // Create FFmpeg using the previous example
                using (var ffmpeg = Process.Start(psi))
                using (var output = ffmpeg.StandardOutput.BaseStream)
                using (var discord = client.CreatePCMStream(AudioApplication.Music))
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
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }

        [Command("disconnect", RunMode =RunMode.Async)]
        private Task DisconnectFromVoice(IVoiceChannel voiceChannel)
        {
            _ = Task.Run(async () =>
                {
                    try
                    {
                        if (voiceChannel == null)
                        {
                            return;
                        }

                        Console.WriteLine($"Disconnecting from channel {voiceChannel.Id}");
                        await voiceChannel.DisconnectAsync();
                        Console.WriteLine($"Disconnected from channel {voiceChannel.Id}");
                    }
                    catch(Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                        Console.Error.WriteLine($"- {ex.StackTrace}");
                    }
                });
            return Task.CompletedTask;
        }

        private async Task SendMessage(ulong id, string message)
        {
            var channel = _client.GetChannel(id) as IMessageChannel;
            await channel.SendMessageAsync(message);
        }
    }
}
