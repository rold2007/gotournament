using GoTournament.Interface;

using Moq;

using Xunit;

namespace GoTournament.UnitTest
{
    using System;
    using System.Diagnostics;

    using GoTournament.Factory;

    public class ProcessManagerFactoryTests
    {
        [Fact]
        public void ProcessWrapperFactoryCtorTest()
        {
            IProcessManagerFactory factory = null;
            try
            {
                factory = new ProcessManagerFactory(null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: processProxy", ex.Message);
            }
            var proxy = new Mock<IProcessProxy>();
            factory = new ProcessManagerFactory(proxy.Object);
            Assert.NotNull(factory);
        }

        [Fact]
        public void ProcessWrapperFactoryCreateTest()
        {
            var proxy = new Mock<IProcessProxy>();
            ProcessStartInfo startInfo = null;
            var process = new Mock<IProcessWrapper>();
            proxy.Setup(s => s.Start(It.IsAny<ProcessStartInfo>()))
                .Callback<ProcessStartInfo>(c => startInfo = c)
                .Returns(()=>process.Object);
            IProcessManagerFactory factory = new ProcessManagerFactory(proxy.Object);
            var wrapper = factory.Create("bot.exe", "args");
            Assert.NotNull(wrapper);
            Assert.NotNull(startInfo);
            Assert.Equal("bot.exe", startInfo.FileName);
            Assert.Equal("args", startInfo.Arguments);
            Assert.Equal(true, startInfo.RedirectStandardInput);
            Assert.Equal(true, startInfo.RedirectStandardOutput);
            Assert.Equal(false, startInfo.UseShellExecute);
            Assert.Equal(true, startInfo.CreateNoWindow);
            proxy.VerifyAll();
            process.VerifyAll();
        }
    }
}
