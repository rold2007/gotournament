using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GoTournament.Interface;

namespace GoTournament
{
    public class GnuGoBot : IGoBot
    {
        #region Fields

        private IProcessWrapper _process;
        private readonly string _binaryPath;
        private int _boardSize = 19;
        private bool _black;
        private string _genmoveCommand;
        private bool _disposed;
        private int _level;
        private bool _waitingForMoveGenerating;
        #endregion

        #region Ctors

        public GnuGoBot(string binaryPath, string name, IFileService fileService)
        {
            if (!fileService.FileExists(binaryPath))
                throw new FileNotFoundException("Bot binnary not found,", binaryPath);
            _binaryPath = binaryPath;
            Name = name;
            //To reduce null reference checking 
            MovePerformed = delegate { };
        }

        public GnuGoBot(string binaryPath, string name) : this(binaryPath, name, new FileService()) { }

        public GnuGoBot(string name): this(Properties.Settings.Default.gnuBotPath, name) { }

        #endregion

        #region Properties

        public int BoardSize
        {
            get { return _boardSize; }
            set
            {
                if (_process != null)
                    throw new NotSupportedException("Board size could be set only before start of the game");
                if (value > 19 || value < 1)
                    throw new NotSupportedException("Board size could be from 1 to 19");
                _boardSize = value;
            }
        }

        public int Level
        {
            get { return _level; }
            set
            {
                if (_process != null)
                    throw new NotSupportedException("Level could be set only before start of the game");
                _level = value;
            }
        }

        public string Name { get; private set; }

        public Action<Move> MovePerformed { get; set; }

        #endregion

        #region Public methods

        public void StartGame(bool goesFirst)
        {
            _black = goesFirst;
            _genmoveCommand = _black ? "genmove black" : "genmove white";
            _process = new ProcessWrapper(_binaryPath, "--mode gtp") { DataReceived = OnDataReceived };
            InitializeGame();
            if (_black)
                PerformMove();
        }

        public void PlaceMove(Move move)
        {
            if (move == null) throw new ArgumentNullException(nameof(move));
            if (_process == null) throw new ObjectDisposedException("proccess");

            _process.WriteData((_black ? "white " : "black ") + move);
            PerformMove();
        }

        #endregion

        #region Private methods

        private void OnDataReceived(string s)
        {
            //debug
            //  File.AppendAllText(Name+".txt", string.Format("{0}|{1}|{2}", DateTime.Now.ToString("h:mm:ss.ff"), s, Environment.NewLine));

            if (!_waitingForMoveGenerating) return;
            if (s == null) return;
            var move = Move.Parse(s);
            if (move != null)
            {
                MovePerformed(move);
                _waitingForMoveGenerating = false;
            }

        }

        private void PerformMove()
        {
            _waitingForMoveGenerating = true;
            _process.WriteData(_genmoveCommand);
        }

        private void InitializeGame()
        {
            if (_boardSize != 19)
                _process.WriteData("boardsize {0}", _boardSize);
            _process.WriteData("level {0}", _level);
        }

        #endregion

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
                        _process.WriteData("quit");
                        _process.Dispose();
                        _process = null;
                    }
                }
                _disposed = true;
            }
        }

        ~GnuGoBot()
        {
            Dispose(false);
        }

        #endregion
    }

}