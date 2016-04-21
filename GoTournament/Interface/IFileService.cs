namespace GoTournament.Interface
{
    public interface IFileService
    {
        bool FileExists(string path);
        void SerializeGameResult(GameResult result, string fileName);
        T ReadConfig<T>(string relativePath);
    }
}