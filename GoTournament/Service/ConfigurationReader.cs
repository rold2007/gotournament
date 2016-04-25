using GoTournament.Interface;
using GoTournament.Model;

namespace GoTournament.Service
{
    public class ConfigurationReader : IConfigurationReader
    {
        private readonly IConfigurationService fileService;

        public ConfigurationReader(IConfigurationService fileService)
        {
            this.fileService = fileService;
        }

        public ConfigurationReader(): this(new ConfigurationService()) { }

        public Tournament ReadTournament(string name)
        {
            return this.fileService.ReadConfig<Tournament>("Tournament\\" + name);
        }

        public BotInstance ReadBotInstance(string name)
        {
            return this.fileService.ReadConfig<BotInstance>("BotInstance\\" + name);
        }

        public BotKind ReadBotKind(string name)
        {
            return this.fileService.ReadConfig<BotKind>("BotKind\\" + name);
        }
    }
}