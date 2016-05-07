namespace GoTournament
{
    using System;
    using System.Diagnostics;

    public class ProcessWrapper : IProcessWrapper
    {
        private readonly Process process;

        public ProcessWrapper(Process process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));
            this.process = process;
        }

        public void WriteData(string data, params object[] args)
        {
            this.process.StandardInput.WriteLine(data, args);
        }

        public void BeginOutputReadLine()
        {
            this.process.BeginOutputReadLine();
        }

        public event DataReceivedEventHandler OutputDataReceived
        {
            add
            {
                this.process.OutputDataReceived += value;
            }
            remove
            {
                this.process.OutputDataReceived -= value;
            }
        }

        public void Dispose()
        {
            this.process.Dispose();
        }
    }
}