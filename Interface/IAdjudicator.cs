using System;
using System.Collections.Generic;

namespace GoTournament.Interface
{
    public interface IAdjudicator
    {
        bool BlackMoves(Move move);
        bool WhiteMoves(Move move);
        Action<bool> WhiteMoveValidated { get; set; }
        Action<bool> BlackMoveValidated { get; set; }
        Action<IEnumerable<string>> BoardUpdated { get; set; }
        void Dispose();
    }
}