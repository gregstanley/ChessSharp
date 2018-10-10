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
            var bitBoard = Create($"rnbqkbnr/pppp1ppp/8/{partialFen}/1P6/2PPPPPP/RNBQKBNR b KQkq -");

            var move = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D2, SquareFlag.D4, PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.Equal(enPassant, bitBoard.EnPassant);
        }

        [Fact]
        public void Make_UnMake_Correct()
        {
            var bitBoard = Create("rnbqkbnr/pppp1ppp/8/8/P3p3/1P6/2PPPPPP/RNBQKBNR b KQkq -");

            var move = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D2, SquareFlag.D4, PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.Equal(SquareFlag.D3, bitBoard.EnPassant);

            bitBoard.UnMakeMove(move);

            Assert.Equal((SquareFlag)0, bitBoard.EnPassant);
        }

        [Fact]
        public void Make_UnMake_EnPassant_Correct()
        {
            var bitBoard = Create("4k3/8/8/8/5Pp1/8/8/4K3 w - f3 0 1");

            var bitBoard2 = Create("4k3/8/8/8/5Pp1/8/8/4K3 w - f3 0 1");

            var move = MoveConstructor.CreateMove(Colour.Black, PieceType.Pawn, SquareFlag.G4, SquareFlag.F3, PieceType.Pawn, MoveType.EnPassant);

            bitBoard.MakeMove(move);

            Assert.NotEqual(bitBoard.White, bitBoard2.White);
            Assert.NotEqual(bitBoard.Black, bitBoard2.Black);
            Assert.NotEqual(bitBoard.EnPassant, bitBoard2.EnPassant);

            bitBoard.UnMakeMove(move);

            Assert.Equal(bitBoard.White, bitBoard2.White);
            Assert.Equal(bitBoard.Black, bitBoard2.Black);
            Assert.Equal(bitBoard.EnPassant, bitBoard2.EnPassant);
        }

        [Fact]
        public void Make_WhiteCastleRemovesRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var move = MoveConstructor.CreateCastle(Colour.White, MoveType.CastleKing);

            bitBoard.MakeMove(move);

            Assert.False(bitBoard.WhiteCanCastleKingSide);
            Assert.False(bitBoard.WhiteCanCastleQueenSide);
            Assert.True(bitBoard.BlackCanCastleKingSide);
            Assert.True(bitBoard.BlackCanCastleQueenSide);
        }

        [Fact]
        public void Make_BlackCastleRemovesRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var move = MoveConstructor.CreateCastle(Colour.Black, MoveType.CastleKing);

            bitBoard.MakeMove(move);

            Assert.True(bitBoard.WhiteCanCastleKingSide);
            Assert.True(bitBoard.WhiteCanCastleQueenSide);
            Assert.False(bitBoard.BlackCanCastleKingSide);
            Assert.False(bitBoard.BlackCanCastleQueenSide);
        }

        [Fact]
        public void Make_WhiteStandardKingMoveRemovesRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var move = MoveConstructor.CreateMove(Colour.White, PieceType.King, SquareFlag.E1, SquareFlag.E2, PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.False(bitBoard.WhiteCanCastleKingSide);
            Assert.False(bitBoard.WhiteCanCastleQueenSide);
            Assert.True(bitBoard.BlackCanCastleKingSide);
            Assert.True(bitBoard.BlackCanCastleQueenSide);
        }

        [Fact]
        public void Make_BlackStandardKingMoveRemovesRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var move = MoveConstructor.CreateMove(Colour.Black, PieceType.King, SquareFlag.E8, SquareFlag.E7, PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.True(bitBoard.WhiteCanCastleKingSide);
            Assert.True(bitBoard.WhiteCanCastleQueenSide);
            Assert.False(bitBoard.BlackCanCastleKingSide);
            Assert.False(bitBoard.BlackCanCastleQueenSide);
        }

        [Fact]
        public void Make_WhiteStandardRookMoveRemovesQueenSideRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var move = MoveConstructor.CreateMove(Colour.White, PieceType.Rook, SquareFlag.A1, SquareFlag.A2, PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.True(bitBoard.WhiteCanCastleKingSide);
            Assert.False(bitBoard.WhiteCanCastleQueenSide);
            Assert.True(bitBoard.BlackCanCastleKingSide);
            Assert.True(bitBoard.BlackCanCastleQueenSide);
        }

        [Fact]
        public void Make_WhiteStandardRookMoveRemovesKingSideRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var move = MoveConstructor.CreateMove(Colour.White, PieceType.Rook, SquareFlag.H1, SquareFlag.H2, PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.False(bitBoard.WhiteCanCastleKingSide);
            Assert.True(bitBoard.WhiteCanCastleQueenSide);
            Assert.True(bitBoard.BlackCanCastleKingSide);
            Assert.True(bitBoard.BlackCanCastleQueenSide);
        }

        [Fact]
        public void Make_BlackStandardRookMoveRemovesQueenSideRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var move = MoveConstructor.CreateMove(Colour.Black, PieceType.Rook, SquareFlag.A8, SquareFlag.A7, PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.True(bitBoard.WhiteCanCastleKingSide);
            Assert.True(bitBoard.WhiteCanCastleQueenSide);
            Assert.True(bitBoard.BlackCanCastleKingSide);
            Assert.False(bitBoard.BlackCanCastleQueenSide);
        }

        [Fact]
        public void Make_BlackStandardRookMoveRemovesKingSideRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var move = MoveConstructor.CreateMove(Colour.Black, PieceType.Rook, SquareFlag.H8, SquareFlag.H7, PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.True(bitBoard.WhiteCanCastleKingSide);
            Assert.True(bitBoard.WhiteCanCastleQueenSide);
            Assert.False(bitBoard.BlackCanCastleKingSide);
            Assert.True(bitBoard.BlackCanCastleQueenSide);
        }

        private BitBoard Create(string fenString)
        {
            var fen = Fen.Parse(fenString);

            return BitBoard.FromFen(fen);
        }
    }
}
