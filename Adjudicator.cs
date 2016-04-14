using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GoTournament.Interface;

namespace GoTournament
{
    public class Adjudicator : IAdjudicator, IDisposable
    {
        private IProcessWrapper _process;
        private bool _disposed;
        private const string Black = "black ";
        private const string White = "white ";
        private bool _whiteGoes;
        private bool _waitingForShowBoard;
        private bool _waitingForMoveResult;
        private readonly List<string> _boardParts = new List<string>(26); 

        #region Ctors

        public Adjudicator(string binaryPath, int boardsize, IFileService fileService)
        {
            if (!fileService.FileExists(binaryPath))
                throw new FileNotFoundException("Bot binnary not found,", binaryPath);
            _process = new ProcessWrapper(binaryPath, "--mode gtp") { DataReceived = OnDataReceived };
            BoardUpdated = delegate { };
            if (boardsize != 19)
            {
                _process.WriteData("boardsize {0}", boardsize);
            }
            WhiteMoveValidated = delegate { };
            BlackMoveValidated = delegate { };
        }

        private void UpdateBoard()
        {
            _waitingForShowBoard = true;
            _process.WriteData("showboard");
        }

        public Adjudicator(string binaryPath, int boardsize) : this(binaryPath, boardsize, new FileService()) { }
        public Adjudicator(string binaryPath) : this(binaryPath, 19, new FileService()) { }

        #endregion

        public bool BlackMoves(Move move)
        {
            if (_whiteGoes) return false;
            MakeMove(move, Black);
            return true;
        }

        public bool WhiteMoves(Move move)
        {
            if (!_whiteGoes) return false;
            MakeMove(move, White);
            return true;
        }

        private void MakeMove(Move move, string color)
        {
            if (move == null) throw new ArgumentNullException(nameof(move));
            if (_process == null) throw new ObjectDisposedException("proccess");
            _waitingForMoveResult = true;
            _process.WriteData(color + move);
        }

        private void OnDataReceived(string line)
        {
            Debug.WriteLine(line);
            if (_waitingForMoveResult)
            {
                _waitingForMoveResult = false;
                if (line == "? illegal move" || line == "? invalid coordinate") PushValidationResult(false);
                if (line == "= ")
                {
                    _whiteGoes = !_whiteGoes; // swtich who has to move next
                    PushValidationResult(true);
                    UpdateBoard();
                }
            } else
                if (_waitingForShowBoard)
                {
                    if (line.Contains(" A ")) //For case when board is 1x1 or more
                    {
                        if (_boardParts.Count > 2 && _boardParts.Any(l=>l.Contains(" ")) && _boardParts.Last().Contains("1"))
                        {
                            int firstLineId = _boardParts.IndexOf(_boardParts.First(l => l.Contains(" A ")));
                            if(firstLineId>0)
                                _boardParts.RemoveRange(0, firstLineId-1);
                            _boardParts.Add(line);
                            _waitingForShowBoard = false;
                            BoardUpdated(_boardParts);
                            _boardParts.Clear();
                        }
                    }
                    _boardParts.Add(line);
                }
        }

        private void PushValidationResult(bool result)
        {
            if (_whiteGoes)
                WhiteMoveValidated(result);
            else BlackMoveValidated(result);
        }

        public Action<bool> WhiteMoveValidated { get; set; }
        public Action<bool> BlackMoveValidated { get; set; }
        public Action<IEnumerable<string>> BoardUpdated { get; set; }


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

        ~Adjudicator()
        {
            Dispose(false);
        }

        #endregion

    }
}