using ChessSharp.Enums;
using ChessSharp.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ChessSharp.MoveGeneration
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
            //var nodeMoves = new List<uint>(256);
            var depthMoves = new List<uint>[64];

            for (var i = 0; i <= depth; i++)
                depthMoves[i] = new List<uint>(256);

            var nodeMoves = depthMoves[depth];

            MoveGenerator.Generate(bitBoard, colour, nodeMoves);

            var count = 0;

            //var movesView = moves.Select(x => new MoveViewer(x));

            var movePerfts = new List<MovePerft>();

            foreach (var move in nodeMoves)
            {
                var moveView = new MoveViewer(move);

                bitBoard.MakeMove(move);

                //var checkers = GetCheckers(bitBoard, colour);

                var nodes = InnerPerft(bitBoard, colour.Opposite(), depth - 1, depthMoves);

                count += nodes;

                movePerfts.Add(new MovePerft(moveView, nodes));

                bitBoard.UnMakeMove(move);
            }

            return movePerfts;
        }

        private int InnerPerft(BitBoard bitBoard, Colour colour, int depth, List<uint>[] depthMoves)
        {
            if (depth == 0)
                return 1;

            //var nodeMoves = new List<uint>(256);
            var nodeMoves = depthMoves[depth];

            // Must wipe any existing moves each time we enter a depth
            nodeMoves.Clear();

            MoveGenerator.Generate(bitBoard, colour, nodeMoves);

            var count = 0;

            //var movesView = moves.Select(x => new MoveViewer(x));
            //var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            //var movePerfts = new List<MovePerft>();

            foreach (var move in nodeMoves)
            {
                //var moveView = new MoveViewer(move);

                bitBoard.MakeMove(move);

                //var checkers = GetCheckers(bitBoard, colour);

                var nodes = InnerPerft(bitBoard, colour.Opposite(), depth - 1, depthMoves);

                count += nodes;

                //movePerfts.Add(new MovePerft(moveView, nodes));

                bitBoard.UnMakeMove(move);
            }

            return count;
        }

        private SquareFlag GetCheckers(BitBoard bitBoard, Colour colour)
        {
            var relativeBitBoard = bitBoard.RelativeTo(colour);

            var checkersPawn = MoveGenerator.GetPawnCheckers(relativeBitBoard, relativeBitBoard.MyKing);
            var checkersKnight = MoveGenerator.GetKnightCheckers(relativeBitBoard, relativeBitBoard.MyKing);
            var checkersRook = MoveGenerator.GetCheckers(relativeBitBoard, relativeBitBoard.MyKing, PieceType.Rook, PieceType.Rook);
            var checkersBishop = MoveGenerator.GetCheckers(relativeBitBoard, relativeBitBoard.MyKing, PieceType.Bishop, PieceType.Bishop);
            var checkersQueenAsRook = MoveGenerator.GetCheckers(relativeBitBoard, relativeBitBoard.MyKing, PieceType.Rook, PieceType.Queen);
            var checkersQueenAsBishop = MoveGenerator.GetCheckers(relativeBitBoard, relativeBitBoard.MyKing, PieceType.Bishop, PieceType.Queen);

            var checkers = checkersPawn | checkersKnight | checkersRook | checkersBishop | checkersQueenAsRook | checkersQueenAsBishop;

            return checkers;
        }
    }
}
