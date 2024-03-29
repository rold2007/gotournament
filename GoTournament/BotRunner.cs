namespace GoTournament
{
    using System;
    using System.Collections.Generic;
    using GoTournament.Interface;
    using GoTournament.Model;

    public class BotRunner : IBotRunner
    {
        private readonly List<IDisposable> bots;

        public BotRunner(IAdjudicator adjudicator, IGoBot black, IGoBot white)
        {
            if (adjudicator == null)
            {
                throw new ArgumentNullException(nameof(adjudicator));
            }

            if (black == null)
            {
                throw new ArgumentNullException(nameof(black));
            }

            if (white == null)
            {
                throw new ArgumentNullException(nameof(white));
            }

            if (ReferenceEquals(black, white))
            {
                throw new ArgumentException("Two instances cannot point to the same object");
            }

            if (black.Name == white.Name)
            {
                throw new NotSupportedException("Give unique names to the bot instances. Be creative.");
            }

            this.bots = new List<IDisposable> { black, white, adjudicator };

            this.EndGame = delegate { };
            black.MovePerformed = adjudicator.BlackMoves;
            white.MovePerformed = adjudicator.WhiteMoves;
            adjudicator.BlackMoveValidated = white.PlaceMove;
            adjudicator.WhiteMoveValidated = black.PlaceMove;

            /* black.MovePerformed = adjudicator.BlackMoves;
             white.MovePerformed = adjudicator.WhiteMoves;*/

            /* adjudicator.BlackMoveValidated = m =>
             {
                 Console.WriteLine("white {0}", m);
                 white.PlaceMove(m);
             };
             adjudicator.WhiteMoveValidated = m =>
             {
                 Console.WriteLine("black {0}", m);
                 black.PlaceMove(m);
             };*/
            adjudicator.Resigned = stat =>
            {
                this.EndGame(stat);
                this.IsFinished = true;
            };

            white.StartGame(false);
            black.StartGame(true);
        }
        
        public bool IsFinished { get; private set; }

        public Action<GameResult> EndGame { get; set; }
        
        public void Cancel()
        {
            this.bots.ForEach(b => b.Dispose());
        }
    }
}