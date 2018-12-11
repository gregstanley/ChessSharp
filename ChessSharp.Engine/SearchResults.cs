using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessSharp.Engine
{
    public class SearchResults
    {
        public SearchResults(int positionCount, IReadOnlyCollection<long> elapsedMilliseconds,
            IReadOnlyCollection<MoveEvaluation> moveEvaluations, uint[][] principalVariations,
            TranspositionTable transpositionTable)
        {
            SearchedPositionCount = positionCount;
            ElapsedMilliseconds = elapsedMilliseconds;
            MoveEvaluations = moveEvaluations;

            var output = new List<List<MoveViewer>>();

            foreach(var principalVariation in principalVariations)
                output.Add(principalVariation.Select(x => new MoveViewer(x)).ToList());

            PrincipalVariations = output;

            _transpositionTable = transpositionTable;
        }

        public string ToResultsString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Total positions: {SearchedPositionCount}");

            sb.AppendLine($"TT access: {_transpositionTable.AccessCount}");
            sb.AppendLine($"TT hits: {_transpositionTable.HitCount}");
            sb.AppendLine($"TT conflict: {_transpositionTable.ReplaceCount}");

            sb.AppendLine("=== Iteration timers ===");

            foreach (var time in ElapsedMilliseconds)
                sb.AppendLine($"{time} ms");

            var nodesPerMilli = (double)SearchedPositionCount / ElapsedMilliseconds.Sum();
            var nps = Math.Round(nodesPerMilli * 1000, 2);
            sb.AppendLine($"NPS: {nps}");

            sb.AppendLine("=== Move evaluations ===");

            foreach (var moveEvaluation in MoveEvaluations.OrderByDescending(x => x.Score))
                sb.AppendLine($"{moveEvaluation.Move.From} {moveEvaluation.Move.To} {moveEvaluation.Score}");

            sb.AppendLine("=== Principal variations ===");

            foreach (var principalVariation in PrincipalVariations)
            {
                foreach (var move in principalVariation)
                    sb.Append($"{move.GetNotation()} ");

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public int SearchedPositionCount { get; }

        public IReadOnlyCollection<long> ElapsedMilliseconds { get; }

        public IReadOnlyCollection<MoveEvaluation> MoveEvaluations { get; }

        public IReadOnlyCollection<IReadOnlyCollection<MoveViewer>> PrincipalVariations { get; }

        private TranspositionTable _transpositionTable { get; }
    }
}
