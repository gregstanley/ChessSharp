using Chess.Extensions;
using Chess.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chess
{
    public class AlphaBeta
    {
        public int PositionCounter => _positionCounter;

        private int _positionCounter = 0;

        public Board AlphaBetaRoot(Board board, Colour colour, int depth, bool isMax, StringBuilder sb)
        {
            _positionCounter = 0;

            PotentialBoard bestChildBoard = null;

            var moves = board.FindMoves();

            List<PotentialBoard> potentialBoards = new List<PotentialBoard>();

            var internalStringBuilder = new StringBuilder();

            Parallel.ForEach(moves, (move) =>
            {
                var childBoard = board.ApplyMove(move);

                if (!childBoard.IsCheck(colour))
                {
                    //var childBoard = new Board(board, move, childBitBoard, _bitBoardMoveFinder);

                    //if (board.ChildBoards.SingleOrDefault(x => x.Code == childBoard.GetCode()) == null)
                    //    ChildBoards.Add(childBoard);

                    var currentChildBoard = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, -10000, 10000, !isMax, internalStringBuilder);

                    var potentialBoard = new PotentialBoard(childBoard, currentChildBoard.PotentialScore, PotentialBoard.NodeType.PV);

                    potentialBoards.Add(potentialBoard);

                    sb.AppendLine($"ROOT: {potentialBoard}");

                    if (bestChildBoard == null || currentChildBoard.PotentialScore > bestChildBoard.Score)
                        bestChildBoard = potentialBoard;
                }
            });

            sb.AppendLine($"ROOT:");

            foreach (var evaluatedBoard in potentialBoards)
                sb.AppendLine($" - {evaluatedBoard}");

            sb.Append(internalStringBuilder);

            return bestChildBoard.Board;
        }

        private PotentialBoard AlphaBetaInternal(Board board, Colour colour, int depth, double alpha, double beta, bool isMax, StringBuilder sb)
        {
            Interlocked.Increment(ref _positionCounter);

            if (depth == 0)
                return new PotentialBoard(board, board.Evaluate(colour), PotentialBoard.NodeType.PV);

            var moves = board.FindMoves();

            PotentialBoard bestChildBoard;

            if (isMax)
            {
                bestChildBoard = new PotentialBoard(null, double.MaxValue, PotentialBoard.NodeType.PV);

                foreach (var move in moves)
                {
                    var childBoard = board.ApplyMove(move);

                    if (childBoard.IsCheck(colour))
                        continue;

                    //board.ChildBoards.Add(childBoard);

                    var currentChildBoard = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    if (currentChildBoard.PotentialScore > bestChildBoard.PotentialScore)
                        bestChildBoard = currentChildBoard;

                    alpha = Math.Max(alpha, bestChildBoard.PotentialScore);

                    if (beta <= alpha)
                    {
                        sb.AppendLine($" >>> CUT {beta} <= {alpha} (D: {depth} Board: {board.Code} MAX)");
                        return bestChildBoard.WithType(PotentialBoard.NodeType.Cut);
                    }
                }
            }
            else
            {
                bestChildBoard = new PotentialBoard(null, double.MinValue, PotentialBoard.NodeType.PV);

                foreach (var move in moves)
                {
                    var childBoard = board.ApplyMove(move);

                    if (childBoard.IsCheck(colour))
                        continue;

                    //board.ChildBoards.Add(childBoard);

                    var currentChildBoard = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    if (currentChildBoard.PotentialScore < bestChildBoard.PotentialScore)
                        bestChildBoard = currentChildBoard;

                    beta = Math.Min(beta, bestChildBoard.PotentialScore);

                    if (beta <= alpha)
                    {
                        sb.AppendLine($" >>> CUT {beta} <= {alpha} (D: {depth} Board: {board.Code} MIN)");
                        return bestChildBoard.WithType(PotentialBoard.NodeType.Cut);
                    }
                }
            }

            if (bestChildBoard.Board == null)
                sb.AppendLine($"EXACT: {board.GetCode()} No Childboards found");
            else
                sb.AppendLine($"EXACT: {board.GetCode()} Best: {bestChildBoard}");

            return bestChildBoard.WithType(PotentialBoard.NodeType.All);
        }
    }
}
