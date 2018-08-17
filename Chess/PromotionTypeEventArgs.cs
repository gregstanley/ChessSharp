using Chess.Models;
using System.Windows;

namespace Chess
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
