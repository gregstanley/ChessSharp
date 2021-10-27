using System.Collections.Generic;
using System.Linq;
using ChessSharp.Common;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Extensions;
using ChessSharp.Common.Helpers;
using ChessSharp.MoveGeneration;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class QueenMoveTests : MoveGeneratorTestsBase
    {
        public QueenMoveTests(MoveGeneratorFixture moveGeneratorFixture)
            : base(moveGeneratorFixture)
        {
        }

        [Theory]
        [InlineData("1k6/7p/8/8/4Q3/8/8/2K5 w - -", SquareFlag.H7, 27)]
        [InlineData("1k6/7P/8/8/4q3/8/8/2K5 b - -", SquareFlag.H7, 27)]
        [InlineData("1k6/8/8/8/4Q2p/8/8/2K5 w - -", SquareFlag.H4, 27)]
        [InlineData("1k6/8/8/8/4q2P/8/8/2K5 b - -", SquareFlag.H4, 27)]
        [InlineData("1k6/8/8/8/4Q3/8/6p1/2K5 w - -", SquareFlag.G2, 26)]
        [InlineData("1k6/8/8/8/4q3/8/6P1/2K5 b - -", SquareFlag.G2, 26)]
        [InlineData("1k6/8/8/8/4Q3/8/4p3/2K5 w - -", SquareFlag.E2, 26)]
        [InlineData("1k6/8/8/8/4q3/8/4P3/2K5 b - -", SquareFlag.E2, 26)]
        [InlineData("1k6/8/8/8/4Q3/8/2p5/2K5 w - -", SquareFlag.C2, 26)]
        [InlineData("1k6/8/8/8/4q3/8/2P5/2K5 b - -", SquareFlag.C2, 26)]
        [InlineData("1k6/8/8/8/2p1Q3/8/8/2K5 w - -", SquareFlag.C4, 25)]
        [InlineData("1k6/8/8/8/2P1q3/8/8/2K5 b - -", SquareFlag.C4, 25)]
        [InlineData("1k6/1p6/8/8/4Q3/8/8/2K5 w - -", SquareFlag.B7, 26)]
        [InlineData("1k6/1P6/8/8/4q3/8/8/2K5 b - -", SquareFlag.B7, 26)]
        [InlineData("1k6/4p3/8/8/4Q3/8/8/2K5 w - -", SquareFlag.E7, 26)]
        [InlineData("1k6/4P3/8/8/4q3/8/8/2K5 b - -", SquareFlag.E7, 26)]
        public void E4_UnblockedCapture(string fenString, SquareFlag captureSquare, int expectedQueenMoveCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(fenString);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var queenMoves = GetQueenMoveViews(moves);

            var captures = GetCaptureMoveViews(moves);

            var capture1 = MoveBuilder.Create(gameState.ToPlay, PieceType.Queen, SquareFlag.E4.ToSquare(), captureSquare.ToSquare(), PieceType.Pawn, MoveType.Ordinary);

            Assert.Equal(expectedQueenMoveCount, queenMoves.Count);
            Assert.Contains(capture1, captures.Select(x => x.Value));
        }

        [Theory]
        [InlineData("8/8/8/8/8/8/5K1k/Q7 w - -", 21)]
        [InlineData("8/8/8/8/8/8/5K1k/q7 b - -", 21)]
        [InlineData("8/8/8/8/8/8/1Q6/5K1k w - -", 23)]
        [InlineData("8/8/8/8/8/8/1q6/5K1k b - -", 23)]
        [InlineData("8/8/8/8/8/2Q5/8/5K1k w - -", 25)]
        [InlineData("8/8/8/8/8/2q5/8/5K1k b - -", 25)]
        [InlineData("8/8/8/8/3Q4/8/8/5K1k w - -", 27)]
        [InlineData("8/8/8/8/3q4/8/8/5K1k b - -", 27)]
        [InlineData("8/8/8/4Q3/8/8/8/5K1k w - -", 27)]
        [InlineData("8/8/8/4q3/8/8/8/5K1k b - -", 27)]
        [InlineData("8/8/5Q2/8/8/8/8/4K2k w - -", 25)]
        [InlineData("8/8/5q2/8/8/8/8/4K2k b - -", 25)]
        [InlineData("8/6Q1/8/8/8/8/8/5K1k w - -", 23)]
        [InlineData("8/6q1/8/8/8/8/8/5K1k b - -", 23)]
        [InlineData("7Q/8/8/8/8/8/8/4K1k1 w - -", 21)]
        [InlineData("7q/8/8/8/8/8/8/4K1k1 b - -", 21)]
        public void A1_H8_0Capture(string fenString, int moveCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var movesViews = GetQueenMoveViews(moves);

            Assert.Equal(moveCount, movesViews.Count);
        }

        [Fact]
        public void B2_21Moves_0Captures_Correct()
        {
            var gameState = FenHelpers.Parse("8/8/8/k7/8/8/1Q6/4K3 w - -");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var movesViews = GetQueenMoveViews(moves);

            Assert.Equal(23, movesViews.Count);
        }

        [Fact]
        public void E4_16Moves_8Captures_Correct()
        {
            var gameState = FenHelpers.Parse("K7/8/2p1p1p1/8/2p1Q1p1/8/2p1p1p1/k7 w - -");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveCount = moves.Count;
            var captureViews = GetCaptureMoveViews(moves);

            var capture1 = MoveBuilder.Create(Colour.White, PieceType.Queen, SquareFlag.E4.ToSquare(), SquareFlag.C6.ToSquare(), PieceType.Pawn, MoveType.Ordinary);
            var capture2 = MoveBuilder.Create(Colour.White, PieceType.Queen, SquareFlag.E4.ToSquare(), SquareFlag.E6.ToSquare(), PieceType.Pawn, MoveType.Ordinary);
            var capture3 = MoveBuilder.Create(Colour.White, PieceType.Queen, SquareFlag.E4.ToSquare(), SquareFlag.G6.ToSquare(), PieceType.Pawn, MoveType.Ordinary);
            var capture4 = MoveBuilder.Create(Colour.White, PieceType.Queen, SquareFlag.E4.ToSquare(), SquareFlag.C4.ToSquare(), PieceType.Pawn, MoveType.Ordinary);
            var capture5 = MoveBuilder.Create(Colour.White, PieceType.Queen, SquareFlag.E4.ToSquare(), SquareFlag.G4.ToSquare(), PieceType.Pawn, MoveType.Ordinary);
            var capture6 = MoveBuilder.Create(Colour.White, PieceType.Queen, SquareFlag.E4.ToSquare(), SquareFlag.C2.ToSquare(), PieceType.Pawn, MoveType.Ordinary);
            var capture7 = MoveBuilder.Create(Colour.White, PieceType.Queen, SquareFlag.E4.ToSquare(), SquareFlag.E2.ToSquare(), PieceType.Pawn, MoveType.Ordinary);
            var capture8 = MoveBuilder.Create(Colour.White, PieceType.Queen, SquareFlag.E4.ToSquare(), SquareFlag.G2.ToSquare(), PieceType.Pawn, MoveType.Ordinary);

            var captures = captureViews.Select(x => x.Value);

            Assert.Contains(capture1, captures);
            Assert.Contains(capture2, captures);
            Assert.Contains(capture3, captures);
            Assert.Contains(capture4, captures);
            Assert.Contains(capture5, captures);
            Assert.Contains(capture6, captures);
            Assert.Contains(capture7, captures);
            Assert.Contains(capture8, captures);
        }
    }
}
