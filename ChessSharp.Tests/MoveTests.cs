using ChessSharp.Enums;
using ChessSharp.Extensions;
using Xunit;

namespace ChessSharp.Tests
{
    public class MoveTests
    {
        [Theory]
        [InlineData(Colour.White)]
        [InlineData(Colour.Black)]
        public void Move_Constructor_Colour(Colour colour)
        {
            var move = MoveBuilder.Create(colour, 0, new Models.Square(), new Models.Square(), 0, MoveType.Ordinary);

            Assert.Equal(colour, move.GetColour());
        }

        [Theory]
        [InlineData(PieceType.None)]
        [InlineData(PieceType.Pawn)]
        [InlineData(PieceType.Rook)]
        [InlineData(PieceType.Bishop)]
        [InlineData(PieceType.Knight)]
        [InlineData(PieceType.Queen)]
        [InlineData(PieceType.King)]
        public void Move_Constructor_PieceType(PieceType pieceType)
        {
            var move   = MoveBuilder.Create(0, pieceType, new Models.Square(), new Models.Square(), 0, MoveType.Ordinary);

            Assert.Equal(pieceType, move.GetPieceType());
        }

        [Theory]
        [InlineData(SquareFlag.A1)]
        [InlineData(SquareFlag.B2)]
        [InlineData(SquareFlag.C3)]
        [InlineData(SquareFlag.D4)]
        [InlineData(SquareFlag.E5)]
        [InlineData(SquareFlag.F6)]
        [InlineData(SquareFlag.G7)]
        [InlineData(SquareFlag.H8)]
        public void Move_Constructor_From(SquareFlag square)
        {
            var move = MoveBuilder.Create(0, 0, square.ToSquare(), new Models.Square(), 0, MoveType.Ordinary);

            Assert.Equal(square, move.GetFrom());
        }

        [Theory]
        [InlineData(SquareFlag.A1)]
        [InlineData(SquareFlag.B2)]
        [InlineData(SquareFlag.C3)]
        [InlineData(SquareFlag.D4)]
        [InlineData(SquareFlag.E5)]
        [InlineData(SquareFlag.F6)]
        [InlineData(SquareFlag.G7)]
        [InlineData(SquareFlag.H8)]
        public void Move_Constructor_To(SquareFlag square)
        {
            var move = MoveBuilder.Create(0, 0, new Models.Square(), square.ToSquare(), 0, MoveType.Ordinary);

            Assert.Equal(square, move.GetTo());
        }

        [Theory]
        [InlineData(PieceType.None)]
        [InlineData(PieceType.Pawn)]
        [InlineData(PieceType.Rook)]
        [InlineData(PieceType.Bishop)]
        [InlineData(PieceType.Knight)]
        [InlineData(PieceType.Queen)]
        [InlineData(PieceType.King)]
        public void Move_Constructor_CapturePieceType(PieceType pieceType)
        {
            var move = MoveBuilder.Create(0, 0, new Models.Square(), new Models.Square(), pieceType, MoveType.Ordinary);

            Assert.Equal(pieceType, move.GetCapturePieceType());
        }

        [Theory]
        [InlineData(MoveType.Ordinary)]
        [InlineData(MoveType.EnPassant)]
        [InlineData(MoveType.CastleKing)]
        [InlineData(MoveType.CastleQueen)]
        [InlineData(MoveType.PromotionQueen)]
        [InlineData(MoveType.PromotionRook)]
        [InlineData(MoveType.PromotionBishop)]
        [InlineData(MoveType.PromotionKnight)]

        public void Move_Constructor_MoveType(MoveType moveType)
        {
            var move = MoveBuilder.Create(0, 0, new Models.Square(), new Models.Square(), 0, moveType);

            Assert.Equal(moveType, move.GetMoveType());
        }

        [Fact]
        public void Move_Constructor_AccessTwice_NoChange()
        {
            var move = MoveBuilder.Create(Colour.White, PieceType.Knight, SquareFlag.D5.ToSquare(), SquareFlag.E7.ToSquare(), PieceType.Pawn, MoveType.CastleKing);

            Assert.Equal(Colour.White, move.GetColour());
            Assert.Equal(Colour.White, move.GetColour());
            Assert.Equal(PieceType.Knight, move.GetPieceType());
            Assert.Equal(PieceType.Knight, move.GetPieceType());
            Assert.Equal(SquareFlag.D5, move.GetFrom());
            Assert.Equal(SquareFlag.D5, move.GetFrom());
            Assert.Equal(SquareFlag.E7, move.GetTo());
            Assert.Equal(SquareFlag.E7, move.GetTo());
            Assert.Equal(PieceType.Pawn, move.GetCapturePieceType());
            Assert.Equal(PieceType.Pawn, move.GetCapturePieceType());
            Assert.Equal(MoveType.CastleKing, move.GetMoveType());
            Assert.Equal(MoveType.CastleKing, move.GetMoveType());
        }

        [Fact]
        public void Move_AreEqual()
        {
            var moveA = MoveBuilder.Create(Colour.White, PieceType.Rook, SquareFlag.G5.ToSquare(), new Models.Square(), 0, MoveType.Ordinary);
            var moveB = MoveBuilder.Create(Colour.White, PieceType.Rook, SquareFlag.G5.ToSquare(), new Models.Square(), 0, MoveType.Ordinary);

            Assert.Equal(moveA, moveB);
        }

        [Fact]
        public void Move_AreNotEqual()
        {
            var moveA = MoveBuilder.Create(Colour.White, 0, new Models.Square(), new Models.Square(), 0, MoveType.Ordinary);
            var moveB = MoveBuilder.Create(Colour.Black, 0, new Models.Square(), new Models.Square(), 0, MoveType.Ordinary);

            Assert.NotEqual(moveA, moveB);
        }
    }
}
