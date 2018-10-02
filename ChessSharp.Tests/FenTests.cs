using ChessSharp.Enums;
using System.Linq;
using Xunit;

namespace ChessSharp.Tests
{
    public class FenTests
    {
        [Fact]
        public void Fen_Parse_Default_Correct()
        {
            var fen = Fen.Parse(Fen.Default);

            Assert.Equal("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", fen.Board);
            Assert.Equal("RNBQKBNR", fen.Rank1);
            Assert.Equal("PPPPPPPP", fen.Rank2);
            Assert.Equal("8", fen.Rank3);
            Assert.Equal("8", fen.Rank4);
            Assert.Equal("8", fen.Rank5);
            Assert.Equal("8", fen.Rank6);
            Assert.Equal("pppppppp", fen.Rank7);
            Assert.Equal("rnbqkbnr", fen.Rank8);
            Assert.Equal(Colour.White, fen.ToPlay);
            Assert.True(fen.BoardState.HasFlag(BoardState.WhiteCanCastleKingSide));
            Assert.True(fen.BoardState.HasFlag(BoardState.WhiteCanCastleQueenSide));
            Assert.True(fen.BoardState.HasFlag(BoardState.BlackCanCastleKingSide));
            Assert.True(fen.BoardState.HasFlag(BoardState.BlackCanCastleQueenSide));
            Assert.Equal((SquareFlag)0, fen.EnPassantSquare);
            Assert.Equal(0, fen.HalfTurnCounter);
            Assert.Equal(1, fen.FullMoveNumber);

            var boardArray = fen.GetExpandedBoardCharArray();

            Assert.Equal('R', boardArray[0]);
            Assert.Equal('N', boardArray[1]);
            Assert.Equal('B', boardArray[2]);
            Assert.Equal('Q', boardArray[3]);
            Assert.Equal('K', boardArray[4]);
            Assert.Equal('B', boardArray[5]);
            Assert.Equal('N', boardArray[6]);
            Assert.Equal('R', boardArray[7]);

            var squares = fen.GetSquaresStates();

            Assert.Equal(PieceType.Rook, squares.ElementAt(0).Type);
        }

        [Fact]
        public void Fen_Parse_Position2_Correct()
        {
            var fen = Fen.Parse(Fen.Position2);

            Assert.Equal("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R", fen.Board);
            Assert.Equal(Colour.White, fen.ToPlay);
            Assert.True(fen.BoardState.HasFlag(BoardState.WhiteCanCastleKingSide));
            Assert.True(fen.BoardState.HasFlag(BoardState.WhiteCanCastleQueenSide));
            Assert.True(fen.BoardState.HasFlag(BoardState.BlackCanCastleKingSide));
            Assert.True(fen.BoardState.HasFlag(BoardState.BlackCanCastleQueenSide));
            Assert.Equal((SquareFlag)0, fen.EnPassantSquare);
        }

        [Fact]
        public void Fen_Parse_Position5_Correct()
        {
            //"rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"
            var fen = Fen.Parse(Fen.Position5);

            Assert.Equal("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R", fen.Board);
            Assert.Equal(Colour.White, fen.ToPlay);
            Assert.True(fen.BoardState.HasFlag(BoardState.WhiteCanCastleKingSide));
            Assert.True(fen.BoardState.HasFlag(BoardState.WhiteCanCastleQueenSide));
            Assert.False(fen.BoardState.HasFlag(BoardState.BlackCanCastleKingSide));
            Assert.False(fen.BoardState.HasFlag(BoardState.BlackCanCastleQueenSide));
            Assert.Equal((SquareFlag)0, fen.EnPassantSquare);
            Assert.Equal(1, fen.HalfTurnCounter);
            Assert.Equal(8, fen.FullMoveNumber);
        }

        [Theory]
        [InlineData("-", (SquareFlag)0)]
        [InlineData("a1", SquareFlag.A1)]
        [InlineData("e3", SquareFlag.E3)]
        [InlineData("h8", SquareFlag.H8)]
        public void Fen_EnPassant_Correct(string enPassantSquareFen, SquareFlag enPassantSquare)
        {
            var fen = Fen.Parse($"rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR w KQ {enPassantSquareFen} 1 8");

            Assert.Equal(enPassantSquare, fen.EnPassantSquare);
        }
    }
}
