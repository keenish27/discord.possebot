using Discord;
using Discord.Audio;
using keeganstudios.possebot.Models;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AudioService> _logger;
        private Dictionary<ulong, AudioClientInfo> _audioClients = new Dictionary<ulong, AudioClientInfo>();        

        public AudioService(ILogger<AudioService> logger)
        {
            _logger = logger;
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

                        _audioClients.TryGetValue(voiceChannel.Guild.Id, out var audioClientInfo);                        

                        if (audioClientInfo == null || !audioClientInfo.IsPlaying)
                        {                            
                            _logger.LogInformation("Connecting to voice channel {voiceChannelId} in Guild Id: {guildId}", voiceChannel.Id, voiceChannel.Guild.Id);

                            var audioClient = await voiceChannel.ConnectAsync();

                            if (!_audioClients.ContainsKey(voiceChannel.Guild.Id))
                            {
                                audioClientInfo = new AudioClientInfo { AudioClient = audioClient, IsPlaying = true };
                                _logger.LogInformation("Adding audio client {guildId}", voiceChannel.Guild.Id);
                                _audioClients.Add(voiceChannel.Guild.Id, audioClientInfo);                                
                            }

                            _logger.LogInformation("Connected to voice channel {voiceChannelId}", voiceChannel.Id);

                            await Task.Delay(1000);
                            await PlayAudioFile(audioClient, theme);                            
                            await DisconnectFromVoice(voiceChannel);
                            audioClientInfo.IsPlaying = false;
                            success = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        _logger.LogError(ex, "Unable to connect to voice and play theme: {@theme} on voice channel id (retry: {retryCount}): {voiceChannelId}", theme, retryCount, voiceChannel.Id);
                    }
                }
            });
            return Task.CompletedTask;
        }

        public ProcessStartInfo CreatePsi(ThemeDetails theme)
        {
            _logger.LogInformation("Building process start info for theme: {@theme}", theme);
            var args = BuildFfmpegArguments(theme.AudioPath, theme.Start, theme.Duration);            
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
            try
            {
                if (File.Exists(theme.AudioPath))
                {
                    _logger.LogInformation("Audio file at path: {audioPath} exists. Starting processing.", theme.AudioPath);

                    await SendAudioAsync(audioClient, theme);                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to play audio file for theme: {@theme}", theme);
            }
        }

        public string BuildFfmpegArguments(string path, int start, int duration)
        {
            var args = new StringBuilder();
            try
            {
                _logger.LogInformation("Building ffmpeg arguments for path: {audioPath} start: {start} duration: {duration}", path, start, duration);

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
                _logger.LogError(ex, "Unable to build ffmpeg argumnets for path: {audioPath} start: {start} duration: {duration}", path, start, duration);
            }
            return args.ToString();
        }

        public async Task SendAudioAsync(IAudioClient client, ThemeDetails theme)
        {
            try
            {
                await client.SetSpeakingAsync(true);
                var psi = CreatePsi(theme);
                
                using (var ffmpeg = Process.Start(psi))
                using (var output = ffmpeg.StandardOutput.BaseStream)
                using (var discord = client.CreatePCMStream(AudioApplication.Music))
                {
                    try
                    {
                        _logger.LogInformation("Sending {audioPath} for user id: {userId} in guild id: {guildId}", theme.AudioPath, theme.UserId, theme.GuildId);
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
                _logger.LogError(ex, "Unable to send audio for theme: {@theme}", theme);
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

                   _logger.LogInformation("Disconnecting from channel {voiceChannelId}", voiceChannel.Id);
                    await voiceChannel.DisconnectAsync();                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to disconnect from voice channel id: {voiceChannelId}", voiceChannel.Id);
                }
            });
            return Task.CompletedTask;
        }
    }
}
