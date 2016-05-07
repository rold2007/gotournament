using System;

namespace GoTournament.Interface
{
    public interface IProcessManager : IDisposable
    {
        Action<string> DataReceived { get; set; }
        void WriteData(string data, params object[] args);
    }
}