using GoTournament.Interface;

namespace GoTournament.Factory
{
    public class ProcessWrapperFactory : IProcessWrapperFactory
    {
        private readonly IProcessProxy processProxy;

        public ProcessWrapperFactory(IProcessProxy processProxy)
        {
            this.processProxy = processProxy;
        }

        public IProcessWrapper Create(string binaryPath, string args)
        {
            return new ProcessWrapper(this.processProxy, binaryPath, args);
        }
    }
}
