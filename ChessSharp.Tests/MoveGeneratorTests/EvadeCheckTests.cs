using ChessSharp.MoveGeneration;
using System.Collections.Generic;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class EvadeCheckTests : MoveGeneratorTestsBase
    {
        public EvadeCheckTests(MoveGeneratorFixture moveGeneratorFixture)
            : base(moveGeneratorFixture)
        {
        }

        [Theory]
        [InlineData("4k3/8/8/8/8/5q2/8/r2QK2r w - - 0 1")]
        public void OnlyKingMovesAllowed(string fenString)
        {
            var fen = Fen.Parse(fenString);

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);

            var pawnMoveViews = GetPawnMoveViews(moves);
            var rookMoveViews = GetRookMoveViews(moves);
            var knightMoveViews = GetKnightMoveViews(moves);
            var bishopMoveViews = GetBishopMoveViews(moves);
            var queenMoveViews = GetQueenMoveViews(moves);
            var kingMoveViews = GetKingMoveViews(moves);

            Assert.Equal(0, rookMoveViews.Count);
            Assert.Equal(0, queenMoveViews.Count);
            Assert.Equal(1, kingMoveViews.Count);
        }

        [Theory]
        [InlineData("8/1P6/3RN3/k1B4Q/2K4q/3P4/4P3/8 w - - 0 1")]
        [InlineData("8/4p3/3p4/2k4Q/K1b4q/3rn3/8/8 b - - 0 1")]
        public void OnlyBlocksAndKingMovesAllowed(string fenString)
        {
            var fen = Fen.Parse(fenString);

            var bitBoard = CreateBitBoard(fen);

            var moves = new List<uint>(10);

            //MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);
            var workspace = new MoveGenerationWorkspace(bitBoard, fen.ToPlay);

            MoveGenerator.Generate(workspace, moves);
            var pawnMoveViews = GetPawnMoveViews(moves);
            var rookMoveViews = GetRookMoveViews(moves);
            var knightMoveViews = GetKnightMoveViews(moves);
            var bishopMoveViews = GetBishopMoveViews(moves);
            var queenMoveViews = GetQueenMoveViews(moves);
            var kingMoveViews = GetKingMoveViews(moves);

            Assert.Equal(2, pawnMoveViews.Count);
            Assert.Equal(1, rookMoveViews.Count);
            Assert.Equal(2, knightMoveViews.Count);
            Assert.Equal(1, bishopMoveViews.Count);
            Assert.Equal(2, queenMoveViews.Count);
            Assert.Equal(3, kingMoveViews.Count);
        }
    }
}
