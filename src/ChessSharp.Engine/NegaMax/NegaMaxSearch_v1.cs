using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChessSharp.Common;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Extensions;
using ChessSharp.Engine.NegaMax.Events;
using ChessSharp.MoveGeneration;

namespace ChessSharp.Engine.NegaMax
{
    public class NegaMaxSearch_v1
    {
        internal class Counters
        {
            public int Positions { get; set; }
        }

        public delegate void SearchProgressEventDelegate(object sender, SearchProgressEventArgs args);

        public event SearchProgressEventDelegate SearchProgress;

        private readonly Stopwatch stopWatch = new();

        private readonly MoveGenerator moveGenerator;

        private readonly IPositionEvaluator positionEvaluator;

        public NegaMaxSearch_v1(MoveGenerator moveGenerator, IPositionEvaluator positionEvaluator)
        {
            this.moveGenerator = moveGenerator;
            this.positionEvaluator = positionEvaluator;
        }

        public NegaMaxSearchResult GoNoIterativeDeepeningAndNoPv(Board board, Colour colour, int maxDepth)
        {
            stopWatch.Restart();

            var counters = new Counters { Positions = 0 };
            var depth = maxDepth;
            var ply = 0;
            var alpha = int.MinValue;
            var beta = int.MaxValue;

            var rootMovesEnumerable = moveGenerator.GenerateStream(board, colour);

            // TODO: This seems to perform very badly on the very first run
            var rootMoves = rootMovesEnumerable.ToArray();

            if (!rootMoves.Any())
                return new NegaMaxSearchResult(depth, ply, stopWatch.ElapsedMilliseconds, counters.Positions, alpha, System.Array.Empty<uint>());

            var rootMoveEvaluations = new List<MoveEvaluation>();
            var bestMove = 0u;

            foreach (var move in rootMoves)
            {
                counters.Positions++;

                board.MakeMove(move);
                var score = -NegaMaxNoPv(board, colour.Opposite(), counters, depth - 1, ply + 1, alpha, beta);
                board.UnMakeMove(move);

                rootMoveEvaluations.Add(new MoveEvaluation(new MoveViewer(move), 0));

                if (score >= alpha)
                {
                    alpha = score;
                    bestMove = move;
                }
            }

            stopWatch.Stop();

            return new NegaMaxSearchResult(depth, ply, stopWatch.ElapsedMilliseconds, counters.Positions, alpha, System.Array.Empty<uint>(), bestMove);
        }


        private int NegaMaxNoPv(Board board, Colour colour, Counters counters, int depth, int ply, int alpha, int beta)
        {
            counters.Positions++;

            if (depth == 0)
            {
                var score = positionEvaluator.Evaluate(board) * (colour == Colour.White ? 1 : -1);
                var status1 = new SearchStatus(depth, ply, stopWatch.ElapsedMilliseconds, counters.Positions, score, System.Array.Empty<uint>());
                SearchProgress?.Invoke(this, new SearchProgressEventArgs(status1));
                return score;
            }

            foreach (var move in GetNextMove(moveGenerator, ply, board, colour))
            {
                board.MakeMove(move);
                var score = -NegaMaxNoPv(board, colour.Opposite(), counters, depth - 1, ply + 1, alpha, beta);
                board.UnMakeMove(move);

                if (score >= alpha)
                {
                    alpha = score;
                }
            }

            var status = new SearchStatus(depth, ply, stopWatch.ElapsedMilliseconds, counters.Positions, alpha, System.Array.Empty<uint>());
            SearchProgress?.Invoke(this, new SearchProgressEventArgs(status));

            return alpha;
        }

        private static IEnumerable<uint> GetNextMove(MoveGenerator moveGenerator, int ply, Board board, Colour colour)
        {
            foreach (var move in moveGenerator.GenerateStream((ushort)ply, board, colour))
            {
                yield return move;
            }
        }
    }
}
