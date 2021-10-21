using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Utils
{
    public interface IDbUtils
    {
        Task EnsureDatabaseCreated();
        Task MigrateData();
    }
}
