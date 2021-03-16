using Serilog;

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

            var bot = new PosseBot();
            bot.Run().ConfigureAwait(false).GetAwaiter().GetResult();            
        }        
    }
}
