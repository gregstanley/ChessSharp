namespace Chess
{
    public class BoardMetrics
    {
        public int PointsChange { get; set; }

        public byte NumPiecesUnderAttack { get; set; }

        public byte NumPiecesUnderAttackValue { get; set; }

        public byte NumCoveredSquares { get; set; }
    }
}
