using System;
using System.Globalization;

namespace GoTournament
{
    public class GameStatistic
    {
        public EndGameReason EndReason { get; set; }
        public string WinnerName { get; set; }
        public int TotalMoves { get; set; }
        public string FinalBoard { get; set; }
        public bool WhiteFinishedGame { get; set; }

        public bool WhiteWonTheGame
        {
            get { return FinalScore.StartsWith("W"); }
        }

        public string GameFinisherName { get; set; }
        public string GameWinnerName { get; set; }
        public string SgfFileName { get; set; }
        public string FinalScore { get; set; }

        public int Score
        {
            get
            {
                var score = 0;
                if (int.TryParse(FinalScore.Substring(2), NumberStyles.AllowDecimalPoint, new NumberFormatInfo { NumberDecimalSeparator = "."}, out score))
                    return score;
                return -1;
            }
        }

    }
}