using keeganstudios.possebot.Entities;
using keeganstudios.possebot.Models;

namespace keeganstudios.possebot.Extensions
{
    public static class EntityExtensions
    {
        public static ThemeDetail ToModel(this Theme themeEntity)
        {
            var themeDetails = new ThemeDetail();

            themeDetails.Id = themeEntity.Id;
            themeDetails.AudioPath = themeEntity.AudioPath;
            themeDetails.UserId = themeEntity.UserId;
            themeDetails.GuildId = themeEntity.GuildId;
            themeDetails.Start = themeEntity.Start;
            themeDetails.Duration = themeEntity.Duration;
            themeDetails.Enabled = themeEntity.Enabled;

            return themeDetails;
        }

        public static void UpdateEntity(this Theme themeEntity, ThemeDetail themeModel)
        {
            themeEntity.AudioPath = themeModel.AudioPath;
            themeEntity.UserId = themeModel.UserId;
            themeEntity.GuildId = themeModel.GuildId;
            themeEntity.Start = themeModel.Start;
            themeEntity.Duration = themeModel.Duration;
            themeEntity.Enabled = themeModel.Enabled;
        }
    }  
}
