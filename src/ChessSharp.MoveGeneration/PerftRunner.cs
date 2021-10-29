using System.Collections.Generic;
using ChessSharp.Common;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Extensions;

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
            // This outer method is 'dividing' the results i.e. showing nodes per moves.
            var movePerfts = new List<MovePerft>(64);

            // Only this 'oter' method will pass in an object for perft 'divide' results.
            Perft(board, colour, depth, movePerfts);

            return movePerfts;
        }

        private int Perft(Board board, Colour colour, ushort depth, List<MovePerft> movePerfts = null)
        {
            var count = 0;

            // Using 'Stream' as that is what is used by engine (rather than pre-allocating)
            foreach (var move in MoveGenerator.GenerateStream(depth, board, colour))
            {
                // We generate legal moves so there is no need to make/unmake the final move.
                // See 'Bulk-counting' here: https://www.chessprogramming.org/Perft
                if (depth == 1)
                {
                    // Count each generated move at final depth
                    ++count;

                    // If search started at depth 1 it's also the root so populate results.
                    // We do not want to do this at any other level as we will not see values
                    // and will wate time allocating with 'new'.
                    movePerfts?.Add(new MovePerft(new MoveViewer(move), 1));
                }
                else
                {
                    board.MakeMove(move);
                    var nodes = Perft(board, colour.Opposite(), (ushort)(depth - 1));
                    count += nodes;
                    board.UnMakeMove(move);

                    movePerfts?.Add(new MovePerft(new MoveViewer(move), nodes));
                }
            }

            return count;
        }
    }
}
