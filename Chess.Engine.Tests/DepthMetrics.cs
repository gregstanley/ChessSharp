using System.Collections.Generic;
using System.Linq;

namespace Chess.Engine.Tests
{
    public class DepthMetrics
    {
        public int Legal { get; private set; }
        public int Captures { get; private set; }
        public int EnPassantCaptures { get; private set; }
        public int Castles { get; private set; }
        public int Checks { get; private set; }
        public int Checkmates { get; private set; }

        public void Process(IEnumerable<Board> boards)
        {
            var captures = boards.Where(x => x.IsCapture);
            var enPassant = boards.Where(x => x.EnPassantCaptureSquare != 0);
            var castles = boards.Where(x => x.Notation.StartsWith("0-0"));
            var checks = boards.Where(x => x.WhiteIsInCheck || x.BlackIsInCheck);
            var checkmates = boards.Where(x => x.WhiteIsInCheckmate || x.BlackIsInCheckmate);

            Legal = boards.Count();
            Captures = captures.Count();
            EnPassantCaptures = enPassant.Count();
            Castles = castles.Count();
            Checks = checks.Count();
            Checkmates = checkmates.Count();
        }
    }
}
