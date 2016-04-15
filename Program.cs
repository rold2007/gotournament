using System.Threading;
using GoTournament.Interface;

namespace GoTournament
{
    using System;
    using System.Threading.Tasks;

    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RunBotRunner();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }

        private static void RunBotRunner()
        {
            var size = 19;
            var finished = false;
            var sync = new object();
            var botWhite = new GnuGoBot(Properties.Settings.Default.gnuBotPath, "WhiteBot")
            {
                BoardSize = size,
                Level = 1
            };
            var botBlack = new GnuGoBot(Properties.Settings.Default.gnuBotPath, "BlackBot")
            {
                BoardSize = size,
                Level = 1
            };
            var judge = new Adjudicator(Properties.Settings.Default.gnuBotPath, size)
            {
                BoardUpdated = board =>
                {
                    lock (sync)
                    {
                        if(!finished)
                        Console.Clear();
                        Console.WriteLine("------------------------------------------");
                        foreach (var line in board)
                        {
                            Console.WriteLine(line);
                        }
                        Console.WriteLine("------------------------------------------");
                    }
                }
            };
            IBotRunner botRunner = null;

            var cancelToken = new CancellationTokenSource();
            Task.Run(() =>
            {
                botRunner = new BotRunner(judge,botBlack, botWhite)
                {
                    EndGame = (reason, s) =>
                    {
                        lock (sync)
                        {
                            finished = true;
                            Console.WriteLine("Games is over by bot '{0}' with the reason: {1}", s, reason.ToString());
                        }
                    }
                };
            }, cancelToken.Token);

            Console.ReadLine();

            if (botRunner != null && !botRunner.IsFinished)
            {
                botRunner.Cancel();
                cancelToken.Cancel();
                Console.WriteLine("Game was canceled");
            }
            else Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
        
    }
}
