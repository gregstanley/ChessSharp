using ChessSharp.Enums;
using ChessSharp.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class CastleTests : MoveGeneratorTestsBase
    {
        public CastleTests(MoveGeneratorFixture moveGeneratorFixture)
            : base(moveGeneratorFixture)
        {
        }

        [Fact]
        public void Castle_BothSidesCreated()
        {
            var bitBoard = CreateBitBoard("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");

            var moves = new List<uint>(10);

            MoveGenerator.Generate(bitBoard, Colour.White, moves);

            var moveCount = moves.Count;

            var castleKing = moves.Where(x => x.GetMoveType() == MoveType.CastleKing);
            var castleQueen = moves.Where(x => x.GetMoveType() == MoveType.CastleQueen);

            var castleKingCount = castleKing.Count();
            var castleQueenCount = castleQueen.Count();

            Assert.Equal(1, castleKingCount);
            Assert.Equal(1, castleQueenCount);
        }

        [Theory]
        [InlineData("p7", 1, 1)]
        [InlineData("1p6", 1, 0)]
        [InlineData("2p5", 1, 0)]
        [InlineData("3p4", 0, 0)]
        [InlineData("4p3", 0, 0)]
        [InlineData("5p2", 0, 0)]
        [InlineData("6p1", 0, 1)]
        [InlineData("7p", 0, 1)]
        public void Castle_BlockedByPassingThroughCheck(string partialFen, int castleKingExpectedCount, int castleQueenExpectedCount)
        {
            var bitBoard = CreateBitBoard($"r3k2r/8/8/8/8/8/{partialFen}/R3K2R w KQkq - 0 1");

            var moves = new List<uint>(10);

            MoveGenerator.Generate(bitBoard, Colour.White, moves);

            var moveCount = moves.Count;

            var castleKing = moves.Where(x => x.GetMoveType() == MoveType.CastleKing);
            var castleQueen = moves.Where(x => x.GetMoveType() == MoveType.CastleQueen);

            var castleKingCount = castleKing.Count();
            var castleQueenCount = castleQueen.Count();

            Assert.Equal(castleKingExpectedCount, castleKingCount);
            Assert.Equal(castleQueenExpectedCount, castleQueenCount);
        }
    }
}
