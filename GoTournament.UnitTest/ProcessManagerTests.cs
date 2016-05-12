namespace GoTournament.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using GoTournament.Interface;

    using Moq;

    using Xunit;

    public class ProcessManagerTests
    {
        [Fact]
        public void ProcessManagerCtorTest()
        {
            IProcessManager manager = null;
            try
            {
                manager = new ProcessManager(null, null, null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: processProxy", ex.Message);
            }

            Assert.Null(manager);
            var proxy = new Mock<IProcessProxy>();
            ProcessStartInfo startInfo = null;
            var process = new Mock<IProcessWrapper>();
            bool disposed = false;
            process.Setup(s => s.Dispose()).Callback(() => disposed = true);
            proxy.Setup(s => s.Start(It.IsAny<ProcessStartInfo>()))
                .Callback<ProcessStartInfo>(c => startInfo = c)
                .Returns(() => process.Object);
            manager = new ProcessManager(proxy.Object, null, null);
            Assert.NotNull(manager);
            manager.Dispose();
            Assert.True(disposed);
            manager = new ProcessManager(proxy.Object, null, null);
            Assert.NotNull(manager);
            proxy.Setup(s => s.Start(It.IsAny<ProcessStartInfo>()))
               .Returns(() => null);
            try
            {
                manager = new ProcessManager(proxy.Object, "too", "args");
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(AggregateException), ex);
                Assert.Equal("Failed to run process 'too' with arguments 'args'", ex.Message);
            }

            process.VerifyAll();
            proxy.VerifyAll();
        }

        [Fact]
        public void ProcessManagerDataReceivedTest()
        {
            var proxy = new Mock<IProcessProxy>();
            ProcessStartInfo startInfo = null;
            var process = new FakeProcessWrapper();

            proxy.Setup(s => s.Start(It.IsAny<ProcessStartInfo>()))
                .Callback<ProcessStartInfo>(c => startInfo = c)
                .Returns(() => process);
            IProcessManager manager = new ProcessManager(proxy.Object, null, null);
            List<string> data = new List<string>();
            manager.DataReceived = s => data.Add(s);
            process.RaiseOutputDataReceived("hello");
            process.RaiseOutputDataReceived("hello v2");
            Assert.Equal("hello", data[0]);
            Assert.Equal("hello v2", data[1]);
            proxy.VerifyAll();
        }

        [Fact]
        public void ProcessManagerWriteDataTest()
        {
            var proxy = new Mock<IProcessProxy>();
            ProcessStartInfo startInfo = null;
            var process = new Mock<IProcessWrapper>();
            string data = null;
            object[] args = null;
            process.Setup(s => s.WriteData(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>(
                (s, o) =>
                    {
                        data = s;
                        args = o;
                    });
            proxy.Setup(s => s.Start(It.IsAny<ProcessStartInfo>()))
                .Callback<ProcessStartInfo>(c => startInfo = c)
                .Returns(() => process.Object);
            IProcessManager manager = new ProcessManager(proxy.Object, null, null);
            manager.WriteData("data", 1, "ok");
            Assert.Equal("data", data);
            Assert.Equal(1, args[0]);
            Assert.Equal("ok", args[1]);
            process.VerifyAll();
        }
    }
}