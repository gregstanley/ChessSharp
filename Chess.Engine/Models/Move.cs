namespace Chess.Engine.Models
{
    public class Move
    {
        public Colour PieceColour { get; }

        public PieceType Type { get; }

        public RankFile StartPosition { get; }

        public RankFile EndPosition { get; }

        public SquareFlag EnPassantSquare { get; } = 0;

        public PieceType PromotionType { get; } = PieceType.None;

        public Move(Colour pieceColour, PieceType pieceType, RankFile startPosition, RankFile endPosition, PieceType promotionType)
            : this(pieceColour, pieceType, startPosition, endPosition)
        {
            PromotionType = promotionType;
        }

        public Move(Colour pieceColour, PieceType pieceType, RankFile startPosition, RankFile endPosition, SquareFlag enPassantSquare)
            : this(pieceColour, pieceType, startPosition, endPosition)
        {
            EnPassantSquare = enPassantSquare;
        }

        public Move(Colour pieceColour, PieceType pieceType, RankFile startPosition, RankFile endPosition)
        {
            PieceColour = pieceColour;
            Type = pieceType;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }

        public virtual string GetCode()
        {
            var baseString = 
                $"{PieceColour}{Type}-{StartPosition.File}{StartPosition.Rank}-{EndPosition.File}{EndPosition.Rank}";

            if (PromotionType == PieceType.None)
                return baseString;

            switch (PromotionType)
            {
                case (PieceType.Queen):
                    return $"{baseString}=Q";
                case (PieceType.Bishop):
                    return $"{baseString}=B";
                case (PieceType.Knight):
                    return $"{baseString}=K";
                case (PieceType.Rook):
                    return $"{baseString}=R";
            }

            return baseString;
        }

        public override string ToString() =>
            $"{StartPosition.File}{StartPosition.Rank}-{EndPosition.File}{EndPosition.Rank}";

        public string ToFriendlyString() =>
            $"{PieceColour} {Type} to {EndPosition.File}{EndPosition.Rank}"; 
    }
}
