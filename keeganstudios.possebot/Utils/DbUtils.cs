using keeganstudios.possebot.Entities;
using keeganstudios.possebot.Extensions;
using keeganstudios.possebot.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Utils
{
    public class DbUtils : IDbUtils
    {
        private readonly ILogger<DbUtils> _logger;
        private readonly IOptionsService _optionsService;
        private readonly SqliteContext _sqliteContext;

        public DbUtils(ILogger<DbUtils> logger, IOptionsService optionsService, SqliteContext sqliteContext)
        {
            _logger = logger;
            _optionsService = optionsService;
            _sqliteContext = sqliteContext;
        }

        public async Task EnsureDatabaseCreated()
        {
            await _sqliteContext.Database.EnsureCreatedAsync();
            _logger.LogInformation("Database either already existed or was created");
        }
        public async Task MigrateData()
        {
            try
            {                
                var themeOptions = await _optionsService.ReadThemeOptionsAsync();
                var themeCollection = await _sqliteContext.Themes.ToArrayAsync();
                var themeEntityCollection = new List<Theme>();

                foreach (var theme in themeOptions.Themes)
                {
                    var existingTheme = themeCollection.Where(x => x.UserId == theme.UserId && x.GuildId == theme.GuildId).FirstOrDefault();
                    if (existingTheme == null)
                    {
                        var themeEntity = theme.ToEntity();                        

                        _logger.LogInformation("Add entity with Guild Id: {guildId} and User Id: {userId}", theme.GuildId, theme.UserId);
                        themeEntityCollection.Add(themeEntity);                                                
                    }
                }

                if (themeEntityCollection.Count > 0)
                {
                    await _sqliteContext.Themes.AddRangeAsync(themeEntityCollection);
                    await _sqliteContext.SaveChangesAsync();                    
                }

                _logger.LogInformation($"Themes Migrated: {themeEntityCollection.Count()}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to migrate data");
            }
        }
    }
}
