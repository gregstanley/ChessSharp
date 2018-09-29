using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ChessSharp.Tests
{
    public class PerftTests : IClassFixture<MoveGeneratorFixture>
    {
        MoveGeneratorFixture _moveGeneratorFixture;

        public PerftTests(MoveGeneratorFixture moveGeneratorFixture)
        {
            _moveGeneratorFixture = moveGeneratorFixture;
        }

        [Fact]
        public void DefaultPosition()
        {
            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            var fen = Fen.Parse(Fen.Default);

            var bitBoard = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            _moveGeneratorFixture.MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);

            var wrappedMoves = moves.Select(x => new MoveWrapper(x));
            var moveCount = moves.Count;

            Assert.Equal(20, moveCount);
            //Assert.Equal(20, metrics1.Legal);
            //Assert.Equal(0, metrics1.Captures);
            //Assert.Equal(0, metrics1.EnPassantCaptures);
            //Assert.Equal(0, metrics1.Castles);
            //Assert.Equal(0, metrics1.Checks);
            //Assert.Equal(0, metrics1.Checkmates);
            //Assert.Equal(400, metrics2.Legal);
            //Assert.Equal(0, metrics2.Captures);
            //Assert.Equal(0, metrics2.EnPassantCaptures);
            //Assert.Equal(0, metrics2.Castles);
            //Assert.Equal(0, metrics2.Checks);
            //Assert.Equal(0, metrics2.Checkmates);
            //Assert.Equal(8902, metrics3.Legal);
            //Assert.Equal(34, metrics3.Captures);
            //Assert.Equal(0, metrics3.EnPassantCaptures);
            //Assert.Equal(0, metrics3.Castles);
            //Assert.Equal(12, metrics3.Checks);
            //Assert.Equal(0, metrics3.Checkmates);
            //Assert.Equal(197281, metrics4.Legal);
            //Assert.Equal(1576, metrics4.Captures);
            //Assert.Equal(0, metrics4.EnPassantCaptures);
            //Assert.Equal(0, metrics4.Castles);
            //Assert.Equal(469, metrics4.Checks);
            //Assert.Equal(8, metrics4.Checkmates);
        }
    }
}
