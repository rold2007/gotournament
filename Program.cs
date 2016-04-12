using System.Security.Cryptography;
using System.Threading;

namespace GoTournament
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() =>
            {
                var botWhite = new GnuGoBot(Properties.Settings.Default.gnuBotPath);
                botWhite.SetBoardSize(15);
                botWhite.SetLevel(1);
                var botBlack = new GnuGoBot(Properties.Settings.Default.gnuBotPath);
                botBlack.SetBoardSize(15);
                botWhite.SetLevel(10);
                botWhite.MovePerformed = move => botBlack.PlaceMove(move);
                botBlack.MovePerformed = move => botWhite.PlaceMove(move);
                botWhite.StartGame(false);
                botBlack.StartGame(true);
            });
            Console.ReadLine();
            Console.WriteLine("----finished----");
            Console.ReadLine();
        }

    }
}
