namespace Chess
{
    public class Piece
    {
        public Colour Colour { get; }

        public PieceType Type { get; }

        public byte Value { get; }

        public RankFile StartPosition { get; }

        public bool HasMoved { get; private set; } = false;

        public bool Captured { get; private set; } = false;

        public string LongName
        {
            get
            {
                return $"{Colour} {Type}";
            }
        }

        public string FullName
        {
            get
            {
                return $"{Colour}{Type}{StartPosition.File}{StartPosition.Rank}";
            }
        }

        public Piece(Colour colour, PieceType type, byte value, RankFile startPosition, bool hasMoved, bool captured)
            : this(colour, type, value, startPosition)
        {
            HasMoved = hasMoved;
            Captured = captured;
        }

        public Piece(Colour colour, PieceType type, byte value, RankFile startPosition)
        {
            Colour = colour;
            Type = type;
            Value = value;
            StartPosition = startPosition;
        }

        public void Move() =>
            HasMoved = true;

        public void Capture() =>
            Captured = true;

        public void PlaceOnBoard() =>
            Captured = false;

        public Piece Clone() =>
            new Piece(Colour, Type, Value, StartPosition, HasMoved, Captured);
    }
}
