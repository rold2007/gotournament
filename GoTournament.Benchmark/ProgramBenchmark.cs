using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoTournament.Interface;
using GoTournament.Model;
using GoTournament.Service;
using SimpleInjector;

namespace GoTournament.Benchmark
{
    using GoTournament.Factory;

    class ProgramBenchmark
    {
        private static readonly Stopwatch Watch = new Stopwatch();
        private static TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        private static IGoBotFactory goBotFactory;
        private static BotKind botKind;

        static void Main()
        {
            var minimumBoardSize = 4;
            var maximumBoardSize = 19;
            var minimumAiLevel = 1;
            var maximumAiLevel = 1;
            Console.WriteLine("Benchmark was started");
            var listToCheck = (from difficulty in Enumerable.Range(minimumAiLevel, maximumAiLevel - minimumAiLevel + 1)
                               from boardSize in Enumerable.Range(minimumBoardSize, maximumBoardSize - minimumBoardSize + 1)
                               select BenchmarkSettings.Create(boardSize, difficulty, difficulty)).ToList();
            var injector = Bootstrap();
            var reader = injector.GetInstance<IConfigurationReader>();
            goBotFactory = injector.GetInstance<IGoBotFactory>();
            botKind = reader.ReadBotKind("GnuGo");
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
            tcs = new TaskCompletionSource<bool>();
            Watch.Restart();

            var botWhite = goBotFactory.CreateBotInstance(botKind, "WhiteBot");
            botWhite.BoardSize = settings.BoardSize;
            botWhite.Level = settings.FirstBotLevel;

            var botBlack = goBotFactory.CreateBotInstance(new BotKind { BinaryPath = botKind.BinaryPath, FullClassName = botKind.FullClassName}, "BlackBot");
            botBlack.BoardSize = settings.BoardSize;
            botBlack.Level = settings.SecondBotLevel;

            var judge = new Adjudicator(Bootstrap(),
                new Tournament
                {
                    BoardSize = settings.BoardSize,
                    BlackBot = "BlackBot",
                    WhiteBot = "WhiteBot",
                    Name = "Benchmarking"
                }) { SaveGameResults = true, GenerateLastBoard = true };
            var runner = new BotRunner(judge, botBlack, botWhite) { EndGame = OnTestFinised };
            await tcs.Task;
            runner.Cancel();
        }

        private static void OnTestFinised(GameResult stat)
        {
            Watch.Stop();
            Console.WriteLine("Game duration: {0}\nReason of the game finish: {1}\nFinal score is: {2}",
                string.Format("{0:D2}m:{1:D2}s:{2:D3}ms", Watch.Elapsed.Minutes, Watch.Elapsed.Seconds, Watch.Elapsed.Milliseconds), stat.EndReason, stat.FinalScore);
            Console.WriteLine(stat.FinalBoard);
            tcs.SetResult(true);
        }

        private static ISimpleInjectorWrapper Bootstrap()
        {
            var container = new Container();
            container.Register<IProcessProxy, ProcessProxy>(Lifestyle.Singleton);
            container.Register<IJsonService, JsonService>(Lifestyle.Singleton);
            container.Register<IFileService, FileService>(Lifestyle.Singleton);
            container.Register<IConfigurationService, ConfigurationService>(Lifestyle.Singleton);
            container.Register<IConfigurationReader, ConfigurationReader>(Lifestyle.Singleton);
            container.Register<IGoBotFactory, GoBotFactory>(Lifestyle.Singleton);
            container.Register<IProcessManagerFactory, ProcessManagerFactory>(Lifestyle.Singleton);
            container.Register<ILogger, DebugLogger>(Lifestyle.Singleton);
            var wrapper = new SimpleInjectorWrapper(container);
            container.Register<ISimpleInjectorWrapper>(() => wrapper);
            return wrapper;
        }
    }
}
