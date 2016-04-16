using System.IO;
using GoTournament.Interface;

namespace GoTournament
{
    public class FileService : IFileService
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }


    }
}