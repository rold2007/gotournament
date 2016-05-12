namespace GoTournament.Model
{
    public class Tournament
    {
        public string Name { get; set; }

        public int BoardSize { get; set; }

        public int GamesCount { get; set; }

        public string BlackBot { get; set; }

        public string WhiteBot { get; set; }
    }
}