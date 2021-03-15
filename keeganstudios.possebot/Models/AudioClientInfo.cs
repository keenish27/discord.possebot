using Discord.Audio;

namespace keeganstudios.possebot.Models
{
    public class AudioClientInfo
    {
        public IAudioClient AudioClient { get; set; }
        public bool IsPlaying { get; set; }
    }
}
