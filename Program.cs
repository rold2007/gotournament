using System.Security.Cryptography;
using System.Threading;
using GoTournament.Interface;

namespace GoTournament
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class Program
    {
        static void Main(string[] args)
        {
            var botWhite = new GnuGoBot(Properties.Settings.Default.gnuBotPath, "WhiteBot")
            {
                BoardSize = 10,
                Level = 1,
                MaxPassCount = 3
            };
            var botBlack = new GnuGoBot(Properties.Settings.Default.gnuBotPath, "BlackBot")
            {
                BoardSize = 10,
                Level = 10,
                MaxPassCount = 3
            };
            IBotRunner botRunner = null;

            var cancelToken = new CancellationTokenSource();
            Task.Run(() =>
            {
                botRunner = new BotRunner(botBlack, botWhite)
                {
                    Resigned = s => Console.WriteLine("Bot {0} has resigned the game", s),
                    PassLimitPassed = s => Console.WriteLine("Bot {0} has passed too many times", s)
                };
            }, cancelToken.Token);

            Console.ReadLine();

            if (botRunner != null && !botRunner.IsFinished)
            {
                botRunner.Cancel();
                cancelToken.Cancel();
                Console.WriteLine("Game was canceled");
            } else Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

    }
}
