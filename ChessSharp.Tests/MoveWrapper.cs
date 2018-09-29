using ChessSharp.Enums;
using ChessSharp.Extensions;

namespace ChessSharp.Tests
{
    public class MoveWrapper
    {
        private uint _value;

        public MoveWrapper(uint value)
        {
            _value = value;
        }

        public Colour Colour => _value.GetColour();

        public PieceType PieceType => _value.GetPieceType();

        public SquareFlag From => _value.GetFrom();

        public SquareFlag To => _value.GetTo();

        public PieceType CapturePieceType => _value.GetCapturePieceType();

        public MoveType MoveType => _value.GetMoveType();
    }
}
