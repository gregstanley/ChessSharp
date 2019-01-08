using ChessSharp.Enums;
using ChessSharp.Extensions;
using System;
using Xunit;

namespace ChessSharp.Tests
{
    public class BoardStateExtensionsTests
    {
        [Theory]
        [InlineData(StateFlag.WhiteCanCastleKingSide)]
        [InlineData(StateFlag.WhiteCanCastleQueenSide)]
        [InlineData(StateFlag.BlackCanCastleKingSide)]
        [InlineData(StateFlag.BlackCanCastleQueenSide)]
        public void GetEnPassantSquare_Correct(StateFlag boardState)
        {
            var retrievedSquare = boardState.GetEnPassantSquare();

            Assert.Equal((SquareFlag)0, retrievedSquare);
        }

        [Theory]
        [InlineData(StateFlag.WhiteCanCastleKingSide | StateFlag.WhiteCanCastleQueenSide, SquareFlag.A1)]
        [InlineData(StateFlag.BlackCanCastleKingSide | StateFlag.BlackCanCastleQueenSide, SquareFlag.E4)]
        [InlineData(StateFlag.WhiteCanCastleKingSide | StateFlag.WhiteCanCastleQueenSide
                | StateFlag.BlackCanCastleKingSide | StateFlag.BlackCanCastleQueenSide, SquareFlag.H8)]
        public void AddEnPassantSquare_Correct(StateFlag boardState, SquareFlag square)
        {
            boardState = boardState.AddEnPassantSquare(square);

            var retrievedSquare = boardState.GetEnPassantSquare();

            Assert.Equal(square, retrievedSquare);
        }

        [Theory]
        [InlineData((StateFlag)0, (SquareFlag)0)]
        [InlineData(StateFlag.WhiteCanCastleKingSide, (SquareFlag)0)]
        public void AddEnPassantSquare_Zero_ThrowsException(StateFlag boardState, SquareFlag square)
        {
            Assert.Throws<IndexOutOfRangeException>(() => boardState.AddEnPassantSquare(square));
        }
    }
}
