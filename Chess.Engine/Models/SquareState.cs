namespace Chess.Engine.Models
{
    public struct SquareState
    {
        public SquareState(SquareFlag square)
            :this (square, Colour.None, PieceType.None)
        { 
        }

        public SquareState(SquareFlag square, Colour colour, PieceType type)
        {
            Square = square;
            Colour = colour;
            Type = type;
        }

        public SquareFlag Square { get; }

        public Colour Colour { get; }

        public PieceType Type { get; }
    }
}
