using keeganstudios.possebot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Services
{
    public interface IOptionsService
    {
        Task<ConfigurationOptions> ReadConfigurationOptionsAsync();
        Task<ThemeOptions> ReadThemeOptionsAsync();
        Task<ThemeDetails> ReadUserThemeDetailsAsync(ulong guildId, ulong userId);
        Task ReloadThemesAsync();
        Task WriteThemeAsync(ThemeDetails theme);
        public ThemeDetails CreateTheme(ulong userId, ulong guildId, string audioPath, int start, int duration, bool enabled);
    }
}
