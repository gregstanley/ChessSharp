using ChessSharp.Common;

namespace ChessSharp.Engine
{
    public class InfoNewPv : Info
    {
        public InfoNewPv(
            int positionCount,
            long elapsedMilliseconds,
            int depth,
            uint currentMove,
            double score,
            TranspositionTable transpositionTable)
            : base(positionCount, elapsedMilliseconds, depth, transpositionTable)
        {
            this.CurrentMove = currentMove;
            this.Score = score;
        }

        public uint CurrentMove { get; }

        public double Score { get; }

        public string GetBestMoveString()
        {
            var move = new MoveViewer(this.CurrentMove);

            return $"Move: {move.GetNotation()} Evaluation: {this.Score}";
        }
    }
}
