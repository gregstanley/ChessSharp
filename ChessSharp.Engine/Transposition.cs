using ChessSharp.Enums;

namespace ChessSharp.Engine
{
    public class Transposition
    {
        public void Set(ulong key, byte depth, Colour colour, int evaluation, uint bestMove, int age)
        {
            Key = key;
            Depth = depth;
            Colour = colour;
            Evaluation = evaluation;
            BestMove = bestMove;
            Age = age;
        }

        public ulong Key { get; private set; }

        public byte Depth { get; private set; }

        public Colour Colour { get; private set; }

        public int Evaluation { get; private set; }

        public uint BestMove { get; private set; }

        public int Age { get; private set; }
    }
}