using System;

namespace GoTournament.Interface
{
    using GoTournament.Model;

    public interface IBotRunner
    {
        Action<GameResult> EndGame { get; set; }
        bool IsFinished { get; }
        void Cancel();
    }
}