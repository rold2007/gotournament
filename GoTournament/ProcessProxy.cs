namespace GoTournament
{
    using System.Diagnostics;

    using GoTournament.Interface;

    public class ProcessProxy : IProcessProxy
    {
        public IProcessWrapper Start(ProcessStartInfo processStartInfo)
        {
            return new ProcessWrapper(Process.Start(processStartInfo));
        }
    }
}