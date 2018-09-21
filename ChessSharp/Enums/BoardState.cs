using System;
using System.Collections.Generic;
using System.Text;

namespace ChessSharp.Enums
{
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
