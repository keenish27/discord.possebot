using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Services
{
    public class LoggingService
    {
        private readonly ILogger _logger;
        private DiscordSocketClient _client;
        private CommandService _commands;

        public LoggingService(ILogger<LoggingService> logger, DiscordSocketClient client, CommandService commands)
        {
            _logger = logger;
            _client = client;
            _commands = commands;

            _client.Ready += OnReadyAsync;
            _client.Log += OnLogAsync;
        }

        public Task OnReadyAsync()
        {
            _logger.LogInformation("Connected as -> [{currentUser}] :)", _client.CurrentUser);
            _logger.LogInformation("We are on [{guildCount}] servers", _client.Guilds.Count);
            return Task.CompletedTask;
        }

        public Task OnLogAsync(LogMessage msg)
        {
            string logText = $": {msg.Exception?.ToString() ?? msg.Message}";
            switch (msg.Severity.ToString())
            {
                case "Critical":
                    {
                        _logger.LogCritical(logText);
                        break;
                    }
                case "Warning":
                    {
                        _logger.LogWarning(logText);
                        break;
                    }
                case "Info":
                    {
                        _logger.LogInformation(logText);
                        break;
                    }
                case "Verbose":
                    {
                        _logger.LogInformation(logText);
                        break;
                    }
                case "Debug":
                    {
                        _logger.LogDebug(logText);
                        break;
                    }
                case "Error":
                    {
                        _logger.LogError(logText);
                        break;
                    }
            }

            return Task.CompletedTask;
        }
    }
}
