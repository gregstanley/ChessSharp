namespace ChessSharp.Tests
{
    public class MovePerft
    {
        public MovePerft(MoveViewer move, int nodes)
        {
            Move = move;
            Nodes = nodes;
        }

        public MoveViewer Move { get; }

        public int Nodes { get; }
    }
}
