using DotNetTools.SharpGrabber;
using DotNetTools.SharpGrabber.Media;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Utils
{
    public interface ICommandUtils
    {
        Task<string> BuildCommandAsyc(string commandName);
        Task<string> BuildCommandAsync(string commandName, bool isHelpCommand);
        public GrabbedMedia GetGrabbedMediaToSave(IList<IGrabbed> resources);
    }
}
