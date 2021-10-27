using ChessSharp.Common;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Extensions;
using ChessSharp.Common.Helpers;
using Xunit;

namespace ChessSharp.Tests
{
    public class BoardTests
    {
        [Theory]
        [InlineData("8/P3p3", SquareFlag.D3)]
        [InlineData("4p3/P7", (SquareFlag)0)]
        public void Make_EnPassantOnlySetIfCapturePossible(string partialFen, SquareFlag enPassant)
        {
            var board = Create($"rnbqkbnr/pppp1ppp/8/{partialFen}/1P6/2PPPPPP/RNBQKBNR b KQkq -");

            var fromSquare = SquareFlag.D2;
            var toSquare = SquareFlag.D4;

            var move = MoveBuilder.Create(Colour.White, PieceType.Pawn, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            board.MakeMove(move);

            Assert.Equal(enPassant, board.EnPassant);
        }

        [Fact]
        public void Make_UnMake_Correct1()
        {
            var board = Create("rnbqkbnr/pppp1ppp/8/8/P3p3/1P6/2PPPPPP/RNBQKBNR b KQkq -");

            var fromSquare = SquareFlag.D2;
            var toSquare = SquareFlag.D4;

            var move = MoveBuilder.Create(Colour.White, PieceType.Pawn, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            board.MakeMove(move);

            Assert.Equal(SquareFlag.D3, board.EnPassant);

            board.UnMakeMove(move);

            Assert.Equal((SquareFlag)0, board.EnPassant);
        }

        [Fact]
        public void Make_UnMake_KnightCaptures_Correct()
        {
            var board = Create("7k/8/3p4/8/4N3/8/8/K7 w - - 0 1");
            var boardReference = Create("7k/8/3p4/8/4N3/8/8/K7 w - - 0 1");
            var boardAfterMake = Create("7k/8/3N4/8/8/8/8/K7 w - - 0 1");

            var fromSquare = SquareFlag.E4;
            var toSquare = SquareFlag.D6;

            var move = MoveBuilder.Create(Colour.White, PieceType.Knight, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.Pawn, MoveType.Ordinary);

            board.MakeMove(move);

            TestHelpers.AssertEqual(board, boardAfterMake);

            board.UnMakeMove(move);

            TestHelpers.AssertEqual(board, boardReference);
        }

        [Fact]
        public void Make_UnMake_EnPassant_Correct()
        {
            var board = Create("4k3/8/8/8/5Pp1/8/8/4K3 w - f3 0 1");

            var boardReference = Create("4k3/8/8/8/5Pp1/8/8/4K3 w - f3 0 1");

            var fromSquare = SquareFlag.G4;
            var toSquare = SquareFlag.F3;

            var move = MoveBuilder.Create(Colour.Black, PieceType.Pawn, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.Pawn, MoveType.EnPassant);

            board.MakeMove(move);

            Assert.NotEqual(board.White, boardReference.White);
            Assert.NotEqual(board.Black, boardReference.Black);
            Assert.NotEqual(board.EnPassant, boardReference.EnPassant);

            board.UnMakeMove(move);

            TestHelpers.AssertEqual(board, boardReference);
        }

        [Fact]
        public void Make_Capture_BlackRookRemovesRights()
        {
            var board = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
            var boardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");

            var fromSquare = SquareFlag.A1;
            var toSquare = SquareFlag.A8;

            var move = MoveBuilder.Create(Colour.White, PieceType.Rook, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.Rook, MoveType.Ordinary);

            board.MakeMove(move);

            Assert.True(board.WhiteCanCastleKingSide);
            Assert.False(board.WhiteCanCastleQueenSide);
            Assert.True(board.BlackCanCastleKingSide);
            Assert.False(board.BlackCanCastleQueenSide);

            board.UnMakeMove(move);

            TestHelpers.AssertEqual(board, boardReference);
        }

        [Fact]
        public void Make_WhiteCastleRemovesRights()
        {
            var board = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var boardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var move = MoveBuilder.CreateCastle(Colour.White, MoveType.CastleKing);

            board.MakeMove(move);

            Assert.False(board.WhiteCanCastleKingSide);
            Assert.False(board.WhiteCanCastleQueenSide);
            Assert.True(board.BlackCanCastleKingSide);
            Assert.True(board.BlackCanCastleQueenSide);

            board.UnMakeMove(move);

            TestHelpers.AssertEqual(board, boardReference);
        }

        [Fact]
        public void Make_BlackCastleRemovesRights()
        {
            var board = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var boardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var move = MoveBuilder.CreateCastle(Colour.Black, MoveType.CastleKing);

            board.MakeMove(move);

            Assert.True(board.WhiteCanCastleKingSide);
            Assert.True(board.WhiteCanCastleQueenSide);
            Assert.False(board.BlackCanCastleKingSide);
            Assert.False(board.BlackCanCastleQueenSide);

            board.UnMakeMove(move);

            TestHelpers.AssertEqual(board, boardReference);
        }

        [Fact]
        public void Make_WhiteStandardKingMoveRemovesRights()
        {
            var board = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var boardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var fromSquare = SquareFlag.E1;
            var toSquare = SquareFlag.E2;

            var move = MoveBuilder.Create(Colour.White, PieceType.King, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            board.MakeMove(move);

            Assert.False(board.WhiteCanCastleKingSide);
            Assert.False(board.WhiteCanCastleQueenSide);
            Assert.True(board.BlackCanCastleKingSide);
            Assert.True(board.BlackCanCastleQueenSide);

            board.UnMakeMove(move);

            TestHelpers.AssertEqual(board, boardReference);
        }

        [Fact]
        public void Make_BlackStandardKingMoveRemovesRights()
        {
            var board = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var boardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var fromSquare = SquareFlag.E8;
            var toSquare = SquareFlag.E7;

            var move = MoveBuilder.Create(Colour.Black, PieceType.King, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            board.MakeMove(move);

            Assert.True(board.WhiteCanCastleKingSide);
            Assert.True(board.WhiteCanCastleQueenSide);
            Assert.False(board.BlackCanCastleKingSide);
            Assert.False(board.BlackCanCastleQueenSide);

            board.UnMakeMove(move);

            TestHelpers.AssertEqual(board, boardReference);
        }

        [Fact]
        public void Make_WhiteStandardRookMoveRemovesQueenSideRights()
        {
            var board = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var boardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var fromSquare = SquareFlag.A1;
            var toSquare = SquareFlag.A2;

            var move = MoveBuilder.Create(Colour.White, PieceType.Rook, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            board.MakeMove(move);

            Assert.True(board.WhiteCanCastleKingSide);
            Assert.False(board.WhiteCanCastleQueenSide);
            Assert.True(board.BlackCanCastleKingSide);
            Assert.True(board.BlackCanCastleQueenSide);

            board.UnMakeMove(move);

            TestHelpers.AssertEqual(board, boardReference);
        }

        [Fact]
        public void Make_WhiteStandardRookMoveRemovesKingSideRights()
        {
            var board = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var boardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var fromSquare = SquareFlag.H1;
            var toSquare = SquareFlag.H2;

            var move = MoveBuilder.Create(Colour.White, PieceType.Rook, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            board.MakeMove(move);

            Assert.False(board.WhiteCanCastleKingSide);
            Assert.True(board.WhiteCanCastleQueenSide);
            Assert.True(board.BlackCanCastleKingSide);
            Assert.True(board.BlackCanCastleQueenSide);

            board.UnMakeMove(move);

            TestHelpers.AssertEqual(board, boardReference);
        }

        [Fact]
        public void Make_BlackStandardRookMoveRemovesQueenSideRights()
        {
            var board = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var boardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var fromSquare = SquareFlag.A8;
            var toSquare = SquareFlag.A7;

            var move = MoveBuilder.Create(Colour.Black, PieceType.Rook, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            board.MakeMove(move);

            Assert.True(board.WhiteCanCastleKingSide);
            Assert.True(board.WhiteCanCastleQueenSide);
            Assert.True(board.BlackCanCastleKingSide);
            Assert.False(board.BlackCanCastleQueenSide);

            board.UnMakeMove(move);

            TestHelpers.AssertEqual(board, boardReference);
        }

        [Fact]
        public void Make_BlackStandardRookMoveRemovesKingSideRights()
        {
            var board = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");
            var boardReference = Create("r3k2r/8/8/8/8/8/8/R3K2R w KQkq -");

            var fromSquare = SquareFlag.H8;
            var toSquare = SquareFlag.H7;

            var move = MoveBuilder.Create(Colour.Black, PieceType.Rook, fromSquare.ToSquare(), toSquare.ToSquare(), PieceType.None, MoveType.Ordinary);

            board.MakeMove(move);

            Assert.True(board.WhiteCanCastleKingSide);
            Assert.True(board.WhiteCanCastleQueenSide);
            Assert.False(board.BlackCanCastleKingSide);
            Assert.True(board.BlackCanCastleQueenSide);

            board.UnMakeMove(move);

            TestHelpers.AssertEqual(board, boardReference);
        }

        private Board Create(string fenString)
        {
            var gameState = FenHelpers.Parse(fenString);

            return Board.FromGameState(gameState);
        }
    }
}
