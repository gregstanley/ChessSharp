using ChessSharp.Enums;

namespace ChessSharp.Models
{
    public struct Piece
    {
        public Piece(Colour colour, PieceType type)
        {
            Colour = colour;
            Type = type;
        }

        public Colour Colour { get; }

        public PieceType Type { get; }
    }
}
