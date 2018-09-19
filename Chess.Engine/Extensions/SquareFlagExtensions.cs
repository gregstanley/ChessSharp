using Chess.Engine.Models;
using System.Collections.Generic;

namespace Chess.Engine.Extensions
{
    public static class SquareFlagExtensions
    {
        public const SquareFlag a = SquareFlag.A1 | SquareFlag.A2 | SquareFlag.A3 | SquareFlag.A4 | SquareFlag.A5 | SquareFlag.A6 | SquareFlag.A7 | SquareFlag.A8;
        public const SquareFlag b = SquareFlag.B1 | SquareFlag.B2 | SquareFlag.B3 | SquareFlag.B4 | SquareFlag.B5 | SquareFlag.B6 | SquareFlag.B7 | SquareFlag.B8;
        public const SquareFlag c = SquareFlag.C1 | SquareFlag.C2 | SquareFlag.C3 | SquareFlag.C4 | SquareFlag.C5 | SquareFlag.C6 | SquareFlag.C7 | SquareFlag.C8;
        public const SquareFlag d = SquareFlag.D1 | SquareFlag.D2 | SquareFlag.D3 | SquareFlag.D4 | SquareFlag.D5 | SquareFlag.D6 | SquareFlag.D7 | SquareFlag.D8;
        public const SquareFlag e = SquareFlag.E1 | SquareFlag.E2 | SquareFlag.E3 | SquareFlag.E4 | SquareFlag.E5 | SquareFlag.E6 | SquareFlag.E7 | SquareFlag.E8;
        public const SquareFlag f = SquareFlag.F1 | SquareFlag.F2 | SquareFlag.F3 | SquareFlag.F4 | SquareFlag.F5 | SquareFlag.F6 | SquareFlag.F7 | SquareFlag.F8;
        public const SquareFlag g = SquareFlag.G1 | SquareFlag.G2 | SquareFlag.G3 | SquareFlag.G4 | SquareFlag.G5 | SquareFlag.G6 | SquareFlag.G7 | SquareFlag.G8;
        public const SquareFlag h = SquareFlag.H1 | SquareFlag.H2 | SquareFlag.H3 | SquareFlag.H4 | SquareFlag.H5 | SquareFlag.H6 | SquareFlag.H7 | SquareFlag.H8;

        public const SquareFlag r1 = SquareFlag.A1 | SquareFlag.B1 | SquareFlag.C1 | SquareFlag.D1 | SquareFlag.E1 | SquareFlag.F1 | SquareFlag.G1 | SquareFlag.H1;
        public const SquareFlag r2 = SquareFlag.A2 | SquareFlag.B2 | SquareFlag.C2 | SquareFlag.D2 | SquareFlag.E2 | SquareFlag.F2 | SquareFlag.G2 | SquareFlag.H2;
        public const SquareFlag r3 = SquareFlag.A3 | SquareFlag.B3 | SquareFlag.C3 | SquareFlag.D3 | SquareFlag.E3 | SquareFlag.F3 | SquareFlag.G3 | SquareFlag.H3;
        public const SquareFlag r4 = SquareFlag.A4 | SquareFlag.B4 | SquareFlag.C4 | SquareFlag.D4 | SquareFlag.E4 | SquareFlag.F4 | SquareFlag.G4 | SquareFlag.H4;
        public const SquareFlag r5 = SquareFlag.A5 | SquareFlag.B5 | SquareFlag.C5 | SquareFlag.D5 | SquareFlag.E5 | SquareFlag.F5 | SquareFlag.G5 | SquareFlag.H5;
        public const SquareFlag r6 = SquareFlag.A6 | SquareFlag.B6 | SquareFlag.C6 | SquareFlag.D6 | SquareFlag.E6 | SquareFlag.F6 | SquareFlag.G6 | SquareFlag.H6;
        public const SquareFlag r7 = SquareFlag.A7 | SquareFlag.B7 | SquareFlag.C7 | SquareFlag.D7 | SquareFlag.E7 | SquareFlag.F7 | SquareFlag.G7 | SquareFlag.H7;
        public const SquareFlag r8 = SquareFlag.A8 | SquareFlag.B8 | SquareFlag.C8 | SquareFlag.D8 | SquareFlag.E8 | SquareFlag.F8 | SquareFlag.G8 | SquareFlag.H8;

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

        public static int ToBoardIndex(this SquareFlag square) =>
            _indices.ContainsKey(square) ? _indices[square] : -1;

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

            for (var i = 1ul; i > 0; i = i << 1)
                if (squares.HasFlag((SquareFlag)i)) ++count;

            return count;
        }

        public static IReadOnlyList<SquareFlag> ToList(this SquareFlag squares)
        {
            IList<SquareFlag> squaresAsList = new List<SquareFlag>();

            for(var i = 1ul; i > 0; i = i << 1)
                if (squares.HasFlag((SquareFlag)i)) squaresAsList.Add((SquareFlag)i);

            return (IReadOnlyList<SquareFlag>)squaresAsList;
        }

        public static RankFile ToRankFile(this SquareFlag square)
        {
            var file = File.a;
            var rank = 0;

            if (a.HasFlag(square))
                file = File.a;
            else if (b.HasFlag(square))
                file = File.b;
            else if (c.HasFlag(square))
                file = File.c;
            else if (d.HasFlag(square))
                file = File.d;
            else if (e.HasFlag(square))
                file = File.e;
            else if (f.HasFlag(square))
                file = File.f;
            else if (g.HasFlag(square))
                file = File.g;
            else if (h.HasFlag(square))
                file = File.h;

            if (r1.HasFlag(square))
                rank = 1;
            else if (r2.HasFlag(square))
                rank = 2;
            else if (r3.HasFlag(square))
                rank = 3;
            else if (r4.HasFlag(square))
                rank = 4;
            else if (r5.HasFlag(square))
                rank = 5;
            else if (r6.HasFlag(square))
                rank = 6;
            else if (r7.HasFlag(square))
                rank = 7;
            else if (r8.HasFlag(square))
                rank = 8;

            return RankFile.Get(rank, file);
        }
    }
}
