using keeganstudios.possebot.Models;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Services
{
    public interface IOptionsService
    {
        Task<ConfigurationOptions> ReadConfigurationOptionsAsync();
    }
}
