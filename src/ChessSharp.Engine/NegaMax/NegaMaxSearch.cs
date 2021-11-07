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
    public class NegaMaxSearch
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

        public NegaMaxSearch(MoveGenerator moveGenerator, IPositionEvaluator positionEvaluator)
        {
            this.moveGenerator = moveGenerator;
            this.positionEvaluator = positionEvaluator;
        }

        public uint Go(Board board, Colour colour, int maxDepth)
        {
            stopWatch.Restart();

            var counters = new Counters { Positions = 0 };
            var depth = maxDepth + 1;
            var ply = 0;
            var alpha = int.MinValue;
            var beta = int.MaxValue;

            var rootMovesEnumerable = moveGenerator.GenerateStream(board, colour);

            // TODO: This seems to perform very badly on the very first run
            var rootMoves = rootMovesEnumerable.ToArray();

            var bestMove = 0u;

            foreach (var move in rootMoves)
            {
                counters.Positions++;
                board.MakeMove(move);
                var score = -NegaMax(board, colour.Opposite(), counters, depth - 1, ply + 1, alpha, beta);
                board.UnMakeMove(move);

                if (score >= alpha)
                {
                    alpha = score;
                    bestMove = move;
                }
            }

            stopWatch.Stop();

            var status = new SearchStatus(depth, ply, stopWatch.ElapsedMilliseconds, counters.Positions, alpha);
            SearchProgress?.Invoke(this, new SearchProgressEventArgs(status));

            return bestMove;
        }

        private int NegaMax(Board board, Colour colour, Counters counters, int depth, int ply, int alpha, int beta)
        {
            counters.Positions++;

            if (depth == 0)
            {
                return positionEvaluator.Evaluate(board) * (colour == Colour.White ? 1 : -1);
            }

            foreach (var move in GetNextMove(moveGenerator, ply, board, colour))
            {
                board.MakeMove(move);
                var score = -NegaMax(board, colour.Opposite(), counters, depth - 1, ply + 1, alpha, beta);
                board.UnMakeMove(move);

                if (score >= alpha)
                {
                    alpha = score;
                }
            }

            var status = new SearchStatus(depth, ply, stopWatch.ElapsedMilliseconds, counters.Positions, alpha);
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
