using System;
using System.Linq;

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

        public GnuGoBot(string binaryPath)
        {
            _binaryPath = binaryPath;
        }

        public void SetBoardSize(int size)
        {
            if (_process != null)
                throw new NotSupportedException("Board size could be set only before start of the game");
            if (size > 19 || size < 1)
                throw new NotSupportedException("Board size could be from 1 to 19");
            _boardSize = size;
        }

        public void StartGame(bool goesFirst)
        {
            _black = goesFirst;
            _genmoveCommand = _black ? "genmove black" : "genmove white";
            _process = new ProccessWrapper(_binaryPath, "--mode gtp") {DataReceived = OnDataReceived};
            InitializeGame();
            if (_black)
                PerformMove();
        }

        public void PlaceMove(Move move)
        {
            _process.WriteData((_black ? "white " : "black ") + move);
            Console.ForegroundColor = _black ? ConsoleColor.Blue : ConsoleColor.DarkYellow;
            Console.WriteLine((_black ? "white " : "black ") + move);
            Console.Clear();
            _process.WriteData("showboard");
            //Thread.Sleep(1000);
            PerformMove();
        }

        public void SetLevel(int level)
        {
            if(_process!=null)
                throw new NotSupportedException("Level could be set only before start of the game");
            _level = level;
        }

        private void OnDataReceived(string s)
        {
            if(s.Replace(" ","").Replace("=","").Any())
                Console.WriteLine(s);
            if (MovePerformed != null)
            {
                var move = Move.Parse(s);
                if (move != null)
                {
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
            if(_boardSize != 19)
                _process.WriteData("boardsize {0}", _boardSize);
            _process.WriteData("level {0}", _level);
        }
        
        
        public Action<Move> MovePerformed { get; set; }

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