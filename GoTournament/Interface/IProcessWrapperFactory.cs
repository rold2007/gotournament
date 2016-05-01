namespace GoTournament.Interface
{
    public interface IProcessWrapperFactory
    {
        IProcessWrapper Create(string binaryPath, string args);
    }
}