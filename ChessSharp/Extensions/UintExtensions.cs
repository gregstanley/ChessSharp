using ChessSharp.Enums;

namespace ChessSharp.Extensions
{
    public static class UintExtensions
    {
        public static SquareFlag ToSquareFlag(this uint value) => _indices[value];

        public static Colour GetColour(this uint value) => (value & _colourMask) == 0 ? Colour.White : Colour.Black;

        public static PieceType GetPieceType(this uint value) => (PieceType)((value & _pieceTypeMask) >> 1);

        public static SquareFlag GetFrom(this uint value) => ((value & _fromMask) >> 4).ToSquareFlag();

        public static SquareFlag GetTo(this uint value) => ((value & _toMask) >> 10).ToSquareFlag();

        public static PieceType GetCapturePieceType(this uint value) => (PieceType)((value & _capturePieceTypeMask) >> 16);

        public static MoveType GetMoveType(this uint value) => (MoveType)((value & _moveTypeMask) >> 19);

        private static SquareFlag[] _indices = new SquareFlag[]
        {
             SquareFlag.A1,
             SquareFlag.B1,
             SquareFlag.C1,
             SquareFlag.D1,
             SquareFlag.E1,
             SquareFlag.F1,
             SquareFlag.G1,
             SquareFlag.H1,
             SquareFlag.A2,
             SquareFlag.B2,
             SquareFlag.C2,
             SquareFlag.D2,
             SquareFlag.E2,
             SquareFlag.F2,
             SquareFlag.G2,
             SquareFlag.H2,
             SquareFlag.A3,
             SquareFlag.B3,
             SquareFlag.C3,
             SquareFlag.D3,
             SquareFlag.E3,
             SquareFlag.F3,
             SquareFlag.G3,
             SquareFlag.H3,
             SquareFlag.A4,
             SquareFlag.B4,
             SquareFlag.C4,
             SquareFlag.D4,
             SquareFlag.E4,
             SquareFlag.F4,
             SquareFlag.G4,
             SquareFlag.H4,
             SquareFlag.A5,
             SquareFlag.B5,
             SquareFlag.C5,
             SquareFlag.D5,
             SquareFlag.E5,
             SquareFlag.F5,
             SquareFlag.G5,
             SquareFlag.H5,
             SquareFlag.A6,
             SquareFlag.B6,
             SquareFlag.C6,
             SquareFlag.D6,
             SquareFlag.E6,
             SquareFlag.F6,
             SquareFlag.G6,
             SquareFlag.H6,
             SquareFlag.A7,
             SquareFlag.B7,
             SquareFlag.C7,
             SquareFlag.D7,
             SquareFlag.E7,
             SquareFlag.F7,
             SquareFlag.G7,
             SquareFlag.H7,
             SquareFlag.A8,
             SquareFlag.B8,
             SquareFlag.C8,
             SquareFlag.D8,
             SquareFlag.E8,
             SquareFlag.F8,
             SquareFlag.G8,
             SquareFlag.H8
        };

        private static readonly uint _colourMask             = 0b00000000_00000000_00000000_00000001;
        private static readonly uint _pieceTypeMask          = 0b00000000_00000000_00000000_00001110;
        private static readonly uint _fromMask               = 0b00000000_00000000_00000011_11110000;
        private static readonly uint _toMask                 = 0b00000000_00000000_11111100_00000000;
        private static readonly uint _capturePieceTypeMask   = 0b00000000_00000111_00000000_00000000;
        private static readonly uint _moveTypeMask           = 0b00000000_00111000_00000000_00000000;
    }
}
