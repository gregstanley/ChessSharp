using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class RookMoveTests : MoveGeneratorTestsBase
    {
        public RookMoveTests(MoveGeneratorFixture moveGeneratorFixture)
            : base(moveGeneratorFixture)
        {
        }

        [Fact]
        public void Rook_Blocked_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/8/8/8/8/1P6/PRP5/1P6 w KQkq -");

            var moves = new List<uint>(10);

            MoveGenerator.AddRookMoves(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            Assert.Empty(moves);
        }

        [Fact]
        public void Rook_Blocked2_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/8/3Q4/7k/1P1R1B2/7K/3N4/8 w KQkq -");

            var moves = new List<uint>(10);

            MoveGenerator.AddRookMoves(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            Assert.Equal(4, moves.Count());
        }

        [Fact]
        public void Rook_Capture_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("8/8/8/8/8/8/p7/RN6 w KQkq -");

            var moves = new List<uint>(10);

            MoveGenerator.AddRookMoves(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            var move = MoveConstructor.CreateMove(Colour.White, PieceType.Rook, SquareFlag.A1, SquareFlag.A2, PieceType.Pawn, MoveType.Ordinary);

            Assert.Collection(moves, item => { Assert.Equal(move, item); });
        }

        [Fact]
        public void Rook_AllDirections_1Capture_Correct()
        {
            var relativeBitBoard = CreateRelativeBitBoard("1p6/8/1p6/8/8/8/1R6/8 w KQkq -");

            var moves = new List<uint>(10);

            MoveGenerator.AddRookMoves(relativeBitBoard, SquareFlagConstants.All, SquareFlagConstants.All, 0, moves);

            var moveCount = moves.Count;
            var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            var capture1 = MoveConstructor.CreateMove(Colour.White, PieceType.Rook, SquareFlag.B2, SquareFlag.B6, PieceType.Pawn, MoveType.Ordinary);

            Assert.Equal(12, moveCount);
            Assert.Collection(captures, item => { Assert.Equal(capture1, item); });
        }
    }
}
