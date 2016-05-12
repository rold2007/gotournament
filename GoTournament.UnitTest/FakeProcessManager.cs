namespace GoTournament.UnitTest
{
    using System;

    using GoTournament.Interface;

    public class FakeProcessManager : IProcessManager
    {
        private readonly IProcessManager process;

        public FakeProcessManager(IProcessManager process)
        {
            this.process = process;
        }
        
        public Action<string> DataReceived { get; set; }

        public void Dispose()
        {
            this.process.Dispose();
        }

        public void WriteData(string data)
        {
            this.process.WriteData(data);
        }

        public void WriteData(string data, params object[] args)
        {
            this.process.WriteData(data, args);
        }
    }
}