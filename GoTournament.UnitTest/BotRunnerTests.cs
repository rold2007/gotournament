namespace GoTournament.UnitTest
{
    using System;
    using GoTournament.Interface;
    using GoTournament.Model;
    using Moq;
    using Xunit;

    public class BotRunnerTests
    {
        [Fact]
        public void BotRunnerCtorWithExceptionsTest()
        {
            var adjudicator = new Mock<IAdjudicator>();
            var blackBot = new Mock<IGoBot>();
            var whiteBot = new Mock<IGoBot>();

            IBotRunner botRunner;
            try
            {
                botRunner = new BotRunner(null, null, null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: adjudicator", ex.Message);
            }

            try
            {
                botRunner = new BotRunner(adjudicator.Object, null, null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: black", ex.Message);
            }

            try
            {
                botRunner = new BotRunner(adjudicator.Object, blackBot.Object, null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: white", ex.Message);
            }

            try
            {
                botRunner = new BotRunner(adjudicator.Object, blackBot.Object, blackBot.Object);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentException), ex);
                Assert.Equal("Two instances cannot point to the same object", ex.Message);
            }

            try
            {
                botRunner = new BotRunner(adjudicator.Object, blackBot.Object, whiteBot.Object);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(NotSupportedException), ex);
                Assert.Equal("Give unique names to the bot instances. Be creative.", ex.Message);
            }

            blackBot.Setup(s => s.Name).Returns(() => "Alice");
            whiteBot.Setup(s => s.Name).Returns(() => "Alice");
            try
            {
                botRunner = new BotRunner(adjudicator.Object, blackBot.Object, whiteBot.Object);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(NotSupportedException), ex);
                Assert.Equal("Give unique names to the bot instances. Be creative.", ex.Message);
            }
        }

        [Fact]
        public void BotRunnerCtorTest()
        {
            var injector = new Mock<ISimpleInjectorWrapper>();
            var confService = new Mock<IConfigurationService>();
            injector.Setup(s => s.GetInstance<IConfigurationService>()).Returns(() => confService.Object);
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists(It.IsAny<string>())).Returns(() => true);
            injector.Setup(s => s.GetInstance<IFileService>()).Returns(() => fileService.Object);
            var processFactory = new Mock<IProcessManagerFactory>();
            processFactory.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(() => new Mock<IProcessManager>().Object);
            injector.Setup(s => s.GetInstance<IProcessManagerFactory>()).Returns(() => processFactory.Object);
            var adjudicator = new Adjudicator(injector.Object, new Duel());
            var blackBot = new Mock<IGoBot>();
            var whiteBot = new Mock<IGoBot>();
            blackBot.Setup(s => s.Name).Returns(() => "Alice");
            whiteBot.Setup(s => s.Name).Returns(() => "Jack");
            bool? whiteStarted = null;
            bool? blackStarted = null;
            whiteBot.Setup(s => s.StartGame(It.IsAny<bool>())).Callback<bool>(c => whiteStarted = c);
            blackBot.Setup(s => s.StartGame(It.IsAny<bool>())).Callback<bool>(c => blackStarted = c);

            IBotRunner botRunner = new BotRunner(adjudicator, blackBot.Object, whiteBot.Object);

            Assert.Equal(false, whiteStarted);
            Assert.Equal(true, blackStarted);

            Assert.False(botRunner.IsFinished);
            GameResult gameResult = null;
            botRunner.EndGame = result => gameResult = result;
            Assert.Null(gameResult);
            adjudicator.Resigned(new GameResult { EndReason = EndGameReason.Resign });

            Assert.True(botRunner.IsFinished);
            Assert.NotNull(gameResult);
            Assert.Equal(EndGameReason.Resign, gameResult.EndReason);

            Assert.NotNull(botRunner);
            Assert.NotNull(botRunner.EndGame);
            blackBot.VerifyAll();
            whiteBot.VerifyAll();
        }

        [Fact]
        public void CancelTests()
        {
            var adjudicator = new Mock<IAdjudicator>();
            var blackBot = new Mock<IGoBot>();
            var whiteBot = new Mock<IGoBot>();
            int count = 0;
            adjudicator.Setup(s => s.Dispose()).Callback(() => count++);
            blackBot.Setup(s => s.Dispose()).Callback(() => count++);
            whiteBot.Setup(s => s.Dispose()).Callback(() => count++);

            blackBot.Setup(s => s.Name).Returns(() => "Alice");
            whiteBot.Setup(s => s.Name).Returns(() => "Tony");

            IBotRunner botRunner = new BotRunner(adjudicator.Object, blackBot.Object, whiteBot.Object);
            Assert.Equal(0, count);

            botRunner.Cancel();

            Assert.Equal(3, count);
            adjudicator.VerifyAll();
            blackBot.VerifyAll();
            whiteBot.VerifyAll();
        }
    }
}
