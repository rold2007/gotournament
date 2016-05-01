namespace GoTournament.Interface
{
    using GoTournament.Model;

    public interface IConfigurationReader
    {
        BotInstance ReadBotInstance(string name);

        BotKind ReadBotKind(string name);

        Tournament ReadTournament(string name);
    }
}