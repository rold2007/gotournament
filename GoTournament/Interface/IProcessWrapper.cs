namespace GoTournament.Interface
{
    using System.Diagnostics;

    public interface IProcessWrapper
    {
        event DataReceivedEventHandler OutputDataReceived;

        void BeginOutputReadLine();

        void Dispose();

        void WriteData(string data, params object[] args);
    }
}