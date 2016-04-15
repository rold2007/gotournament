using System;
using System.Collections.Generic;

namespace GoTournament.Interface
{
    public interface IAdjudicator : IDisposable
    {
        void BlackMoves(Move move);
        void WhiteMoves(Move move);
        Action<Move> WhiteMoveValidated { get; set; }
        Action<Move> BlackMoveValidated { get; set; }
        Action<IEnumerable<string>> BoardUpdated { get; set; }
        Action<EndGameReason, bool> Resigned { get; set; }
    }
}