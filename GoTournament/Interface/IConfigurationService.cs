using GoTournament.Model;

namespace GoTournament.Interface
{
    public interface IConfigurationService
    {
        BotInstance ReadBotInstance(string name);
        BotKind ReadBotKind(string name);
        Tournament ReadTournament(string name);
    }
}