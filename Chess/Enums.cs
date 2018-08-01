using System;

namespace Chess
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
    public enum SquareFlag:long
    {
        A1 = 1L,
        B1 = 1L << 1,
        C1 = 1L << 2,
        D1 = 1L << 3,
        E1 = 1L << 4,
        F1 = 1L << 5,
        G1 = 1L << 6,
        H1 = 1L << 7,
        A2 = 1L << 8,
        B2 = 1L << 9,
        C2 = 1L << 10,
        D2 = 1L << 11,
        E2 = 1L << 12,
        F2 = 1L << 13,
        G2 = 1L << 14,
        H2 = 1L << 15,
        A3 = 1L << 16,
        B3 = 1L << 17,
        C3 = 1L << 18,
        D3 = 1L << 19,
        E3 = 1L << 20,
        F3 = 1L << 21,
        G3 = 1L << 22,
        H3 = 1L << 23,
        A4 = 1L << 24,
        B4 = 1L << 25,
        C4 = 1L << 26,
        D4 = 1L << 27,
        E4 = 1L << 28,
        F4 = 1L << 29,
        G4 = 1L << 30,
        H4 = 1L << 31,
        A5 = 1L << 32,
        B5 = 1L << 33,
        C5 = 1L << 34,
        D5 = 1L << 35,
        E5 = 1L << 36,
        F5 = 1L << 37,
        G5 = 1L << 38,
        H5 = 1L << 39,
        A6 = 1L << 40,
        B6 = 1L << 41,
        C6 = 1L << 42,
        D6 = 1L << 43,
        E6 = 1L << 44,
        F6 = 1L << 45,
        G6 = 1L << 46,
        H6 = 1L << 47,
        A7 = 1L << 48,
        B7 = 1L << 49,
        C7 = 1L << 50,
        D7 = 1L << 51,
        E7 = 1L << 52,
        F7 = 1L << 53,
        G7 = 1L << 54,
        H7 = 1L << 55,
        A8 = 1L << 56,
        B8 = 1L << 57,
        C8 = 1L << 58,
        D8 = 1L << 59,
        E8 = 1L << 60,
        F8 = 1L << 61,
        G8 = 1L << 62,
        H8 = 1L << 63
    }
    /*
    public enum SquareEnum
    {
        A1,
        A2,
        A3,
        A4,
        A5,
        A6,
        A7,
        A8,
        B1,
        B2,
        B3,
        B4,
        B5,
        B6,
        B7,
        B8,
        C1,
        C2,
        C3,
        C4,
        C5,
        C6,
        C7,
        C8,
        D1,
        D2,
        D3,
        D4,
        D5,
        D6,
        D7,
        D8,
        E1,
        E2,
        E3,
        E4,
        E5,
        E6,
        E7,
        E8,
        F1,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        G1,
        G2,
        G3,
        G4,
        G5,
        G6,
        G7,
        G8,
        H1,
        H2,
        H3,
        H4,
        H5,
        H6,
        H7,
        H8
    }
    */
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

    /*
    public enum PieceName
    {
        WhitePawn1,
        WhitePawn2,
        WhitePawn3,
        WhitePawn4,
        WhitePawn5,
        WhitePawn6,
        WhitePawn7,
        WhitePawn8,
        WhiteRook1,
        WhiteRook2,
        WhiteKnight1,
        WhiteKnight2,
        WhiteBishop1,
        WhiteBishop2,
        WhiteQueen,
        WhiteKing,
        BlackPawn1,
        BlackPawn2,
        BlackPawn3,
        BlackPawn4,
        BlackPawn5,
        BlackPawn6,
        BlackPawn7,
        BlackPawn8,
        BlackRook1,
        BlackRook2,
        BlackKnight1,
        BlackKnight2,
        BlackBishop1,
        BlackBishop2,
        BlackQueen,
        BlackKing
    }*/
}
