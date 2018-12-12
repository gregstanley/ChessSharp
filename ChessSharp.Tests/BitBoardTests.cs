using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Helpers;
using Xunit;

namespace ChessSharp.Tests
{
    public class BitBoardTests
    {
        [Theory]
        [InlineData("8/P3p3", SquareFlag.D3)]
        [InlineData("4p3/P7", (SquareFlag)0)]
        public void Make_EnPassantOnlySetIfCapturePossible(string partialFen, SquareFlag enPassant)
        {
            var bitBoard = Create($"rnbqkbnr/pppp1ppp/8/{partialFen}/1P6/2PPPPPP/RNBQKBNR b KQkq -");

            var fromSquare = SquareFlag.D2;
            var toSquare = SquareFlag.D4;

            var move = MoveBuilder.Create(Colour.White, PieceType.Pawn, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.Equal(enPassant, bitBoard.EnPassant);
        }

        [Fact]
        public void Make_UnMake_Correct1()
        {
            var bitBoard = Create("rnbqkbnr/pppp1ppp/8/8/P3p3/1P6/2PPPPPP/RNBQKBNR b KQkq -");

            var fromSquare = SquareFlag.D2;
            var toSquare = SquareFlag.D4;

            var move = MoveBuilder.Create(Colour.White, PieceType.Pawn, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.Equal(SquareFlag.D3, bitBoard.EnPassant);

            bitBoard.UnMakeMove(move);

            Assert.Equal((SquareFlag)0, bitBoard.EnPassant);
        }

        [Fact]
        public void Make_UnMake_KnightCaptures_Correct()
        {
            var bitBoard = Create("7k/8/3p4/8/4N3/8/8/K7 w - - 0 1");
            var bitBoardReference = Create("7k/8/3p4/8/4N3/8/8/K7 w - - 0 1");
            var bitBoardAfterMake= Create("7k/8/3N4/8/8/8/8/K7 w - - 0 1");

            var fromSquare = SquareFlag.E4;
            var toSquare = SquareFlag.D6;

            var move = MoveBuilder.Create(Colour.White, PieceType.Knight, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.Pawn, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            TestHelpers.AssertEqual(bitBoard, bitBoardAfterMake);

            bitBoard.UnMakeMove(move);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);
        }

        [Fact]
        public void Make_UnMake_EnPassant_Correct()
        {
            var bitBoard = Create("4k3/8/8/8/5Pp1/8/8/4K3 w - f3 0 1");

            var bitBoardReference = Create("4k3/8/8/8/5Pp1/8/8/4K3 w - f3 0 1");

            var fromSquare = SquareFlag.G4;
            var toSquare = SquareFlag.F3;

            var move = MoveBuilder.Create(Colour.Black, PieceType.Pawn, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.Pawn, MoveType.EnPassant);

            bitBoard.MakeMove(move);

            Assert.NotEqual(bitBoard.White, bitBoardReference.White);
            Assert.NotEqual(bitBoard.Black, bitBoardReference.Black);
            Assert.NotEqual(bitBoard.EnPassant, bitBoardReference.EnPassant);

            bitBoard.UnMakeMove(move);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);
        }

        [Fact]
        public void Make_Capture_BlackRookRemovesRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
            var bitBoardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");

            var fromSquare = SquareFlag.A1;
            var toSquare = SquareFlag.A8;

            var move = MoveBuilder.Create(Colour.White, PieceType.Rook, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.Rook, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.True(bitBoard.WhiteCanCastleKingSide);
            Assert.False(bitBoard.WhiteCanCastleQueenSide);
            Assert.True(bitBoard.BlackCanCastleKingSide);
            Assert.False(bitBoard.BlackCanCastleQueenSide);

            bitBoard.UnMakeMove(move);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);
        }

        [Fact]
        public void Make_WhiteCastleRemovesRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var bitBoardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var move = MoveBuilder.CreateCastle(Colour.White, MoveType.CastleKing);

            bitBoard.MakeMove(move);

            Assert.False(bitBoard.WhiteCanCastleKingSide);
            Assert.False(bitBoard.WhiteCanCastleQueenSide);
            Assert.True(bitBoard.BlackCanCastleKingSide);
            Assert.True(bitBoard.BlackCanCastleQueenSide);

