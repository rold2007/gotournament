using System;
using GoTournament.Interface;
using GoTournament.Model;
using GoTournament.Service;
using Moq;
using Xunit;

namespace GoTournament.UnitTest
{
    
    public class ConfigurationReaderTests
    {
        [Fact]
        public void ConfigurationReaderCtorTest()
        {
            IConfigurationReader reader = null;
            try
            {
                reader = new ConfigurationReader(null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
            }
            Assert.Null(reader);
            var configurationService = new Mock<IConfigurationService>();
            reader = new ConfigurationReader(configurationService.Object);
            Assert.NotNull(reader);
            Assert.IsType(typeof(ConfigurationReader), reader);
           /* reader = new ConfigurationReader();
            Assert.NotNull(reader);
            Assert.IsType(typeof(ConfigurationReader), reader);*/
        }

        [Fact]
        public void ReadTests()
        {
            var configurationService = new Mock<IConfigurationService>();
            IConfigurationReader reader = new ConfigurationReader(configurationService.Object);
            string tournamentInput = null;
            string instanceInput = null;
            string kindInput = null;

            configurationService.Setup(s => s.ReadConfig<Tournament>(It.IsAny<string>()))
                .Callback<string>(d => tournamentInput = d)
                .Returns(() => new Tournament { Name = "TourName" });
            configurationService.Setup(s => s.ReadConfig<BotInstance>(It.IsAny<string>()))
                .Callback<string>(d => instanceInput = d)
                .Returns(() => new BotInstance { Name = "InstanceName" });
            configurationService.Setup(s => s.ReadConfig<BotKind>(It.IsAny<string>()))
                .Callback<string>(d => kindInput = d)
                .Returns(() => new BotKind { Name = "KindName" });

            var tour = reader.ReadTournament("daisy");
            Assert.NotNull(tour);
            Assert.Equal("TourName", tour.Name);
            Assert.Equal("Configuration\\Tournament\\daisy", tournamentInput);

            var instance = reader.ReadBotInstance("flower");
            Assert.NotNull(instance);
            Assert.Equal("InstanceName", instance.Name);
            Assert.Equal("Configuration\\BotInstance\\flower", instanceInput);

            var kind = reader.ReadBotKind("gnu");
            Assert.NotNull(kind);
            Assert.Equal("KindName", kind.Name);
            Assert.Equal("Configuration\\BotKind\\gnu", kindInput);

            configurationService.VerifyAll();
        }
    }
}
