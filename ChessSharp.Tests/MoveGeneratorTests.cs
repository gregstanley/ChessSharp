using ChessSharp.Enums;
using ChessSharp.Models;
using System.Collections.Generic;
using Xunit;

namespace ChessSharp.Tests
{
    public class MoveGeneratorTests
    {
        [Fact]
        public void Pawn_Empty_OnePush_Correct()
        {
            var bitBoard = Create("8/8/8/8/3P4/8/8/8 w KQkq -");

            var moveGenerator = new MoveGenerator();

            var moves = new List<Move>(10);

            moveGenerator.GeneratePawnMoves(bitBoard, Colour.White, moves);

            Assert.Collection(moves, x => Assert.Equal(SquareFlag.D5, x.To));
        }

        private BitBoard Create(string fenString)
        {
            var fen = Fen.Parse(fenString);
            return BitBoard.FromFen(fen);
        }
    }
}
