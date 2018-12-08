using ChessSharp.Enums;
using ChessSharp.MoveGeneration;
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

            //http://mediocrechess.blogspot.com/2006/12/programming-extracting-principal.html
            var principalVariations = new uint[maxDepth][];

            for (var i = 0; i < maxDepth; ++i)
                principalVariations[i] = new uint[i + 1];

            if (!nodeMoves.Any())
                return new SearchResults(PositionCount, new List<long>(), moveEvaluations, principalVariations, TranspositionTable);

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

                var masterPrincipalVariation = principalVariations[currentMaxDepth - 1];

                var principalVariation = new uint[64];

                moveEvaluations.Clear();

                var bestScore = double.MinValue;
                var bestMove = 0u;

                if (previousBestMove != 0)
                {
                    var move = previousBestMove;

                    var moveView = new MoveViewer(move);

                    var beforeKey = workspace.MakeMove(move);

                    var evaluatedScore = -PrincipalVariationSearch(workspace, currentMaxDepth - 1, 1, double.MinValue, double.MaxValue,
                        depthMoves, principalVariation);

                    moveEvaluations.Add(new MoveEvaluation(moveView, evaluatedScore));

                    var afterKey = workspace.UnMakeMove(move);

                    if (evaluatedScore > bestScore)
                    {
                        bestMove = move;
                        bestScore = evaluatedScore;

                        UpdatePrincipalVariation(principalVariation, masterPrincipalVariation, 0, move);
                    }
                }

                foreach (var move in nodeMoves)
                {
                    if (move == previousBestMove)
                        continue;

                    var moveView = new MoveViewer(move);

                    var beforeKey = workspace.MakeMove(move);

                    var evaluatedScore = -PrincipalVariationSearch(workspace, currentMaxDepth - 1, 1, double.MinValue, double.MaxValue,
                        depthMoves, principalVariation);

                    moveEvaluations.Add(new MoveEvaluation(moveView, evaluatedScore));

                    var afterKey = workspace.UnMakeMove(move);

                    if (evaluatedScore > bestScore)
                    {
                        bestMove = move;
                        bestScore = evaluatedScore;

                        UpdatePrincipalVariation(principalVariation, masterPrincipalVariation, 0, move);
                    }
                }

                TranspositionTable.Set(transpositionKey, currentMaxDepth, workspace.Colour, bestScore, bestMove);

                ++currentMaxDepth;

                iterationLaps.Add(sw.ElapsedMilliseconds);
            }

            sw.Stop();

            return new SearchResults(PositionCount, iterationLaps, moveEvaluations, principalVariations, TranspositionTable);
        }

        private double PrincipalVariationSearch(MoveGenerationWorkspace workspace, int depth, int ply, double alpha, double beta, List<uint>[] depthMoves, uint[] parentPrincipalVariation)
        {
            ++PositionCount;

            if (depth == 0)
                return _positionEvaluator.Evaluate(workspace.BitBoard) * (workspace.Colour == Colour.White ? 1 : -1);

            var nodeMoves = depthMoves[depth];

            var transpositionKey = workspace.BitBoard.Key;

            var existingTransposition = TranspositionTable.Find(transpositionKey);

            uint previousBestMove = 0;

            // Must wipe any existing moves each time we enter a depth
            nodeMoves.Clear();

            if (existingTransposition != null)
            {
                //if (existingTransposition.Depth > 1 && existingTransposition.Depth > depth)
                //    return existingTransposition.Evaluation;

                if (existingTransposition.BestMove != 0)
                    previousBestMove = existingTransposition.BestMove;
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

            var bestScore = double.MinValue;
            var bestMove = 0u;

            var isFirst = true;

            var moveIndex = 0;

            var principalVariation = new uint[64];

            var bSearchPv = true;

            //foreach(var move in GetNextMove(workspace, nodeMoves, previousBestMove))
            while (moveIndex < nodeMoves.Count)
            {
                var move = nodeMoves[moveIndex++];

                if (!isFirst && move == previousBestMove)
                    continue;

                isFirst = false;

                var beforeKey = workspace.MakeMove(move);

                var evaluatedScore = 0d;

                // https://www.chessprogramming.org/Principal_Variation_Search
                if (bSearchPv)
                {
                    evaluatedScore = -PrincipalVariationSearch(workspace, depth - 1, ply + 1, -beta, -alpha, depthMoves, principalVariation);
                }
                else
                {
                    evaluatedScore = -PrincipalVariationSearch(workspace, depth - 1, ply + 1, -alpha - 1, -alpha, depthMoves, principalVariation);

                    if (alpha < evaluatedScore && evaluatedScore < beta) // in fail-soft ... && score < beta ) is common
                        evaluatedScore = -PrincipalVariationSearch(workspace, depth - 1, ply + 1, -beta, -evaluatedScore, depthMoves, principalVariation); // re-search
                }

                var afterKey = workspace.UnMakeMove(move);
                
                if (alpha < evaluatedScore)
                {
                    // alpha = Math.Max(bestScore, evaluatedScore);
                    alpha = evaluatedScore; // alpha acts like max in MiniMax

                    bestMove = move;
                    bestScore = evaluatedScore;

                    UpdatePrincipalVariation(principalVariation, parentPrincipalVariation, ply, move);
                }

                if (alpha > beta)
                {
                    // Cut node
                    break; // fail-hard beta-cutoff
                }

                bSearchPv = false;
                
                if (needsToGenerate)
                {
                    _moveGenerator.Generate(workspace, nodeMoves);
                    needsToGenerate = false;
                }
            }

            TranspositionTable.Set(transpositionKey, depth, workspace.Colour, bestScore, bestMove);

            return alpha;
        }

        private IEnumerable<uint> GetNextMove(MoveGenerationWorkspace workspace, List<uint> nodeMoves, uint previousBestMove)
        {
            if (previousBestMove != 0)
                yield return previousBestMove;

            _moveGenerator.Generate(workspace, nodeMoves);

            foreach (var move in nodeMoves)
                yield return move;
        }

        private void UpdatePrincipalVariation(uint[] source, uint[] target, int ply, uint move)
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
