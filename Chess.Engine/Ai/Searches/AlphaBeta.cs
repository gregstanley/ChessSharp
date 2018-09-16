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

        private TranspositionTable TranspositionTable { get; set; } = new TranspositionTable();

        //private IEnumerable<Board> Prioritise(IEnumerable<Board> primary, IEnumerable<Board> secondary)
        //{
        //    return primary.Concat(secondary.Where(x => !primary.Contains(x)));
        //}

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

            var existingTransposition = TranspositionTable.Find(board.Key);

            Move previousBestMove = null;

            if (existingTransposition != null)
            {
                if (existingTransposition.Key == board.Key)
                {
                    if (existingTransposition.Depth > depth)
                    {
                        return existingTransposition.PotentialBoard;
                    }

                    if (existingTransposition.BestMove != null)
                    {
                        previousBestMove = existingTransposition.BestMove;
                    }
                }
            }

            var moves = board.FindMoves2(colour)
                .OrderByDescending(x => x.CapturePieceType == PieceType.Pawn)
                .OrderByDescending(x => x.CapturePieceType == PieceType.Knight)
                .OrderByDescending(x => x.CapturePieceType == PieceType.Bishop)
                .OrderByDescending(x => x.CapturePieceType == PieceType.Rook)
                .OrderByDescending(x => x.CapturePieceType == PieceType.Queen)
                .OrderByDescending(x => x.CapturePieceType == PieceType.King)
                .ToList();

            // Move previous best move to top
            if (previousBestMove != null)
            {
                var possibleBestMove = moves.SingleOrDefault(x => x.Notation == previousBestMove.Notation);

                if (possibleBestMove != null)
                {
                    moves.Remove(possibleBestMove);
                    moves.Insert(0, possibleBestMove);
                }
                else
                {
                    var bp = true;
                }
            }

            PotentialBoard bestChildBoard;

            var childBoard = new Board(board, colour.Opposite());
            Move bestCurrentMove = null;

            if (isMax)
            {
                bestChildBoard = new PotentialBoard(null, double.MaxValue, PotentialBoard.NodeType.PV);
                
                foreach (var move in moves)
                {
                    childBoard.MakeMove(move);

                    //var existingTransposition = TranspositionTable.Find(childBoard.Key);

                    //PotentialBoard currentChildBoard = null;

                    //if (existingTransposition != null)
                    //{
                    //    if (existingTransposition.Key == childBoard.Key)
                    //    {
                    //        if (existingTransposition.Depth >= depth)
                    //        {
                    //            currentChildBoard = existingTransposition.PotentialBoard;
                    //        }
                    //    }
                    //}

                    //if (currentChildBoard == null)
                    //{
                    //    currentChildBoard = AlphaBetaInternal2(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);
                    //    TranspositionTable.Add(new Transposition(childBoard.Key, depth, childBoard.Turn, currentChildBoard.Score, currentChildBoard));
                    //}

                    var currentChildBoard = AlphaBetaInternal2(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);
                    childBoard.UnMakeMove(move);

                    childBoard.ProjectedEvaluation = currentChildBoard.Score;

                    if (currentChildBoard.PotentialScore > bestChildBoard.PotentialScore)
                    {
                        bestCurrentMove = move;
                        bestChildBoard = currentChildBoard;
                    }

                    alpha = Math.Max(alpha, bestChildBoard.PotentialScore);

                    if (beta <= alpha)
                    {
                        var cbwtm1 = bestChildBoard.WithType(PotentialBoard.NodeType.Cut);
                        TranspositionTable.Add(new Transposition(board.Key, depth, board.Turn, bestChildBoard.Score, cbwtm1, bestCurrentMove));
                        return cbwtm1;
                    }
                }
            }
            else
            {
                bestChildBoard = new PotentialBoard(null, double.MinValue, PotentialBoard.NodeType.PV);

                foreach (var move in moves)
                {
                    childBoard.MakeMove(move);

                    //var existingTransposition = TranspositionTable.Find(childBoard.Key);

                    //PotentialBoard currentChildBoard = null;

                    //if (existingTransposition != null)
                    //{
                    //    if (existingTransposition.Key == childBoard.Key)
                    //    {
                    //        if (existingTransposition.Depth >= depth)
                    //        {
                    //            currentChildBoard = existingTransposition.PotentialBoard;
                    //        }
                    //    }
                    //}

                    //if (currentChildBoard == null)
                    //{
                    //    currentChildBoard = AlphaBetaInternal2(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);
                    //    TranspositionTable.Add(new Transposition(childBoard.Key, depth, childBoard.Turn, currentChildBoard.Score, currentChildBoard));
                    //}

                    var currentChildBoard = AlphaBetaInternal2(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    childBoard.UnMakeMove(move);

                    childBoard.ProjectedEvaluation = currentChildBoard.Score;

                    if (currentChildBoard.PotentialScore < bestChildBoard.PotentialScore)
                    {
                        bestCurrentMove = move;
                        bestChildBoard = currentChildBoard;
                    }

                    beta = Math.Min(beta, bestChildBoard.PotentialScore);

                    if (beta <= alpha)
                    {
                        var cbwtm2 = bestChildBoard.WithType(PotentialBoard.NodeType.Cut);
                        TranspositionTable.Add(new Transposition(board.Key, depth, board.Turn, bestChildBoard.Score, cbwtm2, bestCurrentMove));
                        return cbwtm2;
                        //return bestChildBoard.WithType(PotentialBoard.NodeType.Cut);
                    }
                }
            }

            var cbwt = bestChildBoard.WithType(PotentialBoard.NodeType.All);
            TranspositionTable.Add(new Transposition(board.Key, depth, board.Turn, bestChildBoard.Score, cbwt, bestCurrentMove));

            return cbwt;
            //return bestChildBoard.WithType(PotentialBoard.NodeType.All);
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
