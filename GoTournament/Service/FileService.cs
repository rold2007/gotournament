namespace GoTournament.Service
{
    using System.IO;

    using GoTournament.Interface;

    public class FileService : IFileService
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void FileWriteAllText(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        public string FileReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public string PathCombine(string first, string second)
        {
            return Path.Combine(first, second);
        }
    }
}