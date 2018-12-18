using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using ChessSharp.Extensions;
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

        private readonly MoveGenerator _moveGenerator;

        private PositionEvaluator _positionEvaluator;

        private TranspositionTable _transpositionTable;

        private readonly Stopwatch _stopWatch = new Stopwatch();

        public Search(MoveGenerator moveGenerator, PositionEvaluator positionEvaluator, TranspositionTable transpositionTable)
        {
            _moveGenerator = moveGenerator;
            _positionEvaluator = positionEvaluator;
            _transpositionTable = transpositionTable;
        }

        public SearchResults Go(BitBoard bitBoard, Colour colour, int maxDepth)
        {
            _stopWatch.Restart();

            PositionCount = 0;

            _transpositionTable.NextIteration();

            var transpositionKey = bitBoard.Key;

            var existingTransposition = _transpositionTable.Find(transpositionKey);

            uint previousBestMove = 0;

            if (existingTransposition != null)
            {
                if (existingTransposition.BestMove != 0)
                    previousBestMove = existingTransposition.BestMove;
            }

            var nodeMoves = new List<uint>(256);

            _moveGenerator.Generate(bitBoard, colour, nodeMoves);

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
                existingTransposition = _transpositionTable.Find(bitBoard.Key);

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

                var moveCount = 0;

                if (previousBestMove != 0)
                {
                    ++moveCount;

                    var move = previousBestMove;

                    bitBoard.MakeMove(move);

                    var evaluatedScore = -PrincipalVariationSearch(bitBoard, colour.Opposite(), (byte)(currentMaxDepth - 1), 1, int.MinValue, int.MaxValue, principalVariation);

                    bitBoard.UnMakeMove(move);

                    if (evaluatedScore > bestScore)
                    {
                        bestMove = move;
                        bestScore = evaluatedScore;

                        UpdatePrincipalVariation(principalVariation, masterPrincipalVariation, 0, move);

                        info = new InfoNewPv(PositionCount, _stopWatch.ElapsedMilliseconds, currentMaxDepth, bestMove, bestScore, _transpositionTable);

                        Info?.Invoke(this, new InfoEventArgs(info));
                    }

                    moveEvaluations.Add(new MoveEvaluation(new MoveViewer(move), evaluatedScore));
                }

                
                foreach (var move in nodeMoves)
                {
                    if (move == previousBestMove)
                        continue;

                    var nextDepth = (byte)(currentMaxDepth - 1);

                    ++moveCount;

                    var moveView = new MoveViewer(move);

                    bitBoard.MakeMove(move);

                    var evaluatedScore = -PrincipalVariationSearch(bitBoard, colour.Opposite(), nextDepth, 1, int.MinValue, int.MaxValue, principalVariation);

                    bitBoard.UnMakeMove(move);

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

                nodeMoves = moveEvaluations
                    .OrderByDescending(x => x.Score)
                    .Select(x => x.Move.Value)
                    .ToList();

                _transpositionTable.Set(transpositionKey, (byte)currentMaxDepth, colour, bestScore, bestMove);

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

        private int PrincipalVariationSearch(BitBoard bitBoard, Colour colour, byte depth, ushort ply, int alpha, int beta, uint[] parentPrincipalVariation)
        {
            ++PositionCount;

            if (depth == 0)
                return _positionEvaluator.Evaluate(bitBoard) * (colour == Colour.White ? 1 : -1);

            var transpositionKey = bitBoard.Key;

            var existingTransposition = _transpositionTable.Find(transpositionKey);

            uint previousBestMove = 0;

            if (existingTransposition != null)
            {
                if (existingTransposition.Depth > 1 && existingTransposition.Depth > depth)
                    return existingTransposition.Evaluation;

                if (existingTransposition.BestMove != 0)
                    previousBestMove = existingTransposition.BestMove;
            }

            var bestScore = int.MinValue;
            var bestMove = 0u;

            var isFirst = true;

            var principalVariation = new uint[64];

            //var bSearchPv = true;

            var moveCount = 1;

            foreach(var move in GetNextMove(_moveGenerator, ply, bitBoard, colour, previousBestMove))
            {
                if (!isFirst && move == previousBestMove)
                    continue;

                isFirst = false;

                bitBoard.MakeMove(move);

                var evaluatedScore = 0;

                var oppositeColour = colour.Opposite();

                var nextDepth = (byte)(depth - 1);

                // https://www.chessprogramming.org/Principal_Variation_Search
                if (moveCount == 1)
                {
                    evaluatedScore = -PrincipalVariationSearch(bitBoard, oppositeColour, nextDepth, (ushort)(ply + 1), -beta, -alpha, principalVariation);
                }
                else
                {
                    // Late Move Reduction http://mediocrechess.blogspot.com/2007/03/other-late-move-reduction-lmr.html
                    // TODO: Add IsCheck check
                    if (ply > 3 && moveCount > 3 && move.GetCapturePieceType() == PieceType.None && nextDepth > 0)
                    {
                        --nextDepth;
                    }

                    evaluatedScore = -PrincipalVariationSearch(bitBoard, oppositeColour, nextDepth, (ushort)(ply + 1), -alpha - 1, -alpha, principalVariation);

                    if (alpha < evaluatedScore && evaluatedScore < beta) // in fail-soft ... && score < beta ) is common
                    {
                        evaluatedScore = -PrincipalVariationSearch(bitBoard, oppositeColour, nextDepth, (ushort)(ply + 1), -beta, -evaluatedScore, principalVariation); // re-search
                    }
                }

                bitBoard.UnMakeMove(move);

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

                //bSearchPv = false;

                ++moveCount;

                if (ply == 1)
                {
                    var info = new Info(PositionCount, _stopWatch.ElapsedMilliseconds, depth, _transpositionTable);

                    Info?.Invoke(this, new InfoEventArgs(info));
                }
            }

            _transpositionTable.Set(transpositionKey, depth, colour, bestScore, bestMove);

            if (alpha == int.MinValue)
                alpha = PieceValues.CheckmateValue + ply;

            if (alpha == int.MaxValue)
                alpha = -(PieceValues.CheckmateValue + ply);

            return alpha;
        }

        private IEnumerable<uint> GetNextMove(MoveGenerator moveGenerator, ushort ply, BitBoard bitBoard, Colour colour, uint previousBestMove)
        {
            if (previousBestMove > 0)
                yield return previousBestMove;

            foreach (var move in moveGenerator.GenerateChunk(ply, bitBoard, colour))
                yield return move;
        }

        private void UpdatePrincipalVariation(uint[] source, uint[] target, ushort ply, uint move)
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
