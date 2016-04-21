using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using GoTournament.Interface;
using GoTournament.Model;

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
        private TaskCompletionSource<IEnumerable<string>> _taskBoard;
        private TaskCompletionSource<string> _taskScore;
        private readonly Tournament _tournament;
        private readonly IFileService _fileService;


        #region Ctors

        public Adjudicator(string binaryPath, Tournament tournament, IFileService fileService)
        {
            if (!fileService.FileExists(binaryPath))
                throw new FileNotFoundException("Adjudicator binnary not found,", binaryPath);
            _process = new ProcessWrapper(binaryPath, "--mode gtp") { DataReceived = OnDataReceived };
            _tournament = tournament;
            _fileService = fileService;
            _tournament = tournament;
            if (_tournament.BoardSize != 19)
            {
                _process.WriteData("boardsize {0}", _tournament.BoardSize);
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

        public Adjudicator(string binaryPath, Tournament tournament) : this(binaryPath, tournament, new FileService()) { }
        public Adjudicator(Tournament tournament) : this(Properties.Settings.Default.adjudicatorPath, tournament) { }


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
            if (_taskScore != null && (line.StartsWith("= W+") || (line.StartsWith("= B+"))))
            {
                _taskScore.SetResult(line.Trim('=', ' '));
                _taskScore = null;
                return;
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
                    if (_taskBoard != null)
                        _taskBoard.SetResult(_boardParts);
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

        private void RaiseResigned(EndGameReason reason, bool whiteFinishedGame)
        {
            var statistic = new GameResult
            {
                EndReason = reason,
                TotalMoves = _movesCount,
                BoardSize = _tournament.BoardSize
            };
            if (reason == EndGameReason.Resign)
            {
                statistic.WinnerName = whiteFinishedGame ? _tournament.BlackBot : _tournament.WhiteBot;
                statistic.LooserName = !whiteFinishedGame ? _tournament.BlackBot : _tournament.WhiteBot;
            }
            else
            {
                var score = GetFinalScore().Result;
                statistic.FinalScore = score.Item1;
                if (score.Item2 == Color.White)
                {
                    statistic.WinnerName = _tournament.WhiteBot;
                    statistic.LooserName = _tournament.BlackBot;
                }
                else if (score.Item2 == Color.Black)
                {
                    statistic.WinnerName = _tournament.BlackBot;
                    statistic.LooserName = _tournament.WhiteBot;
                }
            }
            if (GenerateLastBoard)
                statistic.FinalBoard = GetLastBoard().Result;
            if (SaveGameResults)
            {
                var fileName = string.Format("{0}{1}", _tournament.Name, DateTime.Now.ToString("yyyy-mm-dd_HH-mm-ss"));
                _process.WriteData("printsgf " + fileName + ".sgf");
                statistic.ResultsFileName = fileName;
                _fileService.SerializeGameResult(statistic, fileName);
            }
            Resigned(statistic);
        }

        private async Task<Tuple<int, Color>> GetFinalScore()
        {
            _taskScore = new TaskCompletionSource<string>();
            _process.WriteData("final_score");
            var scoreLine = await _taskScore.Task;
            var score = 0;
            if (!int.TryParse(scoreLine.Substring(2), NumberStyles.AllowDecimalPoint, new NumberFormatInfo { NumberDecimalSeparator = "." }, out score))
                Debug.WriteLine("WARNING: could not parse final score: {0}", scoreLine);
            Color color = Color.None;
            if (scoreLine.StartsWith("W"))
                color = Color.White;
            else if (scoreLine.StartsWith("B"))
                color = Color.Black;
            return Tuple.Create(score, color);


        }

        private async Task<string> GetLastBoard()
        {
            _taskBoard = new TaskCompletionSource<IEnumerable<string>>();
            UpdateBoard();
            return string.Join("\n", await _taskBoard.Task);
        }

        public Action<Move> WhiteMoveValidated { get; set; }
        public Action<Move> BlackMoveValidated { get; set; }
        /// <summary>
        /// Resign because of wrong move. True means white
        /// </summary>
        public Action<GameResult> Resigned { get; set; }
        public Action<IEnumerable<string>> BoardUpdated { get; set; }
        public bool SaveGameResults { get; set; }
        public bool GenerateLastBoard { get; set; }
        
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