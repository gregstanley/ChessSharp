using ChessSharp.Enums;
using System.Collections.Generic;

namespace ChessSharp.Extensions
{
    public static class ListSquareFlagExtensions
    {
        public static SquareFlag ToSquareFlag(this IList<SquareFlag> squaresAsList)
        {
            SquareFlag squares = 0;

            foreach (var square in squaresAsList)
                squares |= square;

            return squares;
        }
    }
}
