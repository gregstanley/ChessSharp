using System.Collections.Generic;
using System.Linq;
using ChessSharp.Common;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Extensions;

namespace ChessSharp.MoveGeneration
{
    public class PerftRunnerMetrics
    {
        public PerftRunnerMetrics(MoveGenerator moveGenerator)
        {
            MoveGenerator = moveGenerator;
        }

        public MoveGenerator MoveGenerator { get; }

        public void Go(Board board, Colour colour, int depth, IDictionary<int, PerftMetrics> metrics)
        {
            var moves = new List<uint>[256];

            for (var i = 0; i <= depth; i++)
            {
                moves[i] = new List<uint>(256);
                metrics[i] = new PerftMetrics();
            }

            var nodeMoves = moves[depth];
            var depthMetrics = metrics[depth];

            MoveGenerator.Generate(board, colour, nodeMoves);

            var movesView = nodeMoves.Select(x => new MoveViewer(x));

            depthMetrics.Legal += nodeMoves.Count;
            depthMetrics.Captures += nodeMoves.Where(x => x.GetCapturePieceType() != PieceType.None).Count();
            depthMetrics.EnPassantCaptures += nodeMoves.Where(x => x.GetMoveType() == MoveType.EnPassant).Count();
            depthMetrics.Castles += nodeMoves.Where(x => x.GetMoveType() == MoveType.CastleKing).Count();
            depthMetrics.Castles += nodeMoves.Where(x => x.GetMoveType() == MoveType.CastleQueen).Count();

            foreach (var move in nodeMoves)
            {
                var moveView = new MoveViewer(move);

                if (!depthMetrics.Moves.Where(x => x.Value == move).Any())
                    depthMetrics.Moves.Add(moveView);

                board.MakeMove(move);

                InnerPerft(board, colour.Opposite(), depth - 1, moves, metrics);

                board.UnMakeMove(move);
            }
        }

        private void InnerPerft(Board board, Colour colour, int depth, List<uint>[] moves, IDictionary<int, PerftMetrics> metrics)
        {
            if (depth == 0)
                return;

            var nodeMoves = moves[depth];
            var depthMetrics = metrics[depth];

            // Must wipe any existing moves each time we enter a depth
            nodeMoves.Clear();

            MoveGenerator.Generate(board, colour, nodeMoves);

            var movesView = nodeMoves.Select(x => new MoveViewer(x));

            var captures = nodeMoves.Where(x => x.GetCapturePieceType() != PieceType.None);

            depthMetrics.Legal += nodeMoves.Count;
            depthMetrics.Captures += nodeMoves.Where(x => x.GetCapturePieceType() != PieceType.None).Count();
            depthMetrics.EnPassantCaptures += nodeMoves.Where(x => x.GetMoveType() == MoveType.EnPassant).Count();
            depthMetrics.Castles += nodeMoves.Where(x => x.GetMoveType() == MoveType.CastleKing).Count();
            depthMetrics.Castles += nodeMoves.Where(x => x.GetMoveType() == MoveType.CastleQueen).Count();

            foreach (var move in nodeMoves)
            {
                var moveView = new MoveViewer(move);

                if (!depthMetrics.Moves.Where(x => x.Value == move).Any())
                    depthMetrics.Moves.Add(moveView);

                board.MakeMove(move);

                InnerPerft(board, colour.Opposite(), depth - 1, moves, metrics);

                board.UnMakeMove(move);
            }
        }
    }
}
