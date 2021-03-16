using keeganstudios.possebot.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Services
{
    public class OptionsService : IOptionsService
    {
        private ILogger<OptionsService> _logger;
        private ConfigurationOptions _configurationOptions;
        private ThemeOptions _themeOptions;

        public OptionsService(ILogger<OptionsService> logger)
        {
            _logger = logger;
        }

        public async Task<ConfigurationOptions> ReadConfigurationOptionsAsync()
        {
            if (_configurationOptions == null)
            {
                try
                {
                    _logger.LogInformation("Reading configuration options");                   

                    var json = JObject.Parse(await File.ReadAllTextAsync("settings.json"));
                    _configurationOptions = JsonConvert.DeserializeObject<ConfigurationOptions>(json.GetValue("configuration").ToString());                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to read configuration options");
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
                    _logger.LogInformation("Reading theme options");

                    var json = JObject.Parse(await File.ReadAllTextAsync("themes.json"));
                    _themeOptions = JsonConvert.DeserializeObject<ThemeOptions>(json.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to read theme options");
                }
            }
            return _themeOptions;
        }

        public async Task<ThemeDetails> ReadUserThemeDetailsAsync(ulong guildId, ulong userId)
        {
            ThemeDetails theme = null;
            try
            {
                _logger.LogInformation("Reading theme details for Guild Id: {guildId} User Id: {userId}", guildId, userId);

                var themeOptions = await ReadThemeOptionsAsync();                
                theme = themeOptions.Themes.Where(x => x.GuildId == guildId && x.UserId == userId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to read ThemDetails for User Id: {userId} Guild Id {guildId}", userId, guildId);                
            }

            return theme;
        }

        public async Task ReloadThemesAsync()
        {
            try
            {
                _logger.LogInformation("Reloading themes");

                var json = JObject.Parse(await File.ReadAllTextAsync("themes.json"));
                _themeOptions = JsonConvert.DeserializeObject<ThemeOptions>(json.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to reload themes");                
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
                _logger.LogError(ex, "Unable to write theme {@theme}", theme);                
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
                    if (themeCollection[i].GuildId == theme.GuildId && themeCollection[i].UserId == theme.UserId)
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
                _logger.LogError(ex, "Unable to update theme collection with theme {@theme}", theme);
            }

            return themeCollection;
        }

        public async Task WriteThemeOptionsAsync(ThemeOptions themeOptions)
        {
            try
            {
                _logger.LogInformation("Writing themes");

                var json = JsonConvert.SerializeObject(themeOptions, Formatting.Indented);
                await File.WriteAllTextAsync("themes.json", json);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to write themes to themes.json");                
            }
        }

        public ThemeDetails CreateTheme(ulong userId, ulong guildId, string audioPath, int start, int duration, bool enabled)
        {
            return new ThemeDetails
            {
                UserId = userId,
                GuildId = guildId,
                AudioPath = audioPath,
                Start = start,
                Duration = duration,
                Enabled = enabled
            };
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
