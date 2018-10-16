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

                if (!depthMetrics.Moves.Where(x => x.Value == move).Any())
                    depthMetrics.Moves.Add(moveView);

                //if (moveView.From == SquareFlag.G2)
                //{ var bp = true; }

                bitBoard.MakeMove(move);

                var checkers = GetCheckers(bitBoard, colour);
                
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
                var moveView = new MoveViewer(move);

                if (moveView.From == SquareFlag.B2)
                {
                    var pt = bitBoard.GetPieceColour(SquareFlag.A1);

                    if (moveView.To == SquareFlag.A1)
                    { var bp = true; }
                }

                if (moveView.From == SquareFlag.D1)
                {
                    var pt = bitBoard.GetPieceColour(SquareFlag.A1);

                    if (moveView.To == SquareFlag.A1)
                    { var bp = true; }
                }

                if (!depthMetrics.Moves.Where(x => x.Value == move).Any())
                    depthMetrics.Moves.Add(moveView);

                bitBoard.MakeMove(move);

                var blackPawnCount = bitBoard.BlackPawns.Count();

                if (blackPawnCount > 3)
                { var bp = true; }

                var checkers = GetCheckers(bitBoard, colour);

                InnerPerft(bitBoard, colour.Opposite(), depth - 1, metrics);

                bitBoard.UnMakeMove(move);

                //if (bitBoard.BlackPawns.Count() > 8)
                //{ var bp = true; }
            }

            if (depth == 1)
            { var bp = true; }

            if (depth == 2)
            { var bp = true; }

            if (depth == 3)
            { var bp = true; }
        }

        private SquareFlag GetCheckers(BitBoard bitBoard, Colour colour)
        {
            var relativeBitBoard = bitBoard.ToRelative(colour);

            var checkersPawn = MoveGenerator.GetPawnCheckers(relativeBitBoard, relativeBitBoard.MyKing);
            var checkersKnight = MoveGenerator.GetKnightCheckers(relativeBitBoard, relativeBitBoard.MyKing);
            var checkersRook = MoveGenerator.GetCheckers(relativeBitBoard, relativeBitBoard.MyKing, PieceType.Rook, PieceType.Rook);
            var checkersBishop = MoveGenerator.GetCheckers(relativeBitBoard, relativeBitBoard.MyKing, PieceType.Bishop, PieceType.Bishop);
            var checkersQueenAsRook = MoveGenerator.GetCheckers(relativeBitBoard, relativeBitBoard.MyKing, PieceType.Rook, PieceType.Queen);
            var checkersQueenAsBishop = MoveGenerator.GetCheckers(relativeBitBoard, relativeBitBoard.MyKing, PieceType.Bishop, PieceType.Queen);

            var checkers = checkersPawn | checkersKnight | checkersRook | checkersBishop | checkersQueenAsRook | checkersQueenAsBishop;

            if (checkers > 0)
            { var bp = true; }

            return checkers;
        }
    }
}
