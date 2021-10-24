using keeganstudios.possebot.Entities;
using keeganstudios.possebot.Extensions;
using keeganstudios.possebot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.DataAccessLayer
{
    public class ThemeDal : IThemeDal
    {
        private readonly ILogger<ThemeDal> _logger;
        private readonly SqliteContext _sqliteContext;
        
        public ThemeDal(ILogger<ThemeDal> logger, SqliteContext sqliteContext)
        {
            _logger = logger;
            _sqliteContext = sqliteContext;
        }

        public async Task<ThemeDetail> GetThemeAsync(int themeId)
        {
            var themeDetail = new ThemeDetail();
            try
            {
                var theme = await _sqliteContext.Themes.AsQueryable().Where(x => x.Id == themeId).FirstOrDefaultAsync();

                if (theme != null)
                {
                    themeDetail = theme.ToModel();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve theme for Theme Id: {themeId}", themeId);
            }

            _logger.LogInformation("Theme retrieved for Id: {themeId} User Id: {userId} and Guild Id: {guildId} with Start: {start} Duration: {duration} and Audio Path: {audioPAth}", themeDetail.Id, themeDetail.UserId, themeDetail.GuildId, themeDetail.Start, themeDetail.Duration, themeDetail.AudioPath);
            return themeDetail;
        }

        public async Task<ThemeDetail> GetThemeAsync(ulong userId, ulong guildId)
        {
            var themeDetail = new ThemeDetail();
            try
            {
                var theme = await _sqliteContext.Themes.AsQueryable().Where(x => x.UserId == userId && x.GuildId == guildId).FirstOrDefaultAsync();

                if (theme != null)
                {
                    themeDetail = theme.ToModel();
                }                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve theme for User Id: {userId} and Guild Id: {guildId}", userId, guildId);
            }

            _logger.LogInformation("Theme retrieved for Id: {themeId} User Id: {userId} and Guild Id: {guildId} with Start: {start} Duration: {duration} and Audio Path: {audioPAth}", themeDetail.Id, themeDetail.UserId, themeDetail.GuildId, themeDetail.Start, themeDetail.Duration, themeDetail.AudioPath);
            return themeDetail;
        }

        public async Task WriteThemeAsync(ThemeDetail themeDetail)
        {
            try
            {
                var existingThemeEntity = await _sqliteContext.Themes.AsQueryable().Where(x => x.UserId == themeDetail.UserId && x.GuildId == themeDetail.GuildId).FirstOrDefaultAsync();

                if (existingThemeEntity != null)
                {
                    existingThemeEntity.UpdateEntity(themeDetail);
                }
                else
                {
                    await _sqliteContext.Themes.AddAsync(themeDetail.ToEntity());
                    _logger.LogInformation("Theme saved for Id: {themeId} User Id: {userId} and Guild Id: {guildId} with Start: {start} Duration: {duration} and Audio Path: {audioPAth}", themeDetail.Id, themeDetail.UserId, themeDetail.GuildId, themeDetail.Start, themeDetail.Duration, themeDetail.AudioPath);
                }

                await _sqliteContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable up write theme with Id: {themeId}", themeDetail.Id);
            }
        }
    }
}
