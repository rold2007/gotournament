using System;
using System.IO;
using GoTournament.Interface;
using GoTournament.Service;

namespace GoTournament
{

    public class GnuGoBot : IGoBot
    {
        #region Fields

        private IProcessWrapper process;
        private int boardSize = 19;
        private bool black;
        private string genmoveCommand;
        private bool disposed;
        private int level = -1;
        private bool waitingForMoveGenerating;

        private bool initialized = false;
        #endregion

        #region Ctors

        public GnuGoBot(IProcessWrapper process, string name)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            this.process = process;
            this.Name = name;
            this.process.DataReceived = this.OnDataReceived;
            //To reduce null reference checking 
            this.MovePerformed = delegate { };
        }

        public GnuGoBot(string binaryPath, string name) : this(CreateIProcessWrapper(binaryPath, new FileService(), new ProcessProxy()), name) { }

        /// <summary>
        /// For Unit testing purpose
        /// </summary>
        /// <param name="binaryPath"></param>
        /// <param name="name"></param>
        /// <param name="fileService"></param>
        /// <param name="processProxy"></param>
        public GnuGoBot(string binaryPath, string name, IFileService fileService, IProcessProxy processProxy) : this(CreateIProcessWrapper(binaryPath, fileService, processProxy), name) { }


        [Obsolete]
        public GnuGoBot(string name): this(Properties.Settings.Default.adjudicatorPath, name) { }

        #endregion

        #region Properties

        public int BoardSize
        {
            get { return this.boardSize; }
            set
            {
                if (this.initialized)
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
                if (this.initialized)
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
            this.InitializeGame();
            if (this.black) this.PerformMove();
        }

        public void PlaceMove(Move move)
        {
            if (move == null) throw new ArgumentNullException(nameof(move));
            if (this.process == null) throw new ObjectDisposedException("proccess");
            if (!this.initialized)
                throw new NotSupportedException("Invoke StartGame before placing the move");
            this.process.WriteData((this.black ? "white " : "black ") + move);
            this.PerformMove();
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
                this.MovePerformed(move);
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
            if (this.level != -1)
                this.process.WriteData("level {0}", this.level);
            this.initialized = true;
        }
        
        private static IProcessWrapper CreateIProcessWrapper(string binaryPath, IFileService fileService, IProcessProxy processProxy)
        {
            if (binaryPath == null)
                throw new ArgumentNullException(nameof(binaryPath));
            if (fileService == null)
                throw new ArgumentNullException(nameof(fileService));
            if (processProxy == null)
                throw new ArgumentNullException(nameof(processProxy));
            if (!fileService.FileExists(binaryPath))
                throw new FileNotFoundException("Bot binnary not found,", binaryPath);
            return new ProcessWrapper(processProxy, binaryPath, "--mode gtp");
        }

        #endregion

        #region IDisposable pattern

        public void Dispose()
        {
            this.Dispose(true);
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