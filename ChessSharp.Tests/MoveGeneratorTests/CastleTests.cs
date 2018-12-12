using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.MoveGeneration;
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

            var workspace = new MoveGenerationWorkspace(bitBoard, Colour.White);

            MoveGenerator.Generate(workspace, moves);

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

            var workspace = new MoveGenerationWorkspace(bitBoard, Colour.White);

            MoveGenerator.Generate(workspace, moves);

            var moveCount = moves.Count;

            var castleKing = moves.Where(x => x.GetMoveType() == MoveType.CastleKing);
            var castleQueen = moves.Where(x => x.GetMoveType() == MoveType.CastleQueen);

            var castleKingCount = castleKing.Count();
            var castleQueenCount = castleQueen.Count();

            Assert.Equal(castleKingExpectedCount, castleKingCount);
            Assert.Equal(castleQueenExpectedCount, castleQueenCount);
        }

        // Peter Ellis and Perfect Perft //--Castle Rights
        [Theory]
        [InlineData("r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq -", 1, 1)]
        [InlineData("r3k2r/1b4bq/8/8/8/8/7B/2KR3R b kq -", 1, 0)]
        [InlineData("r3k2r/1b4bq/8/8/8/8/7B/R4RK1 b kq -", 0, 1)]
        [InlineData("r3k2r/1b4b1/8/8/4q3/8/7B/R3K2R w KQkq -", 0, 0)]
        [InlineData("r3k2r/1b4bq/3B4/8/8/8/8/R3K2R b KQkq -", 0, 1)]
        [InlineData("r3k2r/1b4b1/3B4/8/8/8/8/Rq2K2R w KQkq -", 0, 0)]
        [InlineData("R3k2r/1b4bq/8/8/8/8/7B/4K2R b Kk -", 0, 0)]
        public void PeterEllis_Variations(string fenString, int castleKingExpectedCount, int castleQueenExpectedCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var bitBoard = CreateBitBoard(gameState);

            var moves = new List<uint>(10);

            var workspace = new MoveGenerationWorkspace(bitBoard, gameState.ToPlay);

            MoveGenerator.Generate(workspace, moves);

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
