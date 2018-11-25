using ChessSharp.Enums;

namespace ChessSharp.Engine.Events
{
    public class PromotionTypeSelectedEventArgs : UserMovedPieceEventArgs
    {
        public PromotionTypeSelectedEventArgs(int fromSquareIndex, int toSquareIndex, PieceType pieceType)
            : base(fromSquareIndex, toSquareIndex)
        {
            PieceType = pieceType;
        }

        public PieceType PieceType { get; }
    }
}
