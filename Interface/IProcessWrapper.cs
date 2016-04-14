using System;

namespace GoTournament.Interface
{
    public interface IProcessWrapper
    {
        Action<string> DataReceived { get; set; }
        void WriteData(string data);
        void WriteData(string data, params object[] args);
        void Dispose();
    }
}