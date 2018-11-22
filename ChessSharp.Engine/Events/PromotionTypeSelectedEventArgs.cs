using ChessSharp.Enums;

namespace ChessSharp.Engine.Events
{
    public class PromotionTypeSelectedEventArgs
    {
        public PromotionTypeSelectedEventArgs(int fromSquareIndex, int toSquareIndex, PieceType pieceType)
        {
            FromSquareIndex = fromSquareIndex;
            ToSquareIndex = toSquareIndex;
            PieceType = pieceType;
        }

        public int FromSquareIndex { get; }

        public int ToSquareIndex { get; }

        public PieceType PieceType { get; }
    }
}
