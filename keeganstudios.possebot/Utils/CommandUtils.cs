using DotNetTools.SharpGrabber;
using DotNetTools.SharpGrabber.Media;
using keeganstudios.possebot.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Utils
{
    public class CommandUtils : ICommandUtils
    {
        private readonly ILogger<CommandUtils> _logger;
        private readonly IOptionsService _optionsService;

        public CommandUtils(ILogger<CommandUtils> logger, IOptionsService optionsService)
        {
            _logger = logger;
            _optionsService = optionsService;
        }

        public async Task<string> BuildCommandAsyc(string commandName)
        {
            return await BuildCommandAsync(commandName, false);
        }

        public async Task<string> BuildCommandAsync(string commandName, bool isHelpCommand)
        {
            var command = string.Empty;
            try
            {
                var configurationOptions = await _optionsService.ReadConfigurationOptionsAsync();
                command = $"[{configurationOptions.BotPrefix}";
                if (isHelpCommand)
                {
                    command += "help";
                }
                command += $" {commandName}]";
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to build command for {commandName}", commandName);
                throw;
            }
            return command;
        }

        public GrabbedMedia GetGrabbedMediaToSave(IList<IGrabbed> resources)
        {
            GrabbedMedia resourceToSave = null;
            try
            {
                var grabbedAudioResources = resources.Where(x => x.GetType() == typeof(GrabbedMedia) && (x as GrabbedMedia).Channels == MediaChannels.Audio).Select(x => x as GrabbedMedia).ToList();

                if (grabbedAudioResources.Count > 0)
                {
                    var maxBitrate = grabbedAudioResources.Where(x => int.Parse(x.BitRateString.Substring(0, x.BitRateString.LastIndexOf("k"))) <= 128).Max(x => int.Parse(x.BitRateString.Substring(0, x.BitRateString.LastIndexOf("k"))));
                    resourceToSave = grabbedAudioResources.Where(x => x.BitRateString == $"{maxBitrate}k").FirstOrDefault();
                    _logger.LogInformation("Found GrabbedMedia: {@grabbedMedia}", resourceToSave);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to get GrabbedMedia from resources: {@resources}", resources);
            }

            return resourceToSave;
        }
    }
}
