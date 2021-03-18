using Discord;
using Discord.Commands;
using keeganstudios.possebot.Services;
using keeganstudios.possebot.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.CommandModules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<Help> _logger;
        private readonly CommandService _commands;
        private readonly IOptionsService _optionsService;
        private readonly IEmbedBuilderUtils _embedBuilderUtils;

        public Help(ILogger<Help> logger, CommandService commands, IOptionsService optionsSerice, IEmbedBuilderUtils embedBuilderUtils)
        {
            _logger = logger;
            _commands = commands;
            _optionsService = optionsSerice;
            _embedBuilderUtils = embedBuilderUtils;
        }

        [Command("help", RunMode = RunMode.Async)]
        [Summary("Lists all available bot commands")]
        public async Task HelpAsync([Remainder] string commandOrModule = null)
        {
            try
            {
                if (commandOrModule != null)
                {
                    await DetailedHelpAsync(commandOrModule.ToLower());
                    return;
                }

                var builder = new EmbedBuilder();
                {
                    builder.WithColor(new Color(87, 222, 127));
                    builder.WithTitle($"Hey {Context.User.Username}, here is a list of all my commands.");
                    builder.WithFooter(
                        f => f.WithText("Use `help [command-name]` or `help [module-name]` for more information"));
                }
                
                foreach (var module in _commands.Modules.OrderBy(x => x.Name))
                {
                    string fieldValue = await FilterPreconditions(module);                    

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        continue;
                    
                    fieldValue = fieldValue.Substring(0, fieldValue.Length - 2);
                    builder.AddField(_embedBuilderUtils.BuildEmbedField($"\n📚 {module.Name}", fieldValue, false));                        
                }

                await ReplyAsync(string.Empty, false, builder.Build());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to help user id: {userId} in guild id: {guildId}", Context.User.Id, Context.Guild.Id);
            }
        }

        public async Task<string> FilterPreconditions(ModuleInfo module)
        {
            string fieldValue = null;

            try
            {                
                foreach (var cmd in module.Commands.OrderBy(x => x.Name))
                {                    
                    if (string.IsNullOrWhiteSpace(cmd.Summary))
                        _logger.LogInformation("No summary for {commandName}", cmd.Name);
                    
                    if (!module.Name.ToLower().Equals("help") && !module.Name.ToLower().Equals("ping"))
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess) fieldValue += $"`{cmd.Aliases.First()}`, ";
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to filter preconditions for module: {moduleName}", module.Name);
                throw;
            }

            return fieldValue;
        }
       
        public async Task DetailedHelpAsync([Remainder] string command)
        {
            try
            {                
                var moduleFound = _commands.Modules.Select(mod => mod.Name.ToLower()).ToList().Contains(command);
                if (moduleFound)
                {
                    await DetailedModuleHelpAsync(command);
                    return;
                }

                // `command` isn't a module for sure. Now checking if it is a command
                var result = _commands.Search(Context, command);
                if (!result.IsSuccess)
                {
                    await ReplyAsync($"Sorry, I couldn't find a command called *{command}* ☹️");
                    return;
                }

                var builder = new EmbedBuilder
                {
                    Color = new Color(87, 222, 127)
                };

                foreach (var cmd in result.Commands.Select(match => match.Command))
                {
                    var fieldValue = BuildAliasInformation(cmd.Aliases);
                    fieldValue += await BuildParameterInformation(cmd, command);

                    builder.AddField(_embedBuilderUtils.BuildEmbedField($"Help on *{command}* coming right up", fieldValue, false));                    
                }
                builder.WithFooter("Note: Parameters under `[]` are mandatory and the ones under `<>` are optional");

                await ReplyAsync(string.Empty, false, builder.Build());
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to get detailed help for user id: {userId} in guild id: {guild.d}", Context.User.Id, Context.Guild.Id);
            }
        }

        public string BuildAliasInformation(IReadOnlyList<string> aliases)
        {
            var temp = "None";
            if (aliases.Count != 1)
            {
                temp = string.Join(", ", aliases);
            }
            return "**Aliases**: " + temp;
        }

        public async Task<string> BuildParameterInformation(CommandInfo commandInfo, string command)
        {
            var configOptions = await _optionsService.ReadConfigurationOptionsAsync();

            var temp = "`" + configOptions.BotPrefix + command;
            
            if (commandInfo.Parameters.Count != 0)
            {
                var parameterInfoCollection = commandInfo.Parameters.Select(p => p.IsOptional ? "<" + (p.Summary.Length > 1 ? p.Summary : p.Name) + ">" : "[" + (p.Summary.Length > 1 ? p.Summary : p.Name) + "]");
                temp += " " + string.Join(" ", parameterInfoCollection);
            }

            temp += "`";

            return $"\n**Usage**: {temp}\n**Summary**: {commandInfo.Summary}";
        }
       
        public async Task DetailedModuleHelpAsync(string module)
        {
            try
            {
                var first = _commands.Modules.First(mod => mod.Name.ToLower() == module);
                var embed = new EmbedBuilder
                {
                    Title = "List of commands under " + module.ToUpper() + " module",
                    Description = string.Empty,
                    Color = new Color(87, 222, 127)
                };
                embed.WithFooter("Use `help [command-name]` for more information on the command");
                
                foreach (var cmds in first.Commands)
                {
                    embed.Description += $"{cmds.Name}, ";
                }

                embed.Description = embed.Description.Substring(0, embed.Description.Length - 2);
                await ReplyAsync(string.Empty, false, embed.Build());
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to get detailed module help for user id: {userId} in guild id: {guildId}", Context.User.Id, Context.Guild.Id);
            }
        }
    }
}
