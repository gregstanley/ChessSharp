using System;

namespace Chess.Engine.Models
{
    public enum Colour
    {
        None,
        White,
        Black
    }

    public enum PieceType
    {
        None,
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }

    public enum SquareEnum
    {
        A1, B1, C1, D1, E1, F1, G1, H1,
        A2, B2, C2, D2, E2, F2, G2, H2,
        A3, B3, C3, D3, E3, F3, G3, H3,
        A4, B4, C4, D4, E4, F4, G4, H4,
        A5, B5, C5, D5, E5, F5, G5, H5,
        A6, B6, C6, D6, E6, F6, G6, H6,
        A7, B7, C7, D7, E7, F7, G7, H7,
        A8, B8, C8, D8, E8, F8, G8, H8
    }

    [Flags]
    public enum SquareFlag:ulong
    {
        A1 = 1UL,
        B1 = 1UL << 1,
        C1 = 1UL << 2,
        D1 = 1UL << 3,
        E1 = 1UL << 4,
        F1 = 1UL << 5,
        G1 = 1UL << 6,
        H1 = 1UL << 7,
        A2 = 1UL << 8,
        B2 = 1UL << 9,
        C2 = 1UL << 10,
        D2 = 1UL << 11,
        E2 = 1UL << 12,
        F2 = 1UL << 13,
        G2 = 1UL << 14,
        H2 = 1UL << 15,
        A3 = 1UL << 16,
        B3 = 1UL << 17,
        C3 = 1UL << 18,
        D3 = 1UL << 19,
        E3 = 1UL << 20,
        F3 = 1UL << 21,
        G3 = 1UL << 22,
        H3 = 1UL << 23,
        A4 = 1UL << 24,
        B4 = 1UL << 25,
        C4 = 1UL << 26,
        D4 = 1UL << 27,
        E4 = 1UL << 28,
        F4 = 1UL << 29,
        G4 = 1UL << 30,
        H4 = 1UL << 31,
        A5 = 1UL << 32,
        B5 = 1UL << 33,
        C5 = 1UL << 34,
        D5 = 1UL << 35,
        E5 = 1UL << 36,
        F5 = 1UL << 37,
        G5 = 1UL << 38,
        H5 = 1UL << 39,
        A6 = 1UL << 40,
        B6 = 1UL << 41,
        C6 = 1UL << 42,
        D6 = 1UL << 43,
        E6 = 1UL << 44,
        F6 = 1UL << 45,
        G6 = 1UL << 46,
        H6 = 1UL << 47,
        A7 = 1UL << 48,
        B7 = 1UL << 49,
        C7 = 1UL << 50,
        D7 = 1UL << 51,
        E7 = 1UL << 52,
        F7 = 1UL << 53,
        G7 = 1UL << 54,
        H7 = 1UL << 55,
        A8 = 1UL << 56,
        B8 = 1UL << 57,
        C8 = 1UL << 58,
        D8 = 1UL << 59,
        E8 = 1UL << 60,
        F8 = 1UL << 61,
        G8 = 1UL << 62,
        H8 = 1UL << 63
    }

    public enum File
    {
        a,
        b,
        c,
        d,
        e,
        f,
        g,
        h
    }

    [Flags]
    public enum BoardState
    {
        None = 0,
        IsCapture = 1,
        IsPawnPromotion = 2,
        WhiteIsInCheck = 4,
        BlackIsInCheck = 8,
        WhiteIsInCheckmate = 16,
        BlackIsInCheckmate = 32,
        WhiteCanCastleKingSide = 64,
        WhiteCanCastleQueenSide = 128,
        BlackCanCastleKingSide = 256,
        BlackCanCastleQueenSide = 512
    }
}
