namespace GoTournament.Service
{
    using System;
    using System.IO;
    using System.Reflection;
    using GoTournament.Interface;
    using GoTournament.Model;

    public class ConfigurationService : IConfigurationService
    {
        private readonly string currentDirectoryPath;

        private readonly IJsonService jsonService;

        private readonly IFileService fileService;

        public ConfigurationService(IJsonService jsonService, IFileService fileService)
        {
            if (jsonService == null)
                throw new ArgumentNullException(nameof(jsonService));
            if (fileService == null)
                throw new ArgumentNullException(nameof(fileService));
            this.jsonService = jsonService;
            this.fileService = fileService;
            this.currentDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        
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

        public string GetAdjudicatorBinnaryPath()
        {
            return Path.Combine(this.currentDirectoryPath, @"Adjudicator\adjudicator.exe");
        }
    }
}