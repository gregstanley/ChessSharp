using System.Collections.Generic;
using ChessSharp.Common.Helpers;
using ChessSharp.MoveGeneration;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class BishopMoveTests : MoveGeneratorTestsBase
    {
        public BishopMoveTests(MoveGeneratorFixture moveGeneratorFixture)
            : base(moveGeneratorFixture)
        {
        }

        [Theory]
        [InlineData("7b/1k6/8/8/8/8/6K1/B7 w - - 0 1", 7)]
        [InlineData("7b/1k6/8/8/8/8/6K1/B7 b - - 0 1", 7)]
        public void CanCaptureCorners(string fenString, int expectedMoveCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetBishopMoveViews(moves);

            var captureViews = GetCaptureMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
            Assert.Equal(1, captureViews.Count);
        }

        [Theory]
        [InlineData("8/8/8/3R1N1k/4B3/3Q1P1K/8/8 w - -", 0)]
        [InlineData("8/8/8/3r1n1K/4b3/3q1p1k/8/8 b - -", 0)]
        [InlineData("8/8/2R3N1/7k/4B3/7K/2Q3P1/8 w - -", 4)]
        [InlineData("8/8/2r3n1/7K/4b3/7k/2q3p1/8 b - -", 4)]
        [InlineData("8/1R5P/8/7k/4B3/7K/8/1Q5R w - -", 8)]
        [InlineData("8/1r5p/8/7K/4b3/7k/8/1q5r b - -", 8)]
        public void AllDirectionsBlocked(string fenString, int expectedMoveCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetBishopMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
        }

        [Theory]
        [InlineData("b7/1r6/2p5/3p4/4p3/5p2/6p1/k3K2B w - -")]
        [InlineData("B7/1R6/2P5/3P4/4P3/5P2/6P1/K3k2b b - -")]
        [InlineData("7b/6r1/5p2/4p3/3p4/2p5/1p6/B2K1k2 w - -")]
        [InlineData("7B/6R1/5P2/4P3/3P4/2P5/1P6/b2k1K2 b - -")]
        [InlineData("B7/1p6/2p5/3p4/4p3/5p2/6r1/2K2k1b w - -")]
        [InlineData("b7/1P6/2P5/3P4/4P3/5P2/6R1/2k2K1B b - -")]
        [InlineData("7B/6p1/5p2/4p3/3p4/2p5/1r6/b2K1k2 w - -")]
        [InlineData("7b/6P1/5P2/4P3/3P4/2P5/1R6/B2k1K2 b - -")]
        public void LargeOccupancyOneCapture(string fenString)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetBishopMoveViews(moves);

            var captureViews = GetCaptureMoveViews(moves);

            Assert.Equal(1, moveViews.Count);
            Assert.Equal(1, captureViews.Count);
        }
    }
}
