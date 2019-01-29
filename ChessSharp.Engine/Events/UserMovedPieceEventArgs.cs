using System;

namespace ChessSharp.Engine.Events
{
    public class UserMovedPieceEventArgs : EventArgs
    {
        public UserMovedPieceEventArgs(int fromSquareIndex, int toSquareIndex)
        {
            this.FromSquareIndex = fromSquareIndex;
            this.ToSquareIndex = toSquareIndex;
        }

        public int FromSquareIndex { get; }

        public int ToSquareIndex { get; }
    }
}
