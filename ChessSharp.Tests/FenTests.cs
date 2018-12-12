using ChessSharp.Enums;
using ChessSharp.Models;
using Xunit;

namespace ChessSharp.Tests
{
    public class FenTests
    {
        [Fact]
        public void Fen_Parse_Default_Correct()
        {
            var gameState = FenHelpers.Parse(FenHelpers.Default);

            Assert.Equal(SquareFlagConstants.r2, gameState.WhitePawns);
            Assert.Equal(SquareFlag.A1 | SquareFlag.H1, gameState.WhiteRooks);
            Assert.Equal(SquareFlag.B1 | SquareFlag.G1, gameState.WhiteKnights);
            Assert.Equal(SquareFlag.C1 | SquareFlag.F1, gameState.WhiteBishops);
            Assert.Equal(SquareFlag.D1, gameState.WhiteQueens);
            Assert.Equal(SquareFlag.E1, gameState.WhiteKing);

            Assert.Equal(SquareFlagConstants.r7, gameState.BlackPawns);
            Assert.Equal(SquareFlag.A8 | SquareFlag.H8, gameState.BlackRooks);
            Assert.Equal(SquareFlag.B8 | SquareFlag.G8, gameState.BlackKnights);
            Assert.Equal(SquareFlag.C8 | SquareFlag.F8, gameState.BlackBishops);
            Assert.Equal(SquareFlag.D8, gameState.BlackQueens);
            Assert.Equal(SquareFlag.E8, gameState.BlackKing);

            Assert.Equal(Colour.White, gameState.ToPlay);

            Assert.True(gameState.WhiteCanCastleKingSide);
            Assert.True(gameState.WhiteCanCastleQueenSide);
            Assert.True(gameState.BlackCanCastleKingSide);
            Assert.True(gameState.BlackCanCastleQueenSide);

            Assert.Equal((SquareFlag)0, gameState.EnPassant);
            Assert.Equal(0, gameState.HalfMoveClock);
            Assert.Equal(1, gameState.FullTurn);
        }

        [Fact]
        public void Fen_Parse_Position2_Correct()
        {
            var gameState = FenHelpers.Parse(FenHelpers.Position2);

            //Assert.Equal("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R", gameState.Board);

            Assert.Equal(SquareFlag.E1, gameState.WhiteKing);
            Assert.Equal(SquareFlag.E8, gameState.BlackKing);

            Assert.Equal(Colour.White, gameState.ToPlay);
            Assert.True(gameState.WhiteCanCastleKingSide);
            Assert.True(gameState.WhiteCanCastleQueenSide);
            Assert.True(gameState.BlackCanCastleKingSide);
            Assert.True(gameState.BlackCanCastleQueenSide);
            Assert.Equal((SquareFlag)0, gameState.EnPassant);
        }

        [Fact]
        public void Fen_Parse_Position5_Correct()
        {
            //"rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"
            var gameState = FenHelpers.Parse(FenHelpers.Position5);

            Assert.Equal(Colour.White, gameState.ToPlay);
            Assert.True( gameState.WhiteCanCastleKingSide);
            Assert.True( gameState.WhiteCanCastleQueenSide);
            Assert.False(gameState.BlackCanCastleKingSide);
            Assert.False(gameState.BlackCanCastleQueenSide);
            Assert.Equal((SquareFlag)0, gameState.EnPassant);
            Assert.Equal(1, gameState.HalfMoveClock);
            Assert.Equal(8, gameState.FullTurn);
        }

        [Theory]
        [InlineData("-", (SquareFlag)0)]
        [InlineData("a1", SquareFlag.A1)]
        [InlineData("e3", SquareFlag.E3)]
        [InlineData("h8", SquareFlag.H8)]
        public void Fen_EnPassant_Correct(string enPassantSquareFen, SquareFlag enPassantSquare)
        {
            var gameState = FenHelpers.Parse($"rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR w KQ {enPassantSquareFen} 1 8");

            Assert.Equal(enPassantSquare, gameState.EnPassant);
        }
    }
}
