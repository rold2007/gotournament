using System;
using GoTournament.Interface;
using Moq;
using Xunit;

namespace GoTournament.UnitTest
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using GoTournament.Model;

    using Xunit.Sdk;

    public class GnuGoBotTests
    {
        [Fact]
        public void GnuGoBotCtorTest()
        {
            IGoBot bot = null;
            try
            {
                bot = new GnuGoBot(null, null, null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: simpleInjector", ex.Message);
            }
            var injector = new Mock<ISimpleInjectorWrapper>();
            try
            {
                bot = new GnuGoBot(injector.Object, null, null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: binaryPath", ex.Message);
            }
            try
            {
                bot = new GnuGoBot(injector.Object, "bot.exe", null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: botInstanceName", ex.Message);
            }
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists("bot.exe")).Returns(() => true);
            injector.Setup(s => s.GetInstance<IFileService>()).Returns(() => fileService.Object);
            var processFactory = new Mock<IProcessManagerFactory>();
            processFactory.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => new Mock<IProcessManager>().Object);
            injector.Setup(s => s.GetInstance<IProcessManagerFactory>()).Returns(() => processFactory.Object);
            bot = new GnuGoBot(injector.Object, "bot.exe", "Goodman");
            Assert.NotNull(bot);
            //Assert.Equal("Goodman", bot.Name);
            Assert.NotNull(bot.MovePerformed);
            //Assert.Equal(19, bot.BoardSize);


            fileService.Setup(s => s.FileExists("noFile")).Returns(() => false);
            try
            {
                bot = new GnuGoBot(injector.Object, "noFile", "BotName");
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(FileNotFoundException), ex);
                Assert.Equal("Bot binnary not found,", ex.Message);
            }
        }

        [Fact]
        public void BoardSizeTest()
        {
            var injector = new Mock<ISimpleInjectorWrapper>();
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists("bot.exe")).Returns(() => true);
            injector.Setup(s => s.GetInstance<IFileService>()).Returns(() => fileService.Object);
            var processFactory = new Mock<IProcessManagerFactory>();
            processFactory.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => new Mock<IProcessManager>().Object);
            injector.Setup(s => s.GetInstance<IProcessManagerFactory>()).Returns(() => processFactory.Object);
            var bot = new GnuGoBot(injector.Object, "bot.exe", "BotName");
            Assert.Equal(19, bot.BoardSize);
            try
            {
                bot.BoardSize = 0;
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(NotSupportedException), ex);
                Assert.Equal("Board size could be from 1 to 19", ex.Message);
            }
            bot.BoardSize = 5;
            Assert.Equal(5, bot.BoardSize);
            bot.StartGame(true);
            try
            {
                bot.BoardSize = 6;
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(NotSupportedException), ex);
                Assert.Equal("Board size could be set only before start of the game", ex.Message);
            }
        }

        [Fact]
        public void LevelTest()
        {
            var injector = new Mock<ISimpleInjectorWrapper>();
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists("bot.exe")).Returns(() => true);
            injector.Setup(s => s.GetInstance<IFileService>()).Returns(() => fileService.Object);
            var processFactory = new Mock<IProcessManagerFactory>();
            processFactory.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => new Mock<IProcessManager>().Object);
            injector.Setup(s => s.GetInstance<IProcessManagerFactory>()).Returns(() => processFactory.Object);
            var bot = new GnuGoBot(injector.Object, "bot.exe", "BotName");
            Assert.Equal(-1, bot.Level);
            bot.Level = 5;
            Assert.Equal(5, bot.Level);
            bot.StartGame(true);
            try
            {
                bot.Level = 6;
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(NotSupportedException), ex);
                Assert.Equal("Level could be set only before start of the game", ex.Message);
            }
        }

        [Fact]
        public void PlaceMoveTest()
        {
            var writeDataList = new List<string>();
            bool disposed = false;
            var injector = new Mock<ISimpleInjectorWrapper>();
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists("bot.exe")).Returns(() => true);
            injector.Setup(s => s.GetInstance<IFileService>()).Returns(() => fileService.Object);
            var processWrapper = new Mock<IProcessManager>();
            processWrapper.Setup(s => s.WriteData(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback<string, object[]>((s, a) => writeDataList.Add(s));
            processWrapper.Setup(s => s.Dispose()).Callback(() => disposed = true);

            var processFactory = new Mock<IProcessManagerFactory>();
            processFactory.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => processWrapper.Object);
            injector.Setup(s => s.GetInstance<IProcessManagerFactory>()).Returns(() => processFactory.Object);
            var bot = new GnuGoBot(injector.Object, "bot.exe", "BotName");
            try
            {
                bot.PlaceMove(null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: move", ex.Message);
            }
            bot.Dispose();
            Assert.Equal("quit", writeDataList.FirstOrDefault());
            Assert.Equal(true, disposed);
            try
            {
                bot.PlaceMove(Move.SpecialMove(MoveType.Resign));
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ObjectDisposedException), ex);
                Assert.Equal("Cannot access a disposed object.\r\nObject name: 'proccess'.", ex.Message);
            }

            bot = new GnuGoBot(injector.Object, "bot.exe", "BotName");
            try
            {
                bot.PlaceMove(Move.SpecialMove(MoveType.Resign));
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(NotSupportedException), ex);
                Assert.Equal("Invoke StartGame before placing the move", ex.Message);
            }
            bot.StartGame(false);
            bot.PlaceMove(Move.SpecialMove(MoveType.Resign));
            Assert.Equal("black Resign", writeDataList.ElementAt(1));
            Assert.Equal("genmove white", writeDataList.ElementAt(2));
        }

        [Fact]
        public void OnDataReceivedTest()
        {
            var processWrapper = new Mock<IProcessManager>();
            var fakeProcess = new FakeProcessManager(processWrapper.Object);
            var writeDataList = new List<string>();
            processWrapper.Setup(s => s.WriteData(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback<string, object[]>((s, a) => writeDataList.Add(s));
            var injector = new Mock<ISimpleInjectorWrapper>();

            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists("bot.exe")).Returns(() => true);
            injector.Setup(s => s.GetInstance<IFileService>()).Returns(() => fileService.Object);

            var processFactory = new Mock<IProcessManagerFactory>();
            processFactory.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(() => fakeProcess);
            injector.Setup(s => s.GetInstance<IProcessManagerFactory>()).Returns(() => processFactory.Object);

            var bot = new GnuGoBot(injector.Object, "bot.exe", "BotName");
            Assert.NotNull(fakeProcess.DataReceived);
            bot.StartGame(false);
            bot.PlaceMove(Move.Parse("A1"));
            fakeProcess.DataReceived("resign");
            Assert.Equal(2, writeDataList.Count);
            Assert.Equal("black A1", writeDataList.ElementAt(0));
            Assert.Equal("genmove white", writeDataList.ElementAt(1));
        }

    }
}