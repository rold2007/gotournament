namespace GoTournament.Interface
{
    using System;
    using GoTournament.Model;

    public interface IGoBot : IDisposable
    {
        string Name { get; }

        int BoardSize { get; set; }

        int Level { get; set; }

        Action<Move> MovePerformed { get; set; }

        void StartGame(bool goesFirst);

        void PlaceMove(Move move);
    }
}