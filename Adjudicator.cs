using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoTournament.Interface;

namespace GoTournament
{
    public class Adjudicator : IAdjudicator
    {
        private IProcessWrapper _process;
        private bool _disposed;
        private const string Black = "black ";
        private const string White = "white ";
        private bool _whiteGoes;
        private bool _waitingForShowBoard;
        private bool _waitingForMoveResult;
        private readonly List<string> _boardParts = new List<string>(26);
        private bool _lastInputMoveIsPass;
        private Move _lastReceivedMove;

        #region Ctors

        public Adjudicator(string binaryPath, int boardsize, IFileService fileService)
        {
            if (!fileService.FileExists(binaryPath))
                throw new FileNotFoundException("Adjudicator binnary not found,", binaryPath);
            _process = new ProcessWrapper(binaryPath, "--mode gtp") { DataReceived = OnDataReceived };
            BoardUpdated = delegate { };
            if (boardsize != 19)
            {
                _process.WriteData("boardsize {0}", boardsize);
            }
            WhiteMoveValidated = delegate { };
            BlackMoveValidated = delegate { };
            Resigned = delegate { };
        }

        private void UpdateBoard()
        {
            _waitingForShowBoard = true;
            _process.WriteData("showboard");
        }

        public Adjudicator(string binaryPath, int boardsize) : this(binaryPath, boardsize, new FileService()) { }
        public Adjudicator(string binaryPath) : this(binaryPath, 19, new FileService()) { }

        #endregion

        public void BlackMoves(Move move)
        {
            if (_whiteGoes) // its white turn now
            {
                Debug.WriteLine("Unexpected behaviour: black bot makes move when it is not its turn"); //TODO write in logs
                return;
            }
            MakeMove(move, Black);
        }

        public void WhiteMoves(Move move)
        {
            if (!_whiteGoes) //its black turn now
            {
                Debug.WriteLine("Warning: white bot makes move when it is not its turn"); //TODO write in logs
                return;
            }
            MakeMove(move, White);
        }

        private void MakeMove(Move move, string color)
        {
            if (_waitingForMoveResult)
            {
                Debug.WriteLine("Warning: Previous move was not yet validated. Ignoring {0} from {1}", move, color); //TODO write in logs
                return; // ignore because waiting for last turn results
            }
            if (move == null) throw new ArgumentNullException(nameof(move));
            if (_process == null) throw new ObjectDisposedException("proccess");

            if (_lastInputMoveIsPass && move.Pass)
            {
                Resigned(EndGameReason.ConsecutivePass, !_whiteGoes);
                return;
            }
            _lastInputMoveIsPass = move.Pass;
            _waitingForMoveResult = true;
            _process.WriteData(color + move);
            _lastReceivedMove = move;
        }

        private void OnDataReceived(string line)
        {
            Debug.WriteLine(line);
            if (_waitingForMoveResult)
            {
                if (line == "? illegal move" || line == "? invalid coordinate")
                {
                    Resigned(EndGameReason.InvalidMove, _whiteGoes);
                    _waitingForMoveResult = false;
                }
                if (line == "= ")
                {
                    _waitingForMoveResult = false;
                    _whiteGoes = !_whiteGoes; // switch who has to move next
                    PushValidationResult();
                }
            }

            if (_waitingForShowBoard)
            {
                if (line.Contains(" A ")) //For case when board is 1x1 or more
                {
                    if (_boardParts.Count > 2 && _boardParts.Any(l => l.Contains(" ")) && _boardParts.Last().Contains("1"))
                    {
                        if (_boardParts.First().Contains(" A "))
                            _boardParts.RemoveAt(0);
                        int firstLineId = _boardParts.IndexOf(_boardParts.First(l => l.Contains(" A ")));
                        if (firstLineId > 0)
                            _boardParts.RemoveRange(0, firstLineId);
                        _boardParts.Add(line);
                        _waitingForShowBoard = false;
                        BoardUpdated(_boardParts);
                        _boardParts.Clear();
                    }
                }
                _boardParts.Add(line);
            }
        }

        private void PushValidationResult()
        {
            if (!_whiteGoes) //vice versa because not yet switched
                WhiteMoveValidated(_lastReceivedMove);
            else BlackMoveValidated(_lastReceivedMove);
            UpdateBoard();
        }

        public Action<Move> WhiteMoveValidated { get; set; }
        public Action<Move> BlackMoveValidated { get; set; }
        /// <summary>
        /// Resign because of wrong move. True means white
        /// </summary>
        public Action<EndGameReason, bool> Resigned { get; set; }
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

    public enum EndGameReason { None, MoveTimeOut, Resign, InvalidMove, ConsecutivePass }
}