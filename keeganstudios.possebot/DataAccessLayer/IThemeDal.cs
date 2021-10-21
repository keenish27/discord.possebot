using keeganstudios.possebot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.DataAccessLayer
{
    public interface IThemeDal
    {
        Task<ThemeDetails> GetThemeAsync(int themeId);
        Task<ThemeDetails> GetThemeAsync(ulong userId, ulong guildId);
        Task WriteThemeAsync(ThemeDetails themeDetail);
    }
}
