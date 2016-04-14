using System;
using System.Collections.Generic;
using GoTournament.Interface;

namespace GoTournament
{
    public class BotRunner : IBotRunner
    {
        private readonly IAdjudicator _adjudicator;
        private readonly List<IGoBot> _bots;

        public BotRunner(/*IAdjudicator adjudicator,*/ IGoBot black, IGoBot white)
        {
           // _adjudicator = adjudicator;
            if (black == null) throw new ArgumentNullException(nameof(black));
            if (white == null) throw new ArgumentNullException(nameof(white));
            if(ReferenceEquals(black,white)) throw new ArgumentException("Two instances cannot point to the same object");
            _bots = new List<IGoBot> { black, white };
            black.MovePerformed = white.PlaceMove; //BlackMovePerformed
           // _adjudicator.BlackMoveValidated = white.PlaceMove;
            white.MovePerformed = black.PlaceMove;
            _bots.ForEach(b =>
            {
                b.SecondPass = () =>
                {
                    IsFinished = true;
                    if (PassLimitPassed != null)
                        PassLimitPassed(b.Name);
                };
                b.Resign = () =>
                {
                    IsFinished = true;
                    if (Resigned != null)
                        Resigned(b.Name);
                };
            });

            white.StartGame(false);
            black.StartGame(true);
        }

        private void BlackMovePerformed(Move move)
        {
            _adjudicator.BlackMoves(move);
        }

        public void Cancel()
        {
            _bots.ForEach(b=>b.Dispose());
           // _adjudicator.Dispose();
        }

        public bool IsFinished { get; private set; }

        public Action<string> PassLimitPassed { get; set; }
        public Action<string> Resigned { get; set; }
    }
}