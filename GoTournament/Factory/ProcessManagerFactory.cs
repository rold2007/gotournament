namespace GoTournament.Factory
{
    using System;
    using GoTournament.Interface;

    public class ProcessManagerFactory : IProcessManagerFactory
    {
        private readonly IProcessProxy processProxy;

        public ProcessManagerFactory(IProcessProxy processProxy)
        {
            if (processProxy == null)
            {
                throw new ArgumentNullException(nameof(processProxy));
            }

            this.processProxy = processProxy;
        }

        public IProcessManager Create(string binaryPath, string args)
        {
            return new ProcessManager(this.processProxy, binaryPath, args);
        }
    }
}
