using ChessSharp.Enums;
using ChessSharp.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ChessSharp.Tests
{
    public class PerftRunner
    {
        public PerftRunner(MoveGenerator moveGenerator)
        {
            MoveGenerator = moveGenerator;
        }

        public MoveGenerator MoveGenerator { get; }

        public List<MovePerft> Go(BitBoard bitBoard, Colour colour, int depth)
        {
            var moves = new List<uint>(256);

            MoveGenerator.Generate(bitBoard, colour, moves);

            var count = 0;

            var movesView = moves.Select(x => new MoveViewer(x));

            var movePerfts = new List<MovePerft>();

            foreach (var move in moves)
            {
                var moveView = new MoveViewer(move);

                if (moveView.From == SquareFlag.A1)
                { var bp = true; }

                bitBoard.MakeMove(move);

                var checkers = GetCheckers(bitBoard, colour);

                var nodes = InnerPerft(bitBoard, colour.Opposite(), depth - 1);

                count += nodes;

                movePerfts.Add(new MovePerft(moveView, nodes));

                bitBoard.UnMakeMove(move);
            }

            return movePerfts;
        }

        private int InnerPerft(BitBoard bitBoard, Colour colour, int depth)
        {
            if (depth == 0)
                return 1;

            if (depth == 1)
            { var bp = true; }
            var moves = new List<uint>(256);

            MoveGenerator.Generate(bitBoard, colour, moves);

            var count = 0;

            var movesView = moves.Select(x => new MoveViewer(x));
            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            if (captures.Any())
            { var bp = true; }

            var movePerfts = new List<MovePerft>();

            foreach (var move in moves)
            {
                var moveView = new MoveViewer(move);

                bitBoard.MakeMove(move);

                var checkers = GetCheckers(bitBoard, colour);

                var nodes = InnerPerft(bitBoard, colour.Opposite(), depth - 1);

                count += nodes;

                movePerfts.Add(new MovePerft(moveView, nodes));

                bitBoard.UnMakeMove(move);
            }

            if (depth == 1)
            { var bp = true; }

            if (depth == 2)
            { var bp = true; }

            if (depth == 3)
            { var bp = true; }
            return count;
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
