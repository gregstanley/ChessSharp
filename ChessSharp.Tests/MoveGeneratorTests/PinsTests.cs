using System.Collections.Generic;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class PinsTests : MoveGeneratorTestsBase
    {
        public PinsTests(MoveGeneratorFixture moveGeneratorFixture)
            : base(moveGeneratorFixture)
        {
        }

        [Theory]
        [InlineData("8/8/8/k7/1r2pP1K/8/8/8 w - - 0 1", 1)]
        [InlineData("8/8/8/K7/1R2Pp1k/8/8/8 b - - 0 1", 1)]
        public void PawnNotActuallyPinned(string fenString, int expectedMoveCount)
        {
            var fen = Fen.Parse(fenString);

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);

            var moveViews = GetPawnMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
        }
    }
}
