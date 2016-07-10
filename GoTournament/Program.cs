namespace GoTournament
{
    using System;
    using System.Linq;
    using GoTournament.Factory;
    using GoTournament.Interface;
    using GoTournament.Service;
    using SimpleInjector;

    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify tournament file name in the arguments. Optionally it can be set amount of game cycles in the second argument\nFor example: Duel.exe DaisyVsMadison 10");
                return;
            }

            try
            {
                RunGame(Bootstrap(), args);
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

        private static void RunGame(ISimpleInjectorWrapper container, string[] args)
        {
            string gamesCount = (args.Length > 1) ? args[1] : string.Empty;
            IDuelInitializer initializer = new DuelInitializer(container, args.First(), gamesCount);
            initializer.Run();
        }
    }
}
