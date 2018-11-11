using ChessSharp.Enums;
using ChessSharp.Extensions;
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
            var fen = Fen.Parse(fenString);

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var pawnMoveViews = GetPawnMoveViews(moves);

            var pawnMoves = pawnMoveViews.Select(x => x.Value);

            Assert.Collection(pawnMoves, x => Assert.Equal(toSquare, x.GetTo()));
        }

        [Theory]
        [InlineData("8/2b5/3P4/4K3/8/8/8/7k w - -", SquareFlag.D6, SquareFlag.D7)]
        //[InlineData("8/2B5/3p4/4k3/8/8/8/7K b - -", SquareFlag.D6, SquareFlag.D5)]
        public void DiscoverCheck(string fenString, SquareFlag fromSquare, SquareFlag toSquare)
        {
            var fen = Fen.Parse(fenString);

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(20);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var moveCount = moves.Count;

            var illegalMove = MoveBuilder.Create(Colour.White, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary);

            Assert.DoesNotContain(illegalMove, moves);
        }

        [Theory]
        [InlineData("8/8/8/1rP1K3/8/8/8/7k w - - 0 1", SquareFlag.D6, SquareFlag.D7)]
        [InlineData("8/8/2p5/1r1P1K2/8/8/8/7k w - - 0 1", SquareFlag.D6, SquareFlag.D7)]
        public void DiscoverCheckRook(string fenString, SquareFlag fromSquare, SquareFlag toSquare)
        {
            var fen = Fen.Parse(fenString);

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(20);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var moveViews = GetPawnMoveViews(moves);

            //var moveCount = moves.Count;

            //var illegalMove = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary);

            //Assert.DoesNotContain(illegalMove, moves);
            Assert.Equal(0, moveViews.Count);
        }

        [Theory]
        [InlineData("8/2p5/8/1P1p3r/KR2Pp1k/8/6P1/8 b - e3 0 1")]
        public void DiscoverCheckRook2(string fenString)
        {
            var fen = Fen.Parse(fenString);

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(20);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var moveViews = GetPawnMoveViews(moves);

            //var moveCount = moves.Count;

            //var illegalMove = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary);

            //Assert.DoesNotContain(illegalMove, moves);
            Assert.Equal(5, moveViews.Count);
        }

        [Fact]
        public void White_EightPawns_OneAndTwoPushes()
        {
            var fen = Fen.Parse("K6k/8/8/8/8/8/PPPPPPPP/8 w - -");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var pawnMoves = GetPawnMoveViews(moves);

            var moveA3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.A2, SquareFlag.A3, PieceType.None, MoveType.Ordinary);
            var moveA4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.A2, SquareFlag.A4, PieceType.None, MoveType.Ordinary);
            var moveB3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.B2, SquareFlag.B3, PieceType.None, MoveType.Ordinary);
            var moveB4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.B2, SquareFlag.B4, PieceType.None, MoveType.Ordinary);
            var moveC3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.C2, SquareFlag.C3, PieceType.None, MoveType.Ordinary);
            var moveC4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.C2, SquareFlag.C4, PieceType.None, MoveType.Ordinary);
            var moveD3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.D2, SquareFlag.D3, PieceType.None, MoveType.Ordinary);
            var moveD4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.D2, SquareFlag.D4, PieceType.None, MoveType.Ordinary);
            var moveE3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.E2, SquareFlag.E3, PieceType.None, MoveType.Ordinary);
            var moveE4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.E2, SquareFlag.E4, PieceType.None, MoveType.Ordinary);
            var moveF3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.F2, SquareFlag.F3, PieceType.None, MoveType.Ordinary);
            var moveF4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.F2, SquareFlag.F4, PieceType.None, MoveType.Ordinary);
            var moveG3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.G2, SquareFlag.G3, PieceType.None, MoveType.Ordinary);
            var moveG4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.G2, SquareFlag.G4, PieceType.None, MoveType.Ordinary);
            var moveH3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.H2, SquareFlag.H3, PieceType.None, MoveType.Ordinary);
            var moveH4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.H2, SquareFlag.H4, PieceType.None, MoveType.Ordinary);

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
            var fen = Fen.Parse("K6k/pppppppp/8/8/8/8/8/8 b - -");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            // Purely for debugging
            var pawnMoves = GetPawnMoveViews(moves);

            var moveA3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.A7, SquareFlag.A6, PieceType.None, MoveType.Ordinary);
            var moveA4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.A7, SquareFlag.A5, PieceType.None, MoveType.Ordinary);
            var moveB3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.B7, SquareFlag.B6, PieceType.None, MoveType.Ordinary);
            var moveB4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.B7, SquareFlag.B5, PieceType.None, MoveType.Ordinary);
            var moveC3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.C7, SquareFlag.C6, PieceType.None, MoveType.Ordinary);
            var moveC4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.C7, SquareFlag.C5, PieceType.None, MoveType.Ordinary);
            var moveD3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.D7, SquareFlag.D6, PieceType.None, MoveType.Ordinary);
            var moveD4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.D7, SquareFlag.D5, PieceType.None, MoveType.Ordinary);
            var moveE3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.E7, SquareFlag.E6, PieceType.None, MoveType.Ordinary);
            var moveE4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.E7, SquareFlag.E5, PieceType.None, MoveType.Ordinary);
            var moveF3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.F7, SquareFlag.F6, PieceType.None, MoveType.Ordinary);
            var moveF4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.F7, SquareFlag.F5, PieceType.None, MoveType.Ordinary);
            var moveG3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.G7, SquareFlag.G6, PieceType.None, MoveType.Ordinary);
            var moveG4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.G7, SquareFlag.G5, PieceType.None, MoveType.Ordinary);
            var moveH3 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.H7, SquareFlag.H6, PieceType.None, MoveType.Ordinary);
            var moveH4 = MoveBuilder.Create(fen.ToPlay, PieceType.Pawn, SquareFlag.H7, SquareFlag.H5, PieceType.None, MoveType.Ordinary);

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
            var fen = Fen.Parse("K6k/8/8/8/8/1p1p1p1p/1P1P1P1P/8 b - -");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var moveViews = GetPawnMoveViews(moves);

            Assert.Empty(moveViews);
        }

        [Fact]
        public void Black_FourBlocked_NoMoves_Correct()
        {
            var fen = Fen.Parse("K6k/1p1p1p1p/1P1P1P1P/8/8/8/8/8 b - -");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var moveViews = GetPawnMoveViews(moves);

            Assert.Empty(moveViews);
        }

        [Fact]
        public void White_FourCaptures_OnePush_Correct()
        {
            var fen = Fen.Parse("K6k/8/8/8/8/p1p2p1p/1P4P1/8 w - -");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var move1 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.B2, SquareFlag.B3, PieceType.None, MoveType.Ordinary);
            var move2 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.G2, SquareFlag.G3, PieceType.None, MoveType.Ordinary);

            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.B2, SquareFlag.A3, PieceType.Pawn, MoveType.Ordinary);
            var capture2 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.B2, SquareFlag.C3, PieceType.Pawn, MoveType.Ordinary);
            var capture3 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.G2, SquareFlag.F3, PieceType.Pawn, MoveType.Ordinary);
            var capture4 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.G2, SquareFlag.H3, PieceType.Pawn, MoveType.Ordinary);

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
            var fen = Fen.Parse("K6k/3P4/8/8/8/8/8/8 w - -");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var promotion1 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.D8, PieceType.None, MoveType.PromotionQueen);
            var promotion2 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.D8, PieceType.None, MoveType.PromotionRook);
            var promotion3 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.D8, PieceType.None, MoveType.PromotionBishop);
            var promotion4 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.D8, PieceType.None, MoveType.PromotionKnight);

            Assert.Contains(promotion1, moves);
            Assert.Contains(promotion2, moves);
            Assert.Contains(promotion3, moves);
            Assert.Contains(promotion4, moves);
        }

        [Fact]
        public void White_Capture_OneCapture_Promotion_Correct()
        {
            var fen = Fen.Parse("3nn3/3P4/8/8/8/8/8/K6k w - -");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var promotion1 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.E8, PieceType.Knight, MoveType.PromotionQueen);
            var promotion2 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.E8, PieceType.Knight, MoveType.PromotionRook);
            var promotion3 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.E8, PieceType.Knight, MoveType.PromotionBishop);
            var promotion4 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.E8, PieceType.Knight, MoveType.PromotionKnight);

            Assert.Contains(promotion1, moves);
            Assert.Contains(promotion2, moves);
            Assert.Contains(promotion3, moves);
            Assert.Contains(promotion4, moves);
        }

        [Fact]
        public void Black_OneCapture_Promotion_Correct()
        {
            var fen = Fen.Parse("4k3/8/8/8/8/8/1p6/R2QK3 b - -");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var promotion1 = MoveBuilder.Create(Colour.Black, PieceType.Pawn, SquareFlag.B2, SquareFlag.A1, PieceType.Rook, MoveType.PromotionQueen);
            var promotion2 = MoveBuilder.Create(Colour.Black, PieceType.Pawn, SquareFlag.B2, SquareFlag.A1, PieceType.Rook, MoveType.PromotionRook);
            var promotion3 = MoveBuilder.Create(Colour.Black, PieceType.Pawn, SquareFlag.B2, SquareFlag.A1, PieceType.Rook, MoveType.PromotionBishop);
            var promotion4 = MoveBuilder.Create(Colour.Black, PieceType.Pawn, SquareFlag.B2, SquareFlag.A1, PieceType.Rook, MoveType.PromotionKnight);

            var pawnMoves = GetPawnMoveViews(moves);

            Assert.Contains(promotion1, moves);
            Assert.Contains(promotion2, moves);
            Assert.Contains(promotion3, moves);
            Assert.Contains(promotion4, moves);
        }

        [Fact]
        public void White_Capture()
        {
            var fen = Fen.Parse("K6k/8/8/8/8/3p4/4P3/8 w - -");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var capture = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.E2, SquareFlag.D3, PieceType.Pawn, MoveType.Ordinary);

            var movesView = moves.Select(x => new MoveViewer(x));

            Assert.Contains(capture, moves);
        }

        [Fact]
        public void White_EnPassant_Capture()
        {
            var fen = Fen.Parse("K6k/8/3Pp3/8/8/8/8/8 w - e7");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var enPassantCapture = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D6, SquareFlag.E7, PieceType.Pawn, MoveType.EnPassant);

            var movesView = moves.Select(x => new MoveViewer(x));

            Assert.Contains(enPassantCapture, moves);
        }

        [Fact]
        public void White_EnPassant_Capture_DiscoveredCheck()
        {
            var fen = Fen.Parse("8/8/8/q1rPp2K/8/7p/8/8 w - e6");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var enPassantCapture = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D6, SquareFlag.E7, PieceType.Pawn, MoveType.EnPassant);

            var movesView = moves.Select(x => new MoveViewer(x));

            Assert.DoesNotContain(enPassantCapture, moves);
        }

        [Fact]
        public void White_DiscoveredCheck()
        {
            var fen = Fen.Parse("8/2b5/3P4/4K3/8/8/8/7k w - -");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var illegalMove = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D6, SquareFlag.D7, PieceType.None, MoveType.Ordinary);

            var movesView = moves.Select(x => new MoveViewer(x));

            Assert.DoesNotContain(illegalMove, moves);
        }
    }
}
