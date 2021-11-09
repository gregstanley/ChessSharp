using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChessSharp.Common;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Extensions;
using ChessSharp.Engine.Events;
using ChessSharp.MoveGeneration;

namespace ChessSharp.Engine
{
    public class SearchPV
    {
        private readonly Stopwatch stopWatch = new();

        private readonly MoveGenerator moveGenerator;

        private readonly PositionEvaluator positionEvaluator;

        private readonly TranspositionTable transpositionTable;

        public SearchPV(MoveGenerator moveGenerator, PositionEvaluator positionEvaluator, TranspositionTable transpositionTable)
        {
            this.moveGenerator = moveGenerator;
            this.positionEvaluator = positionEvaluator;
            this.transpositionTable = transpositionTable;
        }

        public delegate void InfoEventDelegate(object sender, InfoEventArgs args);

        public event InfoEventDelegate Info;

        public int PositionCount { get; private set; }

        public SearchResults Go(Board board, Colour colour, int maxDepth)
        {
            // A Rust based search
            // https://github.com/redsalmon91/FoxSEE/blob/master/src/search.rs
            stopWatch.Restart();

            PositionCount = 0;

            transpositionTable.NextIteration();

            var transpositionKey = board.Key;

            var nodeMoves = new List<uint>(256);

            moveGenerator.Generate(board, colour, nodeMoves);

            var moveEvaluations = new List<MoveEvaluation>(128);

            // http://mediocrechess.blogspot.com/2006/12/programming-extracting-principal.html
            var principalVariations = new uint[maxDepth][];

            // Initialise the triangular PV table https://www.chessprogramming.org/Triangular_PV-Table
            for (var i = 0; i < maxDepth; ++i)
                principalVariations[i] = new uint[i + 1];

            if (!nodeMoves.Any())
                return new SearchResults(PositionCount, new List<long>(), moveEvaluations, principalVariations);

            var existingTransposition = transpositionTable.Find(transpositionKey);

            if (existingTransposition.Key != 0)
            {
                if (existingTransposition.BestMove != 0)
                {
                    if (nodeMoves.First() != existingTransposition.BestMove)
                    {
                        nodeMoves.Remove(existingTransposition.BestMove);
                        nodeMoves.Insert(0, existingTransposition.BestMove);
                    }
                }
            }

            var currentMaxDepth = 1;

            var lap = stopWatch.ElapsedMilliseconds;

            var iterationLaps = new List<long>();

            Info info;

            var alpha = int.MinValue;
            var beta = int.MaxValue;
            ushort rootPly = 0;
            // TODO: This is added either side of the first 'real' alpha-beta values to create a 'window' - it's value is arbitrary at the moment
            var delta = 10;
            var previousBest = alpha;

            while (currentMaxDepth <= maxDepth)
            {
                // We get a score after each iteration so use this as the 'centre' point for next iterations
                // Stockfish https://github.com/official-stockfish/Stockfish/blob/2046d5da30b2cd505b69bddb40062b0d37b43bc7/src/search.cpp#:~:text=delta%20%3D%20Value(17,prev%20%2B%20delta%2C%20VALUE_INFINITE)%3B
                if (previousBest >= alpha)
                {
                    alpha = previousBest - delta;
                    beta = previousBest + delta;
                }

                // This is the principal variation for the current depth
                var principalVariationForDepth = principalVariations[currentMaxDepth - 1];

                var principalVariation = new uint[currentMaxDepth];
                var path = new uint[currentMaxDepth];

                moveEvaluations.Clear();

                var bestScore = int.MinValue;
                var bestMove = 0u;

                foreach (var move in nodeMoves)
                {
                    var nextDepth = (byte)(currentMaxDepth - 1);

                    path[rootPly] = move;

                    board.MakeMove(move);
                    principalVariation[0] = move;
                    var evaluatedScore = -PrincipalVariationSearch(board, colour.Opposite(), nextDepth, (ushort)(rootPly + 1), alpha, beta, principalVariation, path);

                    board.UnMakeMove(move);

                    if (evaluatedScore > bestScore)
                    {
                        bestMove = move;
                        bestScore = evaluatedScore;
                        alpha = bestScore;

                        // UpdatePrincipalVariation(principalVariation, principalVariationForDepth, 0, move);
                        principalVariation.CopyTo(principalVariationForDepth, 0);

                        info = new InfoNewPv(PositionCount, stopWatch.ElapsedMilliseconds, currentMaxDepth, bestMove, bestScore, transpositionTable);

                        Info?.Invoke(this, new InfoEventArgs(info));
                    }

                    moveEvaluations.Add(new MoveEvaluation(new MoveViewer(move), evaluatedScore));
                }

                // Re-order the root moves between each iteration
                nodeMoves = moveEvaluations
                    .OrderByDescending(x => x.Score)
                    .Select(x => x.Move.Value)
                    .ToList();

                transpositionTable.Set(transpositionKey, (byte)currentMaxDepth, colour, bestScore, bestMove);

                // TODO: Debug this value
                previousBest = bestScore;

                var iterationLength = stopWatch.ElapsedMilliseconds - lap;

                lap = stopWatch.ElapsedMilliseconds;

                iterationLaps.Add(iterationLength);

                info = new InfoDepthComplete(PositionCount, stopWatch.ElapsedMilliseconds, currentMaxDepth, principalVariationForDepth, transpositionTable);

                Info?.Invoke(this, new InfoEventArgs(info));

                ++currentMaxDepth;
            }

            stopWatch.Reset();

            return new SearchResults(PositionCount, iterationLaps, moveEvaluations, principalVariations);
        }

