namespace ChessSharp.Engine.Events
{
    public class PromotionTypeRequiredEventArgs : UserMovedPieceEventArgs
    {
        public PromotionTypeRequiredEventArgs(int fromSquareIndex, int toSquareIndex)
            : base(fromSquareIndex, toSquareIndex)
        {
        }
    }
}
