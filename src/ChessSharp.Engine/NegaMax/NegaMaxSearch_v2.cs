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
    public class NegaMaxSearch_v2
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

        public NegaMaxSearch_v2(MoveGenerator moveGenerator, IPositionEvaluator positionEvaluator)
        {
            this.moveGenerator = moveGenerator;
            this.positionEvaluator = positionEvaluator;
        }

        public NegaMaxSearchResult GoNoIterativeDeepening(Board board, Colour colour, int maxDepth)
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

            // http://mediocrechess.blogspot.com/2006/12/programming-extracting-principal.html
            var pv = new uint[maxDepth];

            if (!rootMoves.Any())
                return new NegaMaxSearchResult(depth, ply, stopWatch.ElapsedMilliseconds, counters.Positions, alpha, pv);

            var rootMoveEvaluations = new List<MoveEvaluation>();

            pv[ply] = rootMoves[0];

            foreach (var move in rootMoves)
            {
                counters.Positions++;

                board.MakeMove(move);
                var score = -NegaMax(board, colour.Opposite(), counters, depth - 1, ply + 1, alpha, beta, pv);
                board.UnMakeMove(move);

                if (score >= alpha)
                {
                    alpha = score;
                    pv[ply] = move;
                }

                var status = new SearchStatus(depth, ply, stopWatch.ElapsedMilliseconds, counters.Positions, alpha, pv, pv[ply]);
                SearchProgress?.Invoke(this, new SearchProgressEventArgs(status));

                rootMoveEvaluations.Add(new MoveEvaluation(new MoveViewer(move), 0));
            }

            stopWatch.Stop();

            return new NegaMaxSearchResult(depth, ply, stopWatch.ElapsedMilliseconds, counters.Positions, alpha, pv, pv[ply]);
        }

        private int NegaMax(Board board, Colour colour, Counters counters, int depth, int ply, int alpha, int beta, uint[] pv)
        {
            counters.Positions++;

            if (depth == 0)
            {
                var score = positionEvaluator.Evaluate(board) * (colour == Colour.White ? 1 : -1);
                var status1 = new SearchStatus(depth, ply, stopWatch.ElapsedMilliseconds, counters.Positions, score, pv);
                SearchProgress?.Invoke(this, new SearchProgressEventArgs(status1));
                return score;
            }

            foreach (var move in GetNextMove(moveGenerator, ply, board, colour))
            {
                board.MakeMove(move);
                var score = -NegaMax(board, colour.Opposite(), counters, depth - 1, ply + 1, alpha, beta, pv);
                board.UnMakeMove(move);

                if (score >= alpha)
                {
                    alpha = score;
                    pv[ply] = move;
                }

                var status1 = new SearchStatus(depth, ply, stopWatch.ElapsedMilliseconds, counters.Positions, alpha, pv);
                SearchProgress?.Invoke(this, new SearchProgressEventArgs(status1));
            }

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
