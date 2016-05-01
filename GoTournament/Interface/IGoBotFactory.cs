namespace GoTournament.Interface
{
    using GoTournament.Model;

    public interface IGoBotFactory
    {
        IGoBot CreateBotInstance(BotKind kind, string botInstanceName);
    }
}