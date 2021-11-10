using System;
using System.Linq;
using ChessSharp.Common;

namespace ChessSharp.Engine.NegaMax
{
    public class NegaMaxSearchResult
    {
        public NegaMaxSearchResult(int depth, int ply, long elapsedMilliseconds, int positionCount, int score, uint[] pv, uint? bestMove = null)
        {
            Depth = depth;
            Ply = ply;
            ElapsedMilliseconds = elapsedMilliseconds;
            PositionCount = positionCount;
            Score = score;
            Pv = pv;
            BestMove = bestMove;
        }

        public int Depth { get; }

        public int Ply { get; }

        public long ElapsedMilliseconds { get; }

        public int PositionCount { get; }

        public int Score { get; }

        public uint? BestMove { get; }

        public uint[] Pv { get; }

        public override string ToString()
        {
            var elapsedMilliseconds = ElapsedMilliseconds == 0 ? 1 : ElapsedMilliseconds;

            var nps = Math.Floor((double)PositionCount / elapsedMilliseconds * 1000);

            var moves = Pv.Select(x => new MoveViewer(x).GetUciNotation());

            return $"info depth {Ply} score cp {Score} nodes {PositionCount} nps {nps} time {ElapsedMilliseconds} pv {string.Join(" ", moves)}";
        }
    }
}