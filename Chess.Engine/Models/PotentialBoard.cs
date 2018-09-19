using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Engine.Models
{
    public class PotentialBoard
    {
        private readonly IList<Move> _moves;

        public enum NodeType { PV, Alpha, Beta, Exact }

        //public PotentialBoard(Board board, double score, NodeType type)
        public PotentialBoard(Move move, double score, NodeType type, PotentialBoard pv)
        {
            //Board = board;
            _moves = new List<Move>(64) { move };
            Score = score;
            Type = type;
            Pv = pv;
        }

        public PotentialBoard(IList<Move> moves, double score, NodeType type, PotentialBoard pv)
        {
            //Board = board;
            _moves = moves;
            Score = score;
            Type = type;
            Pv = pv;
        }

        public void AddMove(Move move)
        {
            _moves.Add(move);
        }

        //public Board Board { get; }
        public IReadOnlyList<Move> Moves => (IReadOnlyList<Move>)_moves;

        public double Score { get; }

        public double PotentialScore => -Score;

        public NodeType Type { get; set; } = NodeType.PV;

        public PotentialBoard Pv { get; }

        public PotentialBoard WithType(NodeType type) =>
            new PotentialBoard(_moves, Score, type, Pv);

        public override string ToString() =>
            $"{_moves.First().ToString()} {Type} {Math.Round(Score, 2)}";
    }
}
