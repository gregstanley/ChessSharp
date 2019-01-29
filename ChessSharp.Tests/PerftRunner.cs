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

        public List<MovePerft> Go(BitBoard board, Colour colour, int depth)
        {
            var moves = new List<uint>(256);

            MoveGenerator.Generate(board, colour, moves);

            var count = 0;

            var movesView = moves.Select(x => new MoveViewer(x));

            var movePerfts = new List<MovePerft>();

            foreach (var move in moves)
            {
                var moveView = new MoveViewer(move);

                board.MakeMove(move);

                var checkers = GetCheckers(board, colour);

                var nodes = InnerPerft(board, colour.Opposite(), depth - 1);

                count += nodes;

                movePerfts.Add(new MovePerft(moveView, nodes));

                board.UnMakeMove(move);
            }

            return movePerfts;
        }

        private int InnerPerft(BitBoard board, Colour colour, int depth)
        {
            if (depth == 0)
                return 1;

            var moves = new List<uint>(256);

            MoveGenerator.Generate(board, colour, moves);

            var count = 0;

            var movesView = moves.Select(x => new MoveViewer(x));
            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var movePerfts = new List<MovePerft>();

            foreach (var move in moves)
            {
                var moveView = new MoveViewer(move);

                board.MakeMove(move);

                var checkers = GetCheckers(board, colour);

                var nodes = InnerPerft(board, colour.Opposite(), depth - 1);

                count += nodes;

                movePerfts.Add(new MovePerft(moveView, nodes));

                board.UnMakeMove(move);
            }

            return count;
        }

        private SquareFlag GetCheckers(BitBoard board, Colour colour)
        {
            var relativeBitBoard = board.ToRelative(colour);

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
