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

            var moveView = moves.Select(x => new MoveViewer(x));
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

        [Fact]
        public void DefaultPosition2()
        {
            var perftRunner = new PerftRunner(_moveGeneratorFixture.MoveGenerator);

            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            var fen = Fen.Parse(Fen.Default);

            var bitBoard = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            var count = perftRunner.Go(bitBoard, fen.ToPlay, 2);

            Assert.Equal(400, count);
        }

        [Fact]
        public void DefaultPosition3()
        {
            var perftRunner = new PerftRunner(_moveGeneratorFixture.MoveGenerator);

            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            var fen = Fen.Parse(Fen.Default);

            var bitBoard = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            var count = perftRunner.Go(bitBoard, fen.ToPlay, 3);

            Assert.Equal(8902, count);
        }

        [Fact]
        public void GenerateChildBoards_Position3()
        {
            var perftRunner = new PerftRunnerMetrics(_moveGeneratorFixture.MoveGenerator);

            // "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -"
            var fen = Fen.Parse(Fen.Position3);

            var bitBoard = BitBoard.FromFen(fen);

            var metrics = new Dictionary<int, PerftMetrics>();

            //var count1 = perftRunner.Go(bitBoard, fen.ToPlay, 1);
            //var count2 = perftRunner.Go(bitBoard, fen.ToPlay, 2);
            //var count3 = perftRunner.Go(bitBoard, fen.ToPlay, 3);
            perftRunner.Go(bitBoard, fen.ToPlay, 3, metrics);

            Assert.Equal(14, metrics[3].Legal);
            Assert.Equal(191, metrics[2].Legal);
            Assert.Equal(2812, metrics[1].Legal);
            //Assert.Equal(14, metrics1.Legal);
            //Assert.Equal(1, metrics1.Captures);
            //Assert.Equal(0, metrics1.EnPassantCaptures);
            //Assert.Equal(0, metrics1.Castles);
            //Assert.Equal(2, metrics1.Checks);
            //Assert.Equal(191, metrics2.Legal);
            //Assert.Equal(14, metrics2.Captures);
            //Assert.Equal(0, metrics2.EnPassantCaptures);
            //Assert.Equal(0, metrics2.Castles);
            //Assert.Equal(10, metrics2.Checks);
            //Assert.Equal(2812, metrics3.Legal);
            //Assert.Equal(209, metrics3.Captures);
            //Assert.Equal(2, metrics3.EnPassantCaptures);
            //Assert.Equal(0, metrics3.Castles);
            //Assert.Equal(267, metrics3.Checks);
            //Assert.Equal(43238, metrics4.Legal);
            //Assert.Equal(3348, metrics4.Captures);
            //Assert.Equal(123, metrics4.EnPassantCaptures);
            //Assert.Equal(0, metrics4.Castles);
            //Assert.Equal(1680, metrics4.Checks);
            //Assert.Equal(674624, metrics5.Legal);
            //Assert.Equal(52051, metrics5.Captures);
            //Assert.Equal(1165, metrics5.EnPassantCaptures);
            //Assert.Equal(0, metrics5.Castles);
            //Assert.Equal(52950, metrics5.Checks);
        }

    }
}
