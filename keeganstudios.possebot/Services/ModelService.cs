using keeganstudios.possebot.Extensions;
using keeganstudios.possebot.Models;

namespace keeganstudios.possebot.Services
{
    public class ModelService : IModelService
    {
        public ThemeDetail CreateThemeDetail(ulong userId, ulong guildId, string audioPath, int start, int duration, bool enabled)
        {
            return new ThemeDetail
            {
                UserId = userId,
                GuildId = guildId,
                AudioPath = audioPath,
                Start = start,
                Duration = duration,
                Enabled = enabled
            }.Validate();
        }
    }
}
