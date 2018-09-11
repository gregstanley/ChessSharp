using System;

namespace Chess.Engine.Models
{
    public class PotentialBoard
    {
        public enum NodeType { PV, Cut, All }

        public PotentialBoard(Board board, double score, NodeType type)
        {
            Board = board;
            Score = score;
            Type = type;
        }

        public Board Board { get; }

        public double Score { get; }

        public double PotentialScore => -Score;

        public NodeType Type { get; set; } = NodeType.PV;

        public PotentialBoard WithType(NodeType type) =>
            new PotentialBoard(Board, Score, type);

        public override string ToString() =>
            $"{Board.MoveToString()} {Type} {Math.Round(Score, 2)}";
    }
}
