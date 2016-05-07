namespace GoTournament.Interface
{
    using System.Diagnostics;

    public interface IProcessProxy
    {
        IProcessWrapper Start(ProcessStartInfo processStartInfo);
    }
}