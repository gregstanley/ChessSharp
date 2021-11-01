using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessSharp.Common;

namespace ChessSharp.Engine
{
    public class InfoDepthComplete : Info
    {
        private readonly uint[] principalVariation = new uint[64];

        public InfoDepthComplete(
            int positionCount,
            long elapsedMilliseconds,
            int depth,
            uint[] principalVariation,
            TranspositionTable transpositionTable)
            : base(positionCount, elapsedMilliseconds, depth, transpositionTable)
        {
            principalVariation.CopyTo(this.principalVariation, 0);
        }

        public IReadOnlyCollection<MoveViewer> PrincipalVariation =>
            this.principalVariation
                .Where(x => x > 0)
                .Select(x => new MoveViewer(x))
                .ToList();

        public string GetPvString()
        {
            var sb = new StringBuilder();

            sb.Append($"Depth: {Depth} Positions: { SearchedPositionCount}");

            foreach (var move in this.PrincipalVariation)
                sb.Append($"{move.GetNotation()} ");

            return sb.ToString();
        }
    }
}
