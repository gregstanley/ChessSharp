﻿using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System;
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

        [Fact]
        public void DiscoverCheck_Correct()
        {
            //var bitBoard = CreateBitBoard("8/2b5/3P4/4K3/8/8/8/7k w KQkq -");
            var fen = Fen.Parse("8/2b5/3P4/4K3/8/8/8/7k w - - 0 1");

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(20);

            MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);

            var moveCount = moves.Count;

            var illegalMove = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D6, SquareFlag.D7, PieceType.None, MoveType.Ordinary);

            Assert.DoesNotContain(illegalMove, moves);
        }

        [Fact]
        public void Pawn_Empty_OnePush_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/8/8/8/3P4/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.AddPawnPushes(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);
            _moveGeneratorFixture.MoveGenerator.AddPawnCaptures(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            Assert.Collection(moves, x => Assert.Equal(SquareFlag.D5, x.GetTo()));
        }

        [Fact]
        public void EightPawns_Empty_OnePush_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/8/8/8/8/8/PPPPPPPP/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.AddPawnPushes(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);
            _moveGeneratorFixture.MoveGenerator.AddPawnCaptures(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            // Purely for debugging
            var wrappedMoves = moves.Select(x => new MoveViewer(x));

            var moveA3 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.A2, SquareFlag.A3, PieceType.None, MoveType.Ordinary);
            var moveA4 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.A2, SquareFlag.A4, PieceType.None, MoveType.Ordinary);
            var moveB3 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.B2, SquareFlag.B3, PieceType.None, MoveType.Ordinary);
            var moveB4 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.B2, SquareFlag.B4, PieceType.None, MoveType.Ordinary);
            var moveC3 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.C2, SquareFlag.C3, PieceType.None, MoveType.Ordinary);
            var moveC4 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.C2, SquareFlag.C4, PieceType.None, MoveType.Ordinary);
            var moveD3 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D2, SquareFlag.D3, PieceType.None, MoveType.Ordinary);
            var moveD4 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D2, SquareFlag.D4, PieceType.None, MoveType.Ordinary);
            var moveE3 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.E2, SquareFlag.E3, PieceType.None, MoveType.Ordinary);
            var moveE4 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.E2, SquareFlag.E4, PieceType.None, MoveType.Ordinary);
            var moveF3 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.F2, SquareFlag.F3, PieceType.None, MoveType.Ordinary);
            var moveF4 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.F2, SquareFlag.F4, PieceType.None, MoveType.Ordinary);
            var moveG3 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.G2, SquareFlag.G3, PieceType.None, MoveType.Ordinary);
            var moveG4 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.G2, SquareFlag.G4, PieceType.None, MoveType.Ordinary);
            var moveH3 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.H2, SquareFlag.H3, PieceType.None, MoveType.Ordinary);
            var moveH4 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.H2, SquareFlag.H4, PieceType.None, MoveType.Ordinary);

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
        public void Pawns_White_FourBlocked_NoMoves_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/8/8/8/8/1p1p1p1p/1P1P1P1P/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.AddPawnPushes(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);
            _moveGeneratorFixture.MoveGenerator.AddPawnCaptures(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Pawns_Black_FourBlocked_NoMoves_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/1p1p1p1p/1P1P1P1P/8/8/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.AddPawnPushes(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);
            _moveGeneratorFixture.MoveGenerator.AddPawnCaptures(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Pawns_White_FourCaptures_OnePush_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/8/8/8/8/p1p2p1p/1P4P1/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.AddPawnPushes(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);
            _moveGeneratorFixture.MoveGenerator.AddPawnCaptures(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            var move1 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.B2, SquareFlag.B3, PieceType.None, MoveType.Ordinary);
            var move2 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.G2, SquareFlag.G3, PieceType.None, MoveType.Ordinary);

            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.B2, SquareFlag.A3, PieceType.Pawn, MoveType.Ordinary);
            var capture2 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.B2, SquareFlag.C3, PieceType.Pawn, MoveType.Ordinary);
            var capture3 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.G2, SquareFlag.F3, PieceType.Pawn, MoveType.Ordinary);
            var capture4 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.G2, SquareFlag.H3, PieceType.Pawn, MoveType.Ordinary);

            Assert.Contains(move1, moves);
            Assert.Contains(move2, moves);

            Assert.Contains(capture1, moves);
            Assert.Contains(capture2, moves);
            Assert.Contains(capture3, moves);
            Assert.Contains(capture4, moves);
        }

        [Fact]
        public void Pawn_Empty_OnePush_Promotion_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/3P4/8/8/8/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.AddPawnPushes(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);
            _moveGeneratorFixture.MoveGenerator.AddPawnCaptures(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            var promotion1 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.D8, PieceType.None, MoveType.PromotionQueen);
            var promotion2 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.D8, PieceType.None, MoveType.PromotionRook);
            var promotion3 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.D8, PieceType.None, MoveType.PromotionBishop);
            var promotion4 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.D8, PieceType.None, MoveType.PromotionKnight);

            Assert.Contains(promotion1, moves);
            Assert.Contains(promotion2, moves);
            Assert.Contains(promotion3, moves);
            Assert.Contains(promotion4, moves);
        }

        [Fact]
        public void Pawn_Capture_OneCapture_Promotion_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("3nn3/3P4/8/8/8/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.AddPawnPushes(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);
            _moveGeneratorFixture.MoveGenerator.AddPawnCaptures(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            var promotion1 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.E8, PieceType.Knight, MoveType.PromotionQueen);
            var promotion2 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.E8, PieceType.Knight, MoveType.PromotionRook);
            var promotion3 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.E8, PieceType.Knight, MoveType.PromotionBishop);
            var promotion4 = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D7, SquareFlag.E8, PieceType.Knight, MoveType.PromotionKnight);

            Assert.Contains(promotion1, moves);
            Assert.Contains(promotion2, moves);
            Assert.Contains(promotion3, moves);
            Assert.Contains(promotion4, moves);
        }

        [Fact]
        public void Pawn_Capture_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/8/8/8/8/3p4/4P3/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.AddPawnPushes(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);
            _moveGeneratorFixture.MoveGenerator.AddPawnCaptures(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            var capture = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.E2, SquareFlag.D3, PieceType.Pawn, MoveType.Ordinary);

            var movesView = moves.Select(x => new MoveViewer(x));

            Assert.Contains(capture, moves);
        }

        [Fact]
        public void Pawn_EnPassant_Capture_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/8/3Pp3/8/8/8/8/8 w KQkq e7");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.AddPawnPushes(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);
            _moveGeneratorFixture.MoveGenerator.AddPawnCaptures(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            var enPassantCapture = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D6, SquareFlag.E7, PieceType.Pawn, MoveType.EnPassant);

            var movesView = moves.Select(x => new MoveViewer(x));

            Assert.Contains(enPassantCapture, moves);
        }

        [Fact]
        public void Pawn_EnPassant_Capture_DiscoveredCheck_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/8/8/q1rPp2K/8/7p/8/8 w KQkq e6");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.AddPawnPushes(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);
            _moveGeneratorFixture.MoveGenerator.AddPawnCaptures(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            var enPassantCapture = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D6, SquareFlag.E7, PieceType.Pawn, MoveType.EnPassant);

            var movesView = moves.Select(x => new MoveViewer(x));

            Assert.DoesNotContain(enPassantCapture, moves);
        }

        [Fact]
        public void Pawn_DiscoveredCheck_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/2b5/3P4/4K3/8/8/8/7k w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.AddPawnPushes(relativeBitBoard, SquareFlagConstants.All, 0, SquareFlag.D6, moves);
            _moveGeneratorFixture.MoveGenerator.AddPawnCaptures(relativeBitBoard, SquareFlagConstants.All, SquareFlag.D7, SquareFlag.D6, moves);

            var illegalMove = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D6, SquareFlag.D7, PieceType.None, MoveType.Ordinary);

            var movesView = moves.Select(x => new MoveViewer(x));

            Assert.DoesNotContain(illegalMove, moves);
        }
    }
}