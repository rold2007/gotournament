namespace GoTournament.Interface
{
    public interface ILogger
    {
        void WriteInfo(string message, params object[] args);

        void WriteWarning(string message, params object[] args);

        void WriteError(string message, params object[] args);
    }
}
