using System.Collections.Generic;
using System.Linq;
using GoTournament.Interface;

namespace GoTournament
{
    using System;


    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify tournament file name in the arguments. Optionally it can be set among of game cycles in the second argument\nFor example: Tournament.exe DaisyVsMadison 10");
                return;
            }
            try
            {
                RunGame(args);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }


        private static void RunGame(string[] args)
        {
            string gamesCount = (args.Length > 1) ? args[1] : string.Empty;

            ITournamentInitializer initializer = new TournamentInitializer(args.First(), gamesCount);
            initializer.Run();


        }
    }
}
