using System;

namespace GoTournament.Interface
{
    public interface IBotRunner
    {
        Action<string> PassLimitPassed { get; set; }
        Action<string> Resigned { get; set; }
        bool IsFinished { get; }
        void Cancel();
    }
}