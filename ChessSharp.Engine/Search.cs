using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.MoveGeneration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChessSharp.Engine
{
    public class Search
    {
        public int PositionCount { get; private set; }

        private MoveGenerator _moveGenerator;

        private PositionEvaluator _positionEvaluator;

        private TranspositionTable TranspositionTable { get; set; } = new TranspositionTable();

        public Search(MoveGenerator moveGenerator, PositionEvaluator positionEvaluator)
        {
            _moveGenerator = moveGenerator;
            _positionEvaluator = positionEvaluator;
        }

        public SearchResults Go(MoveGenerationWorkspace workspace, int maxDepth)
        {
            PositionCount = 0;

            TranspositionTable.ResetCounters();

            var transpositionKey = workspace.BitBoard.Key;

            var existingTransposition = TranspositionTable.Find(transpositionKey);

            uint previousBestMove = 0;

            if (existingTransposition != null)
            {
                if (existingTransposition.BestMove != 0)
                    previousBestMove = existingTransposition.BestMove;
            }

            var depthMoves = new List<uint>[64];

            for (var i = 0; i <= maxDepth; i++)
                depthMoves[i] = new List<uint>(256);

            var nodeMoves = depthMoves[maxDepth];

            _moveGenerator.Generate(workspace, nodeMoves);

            var moveEvaluations = new List<MoveEvaluation>(128);
            var previousMoveEvaluations = new List<MoveEvaluation>(128);

            var primaryVariations = new uint[maxDepth][];

            for (var i = 0; i < maxDepth; ++i)
                primaryVariations[i] = new uint[i + 1];

            if (!nodeMoves.Any())
                return new SearchResults(PositionCount, new List<long>(), moveEvaluations, primaryVariations, TranspositionTable);

            var rootPly = workspace.Ply;

            var currentMaxDepth = 1;

            var sw = new Stopwatch();
            var iterationLaps = new List<long>();

            while (currentMaxDepth <= maxDepth)
            {
                sw.Restart();

                existingTransposition = TranspositionTable.Find(workspace.BitBoard.Key);

                if (existingTransposition != null)
                {
                    if (existingTransposition.BestMove != 0)
                        previousBestMove = existingTransposition.BestMove;
                }

                var primaryVariation = primaryVariations[currentMaxDepth - 1];

                previousMoveEvaluations = moveEvaluations;

                moveEvaluations.Clear(); 

                if (previousBestMove != 0)
                {
                    var move = previousBestMove;

                    var moveView = new MoveViewer(move);

                    var beforeKey = workspace.MakeMove(move);

                    var evaluatedScore = -InnerPerft(workspace, currentMaxDepth - 1, 1, double.MinValue, double.MaxValue,
                        depthMoves, primaryVariation);

                    moveEvaluations.Add(new MoveEvaluation(moveView, evaluatedScore));

                    var afterKey = workspace.UnMakeMove(move);
                }

                foreach (var move in nodeMoves)
                {
                    if (move == previousBestMove)
                        continue;

                    var moveView = new MoveViewer(move);

                    var beforeKey = workspace.MakeMove(move);

                    var evaluatedScore = -InnerPerft(workspace, currentMaxDepth - 1, 1, double.MinValue, double.MaxValue,
                        depthMoves, primaryVariation);

                    moveEvaluations.Add(new MoveEvaluation(moveView, evaluatedScore));

                    var afterKey = workspace.UnMakeMove(move);
                }

                var bestMove = moveEvaluations.OrderByDescending(x => x.Score).First();

                //TranspositionTable.Add(new Transposition(transpositionKey, currentMaxDepth, workspace.Colour, bestMove.Score, bestMove.Move.Value, workspace.BitBoard));
                TranspositionTable.Set(transpositionKey, currentMaxDepth, workspace.Colour, bestMove.Score, bestMove.Move.Value);

                ++currentMaxDepth;

                iterationLaps.Add(sw.ElapsedMilliseconds);
            }

            sw.Stop();

            return new SearchResults(PositionCount, iterationLaps, moveEvaluations, primaryVariations, TranspositionTable);
        }

        private IEnumerable<uint> GetNextMove(MoveGenerationWorkspace workspace, List<uint>nodeMoves, uint previousBestMove)
        {
            if (previousBestMove != 0)
                yield return previousBestMove;

            _moveGenerator.Generate(workspace, nodeMoves);

            foreach (var move in nodeMoves)
                yield return move;
        }

        private double InnerPerft(MoveGenerationWorkspace workspace, int depth, int ply, double alpha, double beta, List<uint>[] depthMoves, uint[] primaryVariations)
        {
            ++PositionCount;

            if (depth == 0)
                return _positionEvaluator.Evaluate(workspace.BitBoard) * (workspace.Colour == Colour.White ? 1 : -1);

            var transpositionKey = workspace.BitBoard.Key;

            var existingTransposition = TranspositionTable.Find(transpositionKey);

            uint previousBestMove = 0;

            
            var nodeMoves = depthMoves[depth];

            // Must wipe any existing moves each time we enter a depth
            nodeMoves.Clear();

            if (existingTransposition != null)
            {
                //if (existingTransposition.Depth > 1 && existingTransposition.Depth > depth)
                //    return existingTransposition.Evaluation;

                if (existingTransposition.BestMove != 0)
                {
                    previousBestMove = existingTransposition.BestMove;

                    //nodeMoves.Add(previousBestMove);
                }
            }

            var needsToGenerate = true;

            if (previousBestMove != 0)
            {
                nodeMoves.Add(previousBestMove);
            }
            else
            {
                _moveGenerator.Generate(workspace, nodeMoves);
                needsToGenerate = false;
            }

            //_moveGenerator.Generate(workspace, nodeMoves);

            //if (previousBestMove != 0)
            //{
            //    if (nodeMoves.Contains(previousBestMove))
            //    {
            //        nodeMoves.Remove(previousBestMove);
            //        nodeMoves.Insert(0, previousBestMove);
            //    }
            //    //else
            //    //{
            //    //    var diff = Diff(workspace.BitBoard, existingTransposition);
            //    //    var moveView = new MoveViewer(previousBestMove);
            //    //    var bp = true;
            //    //}
            //}

            var bestScore = double.MinValue;
            var bestMove = 0u;

            var isFirst = true;

            var moveIndex = 0;

            //foreach (var move in nodeMoves)
            //foreach(var move in GetNextMove(workspace, nodeMoves, previousBestMove))
            while (moveIndex < nodeMoves.Count)
            {
                var move = nodeMoves[moveIndex++];

                if (!isFirst && move == previousBestMove)
                    continue;

                isFirst = false;

                var beforeKey = workspace.MakeMove(move);
                
                var evaluatedScore = -InnerPerft(workspace, depth - 1, ply + 1, -beta, -alpha, depthMoves, primaryVariations);

                var afterKey = workspace.UnMakeMove(move);

                alpha = Math.Max(bestScore, evaluatedScore);

                if (workspace.BitBoard.BlackPawns.Count() > 8)
                {
                    var moveView = new MoveViewer(move);

                    if (nodeMoves.Count == 1)
                        _moveGenerator.Generate(workspace, nodeMoves);
                }

                if (evaluatedScore > bestScore)
                {
                    bestScore = evaluatedScore;
                    bestMove = move;
                }

                if (alpha > beta)
                {
                    // Cut node
                    break;
                }

                if (needsToGenerate)
                {
                    _moveGenerator.Generate(workspace, nodeMoves);
                    needsToGenerate = false;
                }
            }

            // No cut so this is best move so far
            primaryVariations[ply] = bestMove;

            TranspositionTable.Set(transpositionKey, depth, workspace.Colour, bestScore, bestMove);

            //var transposition = new Transposition(transpositionKey, depth, workspace.Colour, bestScore, bestMove, workspace.BitBoard);

            //TranspositionTable.Add(transposition);

            return alpha;
        }

        //private SquareFlag Diff(BitBoard bitBoard, Transposition transposition)
        //{
        //    var output = bitBoard.WhitePawns & ~transposition.WhitePawns;
        //    output |= bitBoard.WhiteRooks & ~transposition.WhiteRooks;
        //    output |= bitBoard.WhiteKnights & ~transposition.WhiteKnights;
        //    output |= bitBoard.WhiteBishops & ~transposition.WhiteBishops;
        //    output |= bitBoard.WhiteQueens & ~transposition.WhiteQueens;
        //    output |= bitBoard.WhiteKing & ~transposition.WhiteKing;
        //    output |= bitBoard.BlackPawns & ~transposition.BlackPawns;
        //    output |= bitBoard.BlackRooks & ~transposition.BlackRooks;
        //    output |= bitBoard.BlackKnights & ~transposition.BlackKnights;
        //    output |= bitBoard.BlackBishops & ~transposition.BlackBishops;
        //    output |= bitBoard.BlackQueens & ~transposition.BlackQueens;
        //    output |= bitBoard.BlackKing & ~transposition.BlackKing;

        //    return output;
        //}
    }
}
