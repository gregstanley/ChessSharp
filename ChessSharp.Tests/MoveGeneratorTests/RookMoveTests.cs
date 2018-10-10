using System.Collections.Generic;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class RookMoveTests : MoveGeneratorTestsBase
    {
        public RookMoveTests(MoveGeneratorFixture moveGeneratorFixture)
            : base(moveGeneratorFixture)
        {
        }

        [Theory]
        [InlineData("k7/8/3n4/8/1r1R1b2/8/3q4/7K w - - 0 1", 8)]
        [InlineData("k7/8/3N4/8/1R1r1B2/8/3Q4/7K b - - 0 1", 8)]
        public void CanCapture4Directions(string fenString, int expectedMoveCount)
        {
            var fen = Fen.Parse(fenString);

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);

            var moveViews = GetRookMoveViews(moves);

            var captureViews = GetCaptureMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
            Assert.Equal(4, captureViews.Count);
        }

        [Theory]
        [InlineData("7n/1k6/8/8/8/8/6K1/r6R w - - 0 1", 14)]
        [InlineData("7N/1k6/8/8/8/8/6K1/R6r b - - 0 1", 14)]
        public void CanCaptureCorners(string fenString, int expectedMoveCount)
        {
            var fen = Fen.Parse(fenString);

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);

            var moveViews = GetRookMoveViews(moves);

            var captureViews = GetCaptureMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
            Assert.Equal(2, captureViews.Count);
        }

        [Theory]
        [InlineData("8/8/4N3/3PRP2/4Q3/8/k6K/8 w - -", 0)]
        [InlineData("8/8/4N3/3PRP2/4Q3/8/k6K/8 b - -", 0)]
        [InlineData("8/4N3/8/2P1R1P1/8/4Q3/k6K/8 w - -", 4)]
        [InlineData("8/4n3/8/2p1r1p1/8/4q3/k6K/8 b - -", 4)]
        [InlineData("4N3/8/8/1P2R2P/8/8/k3Q2K/8 w - -", 8)]
        [InlineData("4n3/8/8/1p2r2p/8/8/k3q2K/8 b - -", 8)]
        public void AllDirectionsBlocked(string fenString, int expectedMoveCount)
        {
            var fen = Fen.Parse(fenString);

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);

            var moveViews = GetRookMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
        }

        [Theory]
        [InlineData("4r3/4P3/4p3/4p3/k3p2K/4p3/4p3/4R3 w - -", 8)]
        [InlineData("4r3/4P3/4p3/4p3/k3p2K/4p3/4p3/4R3 b - -", 8)]
        [InlineData("8/2k2K2/8/8/RpppppPr/8/8/8 w - -", 8)]
        [InlineData("8/2k2K2/8/8/RpppppPr/8/8/8 b - -", 8)]
        public void LargeOccupancyOneCapture(string fenString, int expectedMoveCount)
        {
            var fen = Fen.Parse(fenString);

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);

            var moveViews = GetRookMoveViews(moves);

            var captureViews = GetCaptureMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
            Assert.Equal(1, captureViews.Count);
        }
    }
}
