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
    public class OptionsService : IOptionsService
    {
        private ConfigurationOptions _configurationOptions;
        private ThemeOptions _themeOptions;

        public async Task<ConfigurationOptions> ReadConfigurationOptionsAsync()
        {
            if (_configurationOptions == null)
            {
                try
                {
                    Console.WriteLine($"Reading ConfigurationOptions");

                    var json = JObject.Parse(await File.ReadAllTextAsync("settings.json"));
                    _configurationOptions = JsonConvert.DeserializeObject<ConfigurationOptions>(json.GetValue("configuration").ToString());

                    Console.WriteLine($"Read ConfigurationOptions");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine($"- {ex.StackTrace}");
                }
            }
            return _configurationOptions;
        }

        public async Task<ThemeOptions> ReadThemeOptionsAsync()
        {
            if (_themeOptions == null)
            {
                try
                {
                    Console.WriteLine($"Reading ThemeOptions");

                    var json = JObject.Parse(await File.ReadAllTextAsync("themes.json"));
                    _themeOptions = JsonConvert.DeserializeObject<ThemeOptions>(json.ToString());

                    Console.WriteLine($"Read ThemeOptions");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine($"- {ex.StackTrace}");
                }
            }
            return _themeOptions;
        }

        public async Task<ThemeDetails> ReadUserThemeDetailsAsync(ulong userId)
        {
            ThemeDetails theme = null;
            try
            {
                Console.WriteLine($"Reading ThemeDetails for User Id: {userId}");

                var themeOptions = await ReadThemeOptionsAsync();
                theme = themeOptions.Themes.Where(x => x.UserId == userId).FirstOrDefault();

                Console.WriteLine($"Read ThemeDetails for User Id: {userId}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }

            return theme;
        }

        public async Task ReloadThemesAsync()
        {
            try
            {
                Console.WriteLine($"Reloading themes");

                var json = JObject.Parse(await File.ReadAllTextAsync("themes.json"));
                _themeOptions = JsonConvert.DeserializeObject<ThemeOptions>(json.ToString());

                Console.WriteLine($"Reloaded themes");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }

        public async Task WriteThemeAsync(ThemeDetails theme)
        {
            try
            {
                theme = ValidateTheme(theme);

                await ReadThemeOptionsAsync();

                if (_themeOptions == null)
                {
                    _themeOptions = new ThemeOptions();
                }

                if (_themeOptions.Themes == null)
                {
                    _themeOptions.Themes = new List<ThemeDetails>();
                }

                _themeOptions.Themes = UpdateThemeCollection(theme);

                await WriteThemeOptionsAsync(_themeOptions);
                await ReloadThemesAsync();
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }

        public IEnumerable<ThemeDetails> UpdateThemeCollection(ThemeDetails theme)
        {
            var themeCollection = _themeOptions.Themes.ToList();

            try
            {
                var collectionUpdated = false;
                for (var i = 0; i < themeCollection.Count; i++)
                {
                    if (themeCollection[i].UserId == theme.UserId)
                    {
                        themeCollection[i] = theme;
                        collectionUpdated = true;
                    }
                }
                if (!collectionUpdated)
                {
                    themeCollection.Add(theme);
                }
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }

            return themeCollection;
        }

        public async Task WriteThemeOptionsAsync(ThemeOptions themeOptions)
        {
            try
            {
                Console.WriteLine($"Writing themes");

                var json = JsonConvert.SerializeObject(themeOptions, Formatting.Indented);
                await File.WriteAllTextAsync("themes.json", json);

                Console.WriteLine($"Wrote themes");
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
            }
        }

        public ThemeDetails ValidateTheme(ThemeDetails theme)
        {
            var newTheme = theme;

            if (newTheme.Duration == 0)
            {
                theme.Duration = 15;
            }

            if (newTheme.Duration > 20)
            {
                theme.Duration = 20;
            }

            return newTheme;
        }
    }
}