            bitBoard.UnMakeMove(move);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);
        }

        [Fact]
        public void Make_BlackCastleRemovesRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var bitBoardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var move = MoveBuilder.CreateCastle(Colour.Black, MoveType.CastleKing);

            bitBoard.MakeMove(move);

            Assert.True(bitBoard.WhiteCanCastleKingSide);
            Assert.True(bitBoard.WhiteCanCastleQueenSide);
            Assert.False(bitBoard.BlackCanCastleKingSide);
            Assert.False(bitBoard.BlackCanCastleQueenSide);

            bitBoard.UnMakeMove(move);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);
        }

        [Fact]
        public void Make_WhiteStandardKingMoveRemovesRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var bitBoardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var fromSquare = SquareFlag.E1;
            var toSquare = SquareFlag.E2;

            var move = MoveBuilder.Create(Colour.White, PieceType.King, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.False(bitBoard.WhiteCanCastleKingSide);
            Assert.False(bitBoard.WhiteCanCastleQueenSide);
            Assert.True(bitBoard.BlackCanCastleKingSide);
            Assert.True(bitBoard.BlackCanCastleQueenSide);

            bitBoard.UnMakeMove(move);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);
        }

        [Fact]
        public void Make_BlackStandardKingMoveRemovesRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var bitBoardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var fromSquare = SquareFlag.E8;
            var toSquare = SquareFlag.E7;

            var move = MoveBuilder.Create(Colour.Black, PieceType.King, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.True(bitBoard.WhiteCanCastleKingSide);
            Assert.True(bitBoard.WhiteCanCastleQueenSide);
            Assert.False(bitBoard.BlackCanCastleKingSide);
            Assert.False(bitBoard.BlackCanCastleQueenSide);

            bitBoard.UnMakeMove(move);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);
        }

        [Fact]
        public void Make_WhiteStandardRookMoveRemovesQueenSideRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var bitBoardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var fromSquare = SquareFlag.A1;
            var toSquare = SquareFlag.A2;

            var move = MoveBuilder.Create(Colour.White, PieceType.Rook, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.True(bitBoard.WhiteCanCastleKingSide);
            Assert.False(bitBoard.WhiteCanCastleQueenSide);
            Assert.True(bitBoard.BlackCanCastleKingSide);
            Assert.True(bitBoard.BlackCanCastleQueenSide);

            bitBoard.UnMakeMove(move);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);
        }

        [Fact]
        public void Make_WhiteStandardRookMoveRemovesKingSideRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var bitBoardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var fromSquare = SquareFlag.H1;
            var toSquare = SquareFlag.H2;

            var move = MoveBuilder.Create(Colour.White, PieceType.Rook, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.False(bitBoard.WhiteCanCastleKingSide);
            Assert.True(bitBoard.WhiteCanCastleQueenSide);
            Assert.True(bitBoard.BlackCanCastleKingSide);
            Assert.True(bitBoard.BlackCanCastleQueenSide);

            bitBoard.UnMakeMove(move);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);
        }

        [Fact]
        public void Make_BlackStandardRookMoveRemovesQueenSideRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var bitBoardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var fromSquare = SquareFlag.A8;
            var toSquare = SquareFlag.A7;

            var move = MoveBuilder.Create(Colour.Black, PieceType.Rook, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.True(bitBoard.WhiteCanCastleKingSide);
            Assert.True(bitBoard.WhiteCanCastleQueenSide);
            Assert.True(bitBoard.BlackCanCastleKingSide);
            Assert.False(bitBoard.BlackCanCastleQueenSide);

            bitBoard.UnMakeMove(move);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);
        }

        [Fact]
        public void Make_BlackStandardRookMoveRemovesKingSideRights()
        {
            var bitBoard = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var bitBoardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var fromSquare = SquareFlag.H8;
            var toSquare = SquareFlag.H7;

            var move = MoveBuilder.Create(Colour.Black, PieceType.Rook, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.True(bitBoard.WhiteCanCastleKingSide);
            Assert.True(bitBoard.WhiteCanCastleQueenSide);
            Assert.False(bitBoard.BlackCanCastleKingSide);
            Assert.True(bitBoard.BlackCanCastleQueenSide);

            bitBoard.UnMakeMove(move);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);
        }

        private BitBoard Create(string fenString)
        {
            var gameState = FenHelpers.Parse(fenString);

            return BitBoard.FromGameState(gameState);
        }
    }
}
