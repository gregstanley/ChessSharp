using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ChessSharp.Tests
{
    public class MoveGeneratorTests : IClassFixture<MoveGeneratorFixture>
    {
        MoveGeneratorFixture _moveGeneratorFixture;

        public MoveGeneratorTests(MoveGeneratorFixture moveGeneratorFixture)
        {
            _moveGeneratorFixture = moveGeneratorFixture;
        }

        [Fact]
        public void Pins_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/rRN1KB1q/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            var pinnedPieces = _moveGeneratorFixture.MoveGenerator.GetPinnedPieces(relativeBitBoard);

            Assert.Equal(SquareFlag.F4, pinnedPieces);
        }

        [Fact]
        public void Pawn_Empty_OnePush_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/3P4/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetPawnPushes(relativeBitBoard, moves);
            _moveGeneratorFixture.MoveGenerator.GetPawnCaptures(relativeBitBoard, moves);

            Assert.Collection(moves, x => Assert.Equal(SquareFlag.D5, x.GetTo()));
        }

        [Fact]
        public void EightPawns_Empty_OnePush_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/8/8/PPPPPPPP/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetPawnPushes(relativeBitBoard, moves);
            _moveGeneratorFixture.MoveGenerator.GetPawnCaptures(relativeBitBoard, moves);

            // Purely for debugging
            var wrappedMoves = moves.Select(x => new MoveWrapper(x));

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
            var relativeBitBoard = Create("8/8/8/8/8/1p1p1p1p/1P1P1P1P/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetPawnPushes(relativeBitBoard, moves);
            _moveGeneratorFixture.MoveGenerator.GetPawnCaptures(relativeBitBoard, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Pawns_Black_FourBlocked_NoMoves_Correct()
        {
            var relativeBitBoard = Create("8/1p1p1p1p/1P1P1P1P/8/8/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetPawnPushes(relativeBitBoard, moves);
            _moveGeneratorFixture.MoveGenerator.GetPawnCaptures(relativeBitBoard, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Pawns_White_FourCaptures_OnePush_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/8/p1p2p1p/1P4P1/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetPawnPushes(relativeBitBoard, moves);
            _moveGeneratorFixture.MoveGenerator.GetPawnCaptures(relativeBitBoard, moves);

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
            var relativeBitBoard = Create("8/3P4/8/8/8/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetPawnPushes(relativeBitBoard, moves);
            _moveGeneratorFixture.MoveGenerator.GetPawnCaptures(relativeBitBoard, moves);

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
            var relativeBitBoard = Create("3nn3/3P4/8/8/8/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetPawnPushes(relativeBitBoard, moves);
            _moveGeneratorFixture.MoveGenerator.GetPawnCaptures(relativeBitBoard, moves);

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
        public void Pawn_EnPassant_Capture_Correct()
        {
            var relativeBitBoard = Create("8/8/3Pp3/8/8/8/8/8 w KQkq e7");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetPawnPushes(relativeBitBoard, moves);
            _moveGeneratorFixture.MoveGenerator.GetPawnCaptures(relativeBitBoard, moves);

            var enPassantCapture = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D6, SquareFlag.E7, PieceType.Pawn, MoveType.EnPassant);

            Assert.Contains(enPassantCapture, moves);
        }

        [Fact]
        public void Pawn_EnPassant_Capture_DiscoveredCheck_Correct()
        {
            var relativeBitBoard = Create("8/8/8/q1rPp2K/8/7p/8/8 w KQkq e6");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetPawnPushes(relativeBitBoard, moves);
            _moveGeneratorFixture.MoveGenerator.GetPawnCaptures(relativeBitBoard, moves);

            var enPassantCapture = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D6, SquareFlag.E7, PieceType.Pawn, MoveType.EnPassant);

            Assert.DoesNotContain(enPassantCapture, moves);
        }

        [Fact]
        public void Rook_Blocked_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/8/1P6/PRP5/1P6 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetRookMoves(relativeBitBoard, (SquareFlag)ulong.MaxValue, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Rook_Blocked2_Correct()
        {
            var relativeBitBoard = Create("8/8/3Q4/7k/1P1R1B2/7K/3N4/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetRookMoves(relativeBitBoard, (SquareFlag)ulong.MaxValue, moves);

            Assert.Equal(4, moves.Count());
        }

        [Fact]
        public void Rook_Capture_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/8/8/p7/RN6 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetRookMoves(relativeBitBoard, (SquareFlag)ulong.MaxValue, moves);

            var move = MoveConstructor.CreateMove(Colour.White, PieceType.Rook, SquareFlag.A1, SquareFlag.A2, PieceType.Pawn, MoveType.Ordinary);

            Assert.Collection(moves, item => { Assert.Equal(move, item); });
        }

        [Fact]
        public void Rook_AllDirections_1Capture_Correct()
        {
            var relativeBitBoard = Create("1p6/8/1p6/8/8/8/1R6/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetRookMoves(relativeBitBoard, (SquareFlag)ulong.MaxValue, moves);

            var moveCount = moves.Count;
            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.Rook, SquareFlag.B2, SquareFlag.B6, PieceType.Pawn, MoveType.Ordinary);

            Assert.Equal(12, moveCount);
            Assert.Collection(captures, item => { Assert.Equal(capture1, item); } );
        }

        [Fact]
        public void Bishop_Blocked_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/8/P1P5/1B6/P1P5 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetBishopMoves(relativeBitBoard, (SquareFlag)ulong.MaxValue, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Bishop_4Moves_0Captures_Correct()
        {
            var relativeBitBoard = Create("8/8/2R3N1/7k/4B3/7K/2Q3P1/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetBishopMoves(relativeBitBoard, (SquareFlag)ulong.MaxValue, moves);

            Assert.Equal(4, moves.Count());
        }

        [Theory]
        [InlineData("8/8/8/8/8/8/8/Q7 w KQkq -", 21)]
        [InlineData("8/8/8/8/8/8/1Q6/8 w KQkq -", 23)]
        [InlineData("8/8/8/8/8/2Q5/8/8 w KQkq -", 25)]
        [InlineData("8/8/8/8/3Q4/8/8/8 w KQkq -", 27)]
        [InlineData("8/8/8/4Q3/8/8/8/8 w KQkq -", 27)]
        [InlineData("8/8/5Q2/8/8/8/8/8 w KQkq -", 25)]
        [InlineData("8/6Q1/8/8/8/8/8/8 w KQkq -", 23)]
        [InlineData("7Q/8/8/8/8/8/8/8 w KQkq -", 21)]
        public void Queen_A1_H8_0Captures_Correct(string fen, int moveCount)
        {
            var relativeBitBoard = Create(fen);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetQueenMoves(relativeBitBoard, (SquareFlag)ulong.MaxValue, moves);

            Assert.Equal(moveCount, moves.Count);
        }

        [Fact]
        public void Queen_B2_21Moves_0Captures_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/8/8/1Q6/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetQueenMoves(relativeBitBoard, (SquareFlag)ulong.MaxValue, moves);

            var moveCount = moves.Count;

            Assert.Equal(23, moveCount);
        }

        [Theory]
        [InlineData("7p/8/8/8/3Q4/8/8/8 w KQkq -", SquareFlag.H8)]
        [InlineData("8/p7/8/8/3Q4/8/8/8 w KQkq -", SquareFlag.A7)]
        [InlineData("8/8/8/8/3Q4/8/8/p7 w KQkq -", SquareFlag.A1)]
        [InlineData("8/8/8/8/3Q4/8/8/6p1 w KQkq -", SquareFlag.G1)]
        public void Queen_E4_27Moves_1Capture_Correct(string fen, SquareFlag captureSquare)
        {
            var relativeBitBoard = Create(fen);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetQueenMoves(relativeBitBoard, (SquareFlag)ulong.MaxValue, moves);

            var moveCount = moves.Count;
            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.D4, captureSquare, PieceType.Pawn, MoveType.Ordinary);

            Assert.Equal(27, moveCount);
            Assert.Contains(capture1, captures);
        }

        [Fact]
        public void Queen_E4_16Moves_8Captures_Correct()
        {
            var relativeBitBoard = Create("8/8/2p1p1p1/7k/2p1Q1p1/7K/2p1p1p1/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetQueenMoves(relativeBitBoard, (SquareFlag)ulong.MaxValue, moves);

            var moveCount = moves.Count;
            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.C6, PieceType.Pawn, MoveType.Ordinary);
            var capture2 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.E6, PieceType.Pawn, MoveType.Ordinary);
            var capture3 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.G6, PieceType.Pawn, MoveType.Ordinary);
            var capture4 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.C4, PieceType.Pawn, MoveType.Ordinary);
            var capture5 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.G4, PieceType.Pawn, MoveType.Ordinary);
            var capture6 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.C2, PieceType.Pawn, MoveType.Ordinary);
            var capture7 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.E2, PieceType.Pawn, MoveType.Ordinary);
            var capture8 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.G2, PieceType.Pawn, MoveType.Ordinary);

            //Assert.Equal(16, moveCount);
            Assert.Contains(capture1, captures);
            Assert.Contains(capture2, captures);
            Assert.Contains(capture3, captures);
            Assert.Contains(capture4, captures);
            Assert.Contains(capture5, captures);
            Assert.Contains(capture6, captures);
            Assert.Contains(capture7, captures);
            Assert.Contains(capture8, captures);
        }

        [Fact]
        public void King_8Moves_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/4K3/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetKingMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;

            Assert.Equal(8, moveCount);
        }

        [Fact]
        public void King_6Moves_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/3PKP2/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetKingMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;

            Assert.Equal(6, moveCount);
        }

        [Fact]
        public void King_RookCoveringRank5_3Moves_Correct()
        {
            var relativeBitBoard = Create("8/8/8/r7/3PKP2/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetKingMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;

            Assert.Equal(3, moveCount);
        }

        [Fact]
        public void King_Check_Rook_4Moves_Correct()
        {
            var relativeBitBoard = Create("8/8/8/4r3/3PKP2/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            var checkers = _moveGeneratorFixture.MoveGenerator.GetKingMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;

            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.King, SquareFlag.E4, SquareFlag.E5, PieceType.Rook, MoveType.Ordinary);

            Assert.Equal(3, moveCount);

            Assert.Contains(capture1, captures);
        }

        [Fact]
        public void King_Check_Pawn_6Moves_Correct()
        {
            var relativeBitBoard = Create("8/8/8/5p2/3PKP2/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            var checkers = _moveGeneratorFixture.MoveGenerator.GetKingMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;

            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.King, SquareFlag.E4, SquareFlag.F5, PieceType.Pawn, MoveType.Ordinary);

            Assert.Equal(6, moveCount);

            Assert.Contains(capture1, captures);
        }

        [Fact]
        public void King_Check_Knight_8Moves_Correct()
        {
            var relativeBitBoard = Create("8/8/3n4/8/3PKP2/8/8/8 w KQkq -");

            var moves = new List<uint>(20);

            var checkers = _moveGeneratorFixture.MoveGenerator.GetKingMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;

            Assert.Equal(5, moveCount);
        }

        [Fact]
        public void King_Check_Evade_Correct()
        {
            var bitBoard = CreateBitBoard("k7/2Q5/8/4p3/3K1P2/8/8/8 w - - 0 1");

            var moves = new List<uint>(20);

            _moveGeneratorFixture.MoveGenerator.Generate(bitBoard, Colour.White, moves);

            // Purely for debugging
            var wrappedMoves = moves.Select(x => new MoveWrapper(x));

            var moveCount = moves.Count;

            Assert.Equal(10, moveCount);
        }

        [Fact]
        public void King_DoubleCheck_Evade_Correct()
        {
            var bitBoard = CreateBitBoard("k7/2Q5/8/4p3/3K1P2/2q5/8/8 w - - 0 1");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.Generate(bitBoard, Colour.White, moves);

            // Purely for debugging
            var wrappedMoves = moves.Select(x => new MoveWrapper(x));

            var moveCount = moves.Count;

            Assert.Equal(3, moveCount);
        }

        [Fact]
        public void Castle_Both_Correct()
        {
            var bitBoard = CreateBitBoard("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.Generate(bitBoard, Colour.White, moves);

            // Purely for debugging
            var wrappedMoves = moves.Select(x => new MoveWrapper(x));

            var moveCount = moves.Count;

            var castleKing = moves.Where(x => x.GetMoveType() == MoveType.CastleKing);
            var castleQueen = moves.Where(x => x.GetMoveType() == MoveType.CastleQueen);

            var castleKingCount = castleKing.Count();
            var castleQueenCount = castleQueen.Count();

            Assert.Equal(1, castleKingCount);
            Assert.Equal(1, castleQueenCount);
        }

        private BitBoard CreateBitBoard(string fenString)
        {
            var fen = Fen.Parse(fenString);

            return BitBoard.FromFen(fen);
        }

        private RelativeBitBoard Create(string fenString)
        {
            var fen = Fen.Parse(fenString);

            return BitBoard.FromFen(fen)
                .ToRelative(fen.ToPlay);
        }
    }
}
