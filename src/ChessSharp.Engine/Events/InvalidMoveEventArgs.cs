namespace ChessSharp.Engine.Events
{
    public class InvalidMoveEventArgs : UserMovedPieceEventArgs
    {
        public InvalidMoveEventArgs(int fromSquareIndex, int toSquareIndex)
            : base(fromSquareIndex, toSquareIndex)
        {
        }
    }
}
