namespace ChessSharp.Engine
{
    public class InfoNewPv : Info
    {
        public InfoNewPv(int positionCount, long elapsedMilliseconds, int depth,
            uint currentMove, double score, TranspositionTable transpositionTable)
            : base(positionCount, elapsedMilliseconds, depth, transpositionTable)
        {
            CurrentMove = currentMove;
            Score = score;
        }

        public uint CurrentMove { get; }

        public double Score { get; }

        public string GetBestMoveString()
        {
            var move = new MoveViewer(CurrentMove);
            return $"Move: {move.GetNotation()} Evaluation: {Score}";
        }
    }
}
