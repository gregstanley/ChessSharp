using ChessSharp.Enums;
using ChessSharp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class YieldTests : MoveGeneratorTestsBase
    {
        public YieldTests(MoveGeneratorFixture moveGeneratorFixture)
            : base(moveGeneratorFixture)
        {
        }

        [Fact]
        public void GenerateSameAsYield()
        {
            var gameState = FenHelpers.Parse("k7/2Q5/8/4p3/3K1P2/2q5/8/8 w - - 0 1");

            var bitBoard = CreateBitBoard(gameState);

            var moves1 = new List<uint>(64);

            MoveGenerator.Generate(0, bitBoard, Colour.White, moves1);

            var moves2 = new List<uint>(64);

            foreach (var move in MoveGenerator.GenerateStream(0, bitBoard, Colour.White))
                moves2.Add(move);

            var moveViews1 = moves1.Select(x => new MoveViewer(x));
            var moveViews2 = moves2.Select(x => new MoveViewer(x));

            Assert.Equal(moves1.Count, moves2.Count);
        }
    }
}
