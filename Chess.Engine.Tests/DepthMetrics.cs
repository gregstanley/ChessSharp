using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Engine.Tests
{
    public class DepthMetrics
    {
        public int Legal { get; private set; }
        public int Captures { get; private set; }
        public int EnPassantCaptures { get; private set; }
        public int Castles { get; private set; }
        public int Checks { get; private set; }

        public void Process(IEnumerable<Board> boards)
        {
            var captures = boards.Where(x => x.IsCapture);
            var enPassant = boards.Where(x => x.EnPassantCaptureSquare != 0);
            var castles = boards.Where(x => x.Notation.StartsWith("0-0"));
            var checks = boards.Where(x => x.WhiteIsInCheck || x.BlackIsInCheck);

            Legal = boards.Count();
            Captures = captures.Count();
            EnPassantCaptures = enPassant.Count();
            Castles = castles.Count();
            Checks = checks.Count();
            //var d2 = d1.SelectMany(x => x.GetLegalMoves());

            //var d2LegalCount = d2.Count();
            //var captures2 = d2.Where(x => x.IsCapture);
            //var captures2Count = captures2.Count();
            //var enPassant2 = d2.Where(x => x.EnPassantCaptureSquare != 0);
            //var castles2 = d2.Where(x => x.Notation.StartsWith("0-0"));
            //var castles2Count = castles2.Count();
            //var checks2 = d2.Where(x => x.WhiteIsInCheck || x.BlackIsInCheck);
            //var checks2Count = checks2.Count();

            //var d3 = d2.SelectMany(x => x.GetLegalMoves());

            //var d3LegalCount = d3.Count();
            //var captures3 = d3.Where(x => x.IsCapture);
            //var capturesCount3 = captures3.Count();
            //var enPassant3 = d3.Where(x => x.EnPassantCaptureSquare != 0);
            //var enPassant3Count = enPassant3.Count();
            //var castles3 = d3.Where(x => x.Notation.StartsWith("0-0"));
            //var castles3Count = castles3.Count();
            //var checks3 = d3.Where(x => x.WhiteIsInCheck || x.BlackIsInCheck);
            //var checks3Count = checks3.Count();
        }
    }
}
