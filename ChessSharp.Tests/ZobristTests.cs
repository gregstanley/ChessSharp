using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Helpers;
using ChessSharp.Models;
using ChessSharp.Tests;
using Xunit;

namespace ChessSharp.Engine.Tests
{
    public class ZobristTests : IClassFixture<KeyGeneratorFixture>
    {
        private const ulong DefaultKey = 4872067785823381739ul;

        private KeyGeneratorFixture keyGeneratorFixture;

        public ZobristTests(KeyGeneratorFixture keyGeneratorFixture)
        {
            this.keyGeneratorFixture = keyGeneratorFixture;
        }

        protected Zobrist KeyGenerator => keyGeneratorFixture.KeyGenerator;

        [Fact]
        public void DefaultBoard_HasExpectedKey()
        {
            var board = new Board();

            var rootKey = KeyGenerator.Hash(board, Colour.White);

            Assert.Equal(DefaultKey, rootKey);
        }

        [Fact]
        public void DefaultBoard_MakeUnMake_HashAndUpdateAreTheSame()
        {
            var board = new Board();

            var rootKey = KeyGenerator.Hash(board, Colour.White);

            var whiteMove = 28866u;

            Assert.Equal(DefaultKey, rootKey);

            board.MakeMove(whiteMove);

            var hashKey = KeyGenerator.Hash(board, Colour.Black);
            var updateKey = KeyGenerator.UpdateHash(rootKey, whiteMove);

            Assert.Equal(hashKey, board.Key);
            Assert.Equal(updateKey, board.Key);

            board.UnMakeMove(whiteMove);

            var hashKey2 = KeyGenerator.Hash(board, Colour.White);
            var updateKey2 = KeyGenerator.UpdateHash(updateKey, whiteMove);

            Assert.Equal(hashKey2, board.Key);
            Assert.Equal(updateKey2, board.Key);
        }

        [Fact]
        public void DefaultBoard_MakeUnMake_CorrectKeys()
        {
            var board = new Board();

            var rootKey = KeyGenerator.Hash(board, Colour.White);

            var whiteMove = 28866u;

            Assert.Equal(DefaultKey, rootKey);

            board.MakeMove(whiteMove);

            board.UnMakeMove(whiteMove);

            Assert.Equal(DefaultKey, board.Key);
        }

        [Fact]
        public void Board_MakeUnMake_WhiteCaptureCorrectKeys()
        {
            var board = CreateBitBoard("K6k/8/8/8/8/3p4/4P3/8 w - -");

            var rootKey = KeyGenerator.Hash(board, Colour.White);

            var capture = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.E2.ToSquare(), SquareFlag.D3.ToSquare(), PieceType.Pawn, MoveType.Ordinary);

            var beforeKey = board.Key;

            board.MakeMove(capture);

            var afterKey = board.Key;

            board.UnMakeMove(capture);

            Assert.NotEqual(beforeKey, afterKey);
            Assert.Equal(beforeKey, board.Key);
        }

        [Fact]
        public void Board_MakeUnMake_WhiteEnPassantCapture()
        {
            var board = CreateBitBoard("K6k/8/8/3Pp3/8/8/8/8 w - e6");

            var rootKey = KeyGenerator.Hash(board, Colour.White);

            var enPassantCapture = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D5.ToSquare(), SquareFlag.E6.ToSquare(), PieceType.Pawn, MoveType.EnPassant);

            var beforeKey = board.Key;

            board.MakeMove(enPassantCapture);

            var hashKey = KeyGenerator.Hash(board, Colour.Black);
            var updateKey = KeyGenerator.UpdateHash(rootKey, enPassantCapture);

            Assert.Equal(hashKey, board.Key);
            Assert.Equal(updateKey, board.Key);

            var afterKey = board.Key;

            board.UnMakeMove(enPassantCapture);

            var hashKey2 = KeyGenerator.Hash(board, Colour.White);
            var updateKey2 = KeyGenerator.UpdateHash(updateKey, enPassantCapture);

            Assert.Equal(hashKey2, board.Key);
            Assert.Equal(updateKey2, board.Key);

            Assert.NotEqual(beforeKey, afterKey);
            Assert.Equal(beforeKey, board.Key);
        }

        [Fact]
        public void Board_MakeUnMake_CastleKeysCorrect()
        {
            var board = CreateBitBoard("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");

            var rootKey = KeyGenerator.Hash(board, Colour.White);

            var castle = MoveBuilder.Create(
                Colour.White,
                PieceType.King,
                SquareFlagConstants.WhiteKingStartSquare.ToSquare(),
                SquareFlagConstants.WhiteKingSideRookStartSquare.ToSquare(),
                PieceType.None,
                MoveType.CastleKing);

            var beforeKey = board.Key;

            board.MakeMove(castle);

            var hashKey = KeyGenerator.Hash(board, Colour.Black);
            var updateKey = KeyGenerator.UpdateHash(rootKey, castle);

            Assert.Equal(hashKey, board.Key);
            Assert.Equal(updateKey, board.Key);

            var afterKey = board.Key;

            board.UnMakeMove(castle);

            var hashKey2 = KeyGenerator.Hash(board, Colour.White);
            var updateKey2 = KeyGenerator.UpdateHash(updateKey, castle);

            Assert.Equal(hashKey2, board.Key);
            Assert.Equal(updateKey2, board.Key);

            Assert.NotEqual(beforeKey, afterKey);
            Assert.Equal(beforeKey, board.Key);
        }

        [Fact]
        public void White_Empty_OnePush_Promotion()
        {
            var board = CreateBitBoard("K6k/3P4/8/8/8/8/8/8 w - -");

            var rootKey = KeyGenerator.Hash(board, Colour.White);

            var promotion1 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D8.ToSquare(), PieceType.None, MoveType.PromotionQueen);
            var promotion2 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D8.ToSquare(), PieceType.None, MoveType.PromotionRook);
            var promotion3 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D8.ToSquare(), PieceType.None, MoveType.PromotionBishop);
            var promotion4 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D8.ToSquare(), PieceType.None, MoveType.PromotionKnight);

            var beforeKey = board.Key;

            board.MakeMove(promotion1);

            var hashKey = KeyGenerator.Hash(board, Colour.Black);
            var updateKey = KeyGenerator.UpdateHash(rootKey, promotion1);

            Assert.Equal(hashKey, board.Key);
            Assert.Equal(updateKey, board.Key);

            var afterKey = board.Key;

            board.UnMakeMove(promotion1);

            var hashKey2 = KeyGenerator.Hash(board, Colour.White);
            var updateKey2 = KeyGenerator.UpdateHash(updateKey, promotion1);

            Assert.Equal(hashKey2, board.Key);
            Assert.Equal(updateKey2, board.Key);

            Assert.NotEqual(beforeKey, afterKey);
            Assert.Equal(beforeKey, board.Key);
        }

        protected Board CreateBitBoard(string fen) =>
            Board.FromGameState(FenHelpers.Parse(fen));
    }
}
