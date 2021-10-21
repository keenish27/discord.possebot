using keeganstudios.possebot.Entities;
using keeganstudios.possebot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Extensions
{
    public static class ModelExtensions
    {
        public static Theme ToEntity(this ThemeDetails themeDetails)
        {
            var themeEntity = new Theme();

            themeEntity.Id = themeDetails.Id;
            themeEntity.AudioPath = themeDetails.AudioPath;
            themeEntity.UserId = themeDetails.UserId;
            themeEntity.GuildId = themeDetails.GuildId;
            themeEntity.Start = themeDetails.Start;
            themeEntity.Duration = themeDetails.Duration;
            themeEntity.Enabled = themeDetails.Enabled;

            return themeEntity;
        }
    }
}
