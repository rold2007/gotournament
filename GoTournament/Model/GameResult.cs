namespace GoTournament.Model
{
    public class GameResult
    {
        public string WinnerName { get; set; }

        public string LooserName { get; set; }

        public int FinalScore { get; set; }

        public EndGameReason EndReason { get; set; }

        public int TotalMoves { get; set; }

        public string ResultsFileName { get; set; }

        public int BoardSize { get; set; }

        public string FinalBoard { get; set; }
    }
}