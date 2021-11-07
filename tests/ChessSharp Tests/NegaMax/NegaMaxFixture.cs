using System;
using System.IO;
using ChessSharp.Engine.NegaMax.Events;
using ChessSharp.MoveGeneration;
using Serilog;
using Serilog.Core;

namespace ChessSharp.Engine.NegaMax
{
    public class NegaMaxFixture
    {
        private readonly bool LoggingEnabled = true;
        private readonly Logger log;

        private static readonly MoveGenerator moveGenerator = new(64);
        private static readonly NegaMaxEvaluator evaluator = new();

        public NegaMaxFixture()
        {
            NegaMaxSearch = new NegaMaxSearch(moveGenerator, evaluator);

            if (LoggingEnabled)
            {
                log = new LoggerConfiguration()
                .WriteTo.File(Path.Combine("Logs", "NegaMaxTests-log.txt"), rollingInterval: RollingInterval.Minute)
                .CreateLogger();
            }

            if (LoggingEnabled)
            {
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
