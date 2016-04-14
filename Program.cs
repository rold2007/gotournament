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
                RunAdjudicatorTest();
                return;
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
            var botWhite = new GnuGoBot(Properties.Settings.Default.gnuBotPath, "WhiteBot")
            {
                BoardSize = 9,
                Level = 1
            };
            var botBlack = new GnuGoBot(Properties.Settings.Default.gnuBotPath, "BlackBot")
            {
                BoardSize = 9,
                Level = 10
            };
            IBotRunner botRunner = null;

            var cancelToken = new CancellationTokenSource();
            Task.Run(() =>
            {
                botRunner = new BotRunner(botBlack, botWhite)
                {
                    Resigned = s => Console.WriteLine("Bot {0} has resigned the game", s),
                    PassLimitPassed = s => Console.WriteLine("Bot {0} has passed consecutively", s)
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

        private static void RunAdjudicatorTest()
        {
            Task.Run(() =>
            {
                IAdjudicator a = new Adjudicator(Properties.Settings.Default.gnuBotPath, 8)
                {
                    BlackMoveValidated = b => Console.WriteLine("black move is " + (b ? "correct" : "wrong")),
                    WhiteMoveValidated = b => Console.WriteLine("white move is " + (b ? "correct" : "wrong")),
                    BoardUpdated = board =>
                    {
                        Console.WriteLine("-------------------------");
                        foreach (var line in board)
                        {
                            Console.WriteLine(line);
                        }
                        Console.WriteLine("-------------------------");
                    }
                };
                a.BlackMoves(Move.Parse("A1")); Thread.Sleep(1000);
                a.WhiteMoves(Move.Parse("A1")); Thread.Sleep(1000); //used place
                a.WhiteMoves(Move.Parse("A4")); Thread.Sleep(1000);
                a.BlackMoves(Move.Parse("A2")); Thread.Sleep(1000);
                a.WhiteMoves(Move.Parse("Z2")); Thread.Sleep(1000); //wrong letter
                a.WhiteMoves(Move.Parse("C4")); Thread.Sleep(1000);
                a.BlackMoves(Move.Parse("A3")); Thread.Sleep(1000);
                a.WhiteMoves(Move.Parse("G4")); Thread.Sleep(1000);
                a.BlackMoves(Move.Parse("pass")); Thread.Sleep(1000);
                a.WhiteMoves(Move.Parse("G6")); Thread.Sleep(1000);
                a.BlackMoves(Move.Parse("A7")); Thread.Sleep(1000);
                a.WhiteMoves(Move.Parse("G8"));
            });
            Console.ReadLine();
        }
    }
}
