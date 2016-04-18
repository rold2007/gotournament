using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
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
        private int _movesCount;
        private static TaskCompletionSource<bool> _tcs;


        #region Ctors

        public Adjudicator(string binaryPath, int boardsize, IFileService fileService)
        {
            if (!fileService.FileExists(binaryPath))
                throw new FileNotFoundException("Adjudicator binnary not found,", binaryPath);
            _process = new ProcessWrapper(binaryPath, "--mode gtp") { DataReceived = OnDataReceived };
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
            _boardParts.Clear();
            _process.WriteData("showboard");
        }

        public Adjudicator(string binaryPath, int boardsize) : this(binaryPath, boardsize, new FileService()) { }
        public Adjudicator(string binaryPath) : this(binaryPath, 19, new FileService()) { }
        public Adjudicator(int boardsize) : this(Properties.Settings.Default.gnuBotPath, boardsize) { }
        public Adjudicator() : this(Properties.Settings.Default.gnuBotPath, 19) { }


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

            if (!move.Normal)
            {
                if (_lastInputMoveIsPass && move.Pass)
                {
                    RaiseResigned(EndGameReason.ConsecutivePass, !_whiteGoes);
                    return;
                }
                RaiseResigned(move.ToEndGameReason(), !_whiteGoes);
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
                    RaiseResigned(EndGameReason.InvalidMove, _whiteGoes);
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
                HandleBoardReading(line);
            }
        }

        private void HandleBoardReading(string line)
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
                    if (BoardUpdated != null)
                        BoardUpdated(_boardParts);
                    if (_tcs != null)
                        _tcs.SetResult(true);
                }
            }
            _boardParts.Add(line);
        }

        private void PushValidationResult()
        {
            _movesCount++;
            if (!_whiteGoes) //vice versa because not yet switched
                WhiteMoveValidated(_lastReceivedMove);
            else BlackMoveValidated(_lastReceivedMove);
            if (BoardUpdated != null) // If there is no subscription, don't even get this info
                UpdateBoard();
        }

        private async void RaiseResigned(EndGameReason reason, bool whiteFinishedGame)
        {
            if (GenerateSgfFile)
                _process.WriteData("printsgf trace{0}.sgf", DateTime.Now.ToString("yyyy-mm-dd_HH-mm-ss"));
            var board = GenerateLatBoard ? await GetLastBoard() : string.Empty;
            Resigned(new GameStatistic { EndReason = reason, TotalMoves = _movesCount, WhiteFinishedGame = whiteFinishedGame, FinalBoard = board });
        }

        private async Task<string> GetLastBoard()
        {
            _tcs = new TaskCompletionSource<bool>();
            UpdateBoard();
            await _tcs.Task;
            return string.Join("\n", _boardParts);
        }

        public Action<Move> WhiteMoveValidated { get; set; }
        public Action<Move> BlackMoveValidated { get; set; }
        /// <summary>
        /// Resign because of wrong move. True means white
        /// </summary>
        public Action<GameStatistic> Resigned { get; set; }
        public Action<IEnumerable<string>> BoardUpdated { get; set; }
        public bool GenerateSgfFile { get; set; }
        public bool GenerateLatBoard { get; set; }


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