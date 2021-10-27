using ChessSharp.Common;
using Xunit;

namespace ChessSharp.Tests
{
    public static class TestHelpers
    {
        public static void AssertEqual(Board a, Board b)
        {
            Assert.Equal(a.WhitePawns, b.WhitePawns);
            Assert.Equal(a.WhiteRooks, b.WhiteRooks);
            Assert.Equal(a.WhiteKnights, b.WhiteKnights);
            Assert.Equal(a.WhiteBishops, b.WhiteBishops);
            Assert.Equal(a.WhiteQueens, b.WhiteQueens);
            Assert.Equal(a.WhiteKing, b.WhiteKing);
            Assert.Equal(a.BlackPawns, b.BlackPawns);
            Assert.Equal(a.BlackRooks, b.BlackRooks);
            Assert.Equal(a.BlackKnights, b.BlackKnights);
            Assert.Equal(a.BlackBishops, b.BlackBishops);
            Assert.Equal(a.BlackQueens, b.BlackQueens);
            Assert.Equal(a.BlackKing, b.BlackKing);
            Assert.Equal(a.EnPassant, b.EnPassant);
            Assert.Equal(a.WhiteCanCastleKingSide, b.WhiteCanCastleKingSide);
            Assert.Equal(a.WhiteCanCastleQueenSide, b.WhiteCanCastleQueenSide);
            Assert.Equal(a.BlackCanCastleKingSide, b.BlackCanCastleKingSide);
            Assert.Equal(a.BlackCanCastleQueenSide, b.BlackCanCastleQueenSide);
        }
    }
}
