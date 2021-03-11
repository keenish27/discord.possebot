﻿using keeganstudios.possebot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot
{
    public static class OptionsReader
    {
        public static async Task<ConfigurationOptions> ReadConfigurationOptions()
        {
            var json = JObject.Parse(await File.ReadAllTextAsync("settings.json"));
            return JsonConvert.DeserializeObject<ConfigurationOptions>(json.GetValue("configuration").ToString());
        }

        public static async Task<ThemeOptions> ReadThemeOptions()
        {
            var option = new ThemeOptions();
            try
            {
                var json = JObject.Parse(await File.ReadAllTextAsync("themes.json"));
                option = JsonConvert.DeserializeObject<ThemeOptions>(json.ToString());
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return option;
        }
    }
}
