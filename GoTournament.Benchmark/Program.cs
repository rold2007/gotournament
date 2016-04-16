using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoTournament.Interface;

namespace GoTournament.Benchmark
{
    class Program
    {
        private static readonly Stopwatch Watch = new Stopwatch();
        private static TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        static void Main()
        {
            var minimumBoardSize = 4;
            var maximumBoardSize = 19;
            var minimumAiLevel = 1;
            var maximumAiLevel = 10;
            Console.WriteLine("Benchmark was started");
            var listToCheck = (from difficulty in Enumerable.Range(minimumAiLevel, maximumAiLevel- minimumAiLevel + 1)
                               from boardSize in Enumerable.Range(minimumBoardSize, maximumBoardSize- minimumBoardSize+1)
                               select BenchmarkSettings.Create(boardSize, difficulty, difficulty)).ToList();
            RunBenchmark(listToCheck);
            Console.ReadLine();
        }

        private static async void RunBenchmark(List<BenchmarkSettings> list)
        {
            foreach (var entry in list)
            {
                await DoTest(entry);
            }
            Console.WriteLine("____________________________________");
            Console.WriteLine("Benchmark was finished");
        }

        private static async Task DoTest(BenchmarkSettings settings)
        {
            Console.WriteLine("____________________________________");
            Console.WriteLine("Board size: {0}, bot #1 strength : {1}, bot #2 strength: {2}", settings.BoardSize, settings.FirstBotLevel, settings.SecondBotLevel);
            _tcs = new TaskCompletionSource<bool>();
            Watch.Restart();
            var botWhite = new GnuGoBot("WhiteBot") { BoardSize = settings.BoardSize, Level = settings.FirstBotLevel };
            var botBlack = new GnuGoBot("BlackBot") { BoardSize = settings.BoardSize, Level = settings.SecondBotLevel };
            var judge = new Adjudicator(settings.BoardSize);
            var runner = new BotRunner(judge, botBlack, botWhite) {EndGame = OnTestFinised};
            await _tcs.Task;
            runner.Cancel();
        }

        private static void OnTestFinised(EndGameReason arg1, string arg2)
        {
            Watch.Stop();
            Console.WriteLine("Game duration: {0}\nReason of the game finish: {1}",
                string.Format("{0:D2}m:{1:D2}s:{2:D3}ms", Watch.Elapsed.Minutes, Watch.Elapsed.Seconds, Watch.Elapsed.Milliseconds) , arg1);
            _tcs.SetResult(true);
        }
    }
}
