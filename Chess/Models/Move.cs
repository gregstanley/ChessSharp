namespace Chess.Models
{
    public class Move
    {
        public Colour PieceColour { get; }

        public PieceType Type { get; }

        public RankFile StartPosition { get; }

        public RankFile EndPosition { get; }

        public PieceType PromotionType { get; } = PieceType.None;

        public Move(Colour pieceColour, PieceType pieceType, RankFile startPosition, RankFile endPosition, PieceType promotionType)
            : this(pieceColour, pieceType, startPosition, endPosition)
        {
            PromotionType = promotionType;
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
            $"{PieceColour} {Type} to {EndPosition.File}{EndPosition.Rank}"; 
    }
}
