using Chess.Engine.Ai.Searches;
using Chess.Engine.Extensions;
using Chess.Engine.Models;
using Serilog;
using Serilog.Core;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Chess.Engine.Tests
{
    // https://chessprogramming.wikispaces.com/Perft+Results
    public class BoardTests
    {
        private Logger _log;

        public BoardTests()
        {
            _log = new LoggerConfiguration()
                .WriteTo.File("BoardTest-log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        [Fact]
        public void Constructor_Perft_0()
        {
            var ab = new AlphaBeta();

            var board = new Board();

            Assert.Equal(0, board.ChildBoards.Count);
        }

        [Fact]
        public void GenerateChildBoards_Perft_1()
        {
            var ab = new AlphaBeta();

            var board = new Board();

            board.GenerateChildBoards(Colour.White, 1);

            Assert.Equal(20, board.ChildBoards.Count);
        }

        [Fact]
        public void GenerateChildBoards_Perft_2()
        {
            var ab = new AlphaBeta();

            var board = new Board();

            board.GenerateChildBoards(Colour.White, 3);

            var d1 = board.ChildBoards
                .SelectMany(x => x.GetLegalMoves());

            Assert.Equal(400, d1.Count());
        }

        [Fact]
        public void GenerateChildBoards_DefaultPosition_ToDepth3()
        {
            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            var fen = Fen.Parse(Fen.Default);

            var board = Board.FromFen(fen);

            board.GenerateChildBoards(Colour.White, 4);

            var d1 = board.GetLegalMoves();
            var d2 = d1.SelectMany(x => x.GetLegalMoves());
            var d3 = d2.SelectMany(x => x.GetLegalMoves());
            var d4 = d3.SelectMany(x => x.GetLegalMoves());

            // Need to look one ahead for the Checks to see if they are Checkmate
            var d4InCheck = d4.Where(x => x.WhiteIsInCheck);

            foreach (var d4board in d4InCheck)
                d4board.GenerateChildBoards(Colour.White, 1);

            var metrics1 = GetDepthMetrics(d1);
            var metrics2 = GetDepthMetrics(d2);
            var metrics3 = GetDepthMetrics(d3);
            var metrics4 = GetDepthMetrics(d4);

            Assert.Equal(20, metrics1.Legal);
            Assert.Equal(0, metrics1.Captures);
            Assert.Equal(0, metrics1.EnPassantCaptures);
            Assert.Equal(0, metrics1.Castles);
            Assert.Equal(0, metrics1.Checks);
            Assert.Equal(0, metrics1.Checkmates);
            Assert.Equal(400, metrics2.Legal);
            Assert.Equal(0, metrics2.Captures);
            Assert.Equal(0, metrics2.EnPassantCaptures);
            Assert.Equal(0, metrics2.Castles);
            Assert.Equal(0, metrics2.Checks);
            Assert.Equal(0, metrics2.Checkmates);
            Assert.Equal(8902, metrics3.Legal);
            Assert.Equal(34, metrics3.Captures);
            Assert.Equal(0, metrics3.EnPassantCaptures);
            Assert.Equal(0, metrics3.Castles);
            Assert.Equal(12, metrics3.Checks);
            Assert.Equal(0, metrics3.Checkmates);
            Assert.Equal(197281, metrics4.Legal);
            Assert.Equal(1576, metrics4.Captures);
            Assert.Equal(0, metrics4.EnPassantCaptures);
            Assert.Equal(0, metrics4.Castles);
            Assert.Equal(469, metrics4.Checks);
            Assert.Equal(8, metrics4.Checkmates);
        }

        [Fact]
        public void GenerateChildBoards_Position2_ToDepth3()
        {
            // "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -"
            var fen = Fen.Parse(Fen.Position2);

            var board = Board.FromFen(fen);

            board.GenerateChildBoards(Colour.White, 3);

            var d1 = board.GetLegalMoves();
            var d2 = d1.SelectMany(x => x.GetLegalMoves());
            var d3 = d2.SelectMany(x => x.GetLegalMoves());

            // Need to look one ahead for the Checks to see if they are Checkmate
            var d3InCheck = d3.Where(x => x.BlackIsInCheck);

            foreach (var d3board in d3InCheck)
                d3board.GenerateChildBoards(Colour.Black, 1);

            var metrics1 = GetDepthMetrics(d1);
            var metrics2 = GetDepthMetrics(d2);
            var metrics3 = GetDepthMetrics(d3);

            Assert.Equal(48, metrics1.Legal);
            Assert.Equal(8, metrics1.Captures);
            Assert.Equal(0, metrics1.EnPassantCaptures);
            Assert.Equal(2, metrics1.Castles);
            Assert.Equal(0, metrics1.Checks);
            Assert.Equal(2039, metrics2.Legal);
            Assert.Equal(351, metrics2.Captures);
            Assert.Equal(1, metrics2.EnPassantCaptures);
            Assert.Equal(91, metrics2.Castles);
            Assert.Equal(3, metrics2.Checks);
            Assert.Equal(97862, metrics3.Legal);
            Assert.Equal(17102, metrics3.Captures);
            Assert.Equal(45, metrics3.EnPassantCaptures);
            Assert.Equal(3162, metrics3.Castles);  
            Assert.Equal(993, metrics3.Checks);
            Assert.Equal(1, metrics3.Checkmates);
        }

        [Fact]
        public void GenerateChildBoards_Position3_ToDepth5()
        {
            // "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -"
            var fen = Fen.Parse(Fen.Position3);

            var board = Board.FromFen(fen);

            board.GenerateChildBoards(Colour.White, 5);

            var d1 = board.GetLegalMoves();
            var d2 = d1.SelectMany(x => x.GetLegalMoves());
            var d3 = d2.SelectMany(x => x.GetLegalMoves());
            var d4 = d3.SelectMany(x => x.GetLegalMoves());
            var d5 = d4.SelectMany(x => x.GetLegalMoves());

            LogBoards(board);
            var d2x = d2.Where(x => x.IsCapture);
            var d2checks = d2.Where(x => x.WhiteIsInCheck || x.BlackIsInCheck);

            var metrics1 = GetDepthMetrics(d1);
            var metrics2 = GetDepthMetrics(d2);
            var metrics3 = GetDepthMetrics(d3);
            var metrics4 = GetDepthMetrics(d4);
            var metrics5 = GetDepthMetrics(d5);

            Assert.Equal(14, metrics1.Legal);
            Assert.Equal(1, metrics1.Captures);
            Assert.Equal(0, metrics1.EnPassantCaptures);
            Assert.Equal(0, metrics1.Castles);
            Assert.Equal(2, metrics1.Checks);
            Assert.Equal(191, metrics2.Legal);
            Assert.Equal(14, metrics2.Captures);
            Assert.Equal(0, metrics2.EnPassantCaptures);
            Assert.Equal(0, metrics2.Castles);
            Assert.Equal(10, metrics2.Checks);
            Assert.Equal(2812, metrics3.Legal);
            Assert.Equal(209, metrics3.Captures);
            Assert.Equal(2, metrics3.EnPassantCaptures);
            Assert.Equal(0, metrics3.Castles);
            Assert.Equal(267, metrics3.Checks);
            Assert.Equal(43238, metrics4.Legal);
            Assert.Equal(3348, metrics4.Captures);
            Assert.Equal(123, metrics4.EnPassantCaptures);
            Assert.Equal(0, metrics4.Castles);
            Assert.Equal(1680, metrics4.Checks);
            Assert.Equal(674624, metrics5.Legal);
            Assert.Equal(52051, metrics5.Captures);
            Assert.Equal(1165, metrics5.EnPassantCaptures);
            Assert.Equal(0, metrics5.Castles);
            Assert.Equal(52950, metrics5.Checks);
        }

        [Fact]
        public void GenerateChildBoards_Position4_ToDepth3()
        {
            // "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1"
            var fen = Fen.Parse(Fen.Position4);

            var board = Board.FromFen(fen);

            board.GenerateChildBoards(Colour.White, 3);

            var d1 = board.GetLegalMoves();
            var d2 = d1.SelectMany(x => x.GetLegalMoves());
            var d3 = d2.SelectMany(x => x.GetLegalMoves());

            var metrics1 = GetDepthMetrics(d1);
            var metrics2 = GetDepthMetrics(d2);
            var metrics3 = GetDepthMetrics(d3);

            Assert.Equal(6, metrics1.Legal);
            Assert.Equal(0, metrics1.Captures);
            Assert.Equal(0, metrics1.EnPassantCaptures);
            Assert.Equal(0, metrics1.Castles);
            Assert.Equal(0, metrics1.Checks);
            Assert.Equal(264, metrics2.Legal);
            Assert.Equal(87, metrics2.Captures);
            Assert.Equal(0, metrics2.EnPassantCaptures);
            Assert.Equal(6, metrics2.Castles);
            Assert.Equal(10, metrics2.Checks);
            Assert.Equal(9467, metrics3.Legal);
            Assert.Equal(1021, metrics3.Captures);
            Assert.Equal(4, metrics3.EnPassantCaptures);
            Assert.Equal(0, metrics3.Castles);
            Assert.Equal(38, metrics3.Checks);
        }

        [Fact]
        public void GenerateChildBoards_Position5_ToDepth3()
        {
            // "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"
            var fen = Fen.Parse(Fen.Position5);

            var board = Board.FromFen(fen);

            board.GenerateChildBoards(Colour.White, 3);

            var d1 = board.GetLegalMoves();
            var d2 = d1.SelectMany(x => x.GetLegalMoves());
            var d3 = d2.SelectMany(x => x.GetLegalMoves());

            var metrics1 = GetDepthMetrics(d1);
            var metrics2 = GetDepthMetrics(d2);
            var metrics3 = GetDepthMetrics(d3);

            Assert.Equal(44, metrics1.Legal);
            Assert.Equal(1486, metrics2.Legal);
            Assert.Equal(62379, metrics3.Legal);
        }

        [Fact]
        public void GenerateChildBoards_Position6_ToDepth3()
        {
            // "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10"
            var fen = Fen.Parse(Fen.Position6);

            var board = Board.FromFen(fen);

            board.GenerateChildBoards(Colour.White, 3);

            var d1 = board.GetLegalMoves();
            var d2 = d1.SelectMany(x => x.GetLegalMoves());
            var d3 = d2.SelectMany(x => x.GetLegalMoves());

            var metrics1 = GetDepthMetrics(d1);
            var metrics2 = GetDepthMetrics(d2);
            var metrics3 = GetDepthMetrics(d3);

            Assert.Equal(46, metrics1.Legal);
            Assert.Equal(2079, metrics2.Legal);
            Assert.Equal(89890, metrics3.Legal);
        }

        private void LogBoards(Board board)
        {
            var nodeCount1 = 1;

            var runningNodeCount3 = 1;

            foreach (var childBoard1 in board.ChildBoards)
            {
                var nodeCount2 = 1;

                _log.Information($"{nodeCount1}) {childBoard1.GetFriendlyCode()}   ({childBoard1.ChildBoards.Count()})");
                
                foreach (var childBoard2 in childBoard1.ChildBoards)
                {
                    var nodeCount3 = 1;

                    _log.Information($"    {nodeCount2}) {childBoard2.GetFriendlyCode()}   ({childBoard2.ChildBoards.Count()})");

                    //foreach (var childBoard3 in childBoard2.ChildBoards)
                    //{
                    //    _log.Information($"        {nodeCount3}) {childBoard3.GetFriendlyCode()}   ({childBoard3.ChildBoards.Count()})");

                    //    ++nodeCount3;
                    //    ++runningNodeCount3;
                    //}

                    ++nodeCount2;
                }
                
                ++nodeCount1;
            }

            _log.Information($"Total nodes 3: {runningNodeCount3 - 1})");
        }

        private DepthMetrics GetDepthMetrics(IEnumerable<Board> boards)
        {
            var depthMetrics = new DepthMetrics();

            depthMetrics.Process(boards);

            return depthMetrics;
        }
    }
}
