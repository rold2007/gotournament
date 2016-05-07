namespace GoTournament.Factory
{
    using System;

    using GoTournament.Interface;
    using GoTournament.Model;

    public class GoBotFactory : IGoBotFactory
    {
        private readonly ISimpleInjectorWrapper simpleInjector;

        public GoBotFactory(ISimpleInjectorWrapper simpleInjector)
        {
            if (simpleInjector == null)
                throw new ArgumentNullException(nameof(simpleInjector));
            this.simpleInjector = simpleInjector;
        }

        public IGoBot CreateBotInstance(BotKind kind, string botInstanceName)
        {
            Type type = Type.GetType(kind.FullClassName);
            if (type != null)
                return (IGoBot)Activator.CreateInstance(type, this.simpleInjector, kind.BinaryPath, botInstanceName);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(kind.FullClassName);
                if (type != null)
                    return (IGoBot)Activator.CreateInstance(type, kind.BinaryPath, botInstanceName);
            }
            return null;
        }
    }
}