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
