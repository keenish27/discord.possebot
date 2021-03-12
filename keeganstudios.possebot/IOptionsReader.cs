using keeganstudios.possebot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot
{
    public interface IOptionsReader
    {
        Task<ConfigurationOptions> ReadConfigurationOptions();
        Task<ThemeOptions> ReadThemeOptions();
    }
}
