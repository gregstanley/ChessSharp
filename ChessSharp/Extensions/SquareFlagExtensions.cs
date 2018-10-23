using ChessSharp.Enums;
using System;
using System.Collections.Generic;

namespace ChessSharp.Extensions
{
    public static class SquareFlagExtensions
    {
        private static Dictionary<SquareFlag, int> _indices = new Dictionary<SquareFlag, int>
        {
            [SquareFlag.A1] = 0,
            [SquareFlag.B1] = 1,
            [SquareFlag.C1] = 2,
            [SquareFlag.D1] = 3,
            [SquareFlag.E1] = 4,
            [SquareFlag.F1] = 5,
            [SquareFlag.G1] = 6,
            [SquareFlag.H1] = 7,
            [SquareFlag.A2] = 8,
            [SquareFlag.B2] = 9,
            [SquareFlag.C2] = 10,
            [SquareFlag.D2] = 11,
            [SquareFlag.E2] = 12,
            [SquareFlag.F2] = 13,
            [SquareFlag.G2] = 14,
            [SquareFlag.H2] = 15,
            [SquareFlag.A3] = 16,
            [SquareFlag.B3] = 17,
            [SquareFlag.C3] = 18,
            [SquareFlag.D3] = 19,
            [SquareFlag.E3] = 20,
            [SquareFlag.F3] = 21,
            [SquareFlag.G3] = 22,
            [SquareFlag.H3] = 23,
            [SquareFlag.A4] = 24,
            [SquareFlag.B4] = 25,
            [SquareFlag.C4] = 26,
            [SquareFlag.D4] = 27,
            [SquareFlag.E4] = 28,
            [SquareFlag.F4] = 29,
            [SquareFlag.G4] = 30,
            [SquareFlag.H4] = 31,
            [SquareFlag.A5] = 32,
            [SquareFlag.B5] = 33,
            [SquareFlag.C5] = 34,
            [SquareFlag.D5] = 35,
            [SquareFlag.E5] = 36,
            [SquareFlag.F5] = 37,
            [SquareFlag.G5] = 38,
            [SquareFlag.H5] = 39,
            [SquareFlag.A6] = 40,
            [SquareFlag.B6] = 41,
            [SquareFlag.C6] = 42,
            [SquareFlag.D6] = 43,
            [SquareFlag.E6] = 44,
            [SquareFlag.F6] = 45,
            [SquareFlag.G6] = 46,
            [SquareFlag.H6] = 47,
            [SquareFlag.A7] = 48,
            [SquareFlag.B7] = 49,
            [SquareFlag.C7] = 50,
            [SquareFlag.D7] = 51,
            [SquareFlag.E7] = 52,
            [SquareFlag.F7] = 53,
            [SquareFlag.G7] = 54,
            [SquareFlag.H7] = 55,
            [SquareFlag.A8] = 56,
            [SquareFlag.B8] = 57,
            [SquareFlag.C8] = 58,
            [SquareFlag.D8] = 59,
            [SquareFlag.E8] = 60,
            [SquareFlag.F8] = 61,
            [SquareFlag.G8] = 62,
            [SquareFlag.H8] = 63
        };

        public static SquareFlag PawnForward(this SquareFlag square, Colour colour, int numRanks) =>
            colour == Colour.White
                ? (SquareFlag)((ulong)square << (8 * numRanks))
                : (SquareFlag)((ulong)square >> (8 * numRanks));

        public static SquareFlag ShiftRankUp(this SquareFlag square, int numRanks) =>
            (SquareFlag)((ulong)square << (8 * numRanks));

        public static SquareFlag ShiftRankDown(this SquareFlag square, int numRanks) =>
            (SquareFlag)((ulong)square >> (8 * numRanks));

        public static byte GetInstanceNumber(this SquareFlag squares, SquareFlag square)
        {
            byte instanceNumber = 1;

            for (var i = 1ul; i > 0; i = i << 1)
            {
                if (square.HasFlag((SquareFlag)i))
                    return instanceNumber;

                if (squares.HasFlag((SquareFlag)i))
                    ++instanceNumber;
            }

            return instanceNumber;
        }

        public static byte Count(this SquareFlag squares)
        {
            byte count = 0;

            var a = (ulong)squares;

            while (a > 0)
            {
                if ((a & 1) > 0)
                    ++count;

                a >>= 1;
            }

            return count;
        }

        // Checking the Dictionary first is mightly expensive. Better to not look for something that doesn't exist.
        //public static int ToSquareIndex(this SquareFlag square) =>
        //    _indices.ContainsKey(square) ? _indices[square] : -1;
        // Even looking up in the dictionary is slow. Better to count!
        //public static int ToSquareIndex(this SquareFlag square) =>
        //    _indices[square];

        public static int ToSquareIndex(this SquareFlag square)
        {
            if (square == 0)
                throw new IndexOutOfRangeException("Empty SquareFlag (zero) can not be converted to an index on the board. Ensure it is set to a valid square value.");

            var ulsquares = (ulong)square;

            var a = (uint)(ulsquares & 0b00000000_00000000_00000000_00000000_11111111_11111111_11111111_11111111);
            var b = (uint)((ulsquares & 0b11111111_11111111_11111111_11111111_00000000_00000000_00000000_00000000) >> 32);

            var x = 0;

            while (a > 0)
            {
                a >>= 1;
                ++x;
            }

            if (x > 0)
                return x - 1;

            x = 0;

            while (b > 0)
            {
                b >>= 1;
                ++x;
            }

            return 32 + x - 1;
        }

        public static IEnumerable<SquareFlag> ToList(this SquareFlag squares)
        {
            var ulsquares = (ulong)squares;

            var a = (uint)(ulsquares & 0b00000000_00000000_00000000_00000000_11111111_11111111_11111111_11111111);
            var b = (uint)((ulsquares & 0b11111111_11111111_11111111_11111111_00000000_00000000_00000000_00000000) >> 32);

            var x = 0;

            while (a > 0)
            {
                if ((a & 1) > 0)
                    yield return (SquareFlag)((ulong)1 << x);

                a >>= 1;
                ++x;
            }

            x = 0;

            while (b > 0)
            {
                if ((b & 1) > 0)
                    yield return (SquareFlag)((ulong)1 << (32 + x));

                b >>= 1;
                ++x;
            }
        }

        public static IEnumerable<SquareFlag> ToListFaster(this SquareFlag squares)
        {
            var ulsquares = (ulong)squares;

            for (var i = 1ul; i > 0; i = i << 1)
                if ((ulsquares & i) > 0)
                    yield return (SquareFlag)i;
        }

        public static IReadOnlyList<SquareFlag> ToListOrig(this SquareFlag squares)
        {
            IList<SquareFlag> squaresAsList = new List<SquareFlag>();

            for(var i = 1ul; i > 0; i = i << 1)
                if (squares.HasFlag((SquareFlag)i)) squaresAsList.Add((SquareFlag)i);

            return (IReadOnlyList<SquareFlag>)squaresAsList;
        }
    }
}
