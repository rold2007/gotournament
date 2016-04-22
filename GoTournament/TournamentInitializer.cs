using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoTournament.Interface;
using GoTournament.Model;
using GoTournament.Service;

namespace GoTournament
{
    public class TournamentInitializer : ITournamentInitializer
    {
        private readonly string _name;
        private readonly string _gamesCount;
        private readonly IConfigurationService _configurationService;
        private TaskCompletionSource<GameResult> _taskResult;
        List<GameResult> _results = new List<GameResult>();

        public TournamentInitializer(string name, string gamesCount, IConfigurationService configurationService)
        {
            if (configurationService == null) throw new ArgumentNullException(nameof(configurationService));
            _name = name;
            _gamesCount = gamesCount;
            _configurationService = configurationService;
        }

        public TournamentInitializer(string name, string gamesCount) : this(name, gamesCount, new ConfigurationService()) { }

        public async void Run()
        {
            var scenario = _configurationService.ReadTournament(_name);
            int count;
            if (int.TryParse(_gamesCount, out count))
                scenario.GamesCount = count;
            var blackInstance = _configurationService.ReadBotInstance(scenario.BlackBot);
            var whiteInstance = _configurationService.ReadBotInstance(scenario.WhiteBot);
            BotKind blackKind = _configurationService.ReadBotKind(blackInstance.Kind);
            BotKind whiteKind = _configurationService.ReadBotKind(whiteInstance.Kind);

            for (var i = 0; i < scenario.GamesCount; i++)
            {
                _taskResult = new TaskCompletionSource<GameResult>();
                IGoBot blackBot = GetInstance(blackKind, blackInstance.Name);
                IGoBot whiteBot = GetInstance(whiteKind, whiteInstance.Name);
                blackBot.BoardSize = scenario.BoardSize;
                whiteBot.BoardSize = scenario.BoardSize;
                blackBot.Level = blackInstance.Level;
                whiteBot.Level = whiteInstance.Level;

                RunBotRunner(blackBot, whiteBot, scenario);
                OutputStatistic(await _taskResult.Task, blackInstance.Name, whiteInstance.Name);
            }


        }

        private void OutputStatistic(GameResult gameResult, string blackName, string whiteName)
        {
            _results.Add(gameResult);
            Console.WriteLine("Bot \"{0}\" won {1} time(s)", blackName, _results.Count(r => r.WinnerName == blackName));
            Console.WriteLine("Bot \"{0}\" won {1} time(s)", whiteName, _results.Count(r => r.WinnerName == whiteName));
        }

        private IGoBot GetInstance(BotKind kind, string botInstanceName)
        {
            Type type = Type.GetType(kind.FullClassName);
            if (type != null)
                return (IGoBot)Activator.CreateInstance(type, kind.BinnaryPath, botInstanceName);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(kind.FullClassName);
                if (type != null)
                    return (IGoBot)Activator.CreateInstance(type, kind.BinnaryPath, botInstanceName);
            }
            return null;
        }


        private void RunBotRunner(IGoBot botBlack, IGoBot botWhite, Tournament tourn)
        {
            //var finished = false;
            var sync = new object();
            var judge = new Adjudicator(tourn)
            {
                BoardUpdated = board =>
                {
                    lock (sync)
                    {
                        //  if (!finished)
                              Console.Clear();
                        Console.WriteLine("------------------------------------------");
                        foreach (var line in board)
                        {
                            Console.WriteLine(line);
                        }
                        Console.WriteLine("------------------------------------------");
                    }
                },
                SaveGameResults = true
            };

            //   var cancelToken = new CancellationTokenSource();
            Task.Run(() =>
            {
                IBotRunner botRunner = new BotRunner(judge, botBlack, botWhite)
                {
                    EndGame = stat =>
                    {
                        lock (sync)
                        {
                            //  finished = true;
                            Console.WriteLine("Bot '{0}' won the game with the score: {1}. Total moves: {2}", stat.WinnerName, stat.FinalScore, stat.TotalMoves);
                            _taskResult.SetResult(stat);
                        }
                    }
                };
            });//, cancelToken.Token);

            /* Console.ReadLine();

             if (botRunner != null && !botRunner.IsFinished)
             {
                 botRunner.Cancel();
                 cancelToken.Cancel();
                 Console.WriteLine("Game was canceled");
             }
             else Console.WriteLine("Press enter to exit");
             Console.ReadLine();*/
        }

    }
}