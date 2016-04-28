namespace GoTournament.Interface
{
    using System.Diagnostics;

    public interface IProcessProxy
    {
        Process Start(ProcessStartInfo processStartInfo);
    }
}