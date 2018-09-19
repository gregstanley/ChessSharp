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

        public event EventHandler IterationComplete;

        protected virtual void OnIterationComplete(IterationCompleteEventArgs e)
        {
            IterationComplete?.Invoke(this, e);
        }

        public AlphaBeta()
        {
        }

        private TranspositionTable TranspositionTable { get; set; } = new TranspositionTable();

        //private IEnumerable<Board> Prioritise(IEnumerable<Board> primary, IEnumerable<Board> secondary)
        //{
        //    return primary.Concat(secondary.Where(x => !primary.Contains(x)));
        //}

        public Board DoSearch(Board board, Colour colour, int depth, bool isMax)
        {
            _positionCounter = 0;

            // Generate the ACTUAL immutable boards
            board.GenerateChildBoards(colour, 2);

            

            var existingTransposition = TranspositionTable.Find(board.Key);

            Move previousBestMove = null;

            if (existingTransposition != null)
            {
                if (existingTransposition.Key == board.Key)
                {
                    //if (existingTransposition.Depth > 1 && existingTransposition.Depth > depth)
                    //{
                    //    existingTransposition.PotentialBoard;
                    //}

                    if (existingTransposition.BestMove != null)
                    {
                        previousBestMove = existingTransposition.BestMove;
                    }
                }
            }
            
            var legalMoves = board.GetLegalMoves();

            if (!legalMoves.Any())
                return null;

            //var orderedBoards = colour == Colour.White
            //    ? legalMoves.OrderByDescending(x => x.ProjectedEvaluation).ThenByDescending(x => x.Evaluation)
            //    : legalMoves.OrderBy(x => x.ProjectedEvaluation).ThenBy(x => x.Evaluation);

            var tempBoard = new Board(board, colour.Opposite());

            var maxDepth = depth;
            var currentDepth = 1;

            //PotentialBoard bestChildBoard = null;
            PotentialBoard iterativeBestPotentialBoard = null;
            var iterativeBestMove = previousBestMove;

            var moveStack = new Stack<Move>(64);
            var pvStack = new Stack<Move>(64);

            while (currentDepth <= maxDepth)
            {
                var movePicker = new MovePicker(colour, board, iterativeBestMove);

                iterativeBestMove = null;
                iterativeBestPotentialBoard = null;

                //Move bestCurrentMove = null;
                //Parallel.ForEach(moves, (move) =>
                //Parallel.ForEach(orderedBoards, (childBoard) =>
                //foreach(var childBoard in orderedBoards)
                foreach (var move in movePicker.Get())
                {
                    moveStack.Clear();
                    moveStack.Push(move);
                    tempBoard.MakeMove(move);

                    if (tempBoard.IsInCheck(colour))
                    {
                        moveStack.Pop();
                        tempBoard.UnMakeMove(move);

                        continue;
                    }

                    PotentialBoard currentPotential = null;

                    if (currentDepth == 1)
                    {
                        currentPotential = new PotentialBoard(move, board.Evaluate(colour), PotentialBoard.NodeType.PV, null);
                    }
                    else
                    {
                        currentPotential = AlphaBetaInternal2(tempBoard, move, colour.Opposite(), currentDepth - 1, -10000, 10000, !isMax);

                        //currentPotential.AddMove(move);

                        //var potentialBoard = new PotentialBoard(move, currentPotential.PotentialScore, PotentialBoard.NodeType.PV, null);

                        //potentialBoard.AddMove(move);
                    }
                    
                    //childBoard.ProjectedEvaluation = potentialBoard.PotentialScore;

                    //if (bestChildBoard == null || currentPotential.PotentialScore > bestChildBoard.Score)
                    if (iterativeBestPotentialBoard == null || currentPotential.PotentialScore > iterativeBestPotentialBoard.PotentialScore)
                    {
                        //bestCurrentMove = move;
                        //bestChildBoard = potentialBoard;
                        iterativeBestMove = move;
                        iterativeBestPotentialBoard = currentPotential;
                    }
                    
                    moveStack.Pop();
                    tempBoard.UnMakeMove(move);
                };

                //if (bestCurrentMove == null && bestCurrentMove?.Notation != iterativeBestMove?.Notation)
                //{
                //    bestCurrentMove = iterativeBestMove;
                //}

                OnIterationComplete(new IterationCompleteEventArgs
                {
                    Depth = currentDepth,
                    Eval = iterativeBestPotentialBoard.PotentialScore,
                    PotentialBoard = iterativeBestPotentialBoard
                });

                ++currentDepth;
            }

            return board.ChildBoards.Single(x => x.Notation == iterativeBestPotentialBoard.Moves.Last().Notation);
        }

        private PotentialBoard AlphaBetaInternal2(Board board, Move startMove, Colour colour, int depth, double alpha, double beta, bool isMax)
        {
            Interlocked.Increment(ref _positionCounter);

            if (depth == 0)
                return new PotentialBoard(startMove, board.Evaluate(colour), PotentialBoard.NodeType.PV, null);

            var existingTransposition = TranspositionTable.Find(board.Key);

            Move previousBestMove = null;

            if (existingTransposition != null)
            {
                if (existingTransposition.Key == board.Key)
                {
                    if (existingTransposition.Depth > 1 && existingTransposition.Depth > depth)
                    {
                        return existingTransposition.PotentialBoard;
                    }

                    if (existingTransposition.BestMove != null)
                    {
                        previousBestMove = existingTransposition.BestMove;
                    }
                }
            }

            var movePicker = new MovePicker(colour, board, previousBestMove);

            List<Move> moves = new List<Move>(256);

            //var movesOrig = board.FindMoves2(colour)
            //    .OrderByDescending(x => x.CapturePieceType == PieceType.Pawn)
            //    .ThenByDescending(x => x.CapturePieceType == PieceType.Knight)
            //    .ThenByDescending(x => x.CapturePieceType == PieceType.Bishop)
            //    .ThenByDescending(x => x.CapturePieceType == PieceType.Rook)
            //    .ThenByDescending(x => x.CapturePieceType == PieceType.Queen)
            //    .ThenByDescending(x => x.CapturePieceType == PieceType.King)
            //    .ToList();
            /*
            // Move previous best move to top
            if (previousBestMove != null)
            {
                var possibleBestMove = moves.SingleOrDefault(x => x.Notation == previousBestMove.Notation);

                if (possibleBestMove != null)
                {
                    //moves.Remove(possibleBestMove);
                    moves.Insert(0, possibleBestMove);
                }
                else
                {
                    //var bp = true;
                    //moves = board.FindMoves2(colour)
                    //    .OrderByDescending(x => x.CapturePieceType == PieceType.Pawn)
                    //    .ThenByDescending(x => x.CapturePieceType == PieceType.Knight)
                    //    .ThenByDescending(x => x.CapturePieceType == PieceType.Bishop)
                    //    .ThenByDescending(x => x.CapturePieceType == PieceType.Rook)
                    //    .ThenByDescending(x => x.CapturePieceType == PieceType.Queen)
                    //    .ThenByDescending(x => x.CapturePieceType == PieceType.King)
                    //    .ToList();
                }
            }
            */

            PotentialBoard bestChildBoard;
            Move bestCurrentMove = null;

            var tempBoard = new Board(board, colour.Opposite());

            //if (board.Notation.EndsWith("c2x"))
            //{ var bp = true; }

            if (isMax)
            {
                bestChildBoard = new PotentialBoard(new List<Move>(), double.MaxValue, PotentialBoard.NodeType.PV, null);
                
                //foreach (var move in moves)
                foreach (var move in movePicker.Get())
                {
                    tempBoard.MakeMove(move);

                    if (tempBoard.IsInCheck(colour))
                    {
                        tempBoard.UnMakeMove(move);
                        continue;
                    }

                    var currentPotential = AlphaBetaInternal2(tempBoard, move, colour.Opposite(), depth - 1, alpha, beta, !isMax);

                    currentPotential.AddMove(startMove);

                    tempBoard.UnMakeMove(move);

                    //tempBoard.ProjectedEvaluation = currentPotential.Score;

                    if (currentPotential.PotentialScore > bestChildBoard.PotentialScore)
                    {
                        bestCurrentMove = move;
                        bestChildBoard = currentPotential;
                    }

                    alpha = Math.Max(alpha, bestChildBoard.PotentialScore);

                    if (beta <= alpha)
                    {
                        var cbwtm1 = bestChildBoard.WithType(PotentialBoard.NodeType.Alpha);
                        TranspositionTable.Add(new Transposition(board.Key, depth, board.Turn, bestChildBoard.Score, cbwtm1, bestCurrentMove));
                        return cbwtm1;
                    }
                }
            }
            else
            {
                bestChildBoard = new PotentialBoard(new List<Move>(), double.MinValue, PotentialBoard.NodeType.PV, null);

                //foreach (var move in moves)
                foreach (var move in movePicker.Get())
                {
                    tempBoard.MakeMove(move);

                    if (tempBoard.IsInCheck(colour))
                    {
                        tempBoard.UnMakeMove(move);
                        continue;
                    }

                    var currentPotential = AlphaBetaInternal2(tempBoard, move, colour.Opposite(), depth - 1, alpha, beta, !isMax);

                    currentPotential.AddMove(startMove);

                    tempBoard.UnMakeMove(move);

                    //tempBoard.ProjectedEvaluation = currentPotential.Score;

                    if (currentPotential.PotentialScore < bestChildBoard.PotentialScore)
                    {
                        bestCurrentMove = move;
                        bestChildBoard = currentPotential;
                    }

                    beta = Math.Min(beta, bestChildBoard.PotentialScore);

                    if (beta <= alpha)
                    {
                        var cbwtm2 = bestChildBoard.WithType(PotentialBoard.NodeType.Alpha);
                        TranspositionTable.Add(new Transposition(board.Key, depth, board.Turn, bestChildBoard.Score, cbwtm2, bestCurrentMove));
                        return cbwtm2;
                        //return bestChildBoard.WithType(PotentialBoard.NodeType.Cut);
                    }
                }
            }

            var cbwt = bestChildBoard.WithType(PotentialBoard.NodeType.Exact);
            TranspositionTable.Add(new Transposition(board.Key, depth, board.Turn, bestChildBoard.Score, cbwt, bestCurrentMove));

            return cbwt;
            //return bestChildBoard.WithType(PotentialBoard.NodeType.All);
        }

        private PotentialBoard AlphaBetaInternal(Board board, Colour colour, int depth, double alpha, double beta, bool isMax, StringBuilder sb)
        {
            Interlocked.Increment(ref _positionCounter);

            if (depth == 0)
                return new PotentialBoard(new List<Move>(), board.Evaluate(colour), PotentialBoard.NodeType.PV, null);

            board.GenerateChildBoards(colour, 1);

            var legalMoves = board.GetLegalMoves();

            PotentialBoard bestChildBoard;

            if (isMax)
            {
                bestChildBoard = new PotentialBoard(new List<Move>(), double.MaxValue, PotentialBoard.NodeType.PV, null);

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

                        //bestChildBoard.Board.Orphan();

                        return bestChildBoard.WithType(PotentialBoard.NodeType.Alpha);
                    }
                }
            }
            else
            {
                var orderedBoards = legalMoves
                    .OrderBy(x => x.ProjectedEvaluation)
                    .ThenBy(x => x.Evaluation);

                bestChildBoard = new PotentialBoard(new List<Move>(), double.MinValue, PotentialBoard.NodeType.PV, null);

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

                        //bestChildBoard.Board.Orphan();

                        return bestChildBoard.WithType(PotentialBoard.NodeType.Alpha);
                    }
                }
            }

            //if (bestChildBoard.Board == null)
            //    sb.AppendLine($"EXACT: {board.GetFriendlyCode()} No Childboards found");
            //else
            //    sb.AppendLine($"EXACT: {board.GetFriendlyCode()} Best: {bestChildBoard}");

            //bestChildBoard.Board.Orphan();

            return bestChildBoard.WithType(PotentialBoard.NodeType.Exact);
        }
    }
}
