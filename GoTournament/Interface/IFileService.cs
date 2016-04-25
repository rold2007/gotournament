namespace GoTournament.Interface
{
    public interface IFileService
    {
        bool FileExists(string path);

        void FileWriteAllText(string filePath, string content);

        string FileReadAllText(string filePath);
    }
}