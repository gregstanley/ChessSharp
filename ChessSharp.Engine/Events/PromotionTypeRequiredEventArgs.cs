using System;

namespace ChessSharp.Engine.Events
{
    public class PromotionTypeRequiredEventArgs : EventArgs
    {
        public PromotionTypeRequiredEventArgs(int fromSquareIndex, int toSquareIndex)
        {
            FromSquareIndex = fromSquareIndex;
            ToSquareIndex = toSquareIndex;
        }

        public int FromSquareIndex { get; }

        public int ToSquareIndex { get; }
    }
}
