using keeganstudios.possebot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot
{
    public class OptionsReader : IOptionsReader
    {
        private ConfigurationOptions _configurationOptions;
        private ThemeOptions _themeOptions;

        public async Task<ConfigurationOptions> ReadConfigurationOptions()
        {
            if (_configurationOptions == null)
            {
                try
                {
                    var json = JObject.Parse(await File.ReadAllTextAsync("settings.json"));
                    _configurationOptions = JsonConvert.DeserializeObject<ConfigurationOptions>(json.GetValue("configuration").ToString());
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine($"- {ex.StackTrace}");
                }
            }
            return _configurationOptions;
        }

        public async Task<ThemeOptions> ReadThemeOptions()
        {
            if (_themeOptions == null)
            {
                try
                {
                    var json = JObject.Parse(await File.ReadAllTextAsync("themes.json"));
                    _themeOptions = JsonConvert.DeserializeObject<ThemeOptions>(json.ToString());
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine($"- {ex.StackTrace}");
                }
            }
            return _themeOptions;
        }

        public async Task<ThemeDetails> ReadUserThemeDetails(ulong userId)
        {
            ThemeDetails theme = null;
            try
            {
                var themeOptions = await ReadThemeOptions();
                theme = themeOptions.Themes.Where(x => x.UserId == userId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }

            return theme;
        }
    }
}
