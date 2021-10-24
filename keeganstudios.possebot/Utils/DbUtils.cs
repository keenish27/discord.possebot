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
    }
}
