namespace keeganstudios.possebot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var bot = new PosseBot();
            bot.Run().ConfigureAwait(false).GetAwaiter().GetResult();            
        }        
    }
}
