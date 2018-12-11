using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using ChessSharp.MoveGeneration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChessSharp.Engine
{
    public class Search
    {
        public delegate void InfoEventDelegate(object sender, InfoEventArgs args);

        public event InfoEventDelegate Info;

        public int PositionCount { get; private set; }

        private MoveGenerator _moveGenerator;

        private PositionEvaluator _positionEvaluator;

        private TranspositionTable _transpositionTable;

        private readonly Stopwatch _stopWatch = new Stopwatch();

        public Search(MoveGenerator moveGenerator, PositionEvaluator positionEvaluator, TranspositionTable transpositionTable)
        {
            _moveGenerator = moveGenerator;
            _positionEvaluator = positionEvaluator;
            _transpositionTable = transpositionTable;
        }

        public SearchResults Go(MoveGenerationWorkspace workspace, int maxDepth)
        {
            _stopWatch.Restart();

            PositionCount = 0;

            _transpositionTable.NextIteration();

            var transpositionKey = workspace.BitBoard.Key;

            var existingTransposition = _transpositionTable.Find(transpositionKey);

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
                return new SearchResults(PositionCount, new List<long>(), moveEvaluations, principalVariations, _transpositionTable);

            var currentMaxDepth = 1;

            var lap = _stopWatch.ElapsedMilliseconds;

            var iterationLaps = new List<long>();

            Info info;

            while (currentMaxDepth <= maxDepth)
            {
                existingTransposition = _transpositionTable.Find(workspace.BitBoard.Key);

                if (existingTransposition != null)
                {
                    if (existingTransposition.BestMove != 0)
                        previousBestMove = existingTransposition.BestMove;
                }

                var masterPrincipalVariation = principalVariations[currentMaxDepth - 1];

                var principalVariation = new uint[64];

                moveEvaluations.Clear();

                var bestScore = int.MinValue;
                var bestMove = 0u;

                if (previousBestMove != 0)
                {
                    var move = previousBestMove;

                    var moveView = new MoveViewer(move);

                    var beforeKey = workspace.MakeMove(move);

                    var evaluatedScore = -PrincipalVariationSearch(workspace, (byte)(currentMaxDepth - 1), 1, int.MinValue, int.MaxValue,
                        depthMoves, principalVariation);

                    var afterKey = workspace.UnMakeMove(move);

                    if (evaluatedScore > bestScore)
                    {
                        bestMove = move;
                        bestScore = evaluatedScore;

                        UpdatePrincipalVariation(principalVariation, masterPrincipalVariation, 0, move);

                        info = new InfoNewPv(PositionCount, _stopWatch.ElapsedMilliseconds, currentMaxDepth, bestMove, bestScore, _transpositionTable);

                        Info?.Invoke(this, new InfoEventArgs(info));
                    }

                    moveEvaluations.Add(new MoveEvaluation(moveView, evaluatedScore));
                }

                foreach (var move in nodeMoves)
                {
                    if (move == previousBestMove)
                        continue;

                    var moveView = new MoveViewer(move);

                    var beforeKey = workspace.MakeMove(move);

                    var evaluatedScore = -PrincipalVariationSearch(workspace, (byte)(currentMaxDepth - 1), 1, int.MinValue, int.MaxValue,
                        depthMoves, principalVariation);

                    var afterKey = workspace.UnMakeMove(move);

                    if (evaluatedScore > bestScore)
                    {
                        bestMove = move;
                        bestScore = evaluatedScore;

                        UpdatePrincipalVariation(principalVariation, masterPrincipalVariation, 0, move);

                        info = new InfoNewPv(PositionCount, _stopWatch.ElapsedMilliseconds, currentMaxDepth, bestMove, bestScore, _transpositionTable);

                        Info?.Invoke(this, new InfoEventArgs(info));
                    }

                    moveEvaluations.Add(new MoveEvaluation(moveView, evaluatedScore));
                }

                _transpositionTable.Set(transpositionKey, (byte)currentMaxDepth, workspace.Colour, bestScore, bestMove);

                var iterationLength = _stopWatch.ElapsedMilliseconds - lap;

                lap = _stopWatch.ElapsedMilliseconds;

                iterationLaps.Add(iterationLength);
            
                info = new InfoDepthComplete(PositionCount, _stopWatch.ElapsedMilliseconds, currentMaxDepth, masterPrincipalVariation, _transpositionTable);

                Info?.Invoke(this, new InfoEventArgs(info));

                ++currentMaxDepth;
            }

            _stopWatch.Reset();

            return new SearchResults(PositionCount, iterationLaps, moveEvaluations, principalVariations, _transpositionTable);
        }

        private int PrincipalVariationSearch(MoveGenerationWorkspace workspace, byte depth, int ply, int alpha, int beta, List<uint>[] depthMoves, uint[] parentPrincipalVariation)
        {
            ++PositionCount;

            if (depth == 0)
                return _positionEvaluator.Evaluate(workspace.BitBoard) * (workspace.Colour == Colour.White ? 1 : -1);

            var nodeMoves = depthMoves[depth];

            var transpositionKey = workspace.BitBoard.Key;

            var existingTransposition = _transpositionTable.Find(transpositionKey);

            uint previousBestMove = 0;

            // Must wipe any existing moves each time we enter a depth
            nodeMoves.Clear();

            if (existingTransposition != null)
            {
                if (existingTransposition.Depth > 1 && existingTransposition.Depth > depth)
                    return existingTransposition.Evaluation;

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

            var bestScore = int.MinValue;
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

                var evaluatedScore = 0;

                // https://www.chessprogramming.org/Principal_Variation_Search
                if (bSearchPv)
                {
                    evaluatedScore = -PrincipalVariationSearch(workspace, (byte)(depth - 1), ply + 1, -beta, -alpha, depthMoves, principalVariation);
                }
                else
                {
                    evaluatedScore = -PrincipalVariationSearch(workspace, (byte)(depth - 1), ply + 1, -alpha - 1, -alpha, depthMoves, principalVariation);

                    if (alpha < evaluatedScore && evaluatedScore < beta) // in fail-soft ... && score < beta ) is common
                    {
                        evaluatedScore = -PrincipalVariationSearch(workspace, (byte)(depth - 1), ply + 1, -beta, -evaluatedScore, depthMoves, principalVariation); // re-search
                    }
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

                if (ply == 1)
                {
                    var info = new Info(PositionCount, _stopWatch.ElapsedMilliseconds, depth, _transpositionTable);

                    Info?.Invoke(this, new InfoEventArgs(info));
                }
            }

            _transpositionTable.Set(transpositionKey, depth, workspace.Colour, bestScore, bestMove);

            if (alpha == int.MinValue)
                alpha = -10000 - depth;

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
