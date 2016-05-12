namespace GoTournament
{
    using System.Diagnostics;

    using GoTournament.Interface;

    public class DebugLogger : ILogger
    {
        public void WriteInfo(string message, params object[] args)
        {
            Debug.WriteLine("Info: " + message, args);
        }

        public void WriteWarning(string message, params object[] args)
        {
            Debug.WriteLine("Warning: " + message, args);
        }

        public void WriteError(string message, params object[] args)
        {
            Debug.WriteLine("Error: " + message, args);
        }
    }
}