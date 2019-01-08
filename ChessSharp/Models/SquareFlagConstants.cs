using ChessSharp.Enums;

namespace ChessSharp.Models
{
    public static class SquareFlagConstants
    {
        public const SquareFlag A = SquareFlag.A1 | SquareFlag.A2 | SquareFlag.A3 | SquareFlag.A4 | SquareFlag.A5 | SquareFlag.A6 | SquareFlag.A7 | SquareFlag.A8;
        public const SquareFlag B = SquareFlag.B1 | SquareFlag.B2 | SquareFlag.B3 | SquareFlag.B4 | SquareFlag.B5 | SquareFlag.B6 | SquareFlag.B7 | SquareFlag.B8;
        public const SquareFlag C = SquareFlag.C1 | SquareFlag.C2 | SquareFlag.C3 | SquareFlag.C4 | SquareFlag.C5 | SquareFlag.C6 | SquareFlag.C7 | SquareFlag.C8;
        public const SquareFlag D = SquareFlag.D1 | SquareFlag.D2 | SquareFlag.D3 | SquareFlag.D4 | SquareFlag.D5 | SquareFlag.D6 | SquareFlag.D7 | SquareFlag.D8;
        public const SquareFlag E = SquareFlag.E1 | SquareFlag.E2 | SquareFlag.E3 | SquareFlag.E4 | SquareFlag.E5 | SquareFlag.E6 | SquareFlag.E7 | SquareFlag.E8;
        public const SquareFlag F = SquareFlag.F1 | SquareFlag.F2 | SquareFlag.F3 | SquareFlag.F4 | SquareFlag.F5 | SquareFlag.F6 | SquareFlag.F7 | SquareFlag.F8;
        public const SquareFlag G = SquareFlag.G1 | SquareFlag.G2 | SquareFlag.G3 | SquareFlag.G4 | SquareFlag.G5 | SquareFlag.G6 | SquareFlag.G7 | SquareFlag.G8;
        public const SquareFlag H = SquareFlag.H1 | SquareFlag.H2 | SquareFlag.H3 | SquareFlag.H4 | SquareFlag.H5 | SquareFlag.H6 | SquareFlag.H7 | SquareFlag.H8;

        public const SquareFlag R1 = SquareFlag.A1 | SquareFlag.B1 | SquareFlag.C1 | SquareFlag.D1 | SquareFlag.E1 | SquareFlag.F1 | SquareFlag.G1 | SquareFlag.H1;
        public const SquareFlag R2 = SquareFlag.A2 | SquareFlag.B2 | SquareFlag.C2 | SquareFlag.D2 | SquareFlag.E2 | SquareFlag.F2 | SquareFlag.G2 | SquareFlag.H2;
        public const SquareFlag R3 = SquareFlag.A3 | SquareFlag.B3 | SquareFlag.C3 | SquareFlag.D3 | SquareFlag.E3 | SquareFlag.F3 | SquareFlag.G3 | SquareFlag.H3;
        public const SquareFlag R4 = SquareFlag.A4 | SquareFlag.B4 | SquareFlag.C4 | SquareFlag.D4 | SquareFlag.E4 | SquareFlag.F4 | SquareFlag.G4 | SquareFlag.H4;
        public const SquareFlag R5 = SquareFlag.A5 | SquareFlag.B5 | SquareFlag.C5 | SquareFlag.D5 | SquareFlag.E5 | SquareFlag.F5 | SquareFlag.G5 | SquareFlag.H5;
        public const SquareFlag R6 = SquareFlag.A6 | SquareFlag.B6 | SquareFlag.C6 | SquareFlag.D6 | SquareFlag.E6 | SquareFlag.F6 | SquareFlag.G6 | SquareFlag.H6;
        public const SquareFlag R7 = SquareFlag.A7 | SquareFlag.B7 | SquareFlag.C7 | SquareFlag.D7 | SquareFlag.E7 | SquareFlag.F7 | SquareFlag.G7 | SquareFlag.H7;
        public const SquareFlag R8 = SquareFlag.A8 | SquareFlag.B8 | SquareFlag.C8 | SquareFlag.D8 | SquareFlag.E8 | SquareFlag.F8 | SquareFlag.G8 | SquareFlag.H8;

        public const SquareFlag All = (SquareFlag)ulong.MaxValue;

        public const SquareFlag WhiteKingStartSquare = SquareFlag.E1;

        public const SquareFlag BlackKingStartSquare = SquareFlag.E8;

        public const SquareFlag WhiteKingSideRookStartSquare = SquareFlag.H1;

        public const SquareFlag BlackKingSideRookStartSquare = SquareFlag.H8;

        public const SquareFlag WhiteQueenSideRookStartSquare = SquareFlag.A1;

        public const SquareFlag BlackQueenSideRookStartSquare = SquareFlag.A8;

        public const SquareFlag WhiteQueenSideRookStep1Square = SquareFlag.B1;

        public const SquareFlag BlackQueenSideRookStep1Square = SquareFlag.B8;

        public const SquareFlag WhiteKingSideCastleStep1 = SquareFlag.F1;

        public const SquareFlag BlackKingSideCastleStep1 = SquareFlag.F8;

        public const SquareFlag WhiteKingSideCastleStep2 = SquareFlag.G1;

        public const SquareFlag BlackKingSideCastleStep2 = SquareFlag.G8;

        public const SquareFlag WhiteQueenSideCastleStep1 = SquareFlag.D1;

        public const SquareFlag BlackQueenSideCastleStep1 = SquareFlag.D8;

        public const SquareFlag WhiteQueenSideCastleStep2 = SquareFlag.C1;

        public const SquareFlag BlackQueenSideCastleStep2 = SquareFlag.C8;
    }
}
