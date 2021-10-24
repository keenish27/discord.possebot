using Discord;
using Discord.Audio;
using keeganstudios.possebot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Services
{
    public interface IAudioService
    {
        Task ConnectToVoiceAndPlayTheme(IVoiceChannel voiceChannel, ThemeDetail theme);
        ProcessStartInfo CreatePsi(ThemeDetail theme);
        Task PlayAudioFile(IAudioClient audioClient, ThemeDetail theme);
        string BuildFfmpegArguments(string path, int start, int duration);
        Task SendAudioAsync(IAudioClient client, ThemeDetail theme);
        Task DisconnectFromVoice(IVoiceChannel voiceChannel);
    }
}
