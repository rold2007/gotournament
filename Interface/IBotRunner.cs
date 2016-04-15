using System;

namespace GoTournament.Interface
{
    public interface IBotRunner
    {
        Action<EndGameReason, string> EndGame { get; set; }
        bool IsFinished { get; }
        void Cancel();
    }
}