namespace Chess.Models
{
    public class MoveCastle : Move
    {
        public RankFile KingStartPosition { get; }

        public RankFile KingEndPosition { get; }

        public PieceType Side { get; }

        public MoveCastle(Colour pieceColour, PieceType pieceType, RankFile startPosition, RankFile endPosition, RankFile kingStartPosition, RankFile kingEndPosition, PieceType side)
            : base(pieceColour, pieceType, startPosition, endPosition)
        {
            KingStartPosition = kingStartPosition;
            KingEndPosition = kingEndPosition;
            Side = side;
        }

        public override string GetCode() =>
            Side == PieceType.King ? $"0-0" : "0-0-0";
    }
}
