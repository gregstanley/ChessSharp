namespace Chess
{
    public class BoardMetrics
    {
        public int PointsChange { get; set; }

        public byte NumPiecesUnderThreat { get; set; }

        public byte NumPiecesUnderThreatValue { get; set; }

        public byte NumAccessibleSquares { get; set; }

        public byte NumProtectedPieces { get; set; }
    }
}
