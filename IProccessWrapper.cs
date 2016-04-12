using System;

namespace GoTournament
{
    public interface IProccessWrapper
    {
        Action<string> DataReceived { get; set; }
        void WriteData(string data);
        void WriteData(string data, params object[] args);
        void Dispose();
    }
}