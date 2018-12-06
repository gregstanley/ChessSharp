using ChessSharp.Enums;

namespace ChessSharp.Engine
{
    public class Transposition
    {
        public Transposition()
        {
        }

        public void Set(ulong key, int depth, Colour colour, double evaluation, uint bestMove)
        {
            Key = key;
            Depth = depth;
            Colour = colour;
            Evaluation = evaluation;
            BestMove = bestMove;
        }

        public ulong Key { get; private set; }

        public int Depth { get; private set; }

        public Colour Colour { get; private set; }

        public double Evaluation { get; private set; }

        public uint BestMove { get; private set; }
    }
}