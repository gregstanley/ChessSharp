namespace Chess.Engine.Models
{
    public class Move
    {
        public Colour PieceColour { get; }

        public PieceType Type { get; }

        public RankFile StartPosition { get; }

        public RankFile EndPosition { get; }

        public PieceType CapturePieceType { get; } = PieceType.None;

        public SquareFlag EnPassantSquare { get; } = 0;

        public SquareFlag EnPassantCaptureSquare { get; } = 0;

        public PieceType PromotionType { get; } = PieceType.None;

        public Move(Colour pieceColour, PieceType pieceType, RankFile startPosition, RankFile endPosition, PieceType capturePieceType, PieceType promotionType)
            : this(pieceColour, pieceType, startPosition, endPosition, capturePieceType)
        {
            PromotionType = promotionType;
        }

        public Move(Colour pieceColour, PieceType pieceType, RankFile startPosition, RankFile endPosition, PieceType capturePieceType, SquareFlag enPassantSquare, SquareFlag enPassantCaptureSquare)
            : this(pieceColour, pieceType, startPosition, endPosition, capturePieceType)
        {
            EnPassantSquare = enPassantSquare;
            EnPassantCaptureSquare = enPassantCaptureSquare;
        }

        public Move(Colour pieceColour, PieceType pieceType, RankFile startPosition, RankFile endPosition, PieceType capturePieceType)
            : this(pieceColour, pieceType, startPosition, endPosition)
        {
            CapturePieceType = capturePieceType;
        }

        public Move(Colour pieceColour, PieceType pieceType, RankFile startPosition, RankFile endPosition)
        {
            PieceColour = pieceColour;
            Type = pieceType;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }

        public string UiCode => AppendPromotionString(BaseString);

        public virtual string GetFriendlyCode()
        {
            var baseString = 
                $"{PieceColour}{Type}-{StartPosition.File}{StartPosition.Rank}-{EndPosition.File}{EndPosition.Rank}";

            return AppendPromotionString(baseString);
        }

        public override string ToString()
        {
            var baseString = BaseString;

            if (CapturePieceType == PieceType.None)
                baseString = $"{baseString}";
            else
                baseString = $"{baseString}x";

            return AppendPromotionString(baseString);
        }

        public string ToFriendlyString() =>
            $"{PieceColour} {Type} to {EndPosition.File}{EndPosition.Rank}";

        private string BaseString =>
            $"{StartPosition.File}{StartPosition.Rank}-{EndPosition.File}{EndPosition.Rank}";

        private string AppendPromotionString(string baseString)
        {
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
    }
}
