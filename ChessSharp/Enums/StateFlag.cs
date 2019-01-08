using System;

namespace ChessSharp.Enums
{
    [Flags]
    public enum StateFlag
    {
        None = 0,
        WhiteCanCastleKingSide = 1 << 1,
        WhiteCanCastleQueenSide = 1 << 2,
        BlackCanCastleKingSide = 1 << 3,
        BlackCanCastleQueenSide = 1 << 4,
        WhiteIsInCheck = 1 << 5,
        BlackIsInCheck = 1 << 6,
        WhiteIsInCheckmate = 1 << 7,
        BlackIsInCheckmate = 1 << 8
    }
}
