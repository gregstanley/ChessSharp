using ChessSharp.Enums;
using System.Windows;

namespace ChessSharp_UI
{
    public class PromotionTypeEventArgs : RoutedEventArgs
    {
        public PieceType PieceType { get; }

        public PromotionTypeEventArgs(PieceType pieceType)
        {
            PieceType = pieceType;
        }
    }
}
