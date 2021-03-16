using Discord.Commands;
using Discord.WebSocket;
using keeganstudios.possebot.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace keeganstudios.possebot
{
    public class CommandHandler
    {
        private readonly ILogger<CommandHandler> _logger;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly IOptionsService _optionsService;

        public CommandHandler(ILogger<CommandHandler> logger, DiscordSocketClient client, CommandService commands, IServiceProvider services, IOptionsService options)
        {
            _logger = logger;
            _commands = commands;
            _client = client;
            _services = services;
            _optionsService = options;
        }

        public async Task InstallCommandsAsync()
        {
            try
            {
                _client.MessageReceived += HandleCommandAsync;
                await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                                services: _services);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to install commands.");
            }
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            try
            {
                var configOptions = await _optionsService.ReadConfigurationOptionsAsync();
                var message = messageParam as SocketUserMessage;
                if (message == null)
                {
                    return;
                }

                int argPos = 0;

                if (!(message.HasCharPrefix(configOptions.BotPrefix, ref argPos) ||
                    message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                    message.Author.IsBot)
                {
                    return;
                }

                var context = new SocketCommandContext(_client, message);
                await _commands.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: _services);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to handle command");
            }
        }
    }
}
