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
        Task<ThemeDetails> ReadUserThemeDetailsAsync(ulong userId);
        Task ReloadThemesAsync();
        Task WriteThemeAsync(ThemeDetails theme);
    }
}
