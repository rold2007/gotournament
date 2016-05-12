namespace GoTournament.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using GoTournament.Interface;
    using GoTournament.Model;

    using Moq;

    using Xunit;

    public class AdjudicatorTests
    {
        [Fact]
        public void AdjudicatorBlackWhiteMovesTest()
        {
            var injector = new Mock<ISimpleInjectorWrapper>();
            var confService = new Mock<IConfigurationService>();
            injector.Setup(s => s.GetInstance<IConfigurationService>()).Returns(() => confService.Object);
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists(It.IsAny<string>())).Returns(() => true);
            injector.Setup(s => s.GetInstance<IFileService>()).Returns(() => fileService.Object);
            var processFactory = new Mock<IProcessManagerFactory>();
            var fakeProcess = new FakeProcessManager(new Mock<IProcessManager>().Object);

            processFactory.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(() => fakeProcess);
            injector.Setup(s => s.GetInstance<IProcessManagerFactory>()).Returns(() => processFactory.Object);
            var logger = new Mock<ILogger>();
            var logs = new List<string>();
            injector.Setup(s => s.GetInstance<ILogger>()).Returns(() => logger.Object);
            logger.Setup(s => s.WriteWarning(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback<string, object[]>(
                    (s, o) =>
                        {
                            logs.Add(string.Format(s, o));
                        });
            var adjudicator = new Adjudicator(injector.Object, new Tournament());
            try
            {
                adjudicator.BlackMoves(null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: move", ex.Message);
            }

            Assert.True(!logs.Any());
            adjudicator.WhiteMoves(null);

            Assert.True(logs.Any());
            Assert.Equal("White bot makes move when it is not its turn", logs[0]);
            GameResult gameResult = null;
            adjudicator.Resigned = result => gameResult = result;
            adjudicator.BlackMoves(Move.Parse("resign"));
            Assert.Equal(EndGameReason.Resign, gameResult.EndReason);

            adjudicator = new Adjudicator(injector.Object, new Tournament());

            adjudicator.BlackMoves(Move.Parse("a6"));
            fakeProcess.DataReceived("= ");
            adjudicator.BlackMoves(null);
            adjudicator.WhiteMoves(Move.Parse("f3"));
            adjudicator.WhiteMoves(Move.Parse("f3"));
            Assert.Equal("Previous move was not yet validated. Ignoring f3 from white ", logs.Last());
            fakeProcess.DataReceived("= ");
            adjudicator.BlackMoves(Move.Parse("pass"));
            fakeProcess.DataReceived("= ");
            gameResult = null;
            adjudicator.Resigned = result => gameResult = result;
            Task.Run(
                () =>
                    {
                        Thread.Sleep(1000);
                        fakeProcess.DataReceived("= B+8.0");
                    });

            adjudicator.WhiteMoves(Move.Parse("pass"));
            Assert.NotNull(gameResult);
            Assert.Equal(EndGameReason.ConsecutivePass, gameResult.EndReason);
            Assert.Equal(8, gameResult.FinalScore);
        }

        [Fact]
        public void AdjudicatorOnDataReceivedTest()
        {
            var injector = new Mock<ISimpleInjectorWrapper>();
            var processFactory = new Mock<IProcessManagerFactory>();
            var fakeProcess = new FakeProcessManager(new Mock<IProcessManager>().Object);
            processFactory.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(() => fakeProcess);
            injector.Setup(s => s.GetInstance<IProcessManagerFactory>()).Returns(() => processFactory.Object);
            var confService = new Mock<IConfigurationService>();
            injector.Setup(s => s.GetInstance<IConfigurationService>()).Returns(() => confService.Object);
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists(It.IsAny<string>())).Returns(() => true);
            injector.Setup(s => s.GetInstance<IFileService>()).Returns(() => fileService.Object);
            var adjudicator = new Adjudicator(injector.Object, new Tournament());
            GameResult gameResult = null;
            adjudicator.Resigned = result => gameResult = result;

            adjudicator.BlackMoves(Move.Parse("Z99"));
            Task.Run(
                () =>
                    {
                        Thread.Sleep(1000);
                        fakeProcess.DataReceived("= W+9.0");
                    });
            fakeProcess.DataReceived("? illegal move");
            Assert.NotNull(gameResult);
            Assert.Equal(EndGameReason.InvalidMove, gameResult.EndReason);
            Assert.Equal(9, gameResult.FinalScore);
            injector.VerifyAll();
            processFactory.VerifyAll();
            fileService.VerifyAll();
        }

        [Fact]
        public void AdjudicatorSaveGameResultsTest()
        {
            var injector = new Mock<ISimpleInjectorWrapper>();
            var processFactory = new Mock<IProcessManagerFactory>();

            var processManager = new Mock<IProcessManager>();
            bool disposed = false;
            processManager.Setup(s => s.Dispose()).Callback(() => disposed = true);
            var fakeProcess = new FakeProcessManager(processManager.Object);
            List<string> processWrittenData = new List<string>();
            processManager.Setup(s => s.WriteData(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback<string, object[]>(
                    (s, o) => processWrittenData.Add(s));
            processFactory.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(() => fakeProcess);
            injector.Setup(s => s.GetInstance<IProcessManagerFactory>()).Returns(() => processFactory.Object);
            var confService = new Mock<IConfigurationService>();

            GameResult saveGameResult = null;
            string savedFileName = null;
            confService.Setup(s => s.SerializeGameResult(It.IsAny<GameResult>(), It.IsAny<string>()))
                .Callback<GameResult, string>(
                    (r, f) =>
                        {
                            saveGameResult = r;
                            savedFileName = f;
                        });
            injector.Setup(s => s.GetInstance<IConfigurationService>()).Returns(() => confService.Object);
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists(It.IsAny<string>())).Returns(() => true);
            injector.Setup(s => s.GetInstance<IFileService>()).Returns(() => fileService.Object);
            var logger = new Mock<ILogger>();
            var logs = new List<string>();
            injector.Setup(s => s.GetInstance<ILogger>()).Returns(() => logger.Object);
            logger.Setup(s => s.WriteWarning(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback<string, object[]>(
                    (s, o) =>
                        {
                            logs.Add(string.Format(s, o));
                        });
            var adjudicator = new Adjudicator(injector.Object, new Tournament());
            GameResult gameResult = null;
            adjudicator.Resigned = result => gameResult = result;
            adjudicator.SaveGameResults = true;
            
            adjudicator.BlackMoves(Move.Parse("&99"));
            Task.Run(
                () =>
                    {
                        Thread.Sleep(1000);
                        fakeProcess.DataReceived("= W+aaa");
                    });
            fakeProcess.DataReceived("? invalid coordinate");
            Assert.NotNull(gameResult);
            Assert.Equal(EndGameReason.InvalidMove, gameResult.EndReason);
            Assert.Equal(0, gameResult.FinalScore);
            Assert.Equal(saveGameResult, gameResult);
            Assert.True(logs.Count == 1);
            Assert.Equal("Could not parse final score: W+aaa", logs.First());
            Assert.Equal(4, processWrittenData.Count);
            Assert.Equal("boardsize {0}", processWrittenData.ElementAt(0));
            Assert.Equal("black &99", processWrittenData.ElementAt(1));
            Assert.Equal("final_score", processWrittenData.ElementAt(2));
            Assert.True(processWrittenData.ElementAt(3).StartsWith("printsgf 2"));

            Assert.False(disposed);
            adjudicator.Dispose();
            Assert.True(disposed);
            Assert.Equal("quit", processWrittenData.ElementAt(4));

            injector.VerifyAll();
            processFactory.VerifyAll();
            fileService.VerifyAll();
        }

        [Fact]
        public void AdjudicatorGenerateLastBoardTest()
        {
            var injector = new Mock<ISimpleInjectorWrapper>();
            var processFactory = new Mock<IProcessManagerFactory>();
            var processManager = new Mock<IProcessManager>();
            var fakeProcess = new FakeProcessManager(processManager.Object);
            List<string> processWrittenData = new List<string>();
            processManager.Setup(s => s.WriteData(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback<string, object[]>(
                    (s, o) => processWrittenData.Add(s));
            processFactory.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(() => fakeProcess);
            injector.Setup(s => s.GetInstance<IProcessManagerFactory>()).Returns(() => processFactory.Object);
            var confService = new Mock<IConfigurationService>();
            injector.Setup(s => s.GetInstance<IConfigurationService>()).Returns(() => confService.Object);
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists(It.IsAny<string>())).Returns(() => true);
            injector.Setup(s => s.GetInstance<IFileService>()).Returns(() => fileService.Object);
            var adjudicator = new Adjudicator(injector.Object, new Tournament() { BoardSize = 2, WhiteBot = "weiss", BlackBot = "schwarz" });
            GameResult gameResult = null;
            adjudicator.Resigned = result => gameResult = result;
            adjudicator.GenerateLastBoard = true;
            adjudicator.BoardUpdated = delegate { };

            adjudicator.BlackMoves(Move.Parse("a1"));
            fakeProcess.DataReceived("= ");
            fakeProcess.DataReceived("   A B");
            fakeProcess.DataReceived(" 2 O O 2     WHITE (O) has captured 2 stones");
            fakeProcess.DataReceived(" 1 . . 1     BLACK (X) has captured 0 stones");
            fakeProcess.DataReceived("   A B");
            adjudicator.WhiteMoves(Move.Parse("b1"));
            fakeProcess.DataReceived("= ");
            fakeProcess.DataReceived("   A B");
            fakeProcess.DataReceived(" 2 O O 2     WHITE (O) has captured 2 stones");
            fakeProcess.DataReceived(" 1 . . 1     BLACK (X) has captured 0 stones");
            fakeProcess.DataReceived("   A B");
            Task.Run(
                () =>
                    {
                        Thread.Sleep(2000);
                        fakeProcess.DataReceived("   A B");
                        fakeProcess.DataReceived("   A B");
                        fakeProcess.DataReceived(" 2 O O 2     WHITE (O) has captured 2 stones");
                        fakeProcess.DataReceived(" 1 . . 1     BLACK (X) has captured 0 stones");
                        fakeProcess.DataReceived("   A B");
                    });
            adjudicator.BlackMoves(Move.Parse("resign"));
            Assert.Equal(EndGameReason.Resign, gameResult.EndReason);
            Assert.Equal("   A B\n 2 O O 2     WHITE (O) has captured 2 stones\n 1 . . 1     BLACK (X) has captured 0 stones\n   A B", gameResult.FinalBoard);
            Assert.Equal(6, processWrittenData.Count);
            Assert.Equal("boardsize {0}", processWrittenData.ElementAt(0));
            Assert.Equal("black a1", processWrittenData.ElementAt(1));
            Assert.Equal("showboard", processWrittenData.ElementAt(2));
            Assert.Equal("white b1", processWrittenData.ElementAt(3));
            Assert.Equal("showboard", processWrittenData.ElementAt(4));
            Assert.Equal("showboard", processWrittenData.ElementAt(5));

            injector.VerifyAll();
            processFactory.VerifyAll();
            fileService.VerifyAll();
        }
    }
}