using System.IO;
using System.Reflection;
using GoTournament.Interface;
using GoTournament.Service;

namespace GoTournament
{
    public class FileService : IFileService
    {
        private readonly string _currentDirectoryPath;
        private readonly IJsonService _jsonService;
        public FileService(IJsonService jsonService)
        {
            _jsonService = jsonService;
            _currentDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public FileService() : this(new JsonService()) { }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void SerializeGameResult(GameResult result, string fileName)
        {
            File.WriteAllText(GetAbsolutePath(fileName), _jsonService.SerializeObject(result));
        }

        public T ReadConfig<T>(string relativePath)
        {
            var absolutePath = GetAbsolutePath(relativePath);
            if(!FileExists(absolutePath))
                throw new FileNotFoundException("Could not found file", absolutePath);
            return _jsonService.DeserializeObject<T>(File.ReadAllText(absolutePath));
        }

        private string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(_currentDirectoryPath, relativePath+".json");
        }
    }
}