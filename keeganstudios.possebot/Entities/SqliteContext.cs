using keeganstudios.possebot.Extensions;
using keeganstudios.possebot.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Entities
{
    public class SqliteContext : DbContext
    {       
        public DbSet<Theme> Themes { get; set; }

        public SqliteContext(DbContextOptions<SqliteContext> options) : base(options)
        {

        }
    }
}
