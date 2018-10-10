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
            var bitBoardReference = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            _moveGeneratorFixture.MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);

            var moveView = moves.Select(x => new MoveViewer(x));
            var moveCount = moves.Count;

            AssertEqual(bitBoard, bitBoardReference);
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
            var bitBoardReference = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            var metrics = new Dictionary<int, PerftMetrics>();

            var depth = 4;

            perftRunner.Go(bitBoard, fen.ToPlay, depth, metrics);

            AssertEqual(bitBoard, bitBoardReference);

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

            //Assert.Equal(8902, metrics[depth - 4].Legal);
            //Assert.Equal(34, metrics[depth - 4].Captures);
            //Assert.Equal(0, metrics[depth - 4].EnPassantCaptures);
            //Assert.Equal(0, metrics[depth - 4].Castles);
            //Assert.Equal(12, metrics[depth - 4].Checks);
        }

        [Fact]
        public void Default_Scalar_ToDepth3()
        {
            var perftRunner = new PerftRunner(_moveGeneratorFixture.MoveGenerator);

            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            var fen = Fen.Parse(Fen.Default);

            var bitBoard = BitBoard.FromFen(fen);
            var bitBoardReference = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            var count = perftRunner.Go(bitBoard, fen.ToPlay, 3);

            AssertEqual(bitBoard, bitBoardReference);

            Assert.Equal(8902, count);
        }

        [Fact]
        public void Position2()
        {
            // "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -"
            var fen = Fen.Parse(Fen.Position2);

            var bitBoard = BitBoard.FromFen(fen);
            var bitBoardReference = BitBoard.FromFen(fen);

            var metrics = new Dictionary<int, PerftMetrics>();

            var depth = 3;

            var perftRunner = new PerftRunnerMetrics(_moveGeneratorFixture.MoveGenerator);

            perftRunner.Go(bitBoard, fen.ToPlay, depth, metrics);

            AssertEqual(bitBoard, bitBoardReference);

            Assert.Equal(48, metrics[depth].Legal);
            Assert.Equal(8, metrics[depth].Captures);
            Assert.Equal(0, metrics[depth].EnPassantCaptures);
            Assert.Equal(2, metrics[depth].Castles);
            //Assert.Equal(0, metrics[depth].Checks);
            Assert.Equal(2039, metrics[depth - 1].Legal);
            Assert.Equal(351, metrics[depth - 1].Captures);
            Assert.Equal(1, metrics[depth - 1].EnPassantCaptures);
            Assert.Equal(91, metrics[depth - 1].Castles);
            //Assert.Equal(3, metrics[depth - 1].Checks);
            Assert.Equal(97862, metrics[depth - 2].Legal);
            Assert.Equal(17102, metrics[depth - 2].Captures);
            Assert.Equal(45, metrics[depth - 2].EnPassantCaptures);
            Assert.Equal(3162, metrics[depth - 2].Castles);
            //Assert.Equal(993, metrics[depth - 2].Checks);
            //Assert.Equal(1, metrics[depth - 2].Checkmates);
        }

        [Fact]
        public void Position3()
        {
            // "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -"
            var fen = Fen.Parse(Fen.Position3);

            var bitBoard = BitBoard.FromFen(fen);
            var bitBoardReference = BitBoard.FromFen(fen);

            var metrics = new Dictionary<int, PerftMetrics>();

            var depth = 4;

            var perftRunner = new PerftRunnerMetrics(_moveGeneratorFixture.MoveGenerator);

            perftRunner.Go(bitBoard, fen.ToPlay, depth, metrics);

            AssertEqual(bitBoard, bitBoardReference);

            Assert.Equal(14, metrics[depth].Legal);
            Assert.Equal(1, metrics[depth].Captures);
            Assert.Equal(0, metrics[depth].EnPassantCaptures);
            Assert.Equal(0, metrics[depth].Castles);
            //Assert.Equal(2, metrics[depth].Checks);
            Assert.Equal(191, metrics[depth - 1].Legal);
            Assert.Equal(14, metrics[depth - 1].Captures);
            Assert.Equal(0, metrics[depth - 1].EnPassantCaptures);
            Assert.Equal(0, metrics[depth - 1].Castles);
            //Assert.Equal(10, metrics[depth - 1].Checks);
            Assert.Equal(2812, metrics[depth - 2].Legal);
            Assert.Equal(209, metrics[depth - 2].Captures);
            Assert.Equal(2, metrics[depth - 2].EnPassantCaptures);
            Assert.Equal(0, metrics[depth - 2].Castles);
            //Assert.Equal(267, metrics[depth - 2].Checks);
            Assert.Equal(43238, metrics[depth - 3].Legal);
            Assert.Equal(3348, metrics[depth - 3].Captures);
            Assert.Equal(123, metrics[depth - 3].EnPassantCaptures);
            Assert.Equal(0, metrics[depth - 3].Castles);
            //Assert.Equal(1680, metrics[depth - 3].Checks);
            Assert.Equal(674624, metrics[depth - 4].Legal);
            Assert.Equal(52051, metrics[depth - 4].Captures);
            Assert.Equal(1165, metrics[depth - 4].EnPassantCaptures);
            Assert.Equal(0, metrics[depth - 4].Castles);
            //Assert.Equal(52950, metrics[depth - 4].Checks);

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
        public void Position4()
        {
            // "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1"
            var fen = Fen.Parse(Fen.Position4);

            var bitBoard = BitBoard.FromFen(fen);
            var bitBoardReference = BitBoard.FromFen(fen);

            var metrics = new Dictionary<int, PerftMetrics>();

            var depth = 3;

            var perftRunner = new PerftRunnerMetrics(_moveGeneratorFixture.MoveGenerator);

            perftRunner.Go(bitBoard, fen.ToPlay, depth, metrics);

            AssertEqual(bitBoard, bitBoardReference);

            Assert.Equal(6, metrics[depth].Legal);
            Assert.Equal(0, metrics[depth].Captures);
            Assert.Equal(0, metrics[depth].EnPassantCaptures);
            Assert.Equal(0, metrics[depth].Castles);
            //Assert.Equal(0, metrics[depth].Checks);
            Assert.Equal(264, metrics[depth - 1].Legal);
            Assert.Equal(87, metrics[depth - 1].Captures);
            Assert.Equal(0, metrics[depth - 1].EnPassantCaptures);
            Assert.Equal(6, metrics[depth - 1].Castles);
            //Assert.Equal(10, metrics[depth - 1].Checks);
            Assert.Equal(9467, metrics[depth - 2].Legal);
            Assert.Equal(1021, metrics[depth - 2].Captures);
            Assert.Equal(4, metrics[depth - 2].EnPassantCaptures);
            Assert.Equal(0, metrics[depth - 2].Castles);
            //Assert.Equal(38, metrics[depth - 2].Checks);
        }

        [Fact]
        public void DefaultPosition_2MovesIn()
        {
            // "rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1"
            var fen = Fen.Parse("rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1");

            var bitBoard = BitBoard.FromFen(fen);
            var bitBoardReference = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            _moveGeneratorFixture.MoveGenerator.Generate(bitBoard, fen.ToPlay, moves);

            var moveView = moves.Select(x => new MoveViewer(x));
            var moveCount = moves.Count;

            AssertEqual(bitBoard, bitBoardReference);

            Assert.Equal(29, moveCount);
        }

        // http://cinnamonchess.altervista.org/perft.html
        [Fact]
        public void PromotionsPosition_Metrics_ToDepth4()
        {
            var perftRunner = new PerftRunnerMetrics(_moveGeneratorFixture.MoveGenerator);

            // "8/PPP4k/8/8/8/8/4Kppp/8 w - - 0 1"
            var fen = Fen.Parse("8/PPP4k/8/8/8/8/4Kppp/8 w - - 0 1");

            var bitBoard = BitBoard.FromFen(fen);
            var bitBoardReference = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            var metrics = new Dictionary<int, PerftMetrics>();

            var depth = 4;

            perftRunner.Go(bitBoard, fen.ToPlay, depth, metrics);

            AssertEqual(bitBoard, bitBoardReference);

            Assert.Equal(18, metrics[depth].Legal);
            Assert.Equal(290, metrics[depth - 1].Legal);
            Assert.Equal(5044, metrics[depth - 2].Legal);
            Assert.Equal(89363, metrics[depth - 3].Legal);
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
            var bitBoardReference = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            var metrics = new Dictionary<int, PerftMetrics>();

            var depth = 3;

            perftRunner.Go(bitBoard, fen.ToPlay, depth, metrics);

            AssertEqual(bitBoard, bitBoardReference);

            Assert.Equal(5, metrics[depth].Legal);
            Assert.Equal(117, metrics[depth - 1].Legal);
            Assert.Equal(3293, metrics[depth - 2].Legal);
            //Assert.Equal(67197, metrics[2].Legal);
            //Assert.Equal(1881089, metrics[1].Legal);
        }

        private void AssertEqual(BitBoard a, BitBoard b)
        {
            Assert.Equal(a.WhitePawns, b.WhitePawns);
            Assert.Equal(a.WhiteRooks, b.WhiteRooks);
            Assert.Equal(a.WhiteKnights, b.WhiteKnights);
            Assert.Equal(a.WhiteBishops, b.WhiteBishops);
            Assert.Equal(a.WhiteQueens, b.WhiteQueens);
            Assert.Equal(a.WhiteKing, b.WhiteKing);
            Assert.Equal(a.BlackPawns, b.BlackPawns);
            Assert.Equal(a.BlackRooks, b.BlackRooks);
            Assert.Equal(a.BlackKnights, b.BlackKnights);
            Assert.Equal(a.BlackBishops, b.BlackBishops);
            Assert.Equal(a.BlackQueens, b.BlackQueens);
            Assert.Equal(a.BlackKing, b.BlackKing);
            Assert.Equal(a.EnPassant, b.EnPassant);
            Assert.Equal(a.WhiteCanCastleKingSide, b.WhiteCanCastleKingSide);
            Assert.Equal(a.WhiteCanCastleQueenSide, b.WhiteCanCastleQueenSide);
            Assert.Equal(a.BlackCanCastleKingSide, b.BlackCanCastleKingSide);
            Assert.Equal(a.BlackCanCastleQueenSide, b.BlackCanCastleQueenSide);
        }
    }
}
