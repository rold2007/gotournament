using GoTournament.Interface;
using GoTournament.Service;
using Moq;
using Xunit;

namespace GoTournament.UnitTest
{
    using System;
    using System.IO;

    using GoTournament.Model;

    public class ConfigurationServiceTests
    {
        [Fact]
        public void ConfigurationServiceCtor()
        {
            IConfigurationService configurationService = new ConfigurationService();
            Assert.NotNull(configurationService);
            Assert.IsType(typeof(ConfigurationService), configurationService);
            var jsonService = new Mock<IJsonService>();
            var fileService = new Mock<IFileService>();
            configurationService = new ConfigurationService(jsonService.Object, fileService.Object);
            Assert.NotNull(configurationService);
            Assert.IsType(typeof(ConfigurationService), configurationService);
        }

        [Fact]
        public void SerializeGameResultTest()
        {
            var jsonService = new Mock<IJsonService>();
            var fileService = new Mock<IFileService>();
            IConfigurationService configurationService = new ConfigurationService(jsonService.Object, fileService.Object);
            jsonService.Setup(j => j.SerializeObject(It.IsAny<object>())).Returns(() => "JSON");
            string fileContent = null;
            string filePath = null;
            fileService.Setup(f => f.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>(
                    (path, cont) =>
                    {
                        fileContent = cont;
                        filePath = path;
                    });
            configurationService.SerializeGameResult(new GameResult(), "game");
            Assert.Equal("JSON", fileContent);
            Assert.True(filePath.EndsWith("game.json"));
            jsonService.VerifyAll();
            fileService.VerifyAll();
        }

        [Fact]
        public void ReadConfigTests()
        {
            var jsonService = new Mock<IJsonService>();
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists(It.IsAny<string>())).Returns(() => false);
            IConfigurationService configurationService = new ConfigurationService(jsonService.Object, fileService.Object);
            try
            {
                configurationService.ReadConfig<int>("name");
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(FileNotFoundException), ex);
            }

            fileService.Setup(s => s.FileExists(It.IsAny<string>())).Returns(() => true);
            string filePath = null;
            string fileContent = null;
            fileService.Setup(s => s.FileReadAllText(It.IsAny<string>())).Callback<string>(s=>filePath =s).Returns(() => "<CONFIGS>");
            jsonService.Setup(s => s.DeserializeObject<BotKind>(It.IsAny<string>())).Callback<string>(s=> fileContent = s)
                .Returns(new BotKind { Name = "teeest" });
            var result = configurationService.ReadConfig<BotKind>("tree");
            Assert.IsType(typeof(BotKind), result);
            Assert.True(result.Name== "teeest");
            Assert.Equal("<CONFIGS>", fileContent);
            Assert.True(filePath.EndsWith("tree.json"));
            jsonService.VerifyAll();
            fileService.VerifyAll();
        }
    }
}