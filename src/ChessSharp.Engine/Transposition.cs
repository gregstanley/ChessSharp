using ChessSharp.Common.Enums;

namespace ChessSharp.Engine
{
    public struct Transposition
    {
        public ulong Key { get; private set; }

        public byte Depth { get; private set; }

        public Colour Colour { get; private set; }

        public int Evaluation { get; private set; }

        public uint BestMove { get; private set; }

        public byte Age { get; private set; }

        public void Set(ulong key, byte depth, Colour colour, int evaluation, uint bestMove, byte age)
        {
            Key = key;
            Depth = depth;
            Colour = colour;
            Evaluation = evaluation;
            BestMove = bestMove;
            Age = age;
        }
    }
}
