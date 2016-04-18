namespace GoTournament
{
    public class GameStatistic
    {
        public EndGameReason EndReason { get; set; }
        public string WinnerName { get; set; }
        public int TotalMoves { get; set; }
        public string FinalBoard { get; set; }
        public bool WhiteFinishedGame { get; set; }
        public string GameFinisherName { get; set; }
    }
}