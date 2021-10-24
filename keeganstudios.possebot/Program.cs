using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;

namespace keeganstudios.possebot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs/possebot.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")
                .Build();

            var bot = new PosseBot(configuration);
            bot.Run().ConfigureAwait(false).GetAwaiter().GetResult();            
        }        
    }
}
