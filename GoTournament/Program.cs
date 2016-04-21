using System.Linq;
using System.Threading;
using GoTournament.Interface;
using GoTournament.Model;
using GoTournament.Service;

namespace GoTournament
{
    using System;
    using System.Threading.Tasks;


    public class TournamentInitializer
    {
        private readonly string _name;
        private readonly IConfigurationService _configurationService;

        public TournamentInitializer(string name, IConfigurationService configurationService)
        {
            if (configurationService == null) throw new ArgumentNullException(nameof(configurationService));
            _name = name;
            _configurationService = configurationService;
        }

        public TournamentInitializer(string name) : this(name, new ConfigurationService()) { }

        public void Run()
        {
            var scenario = _configurationService.ReadTournament(_name);
            var blackInstance = _configurationService.ReadBotInstance(scenario.BlackBot);
            var whiteInstance = _configurationService.ReadBotInstance(scenario.WhiteBot);
            BotKind blackKind = _configurationService.ReadBotKind(blackInstance.Kind);
            BotKind whiteKind = _configurationService.ReadBotKind(whiteInstance.Kind);
            IGoBot blackBot = GetInstance(blackKind, blackInstance.Name);
            IGoBot whiteBot = GetInstance(whiteKind, whiteInstance.Name);
            blackBot.BoardSize = scenario.BoardSize;
            whiteBot.BoardSize = scenario.BoardSize;
            blackBot.Level = blackInstance.Level;
            whiteBot.Level = whiteInstance.Level;

            RunBotRunner(blackBot, whiteBot, scenario);
        }
        
        public IGoBot GetInstance(BotKind kind, string botInstanceName)
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
            var finished = false;
            var sync = new object();
            var judge = new Adjudicator(tourn)
            {
                BoardUpdated = board =>
                {
                    lock (sync)
                    {
                        if (!finished)
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
            IBotRunner botRunner = null;

            var cancelToken = new CancellationTokenSource();
            Task.Run(() =>
            {
                botRunner = new BotRunner(judge, botBlack, botWhite)
                {
                    EndGame = stat =>
                    {
                        lock (sync)
                        {
                            finished = true;
                            Console.WriteLine("Bot '{0}' won the game with the score: {1}. Total moves: {2}", stat.WinnerName, stat.FinalScore, stat.TotalMoves);
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

public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var initializer = new TournamentInitializer(args.First());
                initializer.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }

        
    }
}
