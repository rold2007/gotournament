using GoTournament.Interface;
using System;

namespace GoTournament.Factory
{

    public class ProcessManagerFactory : IProcessManagerFactory
    {
        private readonly IProcessProxy processProxy;

        public ProcessManagerFactory(IProcessProxy processProxy)
        {
            if (processProxy == null)
                throw new ArgumentNullException(nameof(processProxy));
            this.processProxy = processProxy;
        }

        public IProcessManager Create(string binaryPath, string args)
        {
            return new ProcessManager(this.processProxy, binaryPath, args);
        }
    }
}
