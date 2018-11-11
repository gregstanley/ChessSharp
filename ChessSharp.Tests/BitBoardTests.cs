using ChessSharp.Enums;
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

            var move = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D2, SquareFlag.D4, PieceType.None, MoveType.Ordinary);

            bitBoard.MakeMove(move);

            Assert.Equal(enPassant, bitBoard.EnPassant);
        }

        [Fact]
        public void Make_UnMake_Correct1()
        {
            var bitBoard = Create("rnbqkbnr/pppp1ppp/8/8/P3p3/1P6/2PPPPPP/RNBQKBNR b KQkq -");

            var move = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D2, SquareFlag.D4, PieceType.None, MoveType.Ordinary);

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

            var move = MoveBuilder.Create(Colour.White, PieceType.Knight, SquareFlag.E4, SquareFlag.D6, PieceType.Pawn, MoveType.Ordinary);

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

            var move = MoveBuilder.Create(Colour.Black, PieceType.Pawn, SquareFlag.G4, SquareFlag.F3, PieceType.Pawn, MoveType.EnPassant);

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

            var move = MoveBuilder.Create(Colour.White, PieceType.Rook, SquareFlag.A1, SquareFlag.A8, PieceType.Rook, MoveType.Ordinary);

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

            var move = MoveBuilder.Create(Colour.White, PieceType.King, SquareFlag.E1, SquareFlag.E2, PieceType.None, MoveType.Ordinary);

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

            var move = MoveBuilder.Create(Colour.Black, PieceType.King, SquareFlag.E8, SquareFlag.E7, PieceType.None, MoveType.Ordinary);

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

            var move = MoveBuilder.Create(Colour.White, PieceType.Rook, SquareFlag.A1, SquareFlag.A2, PieceType.None, MoveType.Ordinary);

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

            var move = MoveBuilder.Create(Colour.White, PieceType.Rook, SquareFlag.H1, SquareFlag.H2, PieceType.None, MoveType.Ordinary);

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

            var move = MoveBuilder.Create(Colour.Black, PieceType.Rook, SquareFlag.A8, SquareFlag.A7, PieceType.None, MoveType.Ordinary);

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

            var move = MoveBuilder.Create(Colour.Black, PieceType.Rook, SquareFlag.H8, SquareFlag.H7, PieceType.None, MoveType.Ordinary);

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
            var fen = Fen.Parse(fenString);

            return BitBoard.FromFen(fen);
        }
    }
}
