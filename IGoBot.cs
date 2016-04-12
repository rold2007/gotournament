using System;

namespace GoTournament
{
    public interface IGoBot
    {
        void SetBoardSize(int size);
        void StartGame(bool goesFirst);
        void PlaceMove(Move move);
        void SetLevel(int level);
        Action<Move> MovePerformed { get; set; }
        void Dispose();
    }
}