namespace GoTournament.UnitTest
{
    using System;

    using GoTournament.Factory;
    using GoTournament.Interface;
    using GoTournament.Model;

    using Moq;

    using Xunit;

    public class GoBotFactoryTests
    {
        [Fact]
        public void GoBotFactoryCtorTest()
        {
            IGoBotFactory botFactory;
            try
            {
                botFactory = new GoBotFactory(null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: simpleInjector", ex.Message);
            }

            var injector = new Mock<ISimpleInjectorWrapper>();
            botFactory = new GoBotFactory(injector.Object);
            Assert.NotNull(botFactory);
        }

        [Fact]
        public void GoBotFactoryCreateBotInstanceTest()
        {
            var injector = new Mock<ISimpleInjectorWrapper>();
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists("gnugo.exe")).Returns(() => true);
            fileService.Setup(s => s.PathCombine(It.IsAny<string>(), It.IsAny<string>())).Returns(() => "gnugo.exe");
            injector.Setup(s => s.GetInstance<IFileService>()).Returns(() => fileService.Object);
            var processFactory = new Mock<IProcessManagerFactory>();
            processFactory.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(() => new Mock<IProcessManager>().Object);
            var confServ = new Mock<IConfigurationService>();
            injector.Setup(s => s.GetInstance<IConfigurationService>()).Returns(() => confServ.Object);
            injector.Setup(s => s.GetInstance<IProcessManagerFactory>()).Returns(() => processFactory.Object);

            IGoBotFactory botFactory = new GoBotFactory(injector.Object);
            BotKind kind = new BotKind { FullClassName = "GoTournament.GnuGoBot", BinaryPath = "bot.exe" };
            IGoBot result = botFactory.CreateBotInstance(kind, "superName");
            Assert.NotNull(result);
            Assert.Equal("superName", result.Name);
            BotKind kind2 = new BotKind { FullClassName = "GoTournament.NotExistedGoBot", BinaryPath = "bot.exe" };
            IGoBot result2 = botFactory.CreateBotInstance(kind2, "superName");
            Assert.Null(result2);
        }
    }
}