namespace GoTournament.Service
{
    using System.IO;
    using System.Reflection;

    using GoTournament.Interface;

    public class ConfigurationService : IConfigurationService
    {
        private readonly string currentDirectoryPath;

        private readonly IJsonService jsonService;

        private readonly IFileService fileService;

        public ConfigurationService(IJsonService jsonService, IFileService fileService)
        {
            this.jsonService = jsonService;
            this.fileService = fileService;
            this.currentDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public ConfigurationService() : this(new JsonService(), new FileService()) { }

        public void SerializeGameResult(GameResult result, string fileName)
        {
            this.fileService.FileWriteAllText(this.GetAbsolutePath(fileName), this.jsonService.SerializeObject(result));
        }

        public T ReadConfig<T>(string relativePath)
        {
            var absolutePath = this.GetAbsolutePath(relativePath);
            if (!this.fileService.FileExists(absolutePath))
                throw new FileNotFoundException("Could not found file", absolutePath);
            return this.jsonService.DeserializeObject<T>(this.fileService.FileReadAllText(absolutePath));
        }

        private string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(this.currentDirectoryPath, relativePath + ".json");
        }
    }
}