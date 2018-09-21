using ChessSharp.Enums;

namespace ChessSharp.Models
{
    public struct SquareState
    {
        public SquareState(SquareFlag square)
             : this(square, new Piece(Colour.None, PieceType.None))
        { 
        }

        public SquareState(SquareFlag square, Piece piece)
        {
            Square = square;
            Piece = piece;
        }

        public SquareFlag Square { get; }

        public Piece Piece { get; }

        public Colour Colour => Piece.Colour;

        public PieceType Type => Piece.Type;
    }
}
