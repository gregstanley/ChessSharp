﻿using System;
using ChessSharp.Common.Enums;

namespace ChessSharp.Common.Extensions
{
    public static class IntSquareExtensions
    {
        public static ulong ToSquareFlagUlong(this int squareIndex)
        {
            return 1ul << squareIndex;
        }

        public static SquareFlag ToSquareFlag(this int squareIndex)
        {
            if (squareIndex < 0 || squareIndex > 63)
                throw new ArgumentOutOfRangeException(nameof(squareIndex), squareIndex, "To convert an int to a SquareFlag the value must be 0-63");

            return (SquareFlag)(1ul << squareIndex);
        }
    }
}
