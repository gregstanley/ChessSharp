using ChessSharp.Models;
using System.Collections.Generic;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class BishopMoveTests : MoveGeneratorTestsBase
    {
        public BishopMoveTests(MoveGeneratorFixture moveGeneratorFixture)
            : base(moveGeneratorFixture)
        {
        }

        [Fact]
        public void Bishop_Blocked_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/8/8/8/8/P1P5/1B6/P1P5 w KQkq -");

            var moves = new List<uint>(10);

            MoveGenerator.AddBishopMoves(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Bishop_4Moves_0Captures_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/8/2R3N1/7k/4B3/7K/2Q3P1/8 w KQkq -");

            var moves = new List<uint>(10);

            MoveGenerator.AddBishopMoves(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            Assert.Equal(4, moves.Count);
        }

        [Fact]
        public void Bishop_LargeOccupancy()
        {
            var fen = Fen.Parse("8/2p5/3p4/4p3/5p2/6p1/7B/k4K2 w - -");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);

            var bishopMoves = GetBishopMoveViews(moves);

            Assert.Equal(2, bishopMoves.Count);
        }
    }
}
