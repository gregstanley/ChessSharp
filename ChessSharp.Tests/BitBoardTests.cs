using ChessSharp.Enums;
using Xunit;

namespace ChessSharp.Tests
{
    public class BitBoardTests
    {
        [Theory]
        [InlineData("8/P3p3", SquareFlag.D3)]
        [InlineData("4p3/P7", (SquareFlag)0)]
        public void Make_EnPassantOnlySetIfCapturePossible(string partialFen, SquareFlag enPassant)
        {
            var bitBoard = Create($"rnbqkbnr/pppp1ppp/8/{partialFen}/1P6/2PPPPPP/RNBQKBNR b KQkq - 0 1");

            var move = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D2, SquareFlag.D4, PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.Equal(enPassant, bitBoard.EnPassant);
        }

        [Fact]
        public void Make_UnMake_Correct()
        {
            var bitBoard = Create("rnbqkbnr/pppp1ppp/8/8/P3p3/1P6/2PPPPPP/RNBQKBNR b KQkq - 0 1");

            var move = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D2, SquareFlag.D4, PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.Equal(SquareFlag.D3, bitBoard.EnPassant);

            bitBoard.UnMakeMove(move);

            Assert.Equal((SquareFlag)0, bitBoard.EnPassant);
        }

        private BitBoard Create(string fenString)
        {
            var fen = Fen.Parse(fenString);

            return BitBoard.FromFen(fen);
        }
    }
}
