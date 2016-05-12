namespace GoTournament.Interface
{
    using System;

    public interface IProcessManager : IDisposable
    {
        Action<string> DataReceived { get; set; }

        void WriteData(string data, params object[] args);
    }
}