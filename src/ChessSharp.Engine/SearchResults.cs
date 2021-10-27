using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessSharp.Common;

namespace ChessSharp.Engine
{
    public class SearchResults
    {
        private readonly TranspositionTable transpositionTable;

        public SearchResults(
            int positionCount,
            IReadOnlyCollection<long> elapsedMilliseconds,
            IReadOnlyCollection<MoveEvaluation> moveEvaluations,
            uint[][] principalVariations,
            TranspositionTable transpositionTable)
        {
            SearchedPositionCount = positionCount;
            ElapsedMilliseconds = elapsedMilliseconds;
            MoveEvaluations = moveEvaluations;

            var output = new List<List<MoveViewer>>();

            foreach (var principalVariation in principalVariations)
                output.Add(principalVariation.Select(x => new MoveViewer(x)).ToList());

            PrincipalVariations = output;

            this.transpositionTable = transpositionTable;
        }

        public int SearchedPositionCount { get; }

        public IReadOnlyCollection<long> ElapsedMilliseconds { get; }

        public IReadOnlyCollection<MoveEvaluation> MoveEvaluations { get; }

        public IReadOnlyCollection<IReadOnlyCollection<MoveViewer>> PrincipalVariations { get; }

        public string ToResultsString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== Principal variations ===");

            var index = 0;

            foreach (var principalVariation in PrincipalVariations)
            {
                var time = ElapsedMilliseconds.ElementAt(index);

                sb.Append($"{index + 1} {time}ms");

                foreach (var move in principalVariation)
                    sb.Append($" {move.GetNotation()} ");

                sb.AppendLine();

                ++index;
            }

            sb.AppendLine("=== Move evaluations ===");

            foreach (var moveEvaluation in MoveEvaluations.OrderByDescending(x => x.Score))
                sb.AppendLine($"{moveEvaluation.Move.From} {moveEvaluation.Move.To} {moveEvaluation.Score}");

            return sb.ToString();
        }
    }
}
