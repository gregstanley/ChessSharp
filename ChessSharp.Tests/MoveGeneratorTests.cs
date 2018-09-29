using ChessSharp.Enums;
using ChessSharp.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ChessSharp.Tests
{
    public class MoveGeneratorFixture
    {
        public MoveGeneratorFixture()
        {
            MoveGenerator = new MoveGenerator();
        }

        public MoveGenerator MoveGenerator { get; private set; }
    }

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
            var relativeBitBoard = Create("8/8/8/8/rRN1KB1q/8/8/8 w KQkq -")
                .ToRelative(Colour.White);

            var moves = new List<uint>(10);

            var pinnedPieces = _moveGeneratorFixture.MoveGenerator.GetPinnedPieces(relativeBitBoard);

            Assert.Equal(SquareFlag.F4, pinnedPieces);
        }

        [Fact]
        public void Pawn_Empty_OnePush_Correct()
        {
            var bitBoard = Create("8/8/8/8/3P4/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetWhitePawnPushes(bitBoard, Colour.White, moves);
            _moveGeneratorFixture.MoveGenerator.GetWhitePawnCaptures(bitBoard, Colour.White, moves);

            Assert.Collection(moves, x => Assert.Equal(SquareFlag.D5, x.GetTo()));
        }

        [Fact]
        public void EightPawns_Empty_OnePush_Correct()
        {
            var bitBoard = Create("8/8/8/8/8/8/PPPPPPPP/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetWhitePawnPushes(bitBoard, Colour.White, moves);
            _moveGeneratorFixture.MoveGenerator.GetWhitePawnCaptures(bitBoard, Colour.White, moves);

            Assert.Collection(moves,
                x => { Assert.Equal(SquareFlag.A3, x.GetTo()); },
                x => { Assert.Equal(SquareFlag.B3, x.GetTo()); },
                x => { Assert.Equal(SquareFlag.C3, x.GetTo()); },
                x => { Assert.Equal(SquareFlag.D3, x.GetTo()); },
                x => { Assert.Equal(SquareFlag.E3, x.GetTo()); },
                x => { Assert.Equal(SquareFlag.F3, x.GetTo()); },
                x => { Assert.Equal(SquareFlag.G3, x.GetTo()); },
                x => { Assert.Equal(SquareFlag.H3, x.GetTo()); });
        }

        [Fact]
        public void Pawns_White_FourBlocked_NoMoves_Correct()
        {
            var bitBoard = Create("8/8/8/8/8/1p1p1p1p/1P1P1P1P/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetWhitePawnPushes(bitBoard, Colour.White, moves);
            _moveGeneratorFixture.MoveGenerator.GetWhitePawnCaptures(bitBoard, Colour.White, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Pawns_Black_FourBlocked_NoMoves_Correct()
        {
            var bitBoard = Create("8/1p1p1p1p/1P1P1P1P/8/8/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetWhitePawnPushes(bitBoard, Colour.White, moves);
            _moveGeneratorFixture.MoveGenerator.GetWhitePawnCaptures(bitBoard, Colour.White, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Pawns_White_FourCaptures_OnePush_Correct()
        {
            var bitBoard = Create("8/8/8/8/8/p1p2p1p/1P4P1/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetWhitePawnPushes(bitBoard, Colour.White, moves);
            _moveGeneratorFixture.MoveGenerator.GetWhitePawnCaptures(bitBoard, Colour.White, moves);

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
            var bitBoard = Create("8/3P4/8/8/8/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetWhitePawnPushes(bitBoard, Colour.White, moves);
            _moveGeneratorFixture.MoveGenerator.GetWhitePawnCaptures(bitBoard, Colour.White, moves);

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
            var bitBoard = Create("3nn3/3P4/8/8/8/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetWhitePawnPushes(bitBoard, Colour.White, moves);
            _moveGeneratorFixture.MoveGenerator.GetWhitePawnCaptures(bitBoard, Colour.White, moves);

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
            var bitBoard = Create("8/8/3Pp3/8/8/8/8/8 w KQkq e7");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetWhitePawnPushes(bitBoard, Colour.White, moves);
            _moveGeneratorFixture.MoveGenerator.GetWhitePawnCaptures(bitBoard, Colour.White, moves);

            var enPassantCapture = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D6, SquareFlag.E7, PieceType.Pawn, MoveType.EnPassant);

            Assert.Contains(enPassantCapture, moves);
        }

        [Fact]
        public void Pawn_EnPassant_Capture_DiscoveredCheck_Correct()
        {
            var bitBoard = Create("8/8/q1rPp2K/8/7p/8/8/8 w KQkq e7");

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetWhitePawnPushes(bitBoard, Colour.White, moves);
            _moveGeneratorFixture.MoveGenerator.GetWhitePawnCaptures(bitBoard, Colour.White, moves);

            var enPassantCapture = MoveConstructor.CreateMove(Colour.White, PieceType.Pawn, SquareFlag.D6, SquareFlag.E7, PieceType.Pawn, MoveType.EnPassant);

            Assert.DoesNotContain(enPassantCapture, moves);
        }

        [Fact]
        public void Rook_Blocked_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/8/1P6/PRP5/1P6 w KQkq -")
                .ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetRookMoves(relativeBitBoard, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Rook_Blocked2_Correct()
        {
            var relativeBitBoard = Create("8/8/3Q4/7k/1P1R1B2/7K/3N4/8 w KQkq -")
                .ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetRookMoves(relativeBitBoard, moves);

            Assert.Equal(4, moves.Count());
        }

        [Fact]
        public void Rook_Capture_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/8/8/p7/RN6 w KQkq -").ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetRookMoves(relativeBitBoard, moves);

            var move = MoveConstructor.CreateMove(Colour.White, PieceType.Rook, SquareFlag.A1, SquareFlag.A2, PieceType.Pawn, MoveType.Ordinary);

            Assert.Collection(moves, item => { Assert.Equal(move, item); });
        }

        [Fact]
        public void Rook_AllDirections_1Capture_Correct()
        {
            var relativeBitBoard = Create("1p6/8/1p6/8/8/8/1R6/8 w KQkq -").ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetRookMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;
            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.Rook, SquareFlag.B2, SquareFlag.B6, PieceType.Pawn, MoveType.Ordinary);

            Assert.Equal(12, moveCount);
            Assert.Collection(captures, item => { Assert.Equal(capture1, item); } );
        }

        [Fact]
        public void Bishop_Blocked_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/8/P1P5/1B6/P1P5 w KQkq -").ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetBishopMoves(relativeBitBoard, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Bishop_4Moves_0Captures_Correct()
        {
            var relativeBitBoard = Create("8/8/2R3N1/7k/4B3/7K/2Q3P1/8 w KQkq -").ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetBishopMoves(relativeBitBoard, moves);

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
            var relativeBitBoard = Create(fen).ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetQueenMoves(relativeBitBoard, moves);

            Assert.Equal(moveCount, moves.Count);
        }

        [Fact]
        public void Queen_B2_21Moves_0Captures_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/8/8/1Q6/8 w KQkq -").ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetQueenMoves(relativeBitBoard, moves);

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
            var relativeBitBoard = Create(fen).ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetQueenMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;
            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.D4, captureSquare, PieceType.Pawn, MoveType.Ordinary);

            Assert.Equal(27, moveCount);
            Assert.Contains(capture1, captures);
        }

        [Fact]
        public void Queen_E4_16Moves_8Captures_Correct()
        {
            var relativeBitBoard = Create("8/8/2p1p1p1/7k/2p1Q1p1/7K/2p1p1p1/8 w KQkq -")
                .ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetQueenMoves(relativeBitBoard, moves);

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
            var relativeBitBoard = Create("8/8/8/8/4K3/8/8/8 w KQkq -").ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetKingMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;

            Assert.Equal(8, moveCount);
        }

        [Fact]
        public void King_6Moves_Correct()
        {
            var relativeBitBoard = Create("8/8/8/8/3PKP2/8/8/8 w KQkq -").ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetKingMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;

            Assert.Equal(6, moveCount);
        }

        [Fact]
        public void King_RookCoveringRank5_3Moves_Correct()
        {
            var relativeBitBoard = Create("8/8/8/r7/3PKP2/8/8/8 w KQkq -").ToRelative(Colour.White);

            var moves = new List<uint>(10);

            _moveGeneratorFixture.MoveGenerator.GetKingMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;

            Assert.Equal(3, moveCount);
        }

        [Fact]
        public void King_Check_Rook_4Moves_Correct()
        {
            var relativeBitBoard = Create("8/8/8/4r3/3PKP2/8/8/8 w KQkq -").ToRelative(Colour.White);

            var moves = new List<uint>(10);

            var checkers = _moveGeneratorFixture.MoveGenerator.GetKingMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;

            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.King, SquareFlag.E4, SquareFlag.E5, PieceType.Rook, MoveType.Ordinary);

            Assert.Equal(4, moveCount);

            Assert.Contains(capture1, captures);
        }

        [Fact]
        public void King_Check_Pawn_6Moves_Correct()
        {
            var relativeBitBoard = Create("8/8/8/5p2/3PKP2/8/8/8 w KQkq -").ToRelative(Colour.White);

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
            var relativeBitBoard = Create("8/8/3n4/8/3PKP2/8/8/8 w KQkq -").ToRelative(Colour.White);

            var moves = new List<uint>(10);

            var checkers = _moveGeneratorFixture.MoveGenerator.GetKingMoves(relativeBitBoard, moves);

            var moveCount = moves.Count;

            Assert.Equal(5, moveCount);
        }

        private BitBoard Create(string fenString)
        {
            var fen = Fen.Parse(fenString);
            return BitBoard.FromFen(fen);
        }
    }
}
