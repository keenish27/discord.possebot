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
        Task ConnectToVoiceAndPlayTheme(IVoiceChannel voiceChannel, ThemeDetails theme);
        ProcessStartInfo CreatePsi(ThemeDetails theme);
        Task PlayAudioFile(IAudioClient audioClient, ThemeDetails theme);
        string BuildFfmegArguments(string path, int start, int duration);
        Task SendAudioAsync(IAudioClient client, ThemeDetails theme);
        Task DisconnectFromVoice(IVoiceChannel voiceChannel);
    }
}
