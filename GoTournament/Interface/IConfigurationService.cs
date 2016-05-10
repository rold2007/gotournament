namespace GoTournament.Interface
{
    using GoTournament.Model;

    public interface IConfigurationService
    {
        void SerializeGameResult(GameResult result, string fileName);
        T ReadConfig<T>(string relativePath);
        string GetAdjudicatorBinaryPath();
    }
}