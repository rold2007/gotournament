namespace GoTournament.Benchmark
{
    public class BenchmarkSettings
    {
        public int BoardSize { get; set; }

        public int FirstBotLevel { get; set; }

        public int SecondBotLevel { get; set; }

        public static BenchmarkSettings Create(int boardSize, int firstBotLevel, int secondBotLevel)
        {
            return new BenchmarkSettings
            {
                BoardSize = boardSize,
                FirstBotLevel = firstBotLevel,
                SecondBotLevel = secondBotLevel
            };
        }
    }
}