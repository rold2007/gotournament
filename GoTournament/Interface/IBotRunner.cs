namespace GoTournament.Interface
{
    using System;
    using GoTournament.Model;

    public interface IBotRunner
    {
        Action<GameResult> EndGame { get; set; }

        bool IsFinished { get; }

        void Cancel();
    }
}