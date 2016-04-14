using System;

namespace GoTournament.Interface
{
    public interface IGoBot
    {
        void StartGame(bool goesFirst);
        void PlaceMove(Move move);
        string Name { get; }
        int BoardSize { get; set; }
        int Level { get; set; }
        Action<Move> MovePerformed { get; set; }
        Action Resign { get; set; }
        Action SecondPass { get; set; }
        void Dispose();
    }
}