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
    }
}
