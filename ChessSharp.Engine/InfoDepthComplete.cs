using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessSharp.Engine
{ 
    public class InfoDepthComplete : Info
    {
        private uint[] _principalVariation = new uint[64];

        public InfoDepthComplete(int positionCount, long elapsedMilliseconds, int depth,
            uint[] principalVariation, TranspositionTable transpositionTable)
            : base(positionCount, elapsedMilliseconds, depth, transpositionTable)
        {
            principalVariation.CopyTo(_principalVariation, 0);
        }

        public string GetPvString()
        {
            var sb = new StringBuilder();

            sb.Append($"Depth: {Depth} ");

            foreach (var move in PrincipalVariation)
                sb.Append($"{move.GetNotation()} ");

            return sb.ToString();
        }

        public IReadOnlyCollection<MoveViewer> PrincipalVariation =>
            _principalVariation
                .Where(x => x > 0)
                .Select(x => new MoveViewer(x))
                .ToList();
    }
}
