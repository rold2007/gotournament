namespace GoTournament.Service
{
    using System;
    using GoTournament.Interface;
    using GoTournament.Model;

    public class ConfigurationReader : IConfigurationReader
    {
        private readonly IConfigurationService configurationService;

        public ConfigurationReader(IConfigurationService configurationService)
        {
            if (configurationService == null)
            {
                throw new ArgumentNullException(nameof(configurationService));
            }

            this.configurationService = configurationService;
        }

        public Duel ReadDuel(string name)
        {
            return this.configurationService.ReadConfig<Duel>("Configuration\\Duel\\" + name);
        }

        public BotInstance ReadBotInstance(string name)
        {
            return this.configurationService.ReadConfig<BotInstance>("Configuration\\BotInstance\\" + name);
        }

        public BotKind ReadBotKind(string name)
        {
            return this.configurationService.ReadConfig<BotKind>("Configuration\\BotKind\\" + name);
        }
    }
}