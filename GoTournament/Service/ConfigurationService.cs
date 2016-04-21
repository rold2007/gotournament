using GoTournament.Interface;
using GoTournament.Model;

namespace GoTournament.Service
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IFileService _fileService;

        public ConfigurationService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public ConfigurationService(): this(new FileService()) { }

        public Tournament ReadTournament(string name)
        {
            return _fileService.ReadConfig<Tournament>("Tournament\\" + name);
        }

        public BotInstance ReadBotInstance(string name)
        {
            return _fileService.ReadConfig<BotInstance>("BotInstance\\" + name);
        }

        public BotKind ReadBotKind(string name)
        {
            return _fileService.ReadConfig<BotKind>("BotKind\\" + name);
        }
    }
}