namespace GoTournament.Service
{
    using System.Collections.Generic;
    
    public interface IRatingService
    {
        int GamesCount { get; set; }

        int Rating { get; }

        void AddLoses(int loses);

        void AddWins(int wins);

        void SetOpponentsRatings(IEnumerable<int> ratings);
    }
}