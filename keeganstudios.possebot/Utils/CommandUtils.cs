using DotNetTools.SharpGrabber;
using DotNetTools.SharpGrabber.Media;
using keeganstudios.possebot.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Utils
{
    public class CommandUtils : ICommandUtils
    {
        private readonly IOptionsService _optionsService;

        public CommandUtils(IOptionsService optionsService)
        {
            _optionsService = optionsService;
        }

        public async Task<string> BuildCommandAsyc(string commandName)
        {
            return await BuildCommandAsync(commandName, false);
        }

        public async Task<string> BuildCommandAsync(string commandName, bool isHelpCommand)
        {
            var configurationOptions = await _optionsService.ReadConfigurationOptionsAsync();
            var command = $"[{configurationOptions.BotPrefix}";
            if (isHelpCommand)
            {
                command += "help";
            }
            command += $" {commandName}]";
            return command;
        }

        public GrabbedMedia GetGrabbedMediaToSave(IList<IGrabbed> resources)
        {
            GrabbedMedia resourceToSave = null;
            var grabbedAudioResources = resources.Where(x => x.GetType() == typeof(GrabbedMedia) && (x as GrabbedMedia).Channels == MediaChannels.Audio).Select(x => x as GrabbedMedia).ToList();

            if (grabbedAudioResources.Count > 0)
            {
                var maxBitrate = grabbedAudioResources.Where(x => int.Parse(x.BitRateString.Substring(0, x.BitRateString.LastIndexOf("k"))) <= 128).Max(x => int.Parse(x.BitRateString.Substring(0, x.BitRateString.LastIndexOf("k"))));
                resourceToSave = grabbedAudioResources.Where(x => x.BitRateString == $"{maxBitrate}k").FirstOrDefault();
            }

            return resourceToSave;
        }
    }
}
