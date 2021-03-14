using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Utils
{
    public interface ICommandUtils
    {
        Task<string> BuildCommand(string commandName);
        Task<string> BuildCommand(string commandName, bool isHelpCommand);
    }
}