        private int PrincipalVariationSearch(Board board, Colour colour, byte depth, ushort ply, int alpha, int beta, uint[] principalVariation, uint[] path)
        {
            ++PositionCount;

            if (depth == 0)
                return positionEvaluator.Evaluate(board) * (colour == Colour.White ? 1 : -1);

            var transpositionKey = board.Key;

            var existingTransposition = transpositionTable.Find(transpositionKey);

            uint previousBestMove = 0;

            if (existingTransposition.Key != 0)
            {
                if (existingTransposition.Depth > 1 && existingTransposition.Depth > depth)
                    return existingTransposition.Evaluation;

                if (existingTransposition.BestMove != 0)
                    previousBestMove = existingTransposition.BestMove;
            }

            var bestScore = int.MinValue;
            var bestMove = 0u;

            // var principalVariation = new uint[64];
            
            var moveCount = 1;

            foreach (var move in GetNextMove(moveGenerator, ply, board, colour, previousBestMove))
            {
                board.MakeMove(move);

                path[ply] = move;

                var evaluatedScore = 0;

                var oppositeColour = colour.Opposite();

                var nextDepth = (byte)(depth - 1);
                var nextPly = (ushort)(ply + 1);

                // https://www.chessprogramming.org/Principal_Variation_Search
                if (moveCount == 1)
                {
                    // Always do a full search on the first/PV move
                    evaluatedScore = -PrincipalVariationSearch(board, oppositeColour, nextDepth, nextPly, -beta, -alpha, principalVariation, path);
                }
                else
                {
                    // Late Move Reduction http://mediocrechess.blogspot.com/2007/03/other-late-move-reduction-lmr.html
                    if (ply > 3 && moveCount > 3 && move.GetCapturePieceType() == PieceType.None && move.GetNumCheckers() == 0 && nextDepth > 0)
                        --nextDepth;

                    // Search with aspiration window
                    evaluatedScore = -PrincipalVariationSearch(board, oppositeColour, nextDepth, nextPly, -alpha - 1, -alpha, principalVariation, path);

                    if (alpha < evaluatedScore && evaluatedScore < beta)
                    {
                        // Re-search
                        evaluatedScore = -PrincipalVariationSearch(board, oppositeColour, nextDepth, nextPly, -beta, -evaluatedScore, principalVariation, path);
                    }
                }

                board.UnMakeMove(move);

                ++moveCount;

                // Looks like I did NegaMax from https://www.chessprogramming.org/Alpha-Beta
                if (evaluatedScore >= beta)
                    return beta;

                if (evaluatedScore > bestScore)
                {
                    // alpha = Math.Max(bestScore, evaluatedScore); // alpha acts like max in MiniMax
                   
                    bestMove = move;
                    bestScore = evaluatedScore;

                    if (evaluatedScore > alpha)
                        alpha = evaluatedScore;

                    // UpdatePrincipalVariation(principalVariation, parentPrincipalVariation, ply, move);
                    // UpdatePrincipalVariation(path, principalVariation, ply);
                    path.CopyTo(principalVariation, 0);
                }


                if (ply == 1)
                {
                    var info = new Info(PositionCount, stopWatch.ElapsedMilliseconds, depth, transpositionTable);

                    Info?.Invoke(this, new InfoEventArgs(info));
                }
            }

            transpositionTable.Set(transpositionKey, depth, colour, bestScore, bestMove);

            if (alpha == int.MinValue)
                alpha = PieceValues.CheckmateValue + ply;

            if (alpha == int.MaxValue)
                alpha = -(PieceValues.CheckmateValue + ply);

            return alpha;
        }

        private static IEnumerable<uint> GetNextMove(MoveGenerator moveGenerator, ushort ply, Board board, Colour colour, uint previousBestMove)
        {
            if (previousBestMove > 0)
                yield return previousBestMove;

            foreach (var move in moveGenerator.GenerateStream(ply, board, colour))
            {
                if (move == previousBestMove)
                    continue;

                yield return move;
            }
        }

        //private static void UpdatePrincipalVariation(uint[] source, uint[] target, ushort ply)
        //{
        //    var i = ply;

        //    while (source[i] != 0)
        //    {
        //        target[i] = source[i];

        //        ++i;
        //    }
        //}

        private static void UpdatePrincipalVariation(uint[] source, uint[] target, ushort ply, uint move)
        {
            var i = ply;

            source[i] = move;

            while (source[i] != 0)
            {
                target[i] = source[i];

                ++i;
            }
        }
    }
}
