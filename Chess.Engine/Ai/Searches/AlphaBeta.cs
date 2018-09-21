using Chess.Engine.Extensions;
using Chess.Engine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            TranspositionTable.Reset();

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

            //var moveStack = new Stack<Move>(64);
            //var pvStack = new Stack<Move>(64);
            Stopwatch sw = new Stopwatch();

            var iterativeBestMoves = new List<Move>(256);

            if (previousBestMove != null)
                iterativeBestMoves.Add(previousBestMove);

            while (currentDepth <= maxDepth)
            {
                sw.Restart();

                Interlocked.Exchange(ref _positionCounter, 0);

                var movePicker = new MovePicker(colour, board, iterativeBestMoves.Any() ? iterativeBestMoves.Last() : null);

                iterativeBestMove = null;
                iterativeBestPotentialBoard = null;

                //Move bestCurrentMove = null;
                //Parallel.ForEach(moves, (move) =>
                //Parallel.ForEach(orderedBoards, (childBoard) =>
                //foreach(var childBoard in orderedBoards)
                foreach (var move in movePicker.Get())
                {
                    var cb = board.ChildBoards.SingleOrDefault(x => x.Notation == move.Notation);

                    if (cb == null)
                    { var bp = true; }
                    //moveStack.Clear();
                    //moveStack.Push(move);
                    tempBoard.MakeMove(move);

                    if (tempBoard.IsInCheck(colour))
                    {
                        //moveStack.Pop();
                        tempBoard.UnMakeMove(move);

                        continue;
                    }

                    PotentialBoard currentPotential = null;

                    if (currentDepth == 1)
                    {
                        Interlocked.Increment(ref _positionCounter);
                        currentPotential = new PotentialBoard(move, board.Evaluate(colour), PotentialBoard.NodeType.PV, null);
                    }
                    else
                    {
                        currentPotential = AlphaBetaInternal(tempBoard, move, colour.Opposite(), currentDepth - 1, -10000, 10000, !isMax);

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
                    
                    //moveStack.Pop();
                    tempBoard.UnMakeMove(move);
                };

                //if (bestCurrentMove == null && bestCurrentMove?.Notation != iterativeBestMove?.Notation)
                //{
                //    bestCurrentMove = iterativeBestMove;
                //}

                iterativeBestMoves.Add(iterativeBestMove);

                OnIterationComplete(new IterationCompleteEventArgs
                {
                    Depth = currentDepth,
                    Eval = iterativeBestPotentialBoard.PotentialScore,
                    NodeCount = _positionCounter,
                    TimeMs = sw.ElapsedMilliseconds,
                    PotentialBoard = iterativeBestPotentialBoard
                });

                ++currentDepth;
            }

            sw.Stop();

            return board.ChildBoards.Single(x => x.Notation == iterativeBestPotentialBoard.Moves.Last().Notation);
        }

        private PotentialBoard AlphaBetaInternal(Board board, Move startMove, Colour colour, int depth, double alpha, double beta, bool isMax)
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
                    if (existingTransposition.Depth > 1 && existingTransposition.Depth >= depth)
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

            PotentialBoard bestChildBoard;

            Move bestCurrentMove = null;

            var tempBoard = new Board(board, colour.Opposite());

            if (isMax)
            {
                bestChildBoard = new PotentialBoard(new List<Move>(), double.MaxValue, PotentialBoard.NodeType.PV, null);
                
                //foreach (var move in moves)
                foreach (var move in movePicker.Get())
                {
                    //if (move.Notation.StartsWith("d4-c2x"))
                    //{ var bp = true; }

                    tempBoard.MakeMove(move);

                    if (tempBoard.IsInCheck(colour))
                    {
                        tempBoard.UnMakeMove(move);
                        continue;
                    }

                    var currentPotential = AlphaBetaInternal(tempBoard, move, colour.Opposite(), depth - 1, alpha, beta, !isMax);

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
                    //if (move.Notation.StartsWith("d4-c2x"))
                    //{ var bp = true; }

                    tempBoard.MakeMove(move);

                    if (tempBoard.IsInCheck(colour))
                    {
                        tempBoard.UnMakeMove(move);
                        continue;
                    }

                    var currentPotential = AlphaBetaInternal(tempBoard, move, colour.Opposite(), depth - 1, alpha, beta, !isMax);

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
                    }
                }
            }

            var cbwt = bestChildBoard.WithType(PotentialBoard.NodeType.Exact);

            TranspositionTable.Add(new Transposition(board.Key, depth, board.Turn, bestChildBoard.Score, cbwt, bestCurrentMove));

            return cbwt;
        }
    }
}
