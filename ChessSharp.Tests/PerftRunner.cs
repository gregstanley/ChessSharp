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

        public int Go(BitBoard bitBoard, Colour colour, int depth)
        {
            var moves = new List<uint>(256);

            MoveGenerator.Generate(bitBoard, colour, moves);

            var count = 0;

            var movesView = moves.Select(x => new MoveViewer(x));

            foreach (var move in moves)
            {
                bitBoard.MakeMove(move);

                count += InnerPerft(bitBoard, colour.Opposite(), depth - 1);

                bitBoard.UnMakeMove(move);
            }

            return count;
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

            foreach (var move in moves)
            {
                bitBoard.MakeMove(move);

                count += InnerPerft(bitBoard, colour.Opposite(), depth - 1);

                bitBoard.UnMakeMove(move);
            }

            return count;
        }
    }
}
