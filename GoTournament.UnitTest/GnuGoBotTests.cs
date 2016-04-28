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

    using Xunit.Sdk;

    public class GnuGoBotTests
    {
        [Fact]
        public void GnuGoBotCtorTest()
        {
            IGoBot bot = null;
            try
            {
                bot = new GnuGoBot(process: null, name: null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: process", ex.Message);
            }
            var process = new Mock<IProcessWrapper>();
            try
            {
                bot = new GnuGoBot(process.Object, null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: name", ex.Message);
            }
            bot = new GnuGoBot(process.Object, "Goodman");
            Assert.NotNull(bot);
            //Assert.Equal("Goodman", bot.Name);
            Assert.NotNull(bot.MovePerformed);
            //Assert.Equal(19, bot.BoardSize);

            try
            {
                bot = new GnuGoBot(binaryPath:null, name:null, fileService:null, processProxy:null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: binaryPath", ex.Message);
            }
            try
            {
                bot = new GnuGoBot("C:\\bot.exe", "BotName", fileService: null, processProxy:null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: fileService", ex.Message);
            }
            var fileService = new Mock<IFileService>();
            fileService.Setup(s => s.FileExists(It.IsAny<string>())).Returns(() => true);
            fileService.Setup(s => s.FileExists("noFile")).Returns(() => false);
            try
            {
                bot = new GnuGoBot("noFile", "BotName", fileService.Object, processProxy:null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: processProxy", ex.Message);
            }
            var processProxy = new Mock<IProcessProxy>();
            try
            {
                bot = new GnuGoBot("noFile", "BotName", fileService.Object, processProxy.Object);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(FileNotFoundException), ex);
                Assert.Equal("Bot binnary not found,", ex.Message);
            }
            bot = new GnuGoBot("someFile", "BotName", fileService.Object, processProxy.Object);
                Assert.NotNull(bot);

        }

        [Fact]
        public void BoardSizeTest()
        {
            var processWrapper = new Mock<IProcessWrapper>();
            var bot = new GnuGoBot(processWrapper.Object, "BotName");
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
            var processWrapper = new Mock<IProcessWrapper>();
            var bot = new GnuGoBot(processWrapper.Object, "BotName");
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
            var processWrapper = new Mock<IProcessWrapper>();
            var writeDataList = new List<string>();
            bool disposed = false;
            processWrapper.Setup(s => s.WriteData(It.IsAny<string>())).Callback<string>(s => writeDataList.Add(s));
            processWrapper.Setup(s => s.Dispose()).Callback(() => disposed = true);
            var bot = new GnuGoBot(processWrapper.Object, "BotName");
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

            bot = new GnuGoBot(processWrapper.Object, "BotName");
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
            var processWrapper = new Mock<IProcessWrapper>();
            var fakeProcess = new FakeProcessWrapper(processWrapper.Object);
            var writeDataList = new List<string>();
            processWrapper.Setup(s => s.WriteData(It.IsAny<string>())).Callback<string>(s => writeDataList.Add(s));
            
            var bot = new GnuGoBot(fakeProcess, "BotName");
            Assert.NotNull(fakeProcess.DataReceived);
            bot.StartGame(false);
            bot.PlaceMove(Move.Parse("A1"));
            fakeProcess.DataReceived("resign");
            Assert.Equal(2, writeDataList.Count);
            Assert.Equal("black A1", writeDataList.ElementAt(0));
            Assert.Equal("genmove white", writeDataList.ElementAt(1));
        }

        public class FakeProcessWrapper : IProcessWrapper
        {
            private readonly IProcessWrapper process;

            public FakeProcessWrapper(IProcessWrapper process)
            {
                this.process = process;
            }
            public void Dispose() { }

            public Action<string> DataReceived { get; set; }

            public void WriteData(string data)
            {
                process.WriteData(data);
            }

            public void WriteData(string data, params object[] args)
            {
                process.WriteData(data, args);
            }
        }
    }
}