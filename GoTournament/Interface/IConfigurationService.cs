namespace GoTournament.Interface
{
    using GoTournament.Model;

    public interface IConfigurationService
    {
        string CurrentDirectoryPath { get; }

        void SerializeGameResult(GameResult result, string fileName);

        T ReadConfig<T>(string relativePath);

        string GetAdjudicatorBinaryPath();
    }
}