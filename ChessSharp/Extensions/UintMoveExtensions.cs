using ChessSharp.Enums;

namespace ChessSharp.Extensions
{
    public static class UintMoveExtensions
    {
        private static readonly uint ColourMask = 0b00000000_00000000_00000000_00000001;
        private static readonly uint PieceTypeMask = 0b00000000_00000000_00000000_00001110;
        private static readonly uint FromMask = 0b00000000_00000000_00000011_11110000;
        private static readonly uint ToMask = 0b00000000_00000000_11111100_00000000;
        private static readonly uint CapturePieceTypeMask = 0b00000000_00000111_00000000_00000000;
        private static readonly uint MoveTypeMask = 0b00000000_00111000_00000000_00000000;
        private static readonly uint NumCheckersMask = 0b00000000_11000000_00000000_00000000;

        private static readonly SquareFlag[] Indices = new SquareFlag[]
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

        public static SquareFlag ToSquareFlag(this uint value) => Indices[value];

        public static Colour GetColour(this uint value) => (value & ColourMask) == 0 ? Colour.White : Colour.Black;

        public static PieceType GetPieceType(this uint value) => (PieceType)((value & PieceTypeMask) >> 1);

        public static SquareFlag GetFrom(this uint value) => ((value & FromMask) >> 4).ToSquareFlag();

        public static int GetFromIndex(this uint value) => (int)(value & FromMask) >> 4;

        public static SquareFlag GetTo(this uint value) => ((value & ToMask) >> 10).ToSquareFlag();

        public static int GetToIndex(this uint value) => (int)(value & ToMask) >> 10;

        public static PieceType GetCapturePieceType(this uint value) => (PieceType)((value & CapturePieceTypeMask) >> 16);

        public static MoveType GetMoveType(this uint value) => (MoveType)((value & MoveTypeMask) >> 19);

        public static byte GetNumCheckers(this uint value) => (byte)((value & NumCheckersMask) >> 22);
    }
}
