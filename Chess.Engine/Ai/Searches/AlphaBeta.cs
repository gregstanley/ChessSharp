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
    public class AlphaBeta
    {
        public int PositionCounter => _positionCounter;

        private int _positionCounter = 0;

        private IEnumerable<Board> Prioritise(IEnumerable<Board> primary, IEnumerable<Board> secondary)
        {
            return primary.Concat(secondary.Where(x => !primary.Contains(x)));
        }

        public Board AlphaBetaRoot(Board board, Colour colour, int depth, bool isMax, StringBuilder sb)
        {
            _positionCounter = 0;

            PotentialBoard bestChildBoard = null;

            //var moves = board.FindMoves();
            board.GenerateChildBoards(colour, 1);

            //var orderedBoards = board.ChildBoards;

            var primaryBoards = board.PotentialChildBoards
                .OrderByDescending(x => x.PotentialScore)
                .Select(x => x.Board);

            var orderedBoards = Prioritise(primaryBoards, board.ChildBoards);

            //List<PotentialBoard> potentialBoards = new List<PotentialBoard>();

            var internalStringBuilder = new StringBuilder();

            //Parallel.ForEach(moves, (move) =>
            Parallel.ForEach(orderedBoards, (childBoard) =>
            {
                //var childBoard = board.ApplyMove(move);

                //if (!childBoard.IsCheck(colour))
                {
                    //var childBoard = new Board(board, move, childBitBoard, _bitBoardMoveFinder);

                    //if (board.ChildBoards.SingleOrDefault(x => x.Code == childBoard.GetCode()) == null)
                    //    ChildBoards.Add(childBoard);

                    var currentChildBoard = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, -10000, 10000, !isMax, internalStringBuilder);

                    var potentialBoard = new PotentialBoard(childBoard, currentChildBoard.PotentialScore, PotentialBoard.NodeType.PV);

                    //potentialBoards.Add(potentialBoard);
                    board.UpdatePotentialBoard(potentialBoard);

                    sb.AppendLine($"ROOT: {potentialBoard}");

                    if (bestChildBoard == null || currentChildBoard.PotentialScore > bestChildBoard.Score)
                        bestChildBoard = potentialBoard;
                }
            });

            sb.AppendLine($"ROOT:");

            foreach (var evaluatedBoard in board.PotentialChildBoards)
                sb.AppendLine($" - {evaluatedBoard}");

            try
            {
                sb.Append(internalStringBuilder);

            }
            catch(ArgumentOutOfRangeException ex)
            {
                var bp = true;
            }

            return bestChildBoard.Board;
        }

        private PotentialBoard AlphaBetaInternal(Board board, Colour colour, int depth, double alpha, double beta, bool isMax, StringBuilder sb)
        {
            Interlocked.Increment(ref _positionCounter);

            if (depth == 0)
                return new PotentialBoard(board, board.Evaluate(colour), PotentialBoard.NodeType.PV);

            //var moves = board.FindMoves();

            board.GenerateChildBoards(colour, 1);

            PotentialBoard bestChildBoard;

            if (isMax)
            {
                var primaryBoards = board.PotentialChildBoards
                .OrderByDescending(x => x.PotentialScore)
                .Select(x => x.Board);

                var orderedBoards = Prioritise(primaryBoards, board.ChildBoards);

                bestChildBoard = new PotentialBoard(null, double.MaxValue, PotentialBoard.NodeType.PV);

                //foreach (var move in moves)
                //{
                //    var childBoard = board.ApplyMove(move);

                //    if (childBoard.IsCheck(colour))
                //        continue;

                //board.ChildBoards.Add(childBoard);

                foreach (var childBoard in orderedBoards)
                {
                    var currentChildBoard = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    board.UpdatePotentialBoard(currentChildBoard);

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
                var primaryBoards = board.PotentialChildBoards
                .OrderBy(x => x.PotentialScore)
                .Select(x => x.Board);

                var orderedBoards = Prioritise(primaryBoards, board.ChildBoards);

                bestChildBoard = new PotentialBoard(null, double.MinValue, PotentialBoard.NodeType.PV);

                //foreach (var move in moves)
                //{
                //    var childBoard = board.ApplyMove(move);

                //    if (childBoard.IsCheck(colour))
                //        continue;

                //board.ChildBoards.Add(childBoard);

                foreach (var childBoard in orderedBoards)
                {
                    var currentChildBoard = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    board.UpdatePotentialBoard(currentChildBoard);

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
