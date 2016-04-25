using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GoTournament.Interface;

namespace GoTournament
{
    using GoTournament.Service;

    public class GnuGoBot : IGoBot
    {
        #region Fields

        private IProcessWrapper process;
        private readonly string binaryPath;
        private int boardSize = 19;
        private bool black;
        private string genmoveCommand;
        private bool disposed;
        private int level;
        private bool waitingForMoveGenerating;
        #endregion

        #region Ctors

        public GnuGoBot(string binaryPath, string name, IFileService fileService)
        {
            if (!fileService.FileExists(binaryPath))
                throw new FileNotFoundException("Bot binnary not found,", binaryPath);
            this.binaryPath = binaryPath;
            Name = name;
            //To reduce null reference checking 
            MovePerformed = delegate { };
        }

        public GnuGoBot(string binaryPath, string name) : this(binaryPath, name, new FileService()) { }

        [Obsolete]
        public GnuGoBot(string name): this(Properties.Settings.Default.adjudicatorPath, name) { }

        #endregion

        #region Properties

        public int BoardSize
        {
            get { return this.boardSize; }
            set
            {
                if (this.process != null)
                    throw new NotSupportedException("Board size could be set only before start of the game");
                if (value > 19 || value < 1)
                    throw new NotSupportedException("Board size could be from 1 to 19");
                this.boardSize = value;
            }
        }

        public int Level
        {
            get { return this.level; }
            set
            {
                if (this.process != null)
                    throw new NotSupportedException("Level could be set only before start of the game");
                this.level = value;
            }
        }

        public string Name { get; private set; }

        public Action<Move> MovePerformed { get; set; }

        #endregion

        #region Public methods

        public void StartGame(bool goesFirst)
        {
            this.black = goesFirst;
            this.genmoveCommand = this.black ? "genmove black" : "genmove white";
            this.process = new ProcessWrapper(this.binaryPath, "--mode gtp") { DataReceived = OnDataReceived };
            InitializeGame();
            if (this.black)
                PerformMove();
        }

        public void PlaceMove(Move move)
        {
            if (move == null) throw new ArgumentNullException(nameof(move));
            if (this.process == null) throw new ObjectDisposedException("proccess");

            this.process.WriteData((this.black ? "white " : "black ") + move);
            PerformMove();
        }

        #endregion

        #region Private methods

        private void OnDataReceived(string s)
        {
            //debug
            //  File.AppendAllText(Name+".txt", string.Format("{0}|{1}|{2}", DateTime.Now.ToString("h:mm:ss.ff"), s, Environment.NewLine));

            if (!this.waitingForMoveGenerating) return;
            if (s == null) return;
            var move = Move.Parse(s);
            if (move != null)
            {
                MovePerformed(move);
                this.waitingForMoveGenerating = false;
            }

        }

        private void PerformMove()
        {
            this.waitingForMoveGenerating = true;
            this.process.WriteData(this.genmoveCommand);
        }

        private void InitializeGame()
        {
            if (this.boardSize != 19)
                this.process.WriteData("boardsize {0}", this.boardSize);
            this.process.WriteData("level {0}", this.level);
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

        ~GnuGoBot()
        {
            Dispose(false);
        }

        #endregion
    }

}