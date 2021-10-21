using DotNetTools.SharpGrabber;
using DotNetTools.SharpGrabber.Grabbed;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Utils
{
    public interface ICommandUtils
    {
        Task<string> BuildCommandAsyc(string commandName);
        Task<string> BuildCommandAsync(string commandName, bool isHelpCommand);
        public GrabbedMedia GetGrabbedMediaToSave(GrabResult resources);
    }
}
