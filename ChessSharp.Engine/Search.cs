using ChessSharp.Engine.Events;
using ChessSharp.Enums;
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

        public Search(MoveGenerator moveGenerator, PositionEvaluator positionEvaluator)
        {
            _moveGenerator = moveGenerator;
            _positionEvaluator = positionEvaluator;
        }

        public SearchResults Go(MoveGenerationWorkspace workspace, int maxDepth)
        {
            PositionCount = 0;

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
                return new SearchResults(PositionCount, new List<long>(), moveEvaluations, primaryVariations);

            var rootPly = workspace.Ply;

            var currentMaxDepth = 1;

            var sw = new Stopwatch();
            var iterationLaps = new List<long>();

            var bestMove = 0u;

            while (currentMaxDepth <= maxDepth)
            {
                var primaryVariation = primaryVariations[currentMaxDepth - 1];

                previousMoveEvaluations = moveEvaluations;

                moveEvaluations.Clear(); 

                sw.Restart();

                if (bestMove != 0)
                {
                    var move = bestMove;

                    var moveView = new MoveViewer(move);

                    workspace.MakeMove(move);

                    var evaluatedScore = -InnerPerft(workspace, currentMaxDepth - 1, 1, double.MinValue, double.MaxValue,
                        depthMoves, primaryVariation);

                    moveEvaluations.Add(new MoveEvaluation(moveView, evaluatedScore));

                    workspace.UnMakeMove(move);
                }

                foreach (var move in nodeMoves)
                {
                    if (move == bestMove)
                        continue;

                    var moveView = new MoveViewer(move);

                    workspace.MakeMove(move);

                    var evaluatedScore = -InnerPerft(workspace, currentMaxDepth - 1, 1, double.MinValue, double.MaxValue,
                        depthMoves, primaryVariation);

                    moveEvaluations.Add(new MoveEvaluation(moveView, evaluatedScore));

                    workspace.UnMakeMove(move);
                }

                var orderedMoves = moveEvaluations.OrderByDescending(x => x.Score);

                primaryVariation[0] = orderedMoves.First().Move.Value;

                bestMove = primaryVariation[0];

                ++currentMaxDepth;

                iterationLaps.Add(sw.ElapsedMilliseconds);
            }

            sw.Stop();

            return new SearchResults(PositionCount, iterationLaps, moveEvaluations, primaryVariations);
        }

        private double InnerPerft(MoveGenerationWorkspace workspace, int depth, int ply, double alpha, double beta, List<uint>[] depthMoves, uint[] primaryVariations)
        {
            ++PositionCount;

            if (depth == 0)
                return _positionEvaluator.Evaluate(workspace.BitBoard) * (workspace.Colour == Colour.White ? 1 : -1);

            var nodeMoves = depthMoves[depth];

            // Must wipe any existing moves each time we enter a depth
            nodeMoves.Clear();

            _moveGenerator.Generate(workspace, nodeMoves);

            var bestScore = double.MinValue;
            var bestMove = 0u;

            foreach (var move in nodeMoves)
            {
                workspace.MakeMove(move);

                var evaluatedScore = -InnerPerft(workspace, depth - 1, ply + 1, -beta, -alpha, depthMoves, primaryVariations);

                workspace.UnMakeMove(move);

                alpha = Math.Max(bestScore, evaluatedScore);

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
            }

            // No cut so this is best move so far
            primaryVariations[ply] = bestMove;

            return alpha;
        }
    }
}
