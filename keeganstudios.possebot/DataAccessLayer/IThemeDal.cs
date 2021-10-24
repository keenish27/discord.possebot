using keeganstudios.possebot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.DataAccessLayer
{
    public interface IThemeDal
    {
        Task<ThemeDetail> GetThemeAsync(int themeId);
        Task<ThemeDetail> GetThemeAsync(ulong userId, ulong guildId);
        Task WriteThemeAsync(ThemeDetail themeDetail);
    }
}
