namespace GoTournament
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using GoTournament.Interface;
    using GoTournament.Model;

    public class Adjudicator : IAdjudicator
    {
        private const string Black = "black ";
        private const string White = "white ";
        private readonly List<string> boardParts = new List<string>(26);
        private readonly Duel duel;
        private readonly IConfigurationService configurationService;
        private readonly ILogger logger;
        private IProcessManager process;
        private bool disposed;
        private bool whiteGoes;
        private bool waitingForShowBoard;
        private bool waitingForMoveResult;
        private bool lastInputMoveIsPass;
        private Move lastReceivedMove;
        private int movesCount;
        private TaskCompletionSource<IEnumerable<string>> taskBoard;
        private TaskCompletionSource<string> taskScore;
        
        #region Ctors

        public Adjudicator(ISimpleInjectorWrapper simpleInjector, Duel duel)
        {
            if (simpleInjector == null)
            {
                throw new ArgumentNullException(nameof(simpleInjector));
            }

            if (duel == null)
            {
                throw new ArgumentNullException(nameof(duel));
            }

            var fileService = simpleInjector.GetInstance<IFileService>();
            this.configurationService = simpleInjector.GetInstance<IConfigurationService>();
            var binaryPath = this.configurationService.GetAdjudicatorBinaryPath();
            if (!fileService.FileExists(binaryPath))
            {
                throw new FileNotFoundException("Adjudicator binary not found,", binaryPath);
            }

            this.process = simpleInjector.GetInstance<IProcessManagerFactory>().Create(binaryPath, "--mode gtp");
            this.logger = simpleInjector.GetInstance<ILogger>();
            this.process.DataReceived = this.OnDataReceived;
            this.duel = duel;
            if (this.duel.BoardSize != 19)
            {
                this.process.WriteData("boardsize {0}", this.duel.BoardSize);
            }

            this.WhiteMoveValidated = delegate { };
            this.BlackMoveValidated = delegate { };
            this.Resigned = delegate { };
        }
        
        ~Adjudicator()
        {
            this.Dispose(false);
        }

        #endregion

        public Action<Move> WhiteMoveValidated { get; set; }

        public Action<Move> BlackMoveValidated { get; set; }

        public Action<GameResult> Resigned { get; set; }

        public Action<IEnumerable<string>> BoardUpdated { get; set; }

        public bool SaveGameResults { get; set; }

        public bool GenerateLastBoard { get; set; }

        public void BlackMoves(Move move)
        {
            if (this.whiteGoes)
            { 
                // its white turn now
                this.logger.WriteWarning("Black bot makes move when it is not its turn");
                return;
            }

            this.MakeMove(move, Black);
        }

        public void WhiteMoves(Move move)
        {
            if (!this.whiteGoes)
            {
                // its black turn now
                this.logger.WriteWarning("White bot makes move when it is not its turn");
                return;
            }

            this.MakeMove(move, White);
        }
        
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void MakeMove(Move move, string color)
        {
            if (this.waitingForMoveResult)
            {
                this.logger.WriteWarning("Previous move was not yet validated. Ignoring {0} from {1}", move, color);
                return; // ignore because waiting for last turn results
            }

            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (this.process == null)
            {
                throw new ObjectDisposedException("proccess");
            }

            if (!move.Normal)
            {
                if (move.Pass)
                {
                    if (this.lastInputMoveIsPass)
                    {
                        this.RaiseResigned(EndGameReason.ConsecutivePass, !this.whiteGoes);
                        return;
                    }
                }
                else
                {
                    this.RaiseResigned(move.ToEndGameReason(), !this.whiteGoes);
                    return;
                }
            }

            this.lastInputMoveIsPass = move.Pass;
            this.waitingForMoveResult = true;
            this.process.WriteData(color + move);
            this.lastReceivedMove = move;
        }

        private void OnDataReceived(string line)
        {
            // logger.WriteInfo(line);
            if (this.waitingForMoveResult)
            {
                if (line == "? illegal move" || line == "? invalid coordinate")
                {
                    this.RaiseResigned(EndGameReason.InvalidMove, this.whiteGoes);
                    this.waitingForMoveResult = false;
                }

                if (line == "= ")
                {
                    this.waitingForMoveResult = false;
                    this.whiteGoes = !this.whiteGoes; // switch who has to move next
                    this.PushValidationResult();
                }
            }

            if (this.taskScore != null && (line.StartsWith("= W+") || line.StartsWith("= B+")))
            {
                this.taskScore.SetResult(line.Trim('=', ' '));
                this.taskScore = null;
                return;
            }

            if (this.waitingForShowBoard)
            {
                this.HandleBoardReading(line);
            }
        }

        private void HandleBoardReading(string line)
        {
            if (line.Contains(" A "))
            { 
                // For case when board is 1x1 or more
                if (this.boardParts.Count > 2 && this.boardParts.Any(l => l.Contains(" ")) && this.boardParts.Last().Contains("1"))
                {
                    if (this.boardParts.First().Contains(" A "))
                    {
                        this.boardParts.RemoveAt(0);
                    }

                    var firstLine = this.boardParts.FirstOrDefault(l => l.Contains(" A "));
                    if (firstLine != null)
                    {
                        // cutting off previous lines if possible
                        int firstLineId = this.boardParts.IndexOf(firstLine);
                        if (firstLineId > 0)
                        {
                            this.boardParts.RemoveRange(0, firstLineId);
                        }
                    }

                    this.boardParts.Add(line);
                    this.waitingForShowBoard = false;
                    if (this.BoardUpdated != null)
                    {
                        this.BoardUpdated(this.boardParts);
                    }

                    if (this.taskBoard != null)
                    {
                        this.taskBoard.SetResult(this.boardParts);
                    }
                }
            }

            this.boardParts.Add(line);
        }

        private void PushValidationResult()
        {
            this.movesCount++;
            if (!this.whiteGoes)
            {
                // vice versa because not yet switched
                this.WhiteMoveValidated(this.lastReceivedMove);
            }
            else
            {
                this.BlackMoveValidated(this.lastReceivedMove);
            }

            if (this.BoardUpdated != null)
            {
                // If there is no subscription, don't even get this info
                this.UpdateBoard();
            }
        }

        private void UpdateBoard()
        {
            this.waitingForShowBoard = true;
            this.boardParts.Clear();
            this.process.WriteData("showboard");
        }

        private void RaiseResigned(EndGameReason reason, bool whiteFinishedGame)
        {
            var statistic = new GameResult
            {
                EndReason = reason,
                TotalMoves = this.movesCount,
                BoardSize = this.duel.BoardSize
            };
            if (reason == EndGameReason.Resign)
            {
                statistic.WinnerName = whiteFinishedGame ? this.duel.BlackBot : this.duel.WhiteBot;
                statistic.LooserName = !whiteFinishedGame ? this.duel.BlackBot : this.duel.WhiteBot;
            }
            else
            {
                var score = this.GetFinalScore().Result;
                statistic.FinalScore = score.Item1;
                if (score.Item2 == Color.White)
                {
                    statistic.WinnerName = this.duel.WhiteBot;
                    statistic.LooserName = this.duel.BlackBot;
                }
                else if (score.Item2 == Color.Black)
                {
                    statistic.WinnerName = this.duel.BlackBot;
                    statistic.LooserName = this.duel.WhiteBot;
                }
            }

            if (this.GenerateLastBoard)
            {
                statistic.FinalBoard = this.GetLastBoard().Result;
            }

            if (this.SaveGameResults)
            {
                var fileName = string.Format("{0}{1}", this.duel.Name, DateTime.Now.ToString("yyyy-mm-dd_HH-mm-ss"));
                this.process.WriteData("printsgf " + fileName + ".sgf");
                statistic.ResultsFileName = fileName;
                this.configurationService.SerializeGameResult(statistic, fileName);
            }

            this.Resigned(statistic);
        }

        private async Task<Tuple<int, Color>> GetFinalScore()
        {
            this.taskScore = new TaskCompletionSource<string>();
            this.process.WriteData("final_score");
            var scoreLine = await this.taskScore.Task;
            var score = 0;
            if (
                !int.TryParse(
                    scoreLine.Substring(2),
                    NumberStyles.AllowDecimalPoint,
                    new NumberFormatInfo { NumberDecimalSeparator = "." },
                    out score))
            {
                this.logger.WriteWarning("Could not parse final score: {0}", scoreLine);
            }

            Color color = Color.None;
            if (scoreLine.StartsWith("W"))
            {
                color = Color.White;
            }
            else if (scoreLine.StartsWith("B"))
            {
                color = Color.Black;
            }

            return Tuple.Create(score, color);
        }

        private async Task<string> GetLastBoard()
        {
            this.taskBoard = new TaskCompletionSource<IEnumerable<string>>();
            this.UpdateBoard();
            return string.Join("\n", await this.taskBoard.Task);
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
    }
}