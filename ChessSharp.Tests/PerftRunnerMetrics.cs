using ChessSharp.Enums;
using ChessSharp.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ChessSharp.Tests
{
    public class PerftRunnerMetrics
    {
        public PerftRunnerMetrics(MoveGenerator moveGenerator)
        {
            MoveGenerator = moveGenerator;
        }

        public MoveGenerator MoveGenerator { get; }

        public void Go(BitBoard bitBoard, Colour colour, int depth, IDictionary<int, PerftMetrics> metrics)
        {
            //if (!metrics.ContainsKey(depth))
            //    metrics[depth] = new PerftMetrics();

            for (var i = 1; i <= depth; i++)
                metrics[i] = new PerftMetrics();

            var moves = new List<uint>(256);

            MoveGenerator.Generate(bitBoard, colour, moves);

            var movesView = moves.Select(x => new MoveViewer(x));

            var depthMetrics = metrics[depth];

            depthMetrics.Legal += moves.Count();
            depthMetrics.Captures += moves.Where(x => x.GetCapturePieceType() != PieceType.None).Count();
            depthMetrics.EnPassantCaptures += moves.Where(x => x.GetMoveType() == MoveType.EnPassant).Count();
            depthMetrics.Castles += moves.Where(x => x.GetMoveType() == MoveType.CastleKing).Count();
            depthMetrics.Castles += moves.Where(x => x.GetMoveType() == MoveType.CastleQueen).Count();

            foreach (var move in moves)
            {
                var moveView = new MoveViewer(move);

                //if (moveView.From == SquareFlag.G2)
                //{ var bp = true; }

                bitBoard.MakeMove(move);

                InnerPerft(bitBoard, colour.Opposite(), depth - 1, metrics);

                bitBoard.UnMakeMove(move);
            }
        }

        private void InnerPerft(BitBoard bitBoard, Colour colour, int depth, IDictionary<int, PerftMetrics> metrics)
        {
            if (depth == 0)
                return;

            var moves = new List<uint>(256);

            MoveGenerator.Generate(bitBoard, colour, moves);

            var movesView = moves.Select(x => new MoveViewer(x));

            //if (!metrics.ContainsKey(depth))
            //    metrics[depth] = new PerftMetrics();

            var depthMetrics = metrics[depth];

            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            depthMetrics.Legal += moves.Count();
            depthMetrics.Captures += moves.Where(x => x.GetCapturePieceType() != PieceType.None).Count();
            depthMetrics.EnPassantCaptures += moves.Where(x => x.GetMoveType() == MoveType.EnPassant).Count();
            depthMetrics.Castles += moves.Where(x => x.GetMoveType() == MoveType.CastleKing).Count();
            depthMetrics.Castles += moves.Where(x => x.GetMoveType() == MoveType.CastleQueen).Count();

            //if (captures.Any())
            //{ var bp = true; }

            foreach (var move in moves)
            {
                bitBoard.MakeMove(move);

                InnerPerft(bitBoard, colour.Opposite(), depth - 1, metrics);

                bitBoard.UnMakeMove(move);

                //if (bitBoard.BlackPawns.Count() > 8)
                //{ var bp = true; }
            }
        }
    }
}
