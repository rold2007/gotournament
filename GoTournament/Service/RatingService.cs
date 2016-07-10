namespace GoTournament.Service
{
    using System.Collections.Generic;
    using System.Linq;

    public class RatingService : IRatingService
    {
        private int totalOpponentsRatings;

        private int winsCount = 0;

        public int GamesCount { get; set; }

        public int Rating
        {
            get
            {
                return (this.totalOpponentsRatings + (400 * this.winsCount)) / this.GamesCount;
            }
        }

        public void SetOpponentsRatings(IEnumerable<int> ratings)
        {
            this.totalOpponentsRatings = ratings.Sum();
        }

        public void AddWins(int wins)
        {
            this.winsCount += wins;
        }

        public void AddLoses(int loses)
        {
            this.winsCount -= loses;
        }
    }
}
