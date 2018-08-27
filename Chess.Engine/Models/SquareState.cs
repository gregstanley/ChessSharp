namespace Chess.Engine.Models
{
    public struct SquareState
    {
        public SquareState(SquareFlag square)
             : this(square, new Piece(Colour.None, PieceType.None))
             //: this (square, Colour.None, PieceType.None)
        { 
        }

        //public SquareState(SquareFlag square, Colour colour, PieceType type)
        //{
        //    Square = square;
        //    Colour = colour;
        //    Type = type;
        //}

        public SquareState(SquareFlag square, Piece piece)
        {
            Square = square;
            //Colour = colour;
            //Type = type;
            Piece = piece;
        }

        public SquareFlag Square { get; }

        public Piece Piece { get; }
        //public Colour Colour { get; }

        //public PieceType Type { get; }

        public Colour Colour => Piece.Colour;

        public PieceType Type => Piece.Type;
    }
}
