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

        protected Zobrist KeyGenerator => _keyGeneratorFixture.KeyGenerator;

        private KeyGeneratorFixture _keyGeneratorFixture;

        public ZobristTests(KeyGeneratorFixture keyGeneratorFixture)
        {
            _keyGeneratorFixture = keyGeneratorFixture;
        }

        [Fact]
        public void DefaultBoard_HasExpectedKey()
        {
            var bitBoard = new BitBoard();

            var rootKey = KeyGenerator.Hash(bitBoard, Colour.White);

            Assert.Equal(DefaultKey, rootKey);
        }

        [Fact]
        public void DefaultBoard_MakeUnMake_HashAndUpdateAreTheSame()
        {
            var bitBoard = new BitBoard();

            var rootKey = KeyGenerator.Hash(bitBoard, Colour.White);

            var whiteMove = 28866u;

            Assert.Equal(DefaultKey, rootKey);

            bitBoard.MakeMove(whiteMove);

            var hashKey = KeyGenerator.Hash(bitBoard, Colour.Black);
            var updateKey = KeyGenerator.UpdateHash(rootKey, whiteMove);

            Assert.Equal(hashKey, bitBoard.Key);
            Assert.Equal(updateKey, bitBoard.Key);

            bitBoard.UnMakeMove(whiteMove);

            var hashKey2 = KeyGenerator.Hash(bitBoard, Colour.White);
            var updateKey2 = KeyGenerator.UpdateHash(updateKey, whiteMove);

            Assert.Equal(hashKey2, bitBoard.Key);
            Assert.Equal(updateKey2, bitBoard.Key);
        }

        [Fact]
        public void DefaultBoard_MakeUnMake_CorrectKeys()
        {
            var bitBoard = new BitBoard();

            var rootKey = KeyGenerator.Hash(bitBoard, Colour.White);

            var whiteMove = 28866u;

            Assert.Equal(DefaultKey, rootKey);

            bitBoard.MakeMove(whiteMove);

            bitBoard.UnMakeMove(whiteMove);

            Assert.Equal(DefaultKey, bitBoard.Key);
        }

        [Fact]
        public void Board_MakeUnMake_WhiteCaptureCorrectKeys()
        {
            var bitBoard = CreateBitBoard("K6k/8/8/8/8/3p4/4P3/8 w - -");

            var rootKey = KeyGenerator.Hash(bitBoard, Colour.White);

            var capture = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.E2.ToSquare(), SquareFlag.D3.ToSquare(), PieceType.Pawn, MoveType.Ordinary);

            var beforeKey = bitBoard.Key;

            bitBoard.MakeMove(capture);

            var afterKey = bitBoard.Key;

            bitBoard.UnMakeMove(capture);

            Assert.NotEqual(beforeKey, afterKey);
            Assert.Equal(beforeKey, bitBoard.Key);
        }

        [Fact]
        public void Board_MakeUnMake_WhiteEnPassantCapture()
        {
            var bitBoard = CreateBitBoard("K6k/8/8/3Pp3/8/8/8/8 w - e6");

            var rootKey = KeyGenerator.Hash(bitBoard, Colour.White);

            var enPassantCapture = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D5.ToSquare(), SquareFlag.E6.ToSquare(), PieceType.Pawn, MoveType.EnPassant);

            var beforeKey = bitBoard.Key;

            bitBoard.MakeMove(enPassantCapture);

            var hashKey = KeyGenerator.Hash(bitBoard, Colour.Black);
            var updateKey = KeyGenerator.UpdateHash(rootKey, enPassantCapture);

            Assert.Equal(hashKey, bitBoard.Key);
            Assert.Equal(updateKey, bitBoard.Key);

            var afterKey = bitBoard.Key;

            bitBoard.UnMakeMove(enPassantCapture);

            var hashKey2 = KeyGenerator.Hash(bitBoard, Colour.White);
            var updateKey2 = KeyGenerator.UpdateHash(updateKey, enPassantCapture);

            Assert.Equal(hashKey2, bitBoard.Key);
            Assert.Equal(updateKey2, bitBoard.Key);

            Assert.NotEqual(beforeKey, afterKey);
            Assert.Equal(beforeKey, bitBoard.Key);
        }

        [Fact]
        public void Board_MakeUnMake_CastleKeysCorrect()
        {
            var bitBoard = CreateBitBoard("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");

            var rootKey = KeyGenerator.Hash(bitBoard, Colour.White);

            var castle = MoveBuilder.Create(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingStartSquare.ToSquare(),
                SquareFlagConstants.WhiteKingSideRookStartSquare.ToSquare(), PieceType.None, MoveType.CastleKing);

            var beforeKey = bitBoard.Key;

            bitBoard.MakeMove(castle);

            var hashKey = KeyGenerator.Hash(bitBoard, Colour.Black);
            var updateKey = KeyGenerator.UpdateHash(rootKey, castle);

            Assert.Equal(hashKey, bitBoard.Key);
            Assert.Equal(updateKey, bitBoard.Key);

            var afterKey = bitBoard.Key;

            bitBoard.UnMakeMove(castle);

            var hashKey2 = KeyGenerator.Hash(bitBoard, Colour.White);
            var updateKey2 = KeyGenerator.UpdateHash(updateKey, castle);

            Assert.Equal(hashKey2, bitBoard.Key);
            Assert.Equal(updateKey2, bitBoard.Key);

            Assert.NotEqual(beforeKey, afterKey);
            Assert.Equal(beforeKey, bitBoard.Key);
        }

        [Fact]
        public void White_Empty_OnePush_Promotion()
        {
            var bitBoard = CreateBitBoard("K6k/3P4/8/8/8/8/8/8 w - -");

            var rootKey = KeyGenerator.Hash(bitBoard, Colour.White);

            var promotion1 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D8.ToSquare(), PieceType.None, MoveType.PromotionQueen);
            var promotion2 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D8.ToSquare(), PieceType.None, MoveType.PromotionRook);
            var promotion3 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D8.ToSquare(), PieceType.None, MoveType.PromotionBishop);
            var promotion4 = MoveBuilder.Create(Colour.White, PieceType.Pawn, SquareFlag.D7.ToSquare(), SquareFlag.D8.ToSquare(), PieceType.None, MoveType.PromotionKnight);

            var beforeKey = bitBoard.Key;

            bitBoard.MakeMove(promotion1);

            var hashKey = KeyGenerator.Hash(bitBoard, Colour.Black);
            var updateKey = KeyGenerator.UpdateHash(rootKey, promotion1);

            Assert.Equal(hashKey, bitBoard.Key);
            Assert.Equal(updateKey, bitBoard.Key);

            var afterKey = bitBoard.Key;

            bitBoard.UnMakeMove(promotion1);

            var hashKey2 = KeyGenerator.Hash(bitBoard, Colour.White);
            var updateKey2 = KeyGenerator.UpdateHash(updateKey, promotion1);

            Assert.Equal(hashKey2, bitBoard.Key);
            Assert.Equal(updateKey2, bitBoard.Key);

            Assert.NotEqual(beforeKey, afterKey);
            Assert.Equal(beforeKey, bitBoard.Key);
        }

        protected BitBoard CreateBitBoard(string fen) =>
            BitBoard.FromGameState(FenHelpers.Parse(fen));
    }
}
