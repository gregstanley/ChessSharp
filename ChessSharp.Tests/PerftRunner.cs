using ChessSharp.Enums;
using ChessSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

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

            foreach(var move in moves)
            {
                var moves2 = new List<uint>(256);

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

            var moves = new List<uint>(256);

            MoveGenerator.Generate(bitBoard, colour, moves);

            var count = 0;

            foreach (var move in moves)
            {
                var moves2 = new List<uint>(256);

                bitBoard.MakeMove(move);

                count += InnerPerft(bitBoard, colour.Opposite(), depth - 1);

                bitBoard.UnMakeMove(move);
            }

            return count;
        }
    }
}
