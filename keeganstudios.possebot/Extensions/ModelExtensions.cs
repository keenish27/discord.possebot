using keeganstudios.possebot.Entities;
using keeganstudios.possebot.Models;

namespace keeganstudios.possebot.Extensions
{
    public static class ModelExtensions
    {
        public static Theme ToEntity(this ThemeDetail themeDetail)
        {
            var themeEntity = new Theme();

            themeEntity.Id = themeDetail.Id;
            themeEntity.AudioPath = themeDetail.AudioPath;
            themeEntity.UserId = themeDetail.UserId;
            themeEntity.GuildId = themeDetail.GuildId;
            themeEntity.Start = themeDetail.Start;
            themeEntity.Duration = themeDetail.Duration;
            themeEntity.Enabled = themeDetail.Enabled;

            return themeEntity;
        }

        public static ThemeDetail Validate(this ThemeDetail themeDetail)
        {
            var newTheme = themeDetail;

            if (newTheme.Duration == 0)
            {
                themeDetail.Duration = 15;
            }

            if (newTheme.Duration > 20)
            {
                themeDetail.Duration = 20;
            }

            return newTheme;
        }
    }
}
