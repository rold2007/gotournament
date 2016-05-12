namespace GoTournament.Interface
{
    using System;
    using System.Collections.Generic;
    using GoTournament.Model;

    public interface IAdjudicator : IDisposable
    {
        Action<Move> WhiteMoveValidated { get; set; }

        Action<Move> BlackMoveValidated { get; set; }

        Action<IEnumerable<string>> BoardUpdated { get; set; }

        Action<GameResult> Resigned { get; set; }

        bool SaveGameResults { get; set; }

        bool GenerateLastBoard { get; set; }

        void BlackMoves(Move move);

        void WhiteMoves(Move move);
    }
}