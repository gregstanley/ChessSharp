using ChessSharp.Enums;
using ChessSharp.Extensions;

namespace ChessSharp
{
    public class MoveViewer
    {
        public MoveViewer(uint value)
        {
            Value = value;
        }

        public Colour Colour => Value.GetColour();

        public PieceType PieceType => Value.GetPieceType();

        public SquareFlag From => Value.GetFrom();

        public SquareFlag To => Value.GetTo();

        public PieceType CapturePieceType => Value.GetCapturePieceType();

        public MoveType MoveType => Value.GetMoveType();

        public uint Value { get; }
    }
}
