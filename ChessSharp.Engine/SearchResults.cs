using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessSharp.Engine
{
    public class SearchResults
    {
        public SearchResults(int positionCount, IReadOnlyCollection<long> elapsedMilliseconds,
            IReadOnlyCollection<MoveEvaluation> moveEvaluations, uint[][] primaryVariations,
            TranspositionTable transpositionTable)
        {
            SearchedPositionCount = positionCount;
            ElapsedMilliseconds = elapsedMilliseconds;
            MoveEvaluations = moveEvaluations;

            var output = new List<List<MoveViewer>>();

            foreach(var primaryVariation in primaryVariations)
                output.Add(primaryVariation.Select(x => new MoveViewer(x)).ToList());

            PrimaryVariations = output;

            _transpositionTable = transpositionTable;
        }

        public string ToResultsString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Total positions: {SearchedPositionCount}");

            sb.AppendLine($"TT access: {_transpositionTable.AccessCount}");
            sb.AppendLine($"TT hits: {_transpositionTable.HitCount}");
            sb.AppendLine($"TT conflict: {_transpositionTable.ConflictCount}");

            sb.AppendLine("=== Iteration timers ===");

            foreach (var time in ElapsedMilliseconds)
                sb.AppendLine($"{time} ms");

            var nodesPerMilli = (double)SearchedPositionCount / ElapsedMilliseconds.Sum();
            var nps = Math.Round(nodesPerMilli * 1000, 2);
            sb.AppendLine($"NPS: {nps}");

            sb.AppendLine("=== Move evaluations ===");

            foreach (var moveEvaluation in MoveEvaluations.OrderByDescending(x => x.Score))
                sb.AppendLine($"{moveEvaluation.Move.From} {moveEvaluation.Move.To} {moveEvaluation.Score}");

            sb.AppendLine("=== Primary variations ===");

            foreach (var primaryVariation in PrimaryVariations)
            {
                foreach (var move in primaryVariation)
                    sb.Append($"{move.From}{move.To} ");

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public int SearchedPositionCount { get; }

        public IReadOnlyCollection<long> ElapsedMilliseconds { get; }

        public IReadOnlyCollection<MoveEvaluation> MoveEvaluations { get; }

        public IReadOnlyCollection<IReadOnlyCollection<MoveViewer>> PrimaryVariations { get; }

        private TranspositionTable _transpositionTable { get; }
    }
}
