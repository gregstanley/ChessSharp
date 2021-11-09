using System;

namespace ChessSharp.Engine.NegaMax.Events
{
    public struct SearchStatus
    {
        public SearchStatus(int depth, int ply, long elapsedMilliseconds, int positionCount, int score, uint? bestMove = null)
        {
            Depth = depth;
            Ply = ply;
            ElapsedMilliseconds = elapsedMilliseconds;
            PositionCount = positionCount;
            Score = score;
            BestMove = bestMove;
        }

        public int Depth { get; }

        public int Ply { get; }

        public long ElapsedMilliseconds { get; }

        public int PositionCount { get; }

        public int Score { get; }

        public uint? BestMove { get; }

        public override string ToString()
        {
            var elapsedMilliseconds = ElapsedMilliseconds == 0 ? 1 : ElapsedMilliseconds;

            var nps = Math.Floor((double)PositionCount / elapsedMilliseconds * 1000);

            return $"info depth {Ply} score cp {Score} nodes {PositionCount} nps {nps} time {ElapsedMilliseconds} pv -";
            // Stockfish 14
            // go movetime 1000
            // info depth 1 seldepth 1 multipv 1 score cp 43 nodes 20 nps 6666 tbhits 0 time 3 pv d2d4
            // info depth 2 seldepth 2 multipv 1 score cp 79 nodes 48 nps 12000 tbhits 0 time 4 pv d2d4 a7a6
            // info depth 3 seldepth 3 multipv 1 score cp 75 nodes 181 nps 45250 tbhits 0 time 4 pv g1f3 c7c6 d2d4
            // info depth 4 seldepth 4 multipv 1 score cp 26 nodes 630 nps 157500 tbhits 0 time 4 pv d2d4 d7d5 c2c4 d5c4
            // info depth 5 seldepth 5 multipv 1 score cp 145 nodes 741 nps 148200 tbhits 0 time 5 pv e2e4 g8f6
            // info depth 6 seldepth 6 multipv 1 score cp 25 nodes 2448 nps 408000 tbhits 0 time 6 pv e2e4 d7d6 c2c3 g8f6 g1f3 e7e5
            // info depth 7 seldepth 7 multipv 1 score cp 29 nodes 3488 nps 498285 tbhits 0 time 7 pv d2d4 d7d5 e2e3 e7e6 c2c4 g8f6 g1f3
            // info depth 8 seldepth 8 multipv 1 score cp 65 nodes 5172 nps 574666 tbhits 0 time 9 pv e2e4 e7e6 d2d4 d7d5 b1c3 d5e4
            // info depth 9 seldepth 11 multipv 1 score cp 37 nodes 10602 nps 815538 tbhits 0 time 13 pv d2d4 g8f6 c2c4 e7e6 b1c3 d7d5 c4c5 g7g6 g1f3

            // Houdini 1.5
            // go movetime 1000
            // info multipv 1 depth 4 seldepth 18 score cp 5  time 0 nodes 1758 nps 0 tbhits 0 hashfull 0 pv g1f3 g8f6 b1c3 b8c6 d2d3 e7e6
            // info multipv 1 depth 5 seldepth 18 score cp 5  time 1 nodes 2057 nps 2057000 tbhits 0 hashfull 0 pv g1f3 g8f6 b1c3 b8c6 d2d3 e7e6
            // info depth 6
            // info multipv 1 depth 6 seldepth 20 score cp 5  time 2 nodes 3837 nps 1918000 tbhits 0 hashfull 0 pv g1f3 g8f6 b1c3 b8c6 d2d3 e7e6
            // info depth 7
            // info multipv 1 depth 7 seldepth 20 score cp 11 lowerbound time 3 nodes 7421 nps 2473000 tbhits 0 hashfull 0 pv g1f3
            // info multipv 1 depth 7 seldepth 20 score cp 14  time 4 nodes 7929 nps 1982000 tbhits 0 hashfull 0 pv g1f3 g8f6 b1c3 b8c6 d2d4 e7e6 c1f4
            // info depth 8
            // info multipv 1 depth 8 seldepth 20 score cp 21 lowerbound time 5 nodes 10316 nps 2063000 tbhits 0 hashfull 0 pv g1f3
            // info multipv 1 depth 8 seldepth 24 score cp 17  time 9 nodes 22830 nps 2536000 tbhits 0 hashfull 0 pv e2e4 d7d5 e4d5 g8f6 d2d4 c8f5 b1c3 f6d5 g1f3 b8c6
            // info depth 9
            // info multipv 1 depth 9 seldepth 24 score cp 23  time 13 nodes 35176 nps 2705000 tbhits 0 hashfull 0 pv e2e4 d7d5 e4d5 g8f6 d2d4 f6d5 c2c4 d5f6 g1f3 b8c6 b1c3 c8f5 f1d3 e7e6 d3f5 e6f5 d1e2 f8e7           
        }
    }
}
