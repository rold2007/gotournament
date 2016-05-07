namespace GoTournament.UnitTest
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    public class FakeProcessWrapper : IProcessWrapper
    {
        public event DataReceivedEventHandler OutputDataReceived;

        public void BeginOutputReadLine() { }

        public void Dispose() { }

        public void WriteData(string data, params object[] args) { }

        public void RaiseOutputDataReceived(string data)
        {
            var type = typeof(DataReceivedEventArgs);
            var ctorInfo = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(String) },
                null);
            DataReceivedEventArgs obj = (DataReceivedEventArgs)(ctorInfo.Invoke(new object[] {data}));

            if (this.OutputDataReceived != null)
                this.OutputDataReceived.Invoke(this, obj );
        }
    }
}