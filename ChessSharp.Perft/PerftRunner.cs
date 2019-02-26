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

        public List<MovePerft> Go(Board board, Colour colour, ushort depth)
        {
            var depthMoves = new List<uint>[64];

            for (var i = 0; i <= depth; i++)
                depthMoves[i] = new List<uint>(256);

            var nodeMoves = depthMoves[depth];

            MoveGenerator.Generate(board, colour, nodeMoves);

            var count = 0;

            var movePerfts = new List<MovePerft>(64);

            foreach (var move in nodeMoves)
            {
                var moveView = new MoveViewer(move);

                board.MakeMove(move);

                var nodes = InnerPerft(board, colour.Opposite(), (ushort)(depth - 1), depthMoves);

                count += nodes;

                movePerfts.Add(new MovePerft(moveView, nodes));

                board.UnMakeMove(move);
            }

            return movePerfts;
        }

        private int InnerPerft(Board board, Colour colour, ushort depth, List<uint>[] depthMoves)
        {
            if (depth == 0)
                return 1;

            var nodeMoves = depthMoves[depth];

            // Must wipe any existing moves each time we enter a depth
            nodeMoves.Clear();

            var count = 0;

            foreach (var move in MoveGenerator.GenerateStream(depth, board, colour))
            {
                var moveView = new MoveViewer(move);

                board.MakeMove(move);

                var nodes = InnerPerft(board, colour.Opposite(), (ushort)(depth - 1), depthMoves);

                count += nodes;

                board.UnMakeMove(move);
            }

            return count;
        }
    }
}
