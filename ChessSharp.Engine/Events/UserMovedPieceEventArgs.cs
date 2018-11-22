using System;

namespace ChessSharp.Engine.Events
{
    public class UserMovedPieceEventArgs : EventArgs
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
