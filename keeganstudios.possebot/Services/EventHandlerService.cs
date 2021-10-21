using Discord.WebSocket;
using keeganstudios.possebot.DataAccessLayer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Services
{
    public class EventHandlerService
    {
        private readonly ILogger<EventHandlerService> _logger;
        private readonly DiscordSocketClient _client;
        private readonly IAudioService _audioService;
        private readonly IThemeDal _themeDal;
        public EventHandlerService(ILogger<EventHandlerService> logger, DiscordSocketClient client, IAudioService audioService, IThemeDal themeDal)
        {
            _logger = logger;
            _client = client;
            _audioService = audioService;
            _themeDal = themeDal;

            _client.UserVoiceStateUpdated += OnVoiceStateUpdated;
        }
        private async Task OnVoiceStateUpdated(SocketUser user, SocketVoiceState state1, SocketVoiceState state2)
        {
            if (user.IsBot)
            {
                return;
            }

            if (state1.VoiceChannel == null && state2.VoiceChannel != null)
            {
                _logger.LogInformation("User (Name: {username} ID: {userId}) joined to a VoiceChannel (Name: {voiceChannelName} Id: {voiceChannelId}) Guild Id: {guildId}", user.Username, user.Id, state2.VoiceChannel.Name, state2.VoiceChannel.Id, state2.VoiceChannel.Guild);

                var theme = await _themeDal.GetThemeAsync(user.Id, state2.VoiceChannel.Guild.Id);

                if (theme != null && theme.Enabled)
                {
                    _logger.LogInformation("Theme found for User (Name: {username} ID: {userId}) at path: {audioPath}", user.Username, user.Id, theme.AudioPath);
                    await _audioService.ConnectToVoiceAndPlayTheme(state2.VoiceChannel, theme);
                }
            }
            if (state1.VoiceChannel != null && state2.VoiceChannel == null)
            {                
                _logger.LogInformation("User (Name: {username} ID: {userId}) left from a VoiceChannel (Name: {voiceChannelName} Id: {voiceChannelId}) Guild Id: {guildId}", user.Username, user.Id, state1.VoiceChannel.Name, state1.VoiceChannel.Id, state1.VoiceChannel.Guild);
            }
        }
    }
}
