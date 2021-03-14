using Discord;
using Discord.Audio;
using Discord.Commands;
using keeganstudios.possebot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Services
{
    public class AudioService : IAudioService
    {        
        private Dictionary<ulong, IAudioClient> _audioClients = new Dictionary<ulong, IAudioClient>();        

        public AudioService()
        {
     
        }

        public Task ConnectToVoiceAndPlayTheme(IVoiceChannel voiceChannel, ThemeDetails theme)
        {
            _ = Task.Run(async () =>
            {
                var maxRetry = 3;
                var retryCount = 0;
                var success = false;

                while (retryCount < maxRetry && !success)
                {
                    try
                    {
                        if(retryCount > 0)
                        {
                            await Task.Delay(20);
                        }

                        if (voiceChannel == null)
                        {
                            return;
                        }

                        _audioClients.TryGetValue(voiceChannel.Id, out var audioClient);

                        if (audioClient == null || audioClient.ConnectionState == ConnectionState.Disconnected)
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
                            await DisconnectFromVoice(voiceChannel);
                            success = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        Console.WriteLine($"Exception: {ex.Message}");
                        Console.Error.WriteLine($"- {ex.StackTrace}");                        
                    }                    
                }
            });
            return Task.CompletedTask;
        }

        public ProcessStartInfo CreatePsi(ThemeDetails theme)
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

        public string BuildFfmegArguments(string path, int start, int duration)
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
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
            return args.ToString();
        }

        public async Task SendAudioAsync(IAudioClient client, ThemeDetails theme)
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

        public Task DisconnectFromVoice(IVoiceChannel voiceChannel)
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
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine($"- {ex.StackTrace}");
                }
            });
            return Task.CompletedTask;
        }
    }
}
