namespace GoTournament
{
    using System.Diagnostics;

    using GoTournament.Interface;

    public class ProcessProxy : IProcessProxy
    {
        public Process Start(ProcessStartInfo processStartInfo)
        {
            return Process.Start(processStartInfo);
        }
    }
}