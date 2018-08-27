using Chess.Engine.Bit;
using Chess.Engine.Models;
using Xunit;

namespace Chess.Engine.Tests
{
    public class BitBoardTests
    {
        [Fact]
        public void Bitboard_FromFen()
        {
            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            var fen = Fen.Parse(Fen.Default);

            var bitBoard = BitBoard.FromFen(fen);

            var whiteKingSquare = bitBoard.FindKingSquare(Colour.White);

            Assert.Equal(SquareFlag.E1, whiteKingSquare);
        }

        [Fact]
        public void Bitboard_FromFen_Position2()
        {
            // "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -";
            var fen = Fen.Parse(Fen.Position2);

            var bitBoard = BitBoard.FromFen(fen);

            var whiteKingSquare = bitBoard.FindKingSquare(Colour.White);

            Assert.Equal(SquareFlag.E1, whiteKingSquare);
        }

        [Fact]
        public void Bitboard_FromFen_Position5()
        {
            // "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"
            var fen = Fen.Parse(Fen.Position5);

            var bitBoard = BitBoard.FromFen(fen);

            var whiteKingSquare = bitBoard.FindKingSquare(Colour.White);

            Assert.Equal(SquareFlag.E1, whiteKingSquare);
        }
    }
}
