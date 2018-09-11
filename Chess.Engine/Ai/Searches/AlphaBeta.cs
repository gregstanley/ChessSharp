﻿using Chess.Engine.Extensions;
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

            board.GenerateChildBoards(colour, 2);

            var legalMoves = board.GetLegalMoves();

            if (!legalMoves.Any())
                return null;

            var internalStringBuilder = new StringBuilder();

            var orderedBoards = colour == Colour.White
                ? legalMoves.OrderByDescending(x => x.ProjectedEvaluation).ThenByDescending(x => x.Evaluation)
                : legalMoves.OrderBy(x => x.ProjectedEvaluation).ThenBy(x => x.Evaluation);

            //Parallel.ForEach(moves, (move) =>
            //Parallel.ForEach(orderedBoards, (childBoard) =>
            foreach(var childBoard in orderedBoards)
            {
                var currentChildBoard = AlphaBetaInternal2(childBoard, colour.Opposite(), depth - 1, -10000, 10000, !isMax, internalStringBuilder);

                var potentialBoard = new PotentialBoard(childBoard, currentChildBoard.PotentialScore, PotentialBoard.NodeType.PV);

                childBoard.ProjectedEvaluation = potentialBoard.PotentialScore;

                if (bestChildBoard == null || currentChildBoard.PotentialScore > bestChildBoard.Score)
                    bestChildBoard = potentialBoard;
            };

            return board.ChildBoards.Single(x => x.Notation == bestChildBoard.Board.Notation);
        }

        private PotentialBoard AlphaBetaInternal2(Board board, Colour colour, int depth, double alpha, double beta, bool isMax, StringBuilder sb)
        {
            Interlocked.Increment(ref _positionCounter);

            if (depth == 0)
                return new PotentialBoard(board, board.Evaluate(colour), PotentialBoard.NodeType.PV);

            var moves = board.FindMoves2(colour);

            PotentialBoard bestChildBoard;

            var childBoard = new Board(board, colour.Opposite());

            if (isMax)
            {
                bestChildBoard = new PotentialBoard(null, double.MaxValue, PotentialBoard.NodeType.PV);
                
                foreach (var move in moves)
                {
                    var previousState = childBoard.MakeMove(move);
 
                    var currentChildBoard = AlphaBetaInternal2(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    childBoard.UnMakeMove(previousState);

                    childBoard.ProjectedEvaluation = currentChildBoard.Score;

                    if (currentChildBoard.PotentialScore > bestChildBoard.PotentialScore)
                        bestChildBoard = currentChildBoard;

                    alpha = Math.Max(alpha, bestChildBoard.PotentialScore);

                    if (beta <= alpha)
                        return bestChildBoard.WithType(PotentialBoard.NodeType.Cut);
                }
            }
            else
            {
                bestChildBoard = new PotentialBoard(null, double.MinValue, PotentialBoard.NodeType.PV);

                foreach (var move in moves)
                {
                    var previousState = childBoard.MakeMove(move);

                    var currentChildBoard = AlphaBetaInternal2(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    childBoard.UnMakeMove(previousState);

                    childBoard.ProjectedEvaluation = currentChildBoard.Score;

                    if (currentChildBoard.PotentialScore < bestChildBoard.PotentialScore)
                        bestChildBoard = currentChildBoard;

                    beta = Math.Min(beta, bestChildBoard.PotentialScore);

                    if (beta <= alpha)
                        return bestChildBoard.WithType(PotentialBoard.NodeType.Cut);
                }
            }

            return bestChildBoard.WithType(PotentialBoard.NodeType.All);
        }

        private PotentialBoard AlphaBetaInternal(Board board, Colour colour, int depth, double alpha, double beta, bool isMax, StringBuilder sb)
        {
            Interlocked.Increment(ref _positionCounter);

            if (depth == 0)
                return new PotentialBoard(board, board.Evaluate(colour), PotentialBoard.NodeType.PV);

            board.GenerateChildBoards(colour, 1);

            var legalMoves = board.GetLegalMoves();

            PotentialBoard bestChildBoard;

            if (isMax)
            {
                bestChildBoard = new PotentialBoard(null, double.MaxValue, PotentialBoard.NodeType.PV);

                var orderedBoards = legalMoves
                    .OrderByDescending(x => x.ProjectedEvaluation)
                    .ThenByDescending(x => x.Evaluation);

                foreach (var childBoard in orderedBoards)
                {
                    var currentChildBoard = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    childBoard.ProjectedEvaluation = currentChildBoard.Score;

                    if (currentChildBoard.PotentialScore > bestChildBoard.PotentialScore)
                        bestChildBoard = currentChildBoard;

                    alpha = Math.Max(alpha, bestChildBoard.PotentialScore);

                    if (beta <= alpha)
                    {
                        //sb.AppendLine($" >>> CUT {beta} <= {alpha} (D: {depth} Board: {board.Code} MAX)");

                        bestChildBoard.Board.Orphan();

                        return bestChildBoard.WithType(PotentialBoard.NodeType.Cut);
                    }
                }
            }
            else
            {
                var orderedBoards = legalMoves
                    .OrderBy(x => x.ProjectedEvaluation)
                    .ThenBy(x => x.Evaluation);

                bestChildBoard = new PotentialBoard(null, double.MinValue, PotentialBoard.NodeType.PV);

                foreach (var childBoard in orderedBoards)
                {
                    var currentChildBoard = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    childBoard.ProjectedEvaluation = currentChildBoard.Score;

                    if (currentChildBoard.PotentialScore < bestChildBoard.PotentialScore)
                        bestChildBoard = currentChildBoard;

                    beta = Math.Min(beta, bestChildBoard.PotentialScore);

                    if (beta <= alpha)
                    {
                        //sb.AppendLine($" >>> CUT {beta} <= {alpha} (D: {depth} Board: {board.Code} MIN)");

                        bestChildBoard.Board.Orphan();

                        return bestChildBoard.WithType(PotentialBoard.NodeType.Cut);
                    }
                }
            }

            //if (bestChildBoard.Board == null)
            //    sb.AppendLine($"EXACT: {board.GetFriendlyCode()} No Childboards found");
            //else
            //    sb.AppendLine($"EXACT: {board.GetFriendlyCode()} Best: {bestChildBoard}");

            bestChildBoard.Board.Orphan();

            return bestChildBoard.WithType(PotentialBoard.NodeType.All);
        }
    }
}
