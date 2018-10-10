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

        [Theory]
        [InlineData("8/4r3/3p4/k3P3/4K3/8/4R3/8 w - - 0 1", 1)]
        [InlineData("8/4r3/4k3/K3p3/3P4/8/4R3/8 b - - 0 1", 1)]
        public void PawnPinnedAndCannotCapture(string fenString, int expectedMoveCount)
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
