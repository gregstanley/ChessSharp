using System;
using System.IO;
using ChessSharp.Common;
using ChessSharp.Engine.NegaMax.Events;
using ChessSharp.MoveGeneration;
using Serilog;
using Serilog.Core;

namespace ChessSharp.Engine.NegaMax
{
    public class NegaMaxFixture
    {
        internal class FakeEvaluator : IPositionEvaluator
        {
            // Real scores will be very similar/the same in simple setups so create fake scores
            public int Evaluate(Board board) =>
                // Use the last bits of the key to create a number between zero and 31
                15 - (int)board.Key & 31;
        }

        private readonly bool LoggingEnabled = true;
        private readonly Logger log;

        private static readonly MoveGenerator moveGenerator = new(64);
        private static readonly FakeEvaluator evaluator = new();

        public NegaMaxFixture()
        {
            NegaMaxSearch = new NegaMaxSearch(moveGenerator, evaluator);

            if (LoggingEnabled)
            {
                log = new LoggerConfiguration()
                .WriteTo.File(Path.Combine("Logs", "NegaMaxTests-log.txt"), rollingInterval: RollingInterval.Minute)
                .CreateLogger();

                NegaMaxSearch.SearchProgress += LogProgress;
            }
        }

        public NegaMaxSearch NegaMaxSearch { get; private set; }

        public void LogProgress(object sender, SearchProgressEventArgs args)
        {
            log.Information(args.Status.ToString());
        }
    }
}
