using System;
using System.Diagnostics;
using GoTournament.Interface;

namespace GoTournament
{
    public class ProcessWrapper : IProcessWrapper
    {
        private bool disposed;
        private Process process;

        public ProcessWrapper(IProcessProxy processProxy, string binnaryPath, string arguments)
        {
            if (processProxy == null)
                throw new ArgumentNullException(nameof(processProxy));
            this.CreateProccess(processProxy, binnaryPath, arguments);
        }

        public Action<string> DataReceived { get; set; }

        public void WriteData(string data)
        {
            this.process.StandardInput.WriteLine(data);
        }

        public void WriteData(string data, params object[] args)
        {
            this.process.StandardInput.WriteLine(data, args);
        }

        private void CreateProccess(IProcessProxy processProxy, string binnaryPath, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = binnaryPath,
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            try
            {
                this.process = processProxy.Start(processStartInfo);

                if (this.process != null)
                {
                    this.process.OutputDataReceived += this.ProcessOutputDataReceived;
                    this.process.BeginOutputReadLine();
                }
            }
            catch (Exception ex)
            {
                throw new AggregateException(string.Format("Failed to run process '{0}' with arguments '{1}", binnaryPath, arguments), new[] {ex});
            }
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (this.disposed) return;
            if (this.DataReceived != null) this.DataReceived(e.Data);
        }

        #region IDisposable pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.process != null)
                    {
                        this.process.Dispose();
                        this.process = null;
                    }
                }
                this.disposed = true;
            }
        }

        ~ProcessWrapper()
        {
            Dispose(false);
        }

        #endregion
    }
}