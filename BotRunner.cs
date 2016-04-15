using System;
using System.Collections.Generic;
using System.Diagnostics;
using GoTournament.Interface;

namespace GoTournament
{
    public class BotRunner : IBotRunner
    {
        private readonly List<IDisposable> _bots;

        public BotRunner(IAdjudicator adjudicator, IGoBot black, IGoBot white)
        {
            if (adjudicator == null) throw new ArgumentNullException(nameof(adjudicator));
            if (black == null) throw new ArgumentNullException(nameof(black));
            if (white == null) throw new ArgumentNullException(nameof(white));
            if(ReferenceEquals(black,white)) throw new ArgumentException("Two instances cannot point to the same object");
            if(black.Name == white.Name) throw new NotSupportedException("Give unique names to the bot instances. Be creative.");

            _bots = new List<IDisposable> { black, white, adjudicator };

            EndGame = delegate { };
            /*  black.MovePerformed = adjudicator.BlackMoves;
              white.MovePerformed = adjudicator.WhiteMoves;
              adjudicator.BlackMoveValidated = white.PlaceMove;
              adjudicator.WhiteMoveValidated = black.PlaceMove;*/

            black.MovePerformed = adjudicator.BlackMoves;
            white.MovePerformed = adjudicator.WhiteMoves;
            
            adjudicator.BlackMoveValidated = m =>
            {
                Console.WriteLine("white {0}", m);
                white.PlaceMove(m);
            };
            adjudicator.WhiteMoveValidated = m =>
            {
                Console.WriteLine("black {0}", m);
                black.PlaceMove(m);
            };


            adjudicator.Resigned = (reason, b) =>
            {
                EndGame(reason, b ? white.Name : black.Name);
                IsFinished = true;
            };
           
            white.StartGame(false);
            black.StartGame(true);
        }
        

        public void Cancel()
        {
            _bots.ForEach(b=>b.Dispose());
        }

        public bool IsFinished { get; private set; }
        
        public Action<EndGameReason, string> EndGame { get; set; }
    }
}