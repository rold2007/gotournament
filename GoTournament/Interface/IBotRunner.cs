using System;

namespace GoTournament.Interface
{
    public interface IBotRunner
    {
        Action<GameResult> EndGame { get; set; }
        bool IsFinished { get; }
        void Cancel();
    }
}