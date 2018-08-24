using Chess.Engine.Extensions;
using Chess.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.Engine.Ai.Searches
{
    public class AlphaBeta : ISearchMoves
    {
        public int PositionCounter => _positionCounter;

        private int _positionCounter = 0;

        private IEnumerable<Board> Prioritise(IEnumerable<Board> primary, IEnumerable<Board> secondary)
        {
            return primary.Concat(secondary.Where(x => !primary.Contains(x)));
        }

        public Board DoSearch(Board board, Colour colour, int depth, bool isMax)
        {
            _positionCounter = 0;

            PotentialBoard bestChildBoard = null;

            board.GenerateChildBoards(colour, 1);
            board.UpdateStateInfo();

            var internalStringBuilder = new StringBuilder();

            var orderedBoards = colour == Colour.White
                ? board.ChildBoards.OrderByDescending(x => x.ProjectedEvaluation).ThenByDescending(x => x.Evaluation)
                : board.ChildBoards.OrderBy(x => x.ProjectedEvaluation).ThenBy(x => x.Evaluation);

            //Parallel.ForEach(moves, (move) =>
            Parallel.ForEach(orderedBoards, (childBoard) =>
            //foreach(var childBoard in orderedBoards)
            {
                var currentChildBoard = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, -10000, 10000, !isMax, internalStringBuilder);

                var potentialBoard = new PotentialBoard(childBoard, currentChildBoard.PotentialScore, PotentialBoard.NodeType.PV);

                childBoard.UpdateStateInfo();
                childBoard.ProjectedEvaluation = potentialBoard.PotentialScore;

                if (bestChildBoard == null || currentChildBoard.PotentialScore > bestChildBoard.Score)
                    bestChildBoard = potentialBoard;
            });

            //sb.AppendLine($"ROOT:");

            //orderedBoards = colour == Colour.White
            //    ? board.ChildBoards.OrderByDescending(x => x.ProjectedEvaluation)
            //    : board.ChildBoards.OrderBy(x => x.ProjectedEvaluation);

            //foreach (var childBoard in orderedBoards)
            //    sb.AppendLine($" - {childBoard}");

            return bestChildBoard.Board;
        }

        private PotentialBoard AlphaBetaInternal(Board board, Colour colour, int depth, double alpha, double beta, bool isMax, StringBuilder sb)
        {
            Interlocked.Increment(ref _positionCounter);

            if (depth == 0)
                return new PotentialBoard(board, board.Evaluate(colour), PotentialBoard.NodeType.PV);

            board.GenerateChildBoards(colour, 1);
            board.UpdateStateInfo();

            PotentialBoard bestChildBoard;

            if (isMax)
            {
                bestChildBoard = new PotentialBoard(null, double.MaxValue, PotentialBoard.NodeType.PV);

                var orderedBoards = board.ChildBoards
                    .OrderByDescending(x => x.ProjectedEvaluation)
                    .ThenByDescending(x => x.Evaluation);

                foreach (var childBoard in orderedBoards)
                {
                    var currentChildBoard = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    //childBoard.UpdateStateInfo();
                    childBoard.ProjectedEvaluation = currentChildBoard.Score;

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
                var orderedBoards = board.ChildBoards
                    .OrderBy(x => x.ProjectedEvaluation)
                    .ThenBy(x => x.Evaluation);

                bestChildBoard = new PotentialBoard(null, double.MinValue, PotentialBoard.NodeType.PV);

                foreach (var childBoard in orderedBoards)
                {
                    var currentChildBoard = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    //childBoard.UpdateStateInfo();
                    childBoard.ProjectedEvaluation = currentChildBoard.Score;

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
