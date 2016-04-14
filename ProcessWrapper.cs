using System;
using System.Diagnostics;
using GoTournament.Interface;

namespace GoTournament
{
    public class ProcessWrapper : IDisposable, IProcessWrapper
    {
        private readonly string _binnaryPath;
        private readonly string _arguments;
        private bool _disposed;
        private Process _process;

        public ProcessWrapper(string binnaryPath, string arguments)
        {
            _binnaryPath = binnaryPath;
            _arguments = arguments;
            CreateProccess();
        }

        public Action<string> DataReceived { get; set; }

        public void WriteData(string data)
        {
            _process.StandardInput.WriteLine(data);
        }

        public void WriteData(string data, params object[] args)
        {
            _process.StandardInput.WriteLine(data, args);
        }

        private void CreateProccess()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _binnaryPath,
                Arguments = _arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            try
            {
                _process = Process.Start(processStartInfo);

                if (_process != null)
                {
                    _process.OutputDataReceived += _process_OutputDataReceived;
                    _process.BeginOutputReadLine();
                }
            }
            catch (Exception ex)
            {
                throw new AggregateException(string.Format("Failed to run process '{0}' with arguments '{1}", _binnaryPath, _arguments), new[] {ex});
            }
        }

        private void _process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (_disposed) return;
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
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_process != null)
                    {
                        _process.Dispose();
                        _process = null;
                    }
                }
                _disposed = true;
            }
        }

        ~ProcessWrapper()
        {
            Dispose(false);
        }

        #endregion
    }
}