using System;

namespace GoTournament.Interface
{
    public interface IGoBot : IDisposable
    {
        void StartGame(bool goesFirst);
        void PlaceMove(Move move);
        string Name { get; }
        int BoardSize { get; set; }
        int Level { get; set; }
        Action<Move> MovePerformed { get; set; }
    }
}