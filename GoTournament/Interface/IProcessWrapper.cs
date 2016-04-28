using System;

namespace GoTournament.Interface
{
    public interface IProcessWrapper : IDisposable
    {
        Action<string> DataReceived { get; set; }
        void WriteData(string data);
        void WriteData(string data, params object[] args);
    }
}