using System.Windows;

namespace ChessSharp_UI
{
    public class UserMovedPieceEventArgs : RoutedEventArgs
    {
        public int FromSquareIndex { get; }

        public int ToSquareIndex { get; }

        public MoveResult Result { get; }

        public enum MoveResult
        {
            Invalid,
            Ordinary,
            Promotion
        }

        public UserMovedPieceEventArgs(int fromSquareIndex, int toSquareIndex, MoveResult result)
        {
            FromSquareIndex = fromSquareIndex;
            ToSquareIndex = toSquareIndex;
            Result = result;
        }
    }
}
