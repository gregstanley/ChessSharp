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
    public class KingMoveTests : MoveGeneratorTestsBase
    {
        public KingMoveTests(MoveGeneratorFixture moveGeneratorFixture)
            : base(moveGeneratorFixture)
        {
        }

        [Theory]
        [InlineData("K7/8/8/8/8/8/8/7k w - -", 3)]
        [InlineData("k7/8/8/8/8/8/8/7K b - -", 3)]
        public void FromCorners(string fenString, int expectedMovesCount)
        {
            // This is mainly checking the pawn check magic bitboards as it uses them
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(20);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveCount = moves.Count;

            Assert.Equal(expectedMovesCount, moveCount);
        }

        [Theory]
        [InlineData("8/8/8/8/4K3/8/8/8 w - -", 8)]
        [InlineData("8/8/8/8/4k3/8/8/8 b - -", 8)]
        [InlineData("8/8/8/8/3PK3/8/8/8 w - -", 7)]
        [InlineData("8/8/8/8/3pk3/8/8/8 b - -", 7)]
        [InlineData("8/8/8/8/3PKP2/8/8/8 w - -", 6)]
        [InlineData("8/8/8/8/3pkp2/8/8/8 b - -", 6)]
        [InlineData("8/8/8/3P4/3PKP2/8/8/8 w - -", 5)]
        [InlineData("8/8/8/3p4/3pkp2/8/8/8 b - -", 5)]
        [InlineData("8/8/8/3PP3/3PKP2/8/8/8 w - -", 4)]
        [InlineData("8/8/8/3pp3/3pkp2/8/8/8 b - -", 4)]
        [InlineData("8/8/8/3PPP2/3PKP2/8/8/8 w - -", 3)]
        [InlineData("8/8/8/3ppp2/3pkp2/8/8/8 b - -", 3)]
        [InlineData("8/8/8/3PPP2/3PKP2/3P4/8/8 w - -", 2)]
        [InlineData("8/8/8/3ppp2/3pkp2/3p4/8/8 b - -", 2)]
        [InlineData("8/8/8/3PPP2/3PKP2/3PP3/8/8 w - -", 1)]
        [InlineData("8/8/8/3ppp2/3pkp2/3pp3/8/8 b - -", 1)]
        [InlineData("8/8/8/3PPP2/3PKP2/3PPP2/8/8 w - -", 0)]
        [InlineData("8/8/8/3ppp2/3pkp2/3ppp2/8/8 b - -", 0)]
        public void BlockCombinations(string fenString, int expectedMovesCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(20);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var kingMoves = GetKingMoveViews(moves);

            Assert.Equal(expectedMovesCount, kingMoves.Count);
        }

        [Theory]
        [InlineData("8/8/8/r7/4K3/8/8/8 w - -", 5)]
        [InlineData("8/8/8/R7/4k3/8/8/8 b - -", 5)]
        [InlineData("8/8/8/8/4K3/7r/8/8 w - -", 5)]
        [InlineData("8/8/8/8/4k3/7R/8/8 b - -", 5)]
        [InlineData("3r4/8/8/8/4K3/8/8/8 w - -", 5)]
        [InlineData("3R4/8/8/8/4k3/8/8/8 b - -", 5)]
        [InlineData("8/8/8/8/4K3/8/8/3r4 w - -", 5)]
        [InlineData("8/8/8/8/4k3/8/8/3R4 b - -", 5)]
        public void AvoidsMovingIntoNonDiagonalCheck(string fenString, int expectedMovesCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(20);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var kingMoves = GetKingMoveViews(moves);

            Assert.Equal(expectedMovesCount, kingMoves.Count);
        }

        [Theory]
        [InlineData("8/8/8/4r3/4K3/8/8/8 w - -", 5, SquareFlag.E5)]
        [InlineData("8/8/8/4R3/4k3/8/8/8 b - -", 5, SquareFlag.E5)]
        [InlineData("8/8/8/8/3rK3/8/8/8 w - -", 5, SquareFlag.D4)]
        [InlineData("8/8/8/8/3Rk3/8/8/8 b - -", 5, SquareFlag.D4)]
        public void EvadesCheckByRook(string fenString, int expectedMovesCount, SquareFlag toSquare)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(20);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var kingMoves = GetKingMoveViews(moves);

            var captureViews = GetCaptureMoveViews(moves);

            var capture1 = MoveBuilder.Create(gameState.ToPlay, PieceType.King, SquareFlag.E4.ToSquare(), toSquare.ToSquare(), PieceType.Rook, MoveType.Ordinary, 1);

            Assert.Equal(expectedMovesCount, kingMoves.Count);

            Assert.Contains(capture1, captureViews.Select(x => x.Value));
        }

        [Theory]
        [InlineData("8/8/8/5p2/3PKP2/8/8/8 w - -", 6, SquareFlag.E4, SquareFlag.F5)]
        [InlineData("8/8/8/3pkp2/5P2/8/8/8 b - -", 6, SquareFlag.E5, SquareFlag.F4)]
        [InlineData("8/8/8/3p4/3PKP2/8/8/8 w - -", 6, SquareFlag.E4, SquareFlag.D5)]
        [InlineData("8/8/8/3pkp2/3P4/8/8/8 b - -", 6, SquareFlag.E5, SquareFlag.D4)]
        public void EvadesCheckByPawn(string fenString, int expectedMovesCount, SquareFlag fromSquare, SquareFlag toSquare)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(20);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveCount = moves.Count;

            var captureViews = GetCaptureMoveViews(moves);

            var capture1 = MoveBuilder.Create(gameState.ToPlay, PieceType.King, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.Pawn, MoveType.Ordinary, 1);

            Assert.Equal(expectedMovesCount, moveCount);

            Assert.Contains(capture1, captureViews.Select(x => x.Value));
        }

        [Theory]
        [InlineData("8/8/3n4/8/3PKP2/8/8/8 w - -", 5)]
        [InlineData("8/8/3N4/8/3pkp2/8/8/8 b - -", 5)]
        public void EvadesCheckByKnight(string fenString, int expectedMovesCount)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(20);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveCount = moves.Count;

            Assert.Equal(expectedMovesCount, moveCount);
        }

        [Fact]
        public void EvadesSingleCheck()
        {
            var gameState = FenHelpers.Parse("k7/2Q5/8/4p3/3K1P2/8/8/8 w - - 0 1");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(20);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveCount = moves.Count;

            Assert.Equal(10, moveCount);

            Assert.Equal(1, moves.First().GetNumCheckers());
        }

        [Fact]
        public void EvadesDoubleCheck()
        {
            var gameState = FenHelpers.Parse("k7/2Q5/8/4p3/3K1P2/2q5/8/8 w - - 0 1");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(20);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveCount = moves.Count;

            Assert.Equal(3, moveCount);

            Assert.Equal(2, moves.First().GetNumCheckers());
        }
    }
}
