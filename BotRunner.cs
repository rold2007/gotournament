using System;
using System.Collections.Generic;
using GoTournament.Interface;

namespace GoTournament
{
    public class BotRunner : IBotRunner
    {
        private readonly List<IGoBot> _bots;

        public BotRunner(IGoBot first, IGoBot second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if(ReferenceEquals(first,second)) throw new ArgumentException("Two instances cannot point to the same object");
            _bots = new List<IGoBot> { first, second };
            first.MovePerformed = second.PlaceMove;
            second.MovePerformed = first.PlaceMove;
            _bots.ForEach(b =>
            {
                b.PassLimitPassed = () =>
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

            second.StartGame(false);
            first.StartGame(true);
        }

        public void Cancel()
        {
            _bots.ForEach(b=>b.Dispose());
        }

        public bool IsFinished { get; private set; }

        public Action<string> PassLimitPassed { get; set; }
        public Action<string> Resigned { get; set; }
    }
}