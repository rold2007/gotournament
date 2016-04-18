using System;

namespace GoTournament.Interface
{
    public interface IBotRunner
    {
        Action<GameStatistic> EndGame { get; set; }
        bool IsFinished { get; }
        void Cancel();
    }
}