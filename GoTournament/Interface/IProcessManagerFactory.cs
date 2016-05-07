namespace GoTournament.Interface
{
    public interface IProcessManagerFactory
    {
        IProcessManager Create(string binaryPath, string args);
    }
}