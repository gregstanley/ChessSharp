using System;
using System.IO;
using ChessSharp.Common;
using ChessSharp.Common.Helpers;
using ChessSharp.Engine.NegaMax.Events;
using ChessSharp.MoveGeneration;
using Serilog;
using Serilog.Core;

namespace ChessSharp.Engine.NegaMax
{
    public class NegaMaxFixture
    {
        // Real scores will be very similar/the same in simple setups so create fake scores
        internal class FakeEvaluator : IPositionEvaluator
        {
            // Use the last bits of the key to create a number between zero and 31
            public int Evaluate(Board board) => 31 - (int)board.Key & 63;
        }

        public GameState GameState1 = FenHelpers.Parse("k7/p7/8/8/8/8/7P/7K w KQkq - 0 1");

        private readonly bool LoggingEnabled = true;
        private readonly Logger log;

        private static readonly MoveGenerator moveGenerator = new(64);
        private static readonly FakeEvaluator evaluator = new();

        public NegaMaxFixture()
        {
            NegaMaxSearch = new NegaMaxSearch(moveGenerator, evaluator);
            NegaMaxSearch_v2 = new NegaMaxSearch_v2(moveGenerator, evaluator);
            NegaMaxSearch_v1 = new NegaMaxSearch_v1(moveGenerator, evaluator);

            if (LoggingEnabled)
            {
                log = new LoggerConfiguration()
                .WriteTo.File(Path.Combine("Logs", "NegaMaxTests-log.txt"), rollingInterval: RollingInterval.Minute)
                .CreateLogger();

                NegaMaxSearch.SearchProgress += LogProgress;
                NegaMaxSearch_v2.SearchProgress += LogProgress;
                NegaMaxSearch_v1.SearchProgress += LogProgress;
            }
        }

        public NegaMaxSearch NegaMaxSearch { get; private set; }

        public NegaMaxSearch_v2 NegaMaxSearch_v2 { get; private set; }

        public NegaMaxSearch_v1 NegaMaxSearch_v1 { get; private set; }

        public Board GetBoard1() => Board.FromGameState(GameState1);

        public void LogNextTest()
        {
            log.Information(" --- ");
        }

        public void LogProgress(object sender, SearchProgressEventArgs args)
        {
            log.Information(args.Status.ToString());
        }

        public void LogResult(NegaMaxSearchResult result)
        {
            log.Information(" RESULT ");

            log.Information(result.ToString());
        }
    }
}
