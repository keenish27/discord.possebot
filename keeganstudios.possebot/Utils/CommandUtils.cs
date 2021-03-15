using keeganstudios.possebot.Services;
using System;
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

        public async Task<string> BuildCommand(string commandName)
        {
            return await BuildCommand(commandName, false);
        }

        public async Task<string> BuildCommand(string commandName, bool isHelpCommand)
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
    }
}
