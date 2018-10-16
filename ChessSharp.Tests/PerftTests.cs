using Serilog;
using Serilog.Core;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ChessSharp.Tests
{
    public class PerftTests : IClassFixture<MoveGeneratorFixture>
    {
        private Logger _log;

        MoveGeneratorFixture _moveGeneratorFixture;

        public PerftTests(MoveGeneratorFixture moveGeneratorFixture)
        {
            _moveGeneratorFixture = moveGeneratorFixture;

            _log = new LoggerConfiguration()
                .WriteTo.File("PerftTests-log.txt", rollingInterval: RollingInterval.Minute)
                .CreateLogger();
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

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);
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

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);

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

            var movePerfts = perftRunner.Go(bitBoard, fen.ToPlay, 3);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);

            Assert.Equal(8902, movePerfts.Sum(x => x.Nodes));
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

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);

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

            var depth = 5;

            var perftRunner = new PerftRunnerMetrics(_moveGeneratorFixture.MoveGenerator);

            perftRunner.Go(bitBoard, fen.ToPlay, depth, metrics);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);

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

            var d3Pawn = metrics[depth - 2].Moves.Where(x => x.PieceType == Enums.PieceType.Pawn)
                .OrderBy(x => x.From)
                .ThenBy(x => x.To)
                .ThenBy(x => x.CapturePieceType);

            var d3Rook = metrics[depth - 2].Moves.Where(x => x.PieceType == Enums.PieceType.Rook).OrderBy(x => x.From)
                .OrderBy(x => x.From)
                .ThenBy(x => x.To)
                .ThenBy(x => x.CapturePieceType);

            var d3Knight = metrics[depth - 2].Moves.Where(x => x.PieceType == Enums.PieceType.Knight).OrderBy(x => x.From)
                .OrderBy(x => x.From)
                .ThenBy(x => x.To)
                .ThenBy(x => x.CapturePieceType);

            var d3Bishop = metrics[depth - 2].Moves.Where(x => x.PieceType == Enums.PieceType.Bishop).OrderBy(x => x.From)
                .OrderBy(x => x.From)
                .ThenBy(x => x.To)
                .ThenBy(x => x.CapturePieceType);

            var d3Queen = metrics[depth - 2].Moves.Where(x => x.PieceType == Enums.PieceType.Queen).OrderBy(x => x.From)
                .OrderBy(x => x.From)
                .ThenBy(x => x.To)
                .ThenBy(x => x.CapturePieceType);

            var d3King = metrics[depth - 2].Moves.Where(x => x.PieceType == Enums.PieceType.King).OrderBy(x => x.From)
                .OrderBy(x => x.From)
                .ThenBy(x => x.To)
                .ThenBy(x => x.CapturePieceType);

            foreach (var move in d3Pawn)
                _log.Information("{pieceType}{from}{to}{catpurePieceType}", move.PieceType, move.From, move.To, move.CapturePieceType);

            foreach (var move in d3Rook)
                _log.Information("{pieceType}{from}{to}{catpurePieceType}", move.PieceType, move.From, move.To, move.CapturePieceType);

            foreach (var move in d3Knight)
                _log.Information("{pieceType}{from}{to}{catpurePieceType}", move.PieceType, move.From, move.To, move.CapturePieceType);

            foreach (var move in d3Bishop)
                _log.Information("{pieceType}{from}{to}{catpurePieceType}", move.PieceType, move.From, move.To, move.CapturePieceType);

            foreach (var move in d3Queen)
                _log.Information("{pieceType}{from}{to}{catpurePieceType}", move.PieceType, move.From, move.To, move.CapturePieceType);

            foreach (var move in d3King)
                _log.Information("{pieceType}{from}{to}{catpurePieceType}", move.PieceType, move.From, move.To, move.CapturePieceType);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);

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

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);

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

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);

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

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);

            Assert.Equal(5, metrics[depth].Legal);
            Assert.Equal(117, metrics[depth - 1].Legal);
            Assert.Equal(3293, metrics[depth - 2].Legal);
            //Assert.Equal(67197, metrics[2].Legal);
            //Assert.Equal(1881089, metrics[1].Legal);
        }

        // https://gist.github.com/peterellisjones/8c46c28141c162d1d8a0f0badbc9cff9
        [Theory]
        [InlineData("r6r/1b2k1bq/8/8/7B/8/8/R3K2R b QK - 3 2", 1, 8)]
        [InlineData("8/8/8/2k5/2pP4/8/B7/4K3 b - d3 5 3", 1, 8)]
        [InlineData("r1bqkbnr/pppppppp/n7/8/8/P7/1PPPPPPP/RNBQKBNR w QqKk - 2 2", 1, 19)]
        [InlineData("r3k2r/p1pp1pb1/bn2Qnp1/2qPN3/1p2P3/2N5/PPPBBPPP/R3K2R b QqKk - 3 2", 1, 5)]
        [InlineData("2kr3r/p1ppqpb1/bn2Qnp1/3PN3/1p2P3/2N5/PPPBBPPP/R3K2R b QK - 3 2", 1, 44)]
        [InlineData("rnb2k1r/pp1Pbppp/2p5/q7/2B5/8/PPPQNnPP/RNB1K2R w QK - 3 9", 1, 39)]
        [InlineData("2r5/3pk3/8/2P5/8/2K5/8/8 w - - 5 4", 1, 9)]
        [InlineData("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", 3, 62379)]
        [InlineData("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", 3, 89890)]
        // https://www.chessprogramming.net/perfect-perft/
        [InlineData("3k4/3p4/8/K1P4r/8/8/8/8 b - - 0 1", 6, 1134888)] //--Illegal ep move #1
        [InlineData("8/8/4k3/8/2p5/8/B2P2K1/8 w - - 0 1", 6, 1015133)] //--Illegal ep move #2
        [InlineData("8/8/1k6/2b5/2pP4/8/5K2/8 b - d3 0 1", 6, 1440467)] //--EP Capture Checks Opponent
        [InlineData("5k2/8/8/8/8/8/8/4K2R w K - 0 1", 6, 661072)] //--Short Castling Gives Check
        [InlineData("3k4/8/8/8/8/8/8/R3K3 w Q - 0 1", 6, 803711)] //--Long Castling Gives Check
        [InlineData("r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1", 4, 1274206)] //--Castle Rights
        [InlineData("r3k2r/8/3Q4/8/8/5q2/8/R3K2R b KQkq - 0 1", 4, 1720476)] //--Castling Prevented
        [InlineData("2K2r2/4P3/8/8/8/8/8/3k4 w - - 0 1", 6, 3821001)] //--Promote out of Check
        [InlineData("8/8/1P2K3/8/2n5/1q6/8/5k2 b - - 0 1", 5, 1004658)] //--Discovered Check
        [InlineData("4k3/1P6/8/8/8/8/K7/8 w - - 0 1", 6, 217342)] //--Promote to give check
        [InlineData("8/P1k5/K7/8/8/8/8/8 w - - 0 1", 6, 92683)] //--Under Promote to give check
        [InlineData("K1k5/8/P7/8/8/8/8/8 w - - 0 1", 6, 2217)] //--Self Stalemate
        [InlineData("8/k1P5/8/1K6/8/8/8/8 w - - 0 1", 7, 567584)] //--Stalemate & Checkmate
        [InlineData("8/8/2k5/5q2/5n2/8/5K2/8 b - - 0 1", 4, 23527)] //--Stalemate & Checkmate
        public void PeterEllisJones(string fenString, int depth, int expectedNodeCount)
        {
            var perftRunner = new PerftRunner(_moveGeneratorFixture.MoveGenerator);

            var fen = Fen.Parse(fenString);

            var bitBoard = BitBoard.FromFen(fen);
            var bitBoardReference = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            var movePerfts = perftRunner.Go(bitBoard, fen.ToPlay, depth);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);

            Assert.Equal(expectedNodeCount, movePerfts.Sum(x => x.Nodes));
        }

        //[Theory]
        ////[InlineData("r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1", 1, 26)] //--Castle Rights
        ////[InlineData("r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1", 2, 1141)]
        //[InlineData("r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1", 3, 27286)]
        ////[InlineData("r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1", 4, 1274206)]
        //public void PeterEllisJones2(string fenString, int depth, int expectedNodeCount)
        //{
        //    var perftRunner = new PerftRunner(_moveGeneratorFixture.MoveGenerator);

        //    var fen = Fen.Parse(fenString);

        //    var bitBoard = BitBoard.FromFen(fen);
        //    var bitBoardReference = BitBoard.FromFen(fen);

        //    var moves = new List<uint>(20);

        //    var movePerfts = perftRunner.Go(bitBoard, fen.ToPlay, depth);

        //    TestHelpers.AssertEqual(bitBoard, bitBoardReference);

        //    Assert.Equal(expectedNodeCount, movePerfts.Sum(x => x.Nodes));
        //}

        [Theory]
        [InlineData("r3k2r/1b4bq/8/8/8/8/7B/1R2K2R w Kkq - 0 1", 2, 1101)]
        public void PeterEllisJones3(string fenString, int depth, int expectedNodeCount)
        {
            var perftRunner = new PerftRunner(_moveGeneratorFixture.MoveGenerator);

            var fen = Fen.Parse(fenString);

            var bitBoard = BitBoard.FromFen(fen);
            var bitBoardReference = BitBoard.FromFen(fen);

            var moves = new List<uint>(20);

            var movePerfts = perftRunner.Go(bitBoard, fen.ToPlay, depth);

            TestHelpers.AssertEqual(bitBoard, bitBoardReference);

            Assert.Equal(expectedNodeCount, movePerfts.Sum(x => x.Nodes));
        }

        //private void AssertEqual(BitBoard a, BitBoard b)
        //{
        //    Assert.Equal(a.WhitePawns, b.WhitePawns);
        //    Assert.Equal(a.WhiteRooks, b.WhiteRooks);
        //    Assert.Equal(a.WhiteKnights, b.WhiteKnights);
        //    Assert.Equal(a.WhiteBishops, b.WhiteBishops);
        //    Assert.Equal(a.WhiteQueens, b.WhiteQueens);
        //    Assert.Equal(a.WhiteKing, b.WhiteKing);
        //    Assert.Equal(a.BlackPawns, b.BlackPawns);
        //    Assert.Equal(a.BlackRooks, b.BlackRooks);
        //    Assert.Equal(a.BlackKnights, b.BlackKnights);
        //    Assert.Equal(a.BlackBishops, b.BlackBishops);
        //    Assert.Equal(a.BlackQueens, b.BlackQueens);
        //    Assert.Equal(a.BlackKing, b.BlackKing);
        //    Assert.Equal(a.EnPassant, b.EnPassant);
        //    Assert.Equal(a.WhiteCanCastleKingSide, b.WhiteCanCastleKingSide);
        //    Assert.Equal(a.WhiteCanCastleQueenSide, b.WhiteCanCastleQueenSide);
        //    Assert.Equal(a.BlackCanCastleKingSide, b.BlackCanCastleKingSide);
        //    Assert.Equal(a.BlackCanCastleQueenSide, b.BlackCanCastleQueenSide);
        //}
    }
}
