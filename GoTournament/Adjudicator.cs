using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GoTournament.Interface;
using GoTournament.Model;

namespace GoTournament
{
    using GoTournament.Service;

    public class Adjudicator : IAdjudicator
    {
        private IProcessWrapper process;
        private bool disposed;
        private const string Black = "black ";
        private const string White = "white ";
        private bool whiteGoes;
        private bool waitingForShowBoard;
        private bool waitingForMoveResult;
        private readonly List<string> boardParts = new List<string>(26);
        private bool lastInputMoveIsPass;
        private Move lastReceivedMove;
        private int movesCount;
        private TaskCompletionSource<IEnumerable<string>> taskBoard;
        private TaskCompletionSource<string> taskScore;
        private readonly Tournament tournament;
        private readonly IConfigurationService configurationService;

        #region Ctors

        public Adjudicator(string binaryPath, Tournament tournament, IConfigurationService configurationService, IFileService fileService)
        {
            if (!fileService.FileExists(binaryPath))
                throw new FileNotFoundException("Adjudicator binnary not found,", binaryPath);
            this.process = new ProcessWrapper(binaryPath, "--mode gtp") { DataReceived = OnDataReceived };
            this.tournament = tournament;
            this.configurationService = configurationService;
            this.tournament = tournament;
            if (this.tournament.BoardSize != 19)
            {
                this.process.WriteData("boardsize {0}", this.tournament.BoardSize);
            }
            WhiteMoveValidated = delegate { };
            BlackMoveValidated = delegate { };
            Resigned = delegate { };
        }

        private void UpdateBoard()
        {
            this.waitingForShowBoard = true;
            this.boardParts.Clear();
            this.process.WriteData("showboard");
        }

        public Adjudicator(string binaryPath, Tournament tournament) : this(binaryPath, tournament, new ConfigurationService(), new FileService()) { }
        public Adjudicator(Tournament tournament) : this(Properties.Settings.Default.adjudicatorPath, tournament) { }


        #endregion

        public void BlackMoves(Move move)
        {
            if (this.whiteGoes) // its white turn now
            {
                Debug.WriteLine("Unexpected behaviour: black bot makes move when it is not its turn"); //TODO write in logs
                return;
            }
            MakeMove(move, Black);
        }

        public void WhiteMoves(Move move)
        {
            if (!this.whiteGoes) //its black turn now
            {
                Debug.WriteLine("Warning: white bot makes move when it is not its turn"); //TODO write in logs
                return;
            }
            MakeMove(move, White);
        }

        private void MakeMove(Move move, string color)
        {
            if (this.waitingForMoveResult)
            {
                Debug.WriteLine("Warning: Previous move was not yet validated. Ignoring {0} from {1}", move, color); //TODO write in logs
                return; // ignore because waiting for last turn results
            }
            if (move == null) throw new ArgumentNullException(nameof(move));
            if (this.process == null) throw new ObjectDisposedException("proccess");

            if (!move.Normal)
            {
                if (this.lastInputMoveIsPass && move.Pass)
                {
                    RaiseResigned(EndGameReason.ConsecutivePass, !this.whiteGoes);
                    return;
                }
                RaiseResigned(move.ToEndGameReason(), !this.whiteGoes);
                return;
            }
            this.lastInputMoveIsPass = move.Pass;
            this.waitingForMoveResult = true;
            this.process.WriteData(color + move);
            this.lastReceivedMove = move;
        }

        private void OnDataReceived(string line)
        {
            Debug.WriteLine(line);
            if (this.waitingForMoveResult)
            {
                if (line == "? illegal move" || line == "? invalid coordinate")
                {
                    RaiseResigned(EndGameReason.InvalidMove, this.whiteGoes);
                    this.waitingForMoveResult = false;
                }
                if (line == "= ")
                {
                    this.waitingForMoveResult = false;
                    this.whiteGoes = !this.whiteGoes; // switch who has to move next
                    PushValidationResult();
                }
            }
            if (this.taskScore != null && (line.StartsWith("= W+") || (line.StartsWith("= B+"))))
            {
                this.taskScore.SetResult(line.Trim('=', ' '));
                this.taskScore = null;
                return;
            }

            if (this.waitingForShowBoard)
            {
                HandleBoardReading(line);
            }
        }

        private void HandleBoardReading(string line)
        {
            if (line.Contains(" A ")) //For case when board is 1x1 or more
            {
                if (this.boardParts.Count > 2 && this.boardParts.Any(l => l.Contains(" ")) && this.boardParts.Last().Contains("1"))
                {
                    if (this.boardParts.First().Contains(" A "))
                        this.boardParts.RemoveAt(0);
                    int firstLineId = this.boardParts.IndexOf(this.boardParts.First(l => l.Contains(" A ")));
                    if (firstLineId > 0)
                        this.boardParts.RemoveRange(0, firstLineId);
                    this.boardParts.Add(line);
                    this.waitingForShowBoard = false;
                    if (BoardUpdated != null)
                        BoardUpdated(this.boardParts);
                    if (this.taskBoard != null)
                        this.taskBoard.SetResult(this.boardParts);
                }
            }
            this.boardParts.Add(line);
        }

        private void PushValidationResult()
        {
            this.movesCount++;
            if (!this.whiteGoes) //vice versa because not yet switched
                WhiteMoveValidated(this.lastReceivedMove);
            else BlackMoveValidated(this.lastReceivedMove);
            if (BoardUpdated != null) // If there is no subscription, don't even get this info
                UpdateBoard();
        }

        private void RaiseResigned(EndGameReason reason, bool whiteFinishedGame)
        {
            var statistic = new GameResult
            {
                EndReason = reason,
                TotalMoves = this.movesCount,
                BoardSize = this.tournament.BoardSize
            };
            if (reason == EndGameReason.Resign)
            {
                statistic.WinnerName = whiteFinishedGame ? this.tournament.BlackBot : this.tournament.WhiteBot;
                statistic.LooserName = !whiteFinishedGame ? this.tournament.BlackBot : this.tournament.WhiteBot;
            }
            else
            {
                var score = GetFinalScore().Result;
                statistic.FinalScore = score.Item1;
                if (score.Item2 == Color.White)
                {
                    statistic.WinnerName = this.tournament.WhiteBot;
                    statistic.LooserName = this.tournament.BlackBot;
                }
                else if (score.Item2 == Color.Black)
                {
                    statistic.WinnerName = this.tournament.BlackBot;
                    statistic.LooserName = this.tournament.WhiteBot;
                }
            }
            if (GenerateLastBoard)
                statistic.FinalBoard = GetLastBoard().Result;
            if (SaveGameResults)
            {
                var fileName = string.Format("{0}{1}", this.tournament.Name, DateTime.Now.ToString("yyyy-mm-dd_HH-mm-ss"));
                this.process.WriteData("printsgf " + fileName + ".sgf");
                statistic.ResultsFileName = fileName;
                this.configurationService.SerializeGameResult(statistic, fileName);
            }
            Resigned(statistic);
        }

        private async Task<Tuple<int, Color>> GetFinalScore()
        {
            this.taskScore = new TaskCompletionSource<string>();
            this.process.WriteData("final_score");
            var scoreLine = await this.taskScore.Task;
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
            this.taskBoard = new TaskCompletionSource<IEnumerable<string>>();
            UpdateBoard();
            return string.Join("\n", await this.taskBoard.Task);
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
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.process != null)
                    {
                        this.process.WriteData("quit");
                        this.process.Dispose();
                        this.process = null;
                    }
                }
                this.disposed = true;
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