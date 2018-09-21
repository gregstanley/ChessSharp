using ChessSharp.Enums;
using ChessSharp.Models;
using Xunit;

namespace ChessSharp.Tests
{
    public class MoveTests
    {
        [Fact]
        public void Move_Constructor_Colour()
        {
            var moveWhite = new Move(Colour.White, 0, 0, 0, 0, 0, 0, false);
            var moveBlack = new Move(Colour.Black, 0, 0, 0, 0, 0, 0, false);

            Assert.Equal(Colour.White, moveWhite.Colour);
            Assert.Equal(Colour.Black, moveBlack.Colour);
        }

        [Fact]
        public void Move_Constructor_PieceType()
        {
            var movePawn   = new Move(0, PieceType.Pawn, 0, 0, 0, 0, 0, false);
            var moveRook   = new Move(0, PieceType.Rook, 0, 0, 0, 0, 0, false);
            var moveKnight = new Move(0, PieceType.Knight, 0, 0, 0, 0, 0, false);
            var moveBishop = new Move(0, PieceType.Bishop, 0, 0, 0, 0, 0, false);
            var moveQueen  = new Move(0, PieceType.Queen, 0, 0, 0, 0, 0, false);
            var moveKing   = new Move(0, PieceType.King, 0, 0, 0, 0, 0, false);

            Assert.Equal(PieceType.Pawn, movePawn.PieceType);
            Assert.Equal(PieceType.Rook, moveRook.PieceType);
            Assert.Equal(PieceType.Knight, moveKnight.PieceType);
            Assert.Equal(PieceType.Bishop, moveBishop.PieceType);
            Assert.Equal(PieceType.Queen, moveQueen.PieceType);
            Assert.Equal(PieceType.King, moveKing.PieceType);
        }

        [Fact]
        public void Move_Constructor_From()
        {
            var move = new Move(0, 0, SquareFlag.A2, 0, 0, 0, 0, false);

            Assert.Equal(SquareFlag.A2, move.From);
        }

        [Fact]
        public void Move_Constructor_To()
        {
            var move = new Move(0, 0, 0, SquareFlag.H8, 0, 0, 0, false);

            Assert.Equal(SquareFlag.H8, move.To);
        }

        [Fact]
        public void Move_Constructor_CapturePieceType()
        {
            var move = new Move(0, 0, 0, 0, PieceType.Bishop, 0, 0, false);

            Assert.Equal(PieceType.Bishop, move.CapturePieceType);
        }

        [Fact]
        public void Move_Constructor_PromotionPieceType()
        {
            var move = new Move(0, 0, 0, 0, 0, PieceType.Queen, 0, false);

            Assert.Equal(PieceType.Queen, move.PromotionPieceType);
        }

        [Fact]
        public void Move_Constructor_Castle()
        {
            var moveCastleKing = new Move(0, 0, 0, 0, 0, 0, CastleType.King, false);
            var moveCastleQueen = new Move(0, 0, 0, 0, 0, 0, CastleType.Queen, false);

            Assert.Equal(CastleType.King, moveCastleKing.CastleType);
            Assert.Equal(CastleType.Queen, moveCastleQueen.CastleType);
        }

        [Fact]
        public void Move_Constructor_EnPassant()
        {
            var move = new Move(0, 0, 0, 0, 0, 0, 0, true);

            Assert.True(move.EnPassant);
        }

        [Fact]
        public void Move_Constructor_AccessTwice_NoChange()
        {
            var move = new Move(Colour.White, PieceType.Knight, SquareFlag.D5, SquareFlag.E7, PieceType.Pawn, PieceType.Rook, CastleType.King, false);

            Assert.Equal(Colour.White, move.Colour);
            Assert.Equal(Colour.White, move.Colour);
            Assert.Equal(PieceType.Knight, move.PieceType);
            Assert.Equal(PieceType.Knight, move.PieceType);
            Assert.Equal(SquareFlag.D5, move.From);
            Assert.Equal(SquareFlag.D5, move.From);
            Assert.Equal(SquareFlag.E7, move.To);
            Assert.Equal(SquareFlag.E7, move.To);
            Assert.Equal(PieceType.Pawn, move.CapturePieceType);
            Assert.Equal(PieceType.Pawn, move.CapturePieceType);
            Assert.Equal(PieceType.Rook, move.PromotionPieceType);
            Assert.Equal(PieceType.Rook, move.PromotionPieceType);
            Assert.Equal(CastleType.King, move.CastleType);
            Assert.Equal(CastleType.King, move.CastleType);
            Assert.False(move.EnPassant);
            Assert.False(move.EnPassant);
        }

        [Fact]
        public void Move_AreEqual()
        {
            var moveA = new Move(Colour.White, PieceType.Rook, SquareFlag.G5, 0, 0, 0, 0, true);
            var moveB = new Move(Colour.White, PieceType.Rook, SquareFlag.G5, 0, 0, 0, 0, true);
            Assert.Equal(moveA, moveB);
        }

        [Fact]
        public void Move_AreNotEqual()
        {
            var moveA = new Move(Colour.White, 0, 0, 0, 0, 0, 0, false);
            var moveB = new Move(Colour.Black, 0, 0, 0, 0, 0, 0, true);
            Assert.NotEqual(moveA, moveB);
        }
    }
}
