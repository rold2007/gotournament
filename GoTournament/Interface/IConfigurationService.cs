using GoTournament.Model;

namespace GoTournament.Interface
{
    public interface IConfigurationService
    {
        void SerializeGameResult(GameResult result, string fileName);
        T ReadConfig<T>(string relativePath);
    }

    public interface IConfigurationReader
    {
        BotInstance ReadBotInstance(string name);

        BotKind ReadBotKind(string name);

        Tournament ReadTournament(string name);
    }
}