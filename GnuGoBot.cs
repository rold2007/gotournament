using System;
using System.Linq;
using GoTournament.Interface;

namespace GoTournament
{
    public class GnuGoBot : IDisposable, IGoBot
    {
        private readonly string _binaryPath;
        private int _boardSize = 19;
        private bool _black;
        private string _genmoveCommand;
        private IProccessWrapper _process;
        private bool _disposed;
        private int _level;
        private int _passCount = 0;
        private int _maxPassCount = 10;

        public GnuGoBot(string binaryPath, string name)
        {
            _binaryPath = binaryPath;
            Name = name;
        }

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

        public void SetBoardSize(int size)
        {

        }

        public void StartGame(bool goesFirst)
        {
            _black = goesFirst;
            _genmoveCommand = _black ? "genmove black" : "genmove white";
            _process = new ProccessWrapper(_binaryPath, "--mode gtp") { DataReceived = OnDataReceived };
            InitializeGame();
            if (_black)
                PerformMove();
        }

        public void PlaceMove(Move move)
        {
            if (move == null) throw new ArgumentNullException(nameof(move));
            if(_process == null) throw new ObjectDisposedException("proccess");
            _process.WriteData((_black ? "white " : "black ") + move);
            Console.ForegroundColor = _black ? ConsoleColor.Blue : ConsoleColor.DarkYellow;
            Console.WriteLine((_black ? "white " : "black ") + move);
            Console.Clear();
            _process.WriteData("showboard");
            //Thread.Sleep(1000);
            PerformMove();
        }

        private void OnDataReceived(string s)
        {
            if (s == null) return;
            if (s.ToLower().Contains("resign"))
            {
                if (Resign != null)
                    Resign();
                else return;
            }
            if (s.Replace(" ", "").Replace("=", "").Any()) //for debugging purpose
                Console.WriteLine(s);
            if (MovePerformed != null)
            {
                var move = Move.Parse(s);
                if (move != null)
                {
                    if (move.Pass)
                        _passCount++;
                    else _passCount = 0;
                    if (_passCount > MaxPassCount)
                    {
                        if (PassLimitPassed != null)
                            PassLimitPassed();
                        return;
                    }
                    MovePerformed(move);
                }
            }
        }

        private void PerformMove()
        {
            _process.WriteData(_genmoveCommand);
        }

        private void InitializeGame()
        {
            if (_boardSize != 19)
                _process.WriteData("boardsize {0}", _boardSize);
            _process.WriteData("level {0}", _level);
        }

        public string Name { get; private set; }

        public int MaxPassCount
        {
            get { return _maxPassCount; }
            set { _maxPassCount = value; }
        }

        public Action<Move> MovePerformed { get; set; }
        public Action Resign { get; set; }
        public Action PassLimitPassed { get; set; }

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