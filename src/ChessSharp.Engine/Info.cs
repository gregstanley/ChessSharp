using System;
using System.Text;

namespace ChessSharp.Engine
{
    public class Info
    {
        public Info(int positionCount, long elapsedMilliseconds, int depth, TranspositionTable transpositionTable)
        {
            SearchedPositionCount = positionCount;
            ElapsedMilliseconds = elapsedMilliseconds;
            Depth = depth;
            TtAccessCount = transpositionTable.AccessCount;
            TtHitCount = transpositionTable.HitCount;
            TtMissCount = transpositionTable.MissCount;
            TtReplaceCount = transpositionTable.ReplaceCount;
            TtUsage = transpositionTable.Usage;
        }

        public int SearchedPositionCount { get; }

        public long ElapsedMilliseconds { get; }

        public int Depth { get; }

        public long TtAccessCount { get; }

        public long TtHitCount { get; }

        public long TtMissCount { get; }

        public long TtReplaceCount { get; }

        public double TtUsage { get; }

        public string GetTimeString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Time: {ElapsedMilliseconds} ms");

            sb.AppendLine($"Positions: {SearchedPositionCount}");

            var nodesPerMilli = (double)SearchedPositionCount / ElapsedMilliseconds;
            var nps = Math.Round(nodesPerMilli * 1000, 2);

            sb.AppendLine($"NPS: {nps}");

            return sb.ToString();
        }

        public string GetTtString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Total positions: {SearchedPositionCount}");
            sb.AppendLine($"TT access: {TtAccessCount}");
            sb.AppendLine($"TT hits: {TtHitCount}");
            sb.AppendLine($"TT misses: {TtMissCount}");
            sb.AppendLine($"TT replaced: {TtReplaceCount}");
            sb.AppendLine($"TT usage: {TtUsage}%");

            return sb.ToString();
        }
    }
}
