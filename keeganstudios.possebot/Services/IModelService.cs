using keeganstudios.possebot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Services
{
    public interface IModelService
    {
        ThemeDetail CreateThemeDetail(ulong userId, ulong guildId, string audioPath, int start, int duration, bool enabled);
    }
}
