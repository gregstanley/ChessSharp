using ChessSharp.Enums;
using ChessSharp.Extensions;
using System.Collections.Generic;

namespace ChessSharp.MoveGeneration
{
    public class PerftRunner
    {
        public PerftRunner(MoveGenerator moveGenerator)
        {
            MoveGenerator = moveGenerator;
        }

        public MoveGenerator MoveGenerator { get; }

        public List<MovePerft> Go(BitBoard bitBoard, Colour colour, ushort depth)
        {
            var depthMoves = new List<uint>[64];

            for (var i = 0; i <= depth; i++)
                depthMoves[i] = new List<uint>(256);

            var nodeMoves = depthMoves[depth];

            MoveGenerator.Generate(bitBoard, colour, nodeMoves);

            var count = 0;

            var movePerfts = new List<MovePerft>(64);

            foreach (var move in nodeMoves)
            {
                var moveView = new MoveViewer(move);

                bitBoard.MakeMove(move);

                var nodes = InnerPerft(bitBoard, colour.Opposite(), (ushort)(depth - 1), depthMoves);

                count += nodes;

                movePerfts.Add(new MovePerft(moveView, nodes));

                bitBoard.UnMakeMove(move);
            }

            return movePerfts;
        }

        private int InnerPerft(BitBoard bitBoard, Colour colour, ushort depth, List<uint>[] depthMoves)
        {
            if (depth == 0)
                return 1;

            var nodeMoves = depthMoves[depth];

            // Must wipe any existing moves each time we enter a depth
            nodeMoves.Clear();

            var count = 0;

            foreach(var move in MoveGenerator.GenerateStream(depth, bitBoard, colour))
            {
                var moveView = new MoveViewer(move);

                bitBoard.MakeMove(move);

                var nodes = InnerPerft(bitBoard, colour.Opposite(), (ushort)(depth - 1), depthMoves);

                count += nodes;

                bitBoard.UnMakeMove(move);
            }

            return count;
        }
    }
}
