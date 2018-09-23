using ChessSharp.Enums;
using ChessSharp.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ChessSharp.Tests
{
    public class MoveGeneratorTests
    {
        private MoveGenerator _moveGenerator;

        public MoveGeneratorTests()
        {
            _moveGenerator = new MoveGenerator();
        }

        [Fact]
        public void Pawn_Empty_OnePush_Correct()
        {
            var bitBoard = Create("8/8/8/8/3P4/8/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGenerator.GeneratePawnMoves(bitBoard, Colour.White, moves);

            Assert.Collection(moves, x => Assert.Equal(SquareFlag.D5, x.GetTo()));
        }

        [Fact]
        public void Rook_Blocked_Correct()
        {
            var bitBoard = Create("8/8/8/8/8/1P6/PRP5/1P6 w KQkq -");

            var moves = new List<uint>(10);

            _moveGenerator.GetRookMoves(bitBoard, Colour.White, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Rook_Blocked2_Correct()
        {
            var bitBoard = Create("8/8/3Q4/7k/1P1R1B2/7K/3N4/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGenerator.GetRookMoves(bitBoard, Colour.White, moves);

            Assert.Equal(4, moves.Count());
        }

        [Fact]
        public void Rook_Capture_Correct()
        {
            var bitBoard = Create("8/8/8/8/8/8/p7/RN6 w KQkq -");

            var moves = new List<uint>(10);

            _moveGenerator.GetRookMoves(bitBoard, Colour.White, moves);

            var move = MoveConstructor.CreateMove(Colour.White, PieceType.Rook, SquareFlag.A1, SquareFlag.A2, PieceType.Pawn, MoveType.Ordinary);

            Assert.Collection(moves, item => { Assert.Equal(move, item); });
        }

        [Fact]
        public void Rook_AllDirections_1Capture_Correct()
        {
            var bitBoard = Create("1p6/8/1p6/8/8/8/1R6/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGenerator.GetRookMoves(bitBoard, Colour.White, moves);

            var moveCount = moves.Count;
            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.Rook, SquareFlag.B2, SquareFlag.B6, PieceType.Pawn, MoveType.Ordinary);

            Assert.Equal(12, moveCount);
            Assert.Collection(captures, item => { Assert.Equal(capture1, item); } );
        }

        [Fact]
        public void Bishop_Blocked_Correct()
        {
            var bitBoard = Create("8/8/8/8/8/P1P5/1B6/P1P5 w KQkq -");

            var moves = new List<uint>(10);

            _moveGenerator.GetBishopMoves(bitBoard, Colour.White, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Bishop_4Moves_0Captures_Correct()
        {
            var bitBoard = Create("8/8/2R3N1/7k/4B3/7K/2Q3P1/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGenerator.GetBishopMoves(bitBoard, Colour.White, moves);

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
            var bitBoard = Create(fen);

            var moves = new List<uint>(10);

            _moveGenerator.GetQueenMoves(bitBoard, Colour.White, moves);

            Assert.Equal(moveCount, moves.Count);
        }

        [Fact]
        public void Queen_B2_21Moves_0Captures_Correct()
        {
            var bitBoard = Create("8/8/8/8/8/8/1Q6/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGenerator.GetQueenMoves(bitBoard, Colour.White, moves);

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
            var bitBoard = Create(fen);

            var moves = new List<uint>(10);

            _moveGenerator.GetQueenMoves(bitBoard, Colour.White, moves);

            var moveCount = moves.Count;
            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.D4, captureSquare, PieceType.Pawn, MoveType.Ordinary);

            Assert.Equal(27, moveCount);
            Assert.Contains(capture1, captures);
        }

        [Fact]
        public void Queen_E4_16Moves_8Captures_Correct()
        {
            var bitBoard = Create("8/8/2p1p1p1/7k/4Q3/7K/8/8 w KQkq -");

            var moves = new List<uint>(10);

            _moveGenerator.GetQueenMoves(bitBoard, Colour.White, moves);

            var moveCount = moves.Count;
            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.C6, PieceType.Pawn, MoveType.Ordinary);
            var capture2 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.E6, PieceType.Pawn, MoveType.Ordinary);
            var capture3 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.G6, PieceType.Pawn, MoveType.Ordinary);
            //var capture4 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.C4, PieceType.Pawn, MoveType.Ordinary);
            //var capture5 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.G4, PieceType.Pawn, MoveType.Ordinary);
            //var capture6 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.C2, PieceType.Pawn, MoveType.Ordinary);
            //var capture7 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.E2, PieceType.Pawn, MoveType.Ordinary);
            //var capture8 = MoveConstructor.CreateMove(Colour.White, PieceType.Queen, SquareFlag.E4, SquareFlag.G2, PieceType.Pawn, MoveType.Ordinary);

            //Assert.Equal(16, moveCount);
            Assert.Contains(capture1, captures);
            Assert.Contains(capture2, captures);
            Assert.Contains(capture3, captures);
            //Assert.Contains(capture4, captures);
            //Assert.Contains(capture5, captures);
            //Assert.Contains(capture6, captures);
            //Assert.Contains(capture7, captures);
            //Assert.Contains(capture8, captures);
        }

        private BitBoard Create(string fenString)
        {
            var fen = Fen.Parse(fenString);
            return BitBoard.FromFen(fen);
        }
    }
}
