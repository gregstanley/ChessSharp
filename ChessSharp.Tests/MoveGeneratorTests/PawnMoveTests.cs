using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Helpers;
using ChessSharp.MoveGeneration;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class PawnMoveTests : MoveGeneratorTestsBase
    {
        public PawnMoveTests(MoveGeneratorFixture moveGeneratorFixture)
            : base(moveGeneratorFixture)
        {
        }

        [Theory]
        [InlineData("k6K/8/8/8/8/3P4/8/8 w - -", SquareFlag.D4)]
        [InlineData("k6K/8/8/8/8/3p4/8/8 b - -", SquareFlag.D2)]
        [InlineData("k6K/8/8/8/3P4/8/8/8 w - -", SquareFlag.D5)]
        [InlineData("k6K/8/8/8/3p4/8/8/8 b - -", SquareFlag.D3)]
        [InlineData("k6K/8/8/3P4/8/8/8/8 w - -", SquareFlag.D6)]
        [InlineData("k6K/8/8/3p4/8/8/8/8 b - -", SquareFlag.D4)]
        [InlineData("k6K/8/3P4/8/8/8/8/8 w - -", SquareFlag.D7)]
        [InlineData("k6K/8/3p4/8/8/8/8/8 b - -", SquareFlag.D5)]
        public void MidBoardOnlyOnePush(string fenString, SquareFlag toSquare)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var pawnMoveViews = GetPawnMoveViews(moves);

            var pawnMoves = pawnMoveViews.Select(x => x.Value);

            Assert.Collection(pawnMoves, x => Assert.Equal(toSquare, x.GetTo()));
        }

        [Theory]
        [InlineData("8/2b5/3P4/4K3/8/8/8/7k w - -", SquareFlag.D6, SquareFlag.D7)]
        [InlineData("8/2B5/3p4/4k3/8/8/8/7K b - -", SquareFlag.D6, SquareFlag.D5)]
        public void DiscoverCheck(string fenString, SquareFlag fromSquare, SquareFlag toSquare)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(20);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveCount = moves.Count;

            var illegalMove = MoveBuilder.Create(Colour.White, PieceType.Pawn, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            Assert.DoesNotContain(illegalMove, moves);
        }

        [Theory]
        [InlineData("8/8/8/1rP1K3/8/8/8/7k w - - 0 1")]
        [InlineData("8/8/2p5/1r1P1K2/8/8/8/7k w - - 0 1")]
        public void DiscoverCheckRook(string fenString)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(20);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetPawnMoveViews(moves);

            Assert.Equal(0, moveViews.Count);
        }

        [Theory]
        [InlineData("8/2p5/8/1P1p3r/KR2Pp1k/8/6P1/8 b - e3 0 1")]
        public void DiscoverCheckRook2(string fenString)
        {
            var gameState = FenHelpers.Parse(fenString);

            var board = CreateBoard(gameState);

            var moves = new List<uint>(20);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetPawnMoveViews(moves);

            Assert.Equal(5, moveViews.Count);
        }

        [Fact]
        public void White_EightPawns_OneAndTwoPushes()
        {
            var gameState = FenHelpers.Parse("K6k/8/8/8/8/8/PPPPPPPP/8 w - -");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var pawnMoves = GetPawnMoveViews(moves);

            var moveA3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.A2.ToSquare(), SquareFlag.A3.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveA4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.A2.ToSquare(), SquareFlag.A4.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveB3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.B2.ToSquare(), SquareFlag.B3.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveB4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.B2.ToSquare(), SquareFlag.B4.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveC3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.C2.ToSquare(), SquareFlag.C3.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveC4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.C2.ToSquare(), SquareFlag.C4.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveD3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.D2.ToSquare(), SquareFlag.D3.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveD4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.D2.ToSquare(), SquareFlag.D4.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveE3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.E2.ToSquare(), SquareFlag.E3.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveE4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.E2.ToSquare(), SquareFlag.E4.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveF3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.F2.ToSquare(), SquareFlag.F3.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveF4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.F2.ToSquare(), SquareFlag.F4.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveG3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.G2.ToSquare(), SquareFlag.G3.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveG4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.G2.ToSquare(), SquareFlag.G4.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveH3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.H2.ToSquare(), SquareFlag.H3.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveH4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.H2.ToSquare(), SquareFlag.H4.ToSquare(), PieceType.None, MoveType.Ordinary);

            Assert.Contains(moveA3, moves);
            Assert.Contains(moveA4, moves);
            Assert.Contains(moveB3, moves);
            Assert.Contains(moveB4, moves);
            Assert.Contains(moveC3, moves);
            Assert.Contains(moveC4, moves);
            Assert.Contains(moveD3, moves);
            Assert.Contains(moveD4, moves);
            Assert.Contains(moveE3, moves);
            Assert.Contains(moveE4, moves);
            Assert.Contains(moveF3, moves);
            Assert.Contains(moveF4, moves);
            Assert.Contains(moveG3, moves);
            Assert.Contains(moveG4, moves);
            Assert.Contains(moveH3, moves);
            Assert.Contains(moveH4, moves);
        }

        [Fact]
        public void Black_EightPawns_OneAndTwoPushes()
        {
            var gameState = FenHelpers.Parse("K6k/pppppppp/8/8/8/8/8/8 b - -");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            // Purely for debugging
            var pawnMoves = GetPawnMoveViews(moves);

            var moveA3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.A7.ToSquare(), SquareFlag.A6.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveA4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.A7.ToSquare(), SquareFlag.A5.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveB3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.B7.ToSquare(), SquareFlag.B6.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveB4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.B7.ToSquare(), SquareFlag.B5.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveC3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.C7.ToSquare(), SquareFlag.C6.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveC4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.C7.ToSquare(), SquareFlag.C5.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveD3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D6.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveD4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D5.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveE3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.E7.ToSquare(), SquareFlag.E6.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveE4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.E7.ToSquare(), SquareFlag.E5.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveF3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.F7.ToSquare(), SquareFlag.F6.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveF4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.F7.ToSquare(), SquareFlag.F5.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveG3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.G7.ToSquare(), SquareFlag.G6.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveG4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.G7.ToSquare(), SquareFlag.G5.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveH3 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.H7.ToSquare(), SquareFlag.H6.ToSquare(), PieceType.None, MoveType.Ordinary);
            var moveH4 = MoveBuilder.Create(gameState.ToPlay, PieceType.Pawn, SquareFlag.H7.ToSquare(), SquareFlag.H5.ToSquare(), PieceType.None, MoveType.Ordinary);

            Assert.Contains(moveA3, moves);
            Assert.Contains(moveA4, moves);
            Assert.Contains(moveB3, moves);
            Assert.Contains(moveB4, moves);
            Assert.Contains(moveC3, moves);
            Assert.Contains(moveC4, moves);
            Assert.Contains(moveD3, moves);
            Assert.Contains(moveD4, moves);
            Assert.Contains(moveE3, moves);
            Assert.Contains(moveE4, moves);
            Assert.Contains(moveF3, moves);
            Assert.Contains(moveF4, moves);
            Assert.Contains(moveG3, moves);
            Assert.Contains(moveG4, moves);
            Assert.Contains(moveH3, moves);
            Assert.Contains(moveH4, moves);
        }

        [Fact]
        public void White_FourBlocked_NoMoves_Correct()
        {
            var gameState = FenHelpers.Parse("K6k/8/8/8/8/1p1p1p1p/1P1P1P1P/8 b - -");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetPawnMoveViews(moves);

            Assert.Empty(moveViews);
        }

        [Fact]
        public void Black_FourBlocked_NoMoves_Correct()
        {
            var gameState = FenHelpers.Parse("K6k/1p1p1p1p/1P1P1P1P/8/8/8/8/8 b - -");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var moveViews = GetPawnMoveViews(moves);

            Assert.Empty(moveViews);
        }

        [Fact]
        public void White_FourCaptures_OnePush_Correct()
        {
            var gameState = FenHelpers.Parse("K6k/8/8/8/8/p1p2p1p/1P4P1/8 w - -");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var move1 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.B2.ToSquare(), SquareFlag.B3.ToSquare(), PieceType.None, MoveType.Ordinary);
            var move2 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.G2.ToSquare(), SquareFlag.G3.ToSquare(), PieceType.None, MoveType.Ordinary);

            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.B2.ToSquare(), SquareFlag.A3.ToSquare(), PieceType.Pawn, MoveType.Ordinary);
            var capture2 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.B2.ToSquare(), SquareFlag.C3.ToSquare(), PieceType.Pawn, MoveType.Ordinary);
            var capture3 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.G2.ToSquare(), SquareFlag.F3.ToSquare(), PieceType.Pawn, MoveType.Ordinary);
            var capture4 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.G2.ToSquare(), SquareFlag.H3.ToSquare(), PieceType.Pawn, MoveType.Ordinary);

            Assert.Contains(move1, moves);
            Assert.Contains(move2, moves);

            Assert.Contains(capture1, moves);
            Assert.Contains(capture2, moves);
            Assert.Contains(capture3, moves);
            Assert.Contains(capture4, moves);
        }

        [Fact]
        public void White_Empty_OnePush_Promotion()
        {
            var gameState = FenHelpers.Parse("K6k/3P4/8/8/8/8/8/8 w - -");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var promotion1 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D8.ToSquare(), PieceType.None, MoveType.PromotionQueen);
            var promotion2 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D8.ToSquare(), PieceType.None, MoveType.PromotionRook);
            var promotion3 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D8.ToSquare(), PieceType.None, MoveType.PromotionBishop);
            var promotion4 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D8.ToSquare(), PieceType.None, MoveType.PromotionKnight);

            Assert.Contains(promotion1, moves);
            Assert.Contains(promotion2, moves);
            Assert.Contains(promotion3, moves);
            Assert.Contains(promotion4, moves);
        }

        [Fact]
        public void White_Capture_OneCapture_Promotion_Correct()
        {
            var gameState = FenHelpers.Parse("3nn3/3P4/8/8/8/8/8/K6k w - -");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var promotion1 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.E8.ToSquare(), PieceType.Knight, MoveType.PromotionQueen);
            var promotion2 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.E8.ToSquare(), PieceType.Knight, MoveType.PromotionRook);
            var promotion3 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.E8.ToSquare(), PieceType.Knight, MoveType.PromotionBishop);
            var promotion4 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.E8.ToSquare(), PieceType.Knight, MoveType.PromotionKnight);

            Assert.Contains(promotion1, moves);
            Assert.Contains(promotion2, moves);
            Assert.Contains(promotion3, moves);
            Assert.Contains(promotion4, moves);
        }

        [Fact]
        public void Black_OneCapture_Promotion_Correct()
        {
            var gameState = FenHelpers.Parse("4k3/8/8/8/8/8/1p6/R2QK3 b - -");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var promotion1 = MoveBuilder.Create(Colour.Black, PieceType.Pawn, SquareFlag.B2.ToSquare(), SquareFlag.A1.ToSquare(), PieceType.Rook, MoveType.PromotionQueen);
            var promotion2 = MoveBuilder.Create(Colour.Black, PieceType.Pawn, SquareFlag.B2.ToSquare(), SquareFlag.A1.ToSquare(), PieceType.Rook, MoveType.PromotionRook);
            var promotion3 = MoveBuilder.Create(Colour.Black, PieceType.Pawn, SquareFlag.B2.ToSquare(), SquareFlag.A1.ToSquare(), PieceType.Rook, MoveType.PromotionBishop);
            var promotion4 = MoveBuilder.Create(Colour.Black, PieceType.Pawn, SquareFlag.B2.ToSquare(), SquareFlag.A1.ToSquare(), PieceType.Rook, MoveType.PromotionKnight);

            var pawnMoves = GetPawnMoveViews(moves);

            Assert.Contains(promotion1, moves);
            Assert.Contains(promotion2, moves);
            Assert.Contains(promotion3, moves);
            Assert.Contains(promotion4, moves);
        }

        [Fact]
        public void White_Capture()
        {
            var gameState = FenHelpers.Parse("K6k/8/8/8/8/3p4/4P3/8 w - -");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var capture = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.E2.ToSquare(), SquareFlag.D3.ToSquare(), PieceType.Pawn, MoveType.Ordinary);

            var movesView = moves.Select(x => new MoveViewer(x));

            Assert.Contains(capture, moves);
        }

        [Fact]
        public void White_EnPassant_Capture()
        {
            var gameState = FenHelpers.Parse("K6k/8/8/3Pp3/8/8/8/8 w - e6");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var enPassantCapture = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D5.ToSquare(), SquareFlag.E6.ToSquare(), PieceType.Pawn, MoveType.EnPassant);

            var movesView = moves.Select(x => new MoveViewer(x));

            Assert.Contains(enPassantCapture, moves);
        }

        [Fact]
        public void White_EnPassant_Capture_DiscoveredCheck()
        {
            var gameState = FenHelpers.Parse("8/8/8/q1rPp2K/8/7p/8/8 w - e6");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var enPassantCapture = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D6.ToSquare(), SquareFlag.E7.ToSquare(), PieceType.Pawn, MoveType.EnPassant);

            var movesView = moves.Select(x => new MoveViewer(x));

            Assert.DoesNotContain(enPassantCapture, moves);
        }

        [Fact]
        public void White_DiscoveredCheck()
        {
            var gameState = FenHelpers.Parse("8/2b5/3P4/4K3/8/8/8/7k w - -");

            var board = CreateBoard(gameState);

            var moves = new List<uint>(10);

            MoveGenerator.Generate(board, gameState.ToPlay, moves);

            var illegalMove = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D6.ToSquare(), SquareFlag.D7.ToSquare(), PieceType.None, MoveType.Ordinary);

            var movesView = moves.Select(x => new MoveViewer(x));

            Assert.DoesNotContain(illegalMove, moves);
        }
    }
}
