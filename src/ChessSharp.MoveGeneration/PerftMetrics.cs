using System.Collections.Generic;
using ChessSharp.Common;

namespace ChessSharp.MoveGeneration
{
    public class PerftMetrics
    {
        public int Legal { get; set; }

        public int Captures { get; set; }

        public int EnPassantCaptures { get; set; }

        public int Castles { get; set; }

        public int Checks { get; set; }

        public List<MoveViewer> Moves { get; set; } = new List<MoveViewer>();
    }
}
