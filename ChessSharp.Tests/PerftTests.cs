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
        public void Default_Depth1Only()
        {
            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            var fen = Fen.Parse(Fen.Default);

            var bitBoard = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            _moveGeneratorFixture.MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);

            var moveView = moves.Select(x => new MoveViewer(x));
            var moveCount = moves.Count;


            //Assert.Equal(20, moveCount);
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
        public void Default_Metrics_ToDepth4()
        {
            var perftRunner = new PerftRunnerMetrics(_moveGeneratorFixture.MoveGenerator);

            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            var fen = Fen.Parse(Fen.Default);

            var bitBoard = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            var metrics = new Dictionary<int, PerftMetrics>();

            var depth = 5;

            perftRunner.Go(bitBoard, fen.ToPlay, depth, metrics);

            Assert.Equal(20, metrics[depth].Legal);
            Assert.Equal(0, metrics[depth].Captures);
            Assert.Equal(0, metrics[depth].EnPassantCaptures);
            Assert.Equal(0, metrics[depth].Castles);
            //Assert.Equal(0, metrics[depth].Checks);

            Assert.Equal(400, metrics[depth - 1].Legal);
            Assert.Equal(0, metrics[depth - 1].Captures);
            Assert.Equal(0, metrics[depth - 1].EnPassantCaptures);
            Assert.Equal(0, metrics[depth - 1].Castles);
            //Assert.Equal(0, metrics[depth - 1].Checks);

            Assert.Equal(8902, metrics[depth - 2].Legal);
            Assert.Equal(34, metrics[depth - 2].Captures);
            Assert.Equal(0, metrics[depth - 2].EnPassantCaptures);
            Assert.Equal(0, metrics[depth - 2].Castles);
            //Assert.Equal(12, metrics[depth - 2].Checks);

            Assert.Equal(197281, metrics[depth - 3].Legal);
            Assert.Equal(1576, metrics[depth - 3].Captures);
            Assert.Equal(0, metrics[depth - 3].EnPassantCaptures);
            Assert.Equal(0, metrics[depth - 3].Castles);
            //Assert.Equal(12, metrics[depth - 3].Checks);

            Assert.Equal(8902, metrics[depth - 4].Legal);
            Assert.Equal(34, metrics[depth - 4].Captures);
            Assert.Equal(0, metrics[depth - 4].EnPassantCaptures);
            Assert.Equal(0, metrics[depth - 4].Castles);
            //Assert.Equal(12, metrics[depth - 4].Checks);
        }

        [Fact]
        public void Default_Scalar_ToDepth3()
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
        public void Position3()
        {
            var perftRunner = new PerftRunnerMetrics(_moveGeneratorFixture.MoveGenerator);

            // "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -"
            var fen = Fen.Parse(Fen.Position3);

            var bitBoard = BitBoard.FromFen(fen);

            var metrics = new Dictionary<int, PerftMetrics>();

            //var count1 = perftRunner.Go(bitBoard, fen.ToPlay, 1);
            //var count2 = perftRunner.Go(bitBoard, fen.ToPlay, 2);
            //var count3 = perftRunner.Go(bitBoard, fen.ToPlay, 3);
            perftRunner.Go(bitBoard, fen.ToPlay, 5, metrics);

            Assert.Equal(14, metrics[5].Legal);
            Assert.Equal(1, metrics[5].Captures);
            Assert.Equal(0, metrics[5].EnPassantCaptures);
            Assert.Equal(0, metrics[5].Castles);
            //Assert.Equal(2, metrics[5].Checks);
            Assert.Equal(191, metrics[4].Legal);
            Assert.Equal(14, metrics[4].Captures);
            Assert.Equal(0, metrics[4].EnPassantCaptures);
            Assert.Equal(0, metrics[4].Castles);
            //Assert.Equal(10, metrics[4].Checks);
            Assert.Equal(2812, metrics[3].Legal);
            Assert.Equal(209, metrics[3].Captures);
            Assert.Equal(2, metrics[3].EnPassantCaptures);
            Assert.Equal(0, metrics[3].Castles);
            //Assert.Equal(267, metrics[3].Checks);
            Assert.Equal(43238, metrics[2].Legal);
            Assert.Equal(3348, metrics[2].Captures);
            Assert.Equal(123, metrics[2].EnPassantCaptures);
            Assert.Equal(0, metrics[2].Castles);
            //Assert.Equal(1680, metrics[2].Checks);
            Assert.Equal(674624, metrics[1].Legal);
            Assert.Equal(52051, metrics[1].Captures);
            Assert.Equal(1165, metrics[1].EnPassantCaptures);
            Assert.Equal(0, metrics[1].Castles);
            //Assert.Equal(52950, metrics[1].Checks);

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

        [Fact]
        public void DefaultPosition_2MovesIn()
        {
            // "rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1"
            var fen = Fen.Parse("rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1");

            var bitBoard = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            _moveGeneratorFixture.MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);

            var moveView = moves.Select(x => new MoveViewer(x));
            var moveCount = moves.Count;

            Assert.Equal(29, moveCount);
        }

        // http://cinnamonchess.altervista.org/perft.html
        [Fact]
        public void PromotionsPosition_Metrics_ToDepth2()
        {
            var perftRunner = new PerftRunnerMetrics(_moveGeneratorFixture.MoveGenerator);

            // "8/PPP4k/8/8/8/8/4Kppp/8 w - - 0 1"
            var fen = Fen.Parse("8/PPP4k/8/8/8/8/4Kppp/8 w - - 0 1");

            var bitBoard = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            var metrics = new Dictionary<int, PerftMetrics>();

            var depth = 3;

            perftRunner.Go(bitBoard, fen.ToPlay, depth, metrics);

            Assert.Equal(18, metrics[depth].Legal);
            Assert.Equal(290, metrics[depth - 1].Legal);
            Assert.Equal(5044, metrics[depth - 2].Legal);
            //Assert.Equal(89363, metrics[2].Legal);
            //Assert.Equal(1745545, metrics[1].Legal);
        }

        // http://cinnamonchess.altervista.org/perft.html
        [Fact]
        public void EnPassantPosition_Metrics_ToDepth2()
        {
            var perftRunner = new PerftRunnerMetrics(_moveGeneratorFixture.MoveGenerator);

            // "8/7p/p5pb/4k3/P1pPn3/8/P5PP/1rB2RK1 b - d3 0 28"
            var fen = Fen.Parse("8/7p/p5pb/4k3/P1pPn3/8/P5PP/1rB2RK1 b - d3 0 28");

            var bitBoard = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            var metrics = new Dictionary<int, PerftMetrics>();

            var depth = 3;

            perftRunner.Go(bitBoard, fen.ToPlay, depth, metrics);

            Assert.Equal(5, metrics[depth].Legal);
            Assert.Equal(117, metrics[depth - 1].Legal);
            Assert.Equal(3293, metrics[depth - 2].Legal);
            //Assert.Equal(67197, metrics[2].Legal);
            //Assert.Equal(1881089, metrics[1].Legal);
        }
    }
}
