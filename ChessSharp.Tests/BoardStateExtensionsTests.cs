using ChessSharp.Enums;
using ChessSharp.Extensions;
using Xunit;

namespace ChessSharp.Tests
{
    public class BoardStateExtensionsTests
    {
        [Theory]
        [InlineData(BoardState.WhiteCanCastleKingSide)]
        [InlineData(BoardState.WhiteCanCastleQueenSide)]
        [InlineData(BoardState.BlackCanCastleKingSide)]
        [InlineData(BoardState.BlackCanCastleQueenSide)]
        public void Get_EnPassantSquare_Correct(BoardState boardState)
        {
            var retrievedSquare = boardState.GetEnPassantSquare();

            Assert.Equal((SquareFlag)0, retrievedSquare);
        }

        [Theory]
        [InlineData((BoardState)0, (SquareFlag)0)]
        [InlineData(BoardState.WhiteCanCastleKingSide, (SquareFlag)0)]
        [InlineData(BoardState.WhiteCanCastleKingSide | BoardState.WhiteCanCastleQueenSide, SquareFlag.A1)]
        [InlineData(BoardState.BlackCanCastleKingSide | BoardState.BlackCanCastleQueenSide, SquareFlag.E4)]
        [InlineData(BoardState.WhiteCanCastleKingSide | BoardState.WhiteCanCastleQueenSide
                | BoardState.BlackCanCastleKingSide | BoardState.BlackCanCastleQueenSide, SquareFlag.H8)]
        public void AddGet_EnPassantSquare_Correct(BoardState boardState, SquareFlag square)
        {
            boardState = boardState.AddEnPassantSquare(square);

            var retrievedSquare = boardState.GetEnPassantSquare();

            Assert.Equal(square, retrievedSquare);
        }
    }
}
