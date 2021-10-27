using System.Collections.Generic;
using ChessSharp.Common.Helpers;
using ChessSharp.MoveGeneration;
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
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetPawnMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
        }

        [Theory]
        [InlineData("8/8/8/K7/1R2Pp1k/8/6P1/8 b - e3 0 1", 1)]
        [InlineData("8/8/8/K7/1R3pPk/8/4P3/8 b - g3 0 1", 1)]
        [InlineData("8/8/8/K7/1R3p1k/6P1/4P3/8 b - - 0 1", 0)]
        [InlineData("8/8/8/K7/1R3p1k/4P3/6P1/8 b - - 0 1", 0)]
        [InlineData("8/8/8/K7/1R2PpPk/8/8/8 b - e3 0 1", 2)]
        [InlineData("8/8/8/K7/1R2PpPk/8/8/8 b - g3 0 1", 2)]
        [InlineData("8/8/8/K7/1R2Pp1k/6P1/8/8 b - e3 0 1", 1)]
        [InlineData("8/8/8/K7/1R3pPk/4P3/8/8 b - g3 0 1", 2)]
        [InlineData("8/8/8/K7/4Pp2/1R5k/6P1/8 b - e3 0 1", 0)]
        [InlineData("8/8/8/K7/5pP1/1R5k/4P3/8 b - g3 0 1", 2)]
        [InlineData("8/8/8/K7/5p2/1R2P2k/6P1/8 b - - 0 1", 0)]
        [InlineData("8/8/8/K7/5p2/1R4Pk/4P3/8 b - - 0 1", 2)]
        [InlineData("8/8/8/K6k/1R2Pp2/8/6P1/8 b - e3 0 1", 2)]
        [InlineData("8/8/8/K6k/1R3pP1/8/4P3/8 b - g3 0 1", 1)]
        [InlineData("8/8/8/KR5k/4Pp2/8/6P1/8 b - e3 0 1", 0)]
        [InlineData("8/8/8/KR5k/5pP1/8/4P3/8 b - g3 0 1", 0)]
        [InlineData("8/8/8/8/KR2Ppk1/8/6P1/8 b - e3 0 1", 1)]
        public void Position3_Variants_EnPassantDiscovered(string fenString, int expectedMoveCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetPawnMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
        }

        [Theory]
        [InlineData("8/4r3/3p4/k3P3/4K3/8/4R3/8 w - - 0 1", 1)]
        [InlineData("8/4r3/4k3/K3p3/3P4/8/4R3/8 b - - 0 1", 1)]
        public void PawnPinnedByRookAndCannotCapture(string fenString, int expectedMoveCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetPawnMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
        }

        [Theory]
        [InlineData("8/8/8/2b5/3P4/8/5K2/k7 w - - 0 1", 1)]
        [InlineData("8/5k2/8/3p4/2B5/8/8/K7 b - - 0 1", 1)]
        public void PawnPinnedByBishopButCanCapture(string fenString, int expectedMoveCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetPawnMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
        }

        [Theory]
        [InlineData("8/8/8/k7/1r2pN1K/8/8/8 w - - 0 1", 8)]
        [InlineData("8/8/8/K7/1R2Pn1k/8/8/8 b - - 0 1", 8)]
        public void KnightNotActuallyPinned(string fenString, int expectedMoveCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetKnightMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
        }

        [Theory]
        [InlineData("8/8/5p2/k3K3/4R3/8/4r3/8 w - - 0 1", 0)]
        [InlineData("8/8/8/k3Kp2/4R3/8/4r3/8 w - - 0 1", 2)]
        [InlineData("8/8/8/k3K3/4Rp2/8/4r3/8 w - - 0 1", 2)]
        [InlineData("8/4R3/8/4r3/K3k3/5P2/8/8 b - - 0 1", 0)]
        [InlineData("8/4R3/8/4r3/K3kP2/8/8/8 b - - 0 1", 2)]
        [InlineData("8/4R3/8/4rP2/K3k3/8/8/8 b - - 0 1", 2)]
        public void RookPinned(string fenString, int expectedMoveCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetRookMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
        }

        [Theory]
        [InlineData("8/8/4k3/K3n3/8/5P2/4R3/8 w - - 0 1", 0)]
        [InlineData("8/8/4K3/k3N3/8/5p2/4r3/8 b - - 0 1", 0)]
        public void KnightPinned(string fenString, int expectedMoveCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetKnightMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
        }

        [Theory]
        [InlineData("8/8/4K3/k2B4/4p3/1q6/8/8 w - - 0 1", 2)]
        [InlineData("8/8/4k3/K2b4/4P3/1Q6/8/8 b - - 0 1", 2)]
        public void BishopPinned(string fenString, int expectedMoveCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetBishopMoveViews(moves);

            Assert.Equal(expectedMoveCount, moveViews.Count);
        }
    }
}
