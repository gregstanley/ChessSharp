using System.Collections.Generic;

namespace Chess
{
    public class Square
    {
        private RankFile _rankFile;

        public int Rank => _rankFile.Rank;

        public File File => _rankFile.File;

        public Piece Piece { get; private set; }

        public IReadOnlyCollection<Piece> CoveredBy { get { return _coveredBy; } }

        private List<Piece> _coveredBy { get; set; } = new List<Piece>();

        public Square(RankFile rankFile)
        {
            _rankFile = rankFile ?? throw new System.ArgumentNullException(nameof(rankFile));
        }

        public void SetPiece(Piece piece)
        {
            Piece = piece;
        }

        public void RemovePiece()
        {
            Piece = null;
        }

        public void AddCoveredBy(Piece piece)
        {
            _coveredBy.Add(piece);
        }

        public RelativePosition To(Square square)
        {
            var rankFile = RankFile.Get(square.Rank, square.File);

            return _rankFile.To(rankFile);
        }
    }
}
