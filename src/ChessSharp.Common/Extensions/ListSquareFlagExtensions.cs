using System.Collections.Generic;
using ChessSharp.Common.Enums;

namespace ChessSharp.Common.Extensions
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
