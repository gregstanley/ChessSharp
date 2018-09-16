using Chess.Engine.Models;

namespace Chess.Engine
{
    public class Transposition
    {
        public Transposition(ulong key, int depth, Colour colour, double evaluation, PotentialBoard potentialBoard, Move bestMove = null)
        {
            Key = key;
            Depth = depth;
            Colour = colour;
            Evaluation = evaluation;
            PotentialBoard = potentialBoard;
            BestMove = bestMove;
        }

        public ulong Key { get; }

        public int Depth { get; }

        public Colour Colour { get; }

        public double Evaluation { get; }

        public PotentialBoard PotentialBoard { get; set; }

        public Move BestMove { get; }
    }
}