using Chess.Engine.Ai.Searches;
using Chess.Engine.Bit;
using Chess.Engine.Extensions;
using Chess.Engine.Models;
using Serilog;
using Serilog.Core;
using System;
using System.Linq;
using Xunit;

namespace Chess.Engine.Tests
{
    // https://chessprogramming.wikispaces.com/Perft+Results
    public class AlphaBetaTests
    {
        private Logger _log;

        public AlphaBetaTests()
        {

            _log = new LoggerConfiguration()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        [Fact]
        public void AlphaBeta_Position3()
        {
            // "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -"
            var fen = Fen.Parse(Fen.Position3);

            var bitBoard = BitBoard.FromFen(fen);
            var bitBoardMoveFinder = new BitBoardMoveFinder();

            // Provide a default root board
            var board = new Board();

            // TODO: Have to do this to flip current colour
            var board2 = new Board(board, null, bitBoard, bitBoardMoveFinder);

            var fenBoard = new Board(board2, null, bitBoard, bitBoardMoveFinder);

            fenBoard.GenerateChildBoards(Colour.White, 4);
            //board.UpdateStateInfo();
            RecursiveUpdateStateInfo(fenBoard, Colour.White, 4);

            var d1 = fenBoard.GetLegalMoves();

            var d1Count = d1.Count();
            var isWhiteInCheck = d1.Where(x => x.WhiteIsInCheck);
            var isBlackInCheck = d1.Where(x => x.BlackIsInCheck);
            var totalChecks = isWhiteInCheck.Count() + isBlackInCheck.Count();
            var isCapture = d1.Where(x => x.IsCapture);

            Assert.Equal(14, d1Count);
            Assert.Equal(1, isCapture.Count());
            Assert.Equal(2, totalChecks);
        }

        [Fact]
        public void AlphaBeta_Perft_0()
        {
            var ab = new AlphaBeta();

            var board = new Board();

            //board.GenerateChildBoards(Colour.White, 2);

            Assert.Equal(0, board.ChildBoards.Count);
        }

        [Fact]
        public void AlphaBeta_Perft_1()
        {
            var ab = new AlphaBeta();

            var board = new Board();

            board.GenerateChildBoards(Colour.White, 1);
            board.UpdateStateInfo();

            //var results = ab.DoSearch(board, Colour.White, 2, true);

            Assert.Equal(20, board.ChildBoards.Count);
        }

        [Fact]
        public void AlphaBeta_Perft_2()
        {
            var ab = new AlphaBeta();

            var board = new Board();

            //board.GenerateChildBoards(Colour.White, 2);
            //board.UpdateStateInfo();
            RecursiveGenerate(board, Colour.White, 4);
            //var results = ab.DoSearch(board, Colour.White, 2, true);

            var d1 = board.ChildBoards
                .SelectMany(x => x.GetLegalMoves());

            Assert.Equal(400, d1.Count());
        }

        [Fact]
        public void AlphaBeta_Perft_3()
        {
            var ab = new AlphaBeta();

            var board = new Board();

            board.GenerateChildBoards(Colour.White, 4);
            //board.UpdateStateInfo();
            RecursiveUpdateStateInfo(board, Colour.White, 4);

            //var results = ab.DoSearch(board, Colour.White, 2, true);
            var d1 = board.GetLegalMoves();

            var d2 = d1.SelectMany(x => x.GetLegalMoves());

            var d2LegalCount = d2.Count();

            var d3 = d2.SelectMany(x => x.GetLegalMoves());
            var d3ChildBoards = d2.SelectMany(x => x.ChildBoards);

            var d3CountLegal = d3.Count();
            var d3CountChildboard = d3ChildBoards.Count();

            var isWhiteInCheck = d3.Where(x => x.WhiteIsInCheck);
            var isBlackInCheck = d3.Where(x => x.BlackIsInCheck);
            var totalChecks = isWhiteInCheck.Count() + isBlackInCheck.Count();
            var isCapture = d3.Where(x => x.IsCapture);

            var d1toh5s = d3.Where(x => x.Code.StartsWith("d1-h5"));

            _log.Information("Perft3");

            var nodeCount1 = 1;

            var runningNodeCount3 = 1;

            foreach(var childBoard1 in board.ChildBoards)
            {
               var nodeCount2 = 1;

                _log.Information($"{nodeCount1}) {childBoard1.GetFriendlyCode()}   ({childBoard1.ChildBoards.Count()})");

                foreach (var childBoard2 in childBoard1.ChildBoards)
                {
                    var nodeCount3 = 1;

                    _log.Information($"    {nodeCount2}) {childBoard2.GetFriendlyCode()}   ({childBoard2.ChildBoards.Count()})");

                    foreach (var childBoard3 in childBoard2.ChildBoards)
                    {
                        _log.Information($"        {nodeCount3}) {childBoard3.GetFriendlyCode()}   ({childBoard3.ChildBoards.Count()})");

                        ++nodeCount3;
                        ++runningNodeCount3;
                    }

                    ++nodeCount2;
                }

                ++nodeCount1;
            }

            _log.Information($"Total nodes 3: {runningNodeCount3 - 1})");

            var parents = d1toh5s.Where(x => x.ParentBoard.Code.StartsWith("e7"));

            Assert.Equal(34, isCapture.Count());
            Assert.Equal(12, totalChecks);
            //Assert.Equal(8902, d3CountLegal);
        }

        [Fact]
        public void AlphaBeta_Perft_4()
        {
            var ab = new AlphaBeta();

            var board = new Board();

            board.GenerateChildBoards(Colour.White, 5);

            RecursiveUpdateStateInfo(board, Colour.White, 5);

            var d1 = board.GetLegalMoves();

            var d2 = d1.SelectMany(x => x.GetLegalMoves());

            var d3 = d2.SelectMany(x => x.GetLegalMoves());

            var d4 = d3.SelectMany(x => x.GetLegalMoves());

            var isWhiteInCheck = d4.Where(x => x.WhiteIsInCheck);
            var isBlackInCheck = d4.Where(x => x.BlackIsInCheck);
            var totalChecks = isWhiteInCheck.Count() + isBlackInCheck.Count();
            var isCapture = d4.Where(x => x.IsCapture);

            var captureCount = isCapture.Count();
            Assert.Equal(1576, captureCount);
            //Assert.Equal(469, totalChecks);
            //Assert.Equal(8, totalCheckmates);
            //Assert.Equal(197281, d4.Count());
        }
        /*
        // For future use
        [Fact]
        public void AlphaBeta_Perft_5()
        {
            //Assert.Equal(82719, captureCount);
            //Assert.Equal(27351, totalChecks);
            //Assert.Equal(347, totalCheckmates);
            //Assert.Equal(4865609, d5.Count());
        }
        */
        private void RecursiveUpdateStateInfo(Board board, Colour colour, int depth)
        {
            if (!board.ChildBoards.Any())
                return;

            if (--depth < 1)
                return;

            foreach (var childBoard in board.ChildBoards)
            {
                RecursiveUpdateStateInfo(childBoard, colour.Opposite(), depth);

                if (board.Code.StartsWith("f7"))
                {
                    if (childBoard.Code.StartsWith("d1-h5"))
                    {
                        var bp = true;
                    }
                }

                childBoard.UpdateStateInfo();
            }

            board.UpdateStateInfo();
        }

        private void RecursiveGenerate(Board board, Colour colour, int depth)
        {
            board.GenerateChildBoards(colour, 1);

            if (depth < 1)
            {
                //board.UpdateStateInfo();
                return;
            }

            //if (--depth < 1)
            //    return;

            foreach (var childBoard in board.ChildBoards)
                RecursiveGenerate(childBoard, colour.Opposite(), --depth);

            board.UpdateStateInfo();
        }
    }
}
