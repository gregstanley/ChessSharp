using System;

namespace Chess
{
    public class OptionsStats
    {
        public int NumBoards { get; }

        public double AvgWhiteScore { get; }

        public double AvgBlackScore { get; }

        public double AvgWhiteChange { get; }

        public double AvgBlackChange { get; }

        public double GetAverageScore(Colour colour) =>
            colour == Colour.White ? AvgWhiteScore : AvgBlackScore;

        public double GetAverageChange(Colour colour) =>
           colour == Colour.White ? AvgWhiteChange : AvgBlackChange;

        public OptionsStats(int numBoards, double avgWhiteScore, double avgBlackScore, double avgWhiteChange, double avgBlackChange)
        {
            NumBoards = numBoards;
            AvgWhiteScore = Math.Floor(avgWhiteScore * 100) / 100;
            AvgBlackScore = Math.Floor(avgBlackScore * 100) / 100;
            AvgWhiteChange = avgWhiteChange;
            AvgBlackChange = avgBlackChange;
        }
    }
}
