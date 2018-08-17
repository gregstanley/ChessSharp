using Chess.Bit;
using Chess.Extensions;
using Chess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess
{
    public class AlphaBeta
    {
        private BitBoardMoveFinder _bitBoardMoveFinder { get; set; }

        public AlphaBeta(BitBoardMoveFinder bitBoardMoveFinder)
        {
            _bitBoardMoveFinder = bitBoardMoveFinder;
        }

        public Board AlphaBetaRoot(Board board, Colour colour, int depth, bool isMax, StringBuilder sb)
        {
            PotentialBoard bestChildBoard = null;

            var moves = board.FindMoves(colour);

            List<PotentialBoard> potentialBoards = new List<PotentialBoard>();

            foreach (var move in moves)
            {
                var childBoard = board.ApplyMove(move);

                if (childBoard.IsCheck(colour))
                    continue;

                //var childBoard = new Board(board, move, childBitBoard, _bitBoardMoveFinder);

                //if (board.ChildBoards.SingleOrDefault(x => x.Code == childBoard.GetCode()) == null)
                //    ChildBoards.Add(childBoard);

                var currentChildBoard = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, -10000, 10000, !isMax, sb);

                var potentialBoard = new PotentialBoard(childBoard, currentChildBoard.PotentialScore, PotentialBoard.NodeType.PV);

                potentialBoards.Add(potentialBoard);

                sb.AppendLine($"ROOT: {potentialBoard}");

                if (bestChildBoard == null || currentChildBoard.PotentialScore > bestChildBoard.Score)
                    bestChildBoard = potentialBoard;
            }

            sb.AppendLine($"ROOT:");

            foreach (var evaluatedBoard in potentialBoards)
                sb.AppendLine($" - {evaluatedBoard}");

            return bestChildBoard.Board;
        }

        private PotentialBoard AlphaBetaInternal(Board board, Colour colour, int depth, double alpha, double beta, bool isMax, StringBuilder sb)
        {
            if (depth == 0)
                return new PotentialBoard(board, board.Evaluate(colour), PotentialBoard.NodeType.PV);

            var moves = board.FindMoves(colour);

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

            sb.AppendLine($"EXACT: {board.GetCode()} Best: {bestChildBoard} ");

            return bestChildBoard.WithType(PotentialBoard.NodeType.All);
        }
    }
}
