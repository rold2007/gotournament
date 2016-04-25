using System;
using System.Diagnostics;
using GoTournament.Interface;

namespace GoTournament
{
    public class ProcessWrapper : IDisposable, IProcessWrapper
    {
        private readonly string binnaryPath;
        private readonly string arguments;
        private bool disposed;
        private Process process;

        public ProcessWrapper(string binnaryPath, string arguments)
        {
            this.binnaryPath = binnaryPath;
            this.arguments = arguments;
            CreateProccess();
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

        private void CreateProccess()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = this.binnaryPath,
                Arguments = this.arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            try
            {
                this.process = Process.Start(processStartInfo);

                if (this.process != null)
                {
                    this.process.OutputDataReceived += this.ProcessOutputDataReceived;
                    this.process.BeginOutputReadLine();
                }
            }
            catch (Exception ex)
            {
                throw new AggregateException(string.Format("Failed to run process '{0}' with arguments '{1}", this.binnaryPath, this.arguments), new[] {ex});
            }
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (this.disposed) return;
            if (DataReceived != null)
                DataReceived(e.Data);
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