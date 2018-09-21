using ChessSharp.Enums;
using System.Collections.Generic;

namespace ChessSharp.Extensions
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

        public const SquareFlag du8 = SquareFlag.A8;
        public const SquareFlag du7 = SquareFlag.A7 | SquareFlag.B8;
        public const SquareFlag du6 = SquareFlag.A6 | SquareFlag.B7 | SquareFlag.C8;
        public const SquareFlag du5 = SquareFlag.A5 | SquareFlag.B6 | SquareFlag.C7 | SquareFlag.D8;
        public const SquareFlag du4 = SquareFlag.A4 | SquareFlag.B5 | SquareFlag.C6 | SquareFlag.D7 | SquareFlag.E8;
        public const SquareFlag du3 = SquareFlag.A3 | SquareFlag.B4 | SquareFlag.C5 | SquareFlag.D6 | SquareFlag.E7 | SquareFlag.F8;
        public const SquareFlag du2 = SquareFlag.A2 | SquareFlag.B3 | SquareFlag.C4 | SquareFlag.D5 | SquareFlag.E6 | SquareFlag.F7 | SquareFlag.G8;
        public const SquareFlag du1 = SquareFlag.A1 | SquareFlag.B2 | SquareFlag.C3 | SquareFlag.D4 | SquareFlag.E5 | SquareFlag.F6 | SquareFlag.G7 | SquareFlag.H8;
        public const SquareFlag dub = SquareFlag.B1 | SquareFlag.C2 | SquareFlag.D3 | SquareFlag.E4 | SquareFlag.F5 | SquareFlag.G6 | SquareFlag.H7;
        public const SquareFlag duc = SquareFlag.C1 | SquareFlag.D2 | SquareFlag.E3 | SquareFlag.F4 | SquareFlag.G5 | SquareFlag.H6;
        public const SquareFlag dud = SquareFlag.D1 | SquareFlag.E2 | SquareFlag.F3 | SquareFlag.G4 | SquareFlag.H5;
        public const SquareFlag due = SquareFlag.E1 | SquareFlag.F2 | SquareFlag.F3 | SquareFlag.H4;
        public const SquareFlag duf = SquareFlag.F1 | SquareFlag.G2 | SquareFlag.H3;
        public const SquareFlag dug = SquareFlag.G1 | SquareFlag.H2;
        public const SquareFlag duh = SquareFlag.H1;

        public const SquareFlag dd8 = SquareFlag.H8;
        public const SquareFlag dd7 = SquareFlag.H7 | SquareFlag.G8;
        public const SquareFlag dd6 = SquareFlag.H6 | SquareFlag.G7 | SquareFlag.F8;
        public const SquareFlag dd5 = SquareFlag.H5 | SquareFlag.G6 | SquareFlag.F7 | SquareFlag.E8;
        public const SquareFlag dd4 = SquareFlag.H4 | SquareFlag.G5 | SquareFlag.F6 | SquareFlag.E7 | SquareFlag.D8;
        public const SquareFlag dd3 = SquareFlag.H3 | SquareFlag.G4 | SquareFlag.F5 | SquareFlag.E6 | SquareFlag.D7 | SquareFlag.C8;
        public const SquareFlag dd2 = SquareFlag.H2 | SquareFlag.G3 | SquareFlag.F4 | SquareFlag.E5 | SquareFlag.D6 | SquareFlag.C7 | SquareFlag.B8;
        public const SquareFlag dd1 = SquareFlag.H1 | SquareFlag.G2 | SquareFlag.F3 | SquareFlag.E4 | SquareFlag.D5 | SquareFlag.C6 | SquareFlag.B7 | SquareFlag.A8;
        public const SquareFlag ddg = SquareFlag.G1 | SquareFlag.F2 | SquareFlag.E3 | SquareFlag.D4 | SquareFlag.C5 | SquareFlag.B6 | SquareFlag.A7;
        public const SquareFlag ddf = SquareFlag.F1 | SquareFlag.E2 | SquareFlag.D3 | SquareFlag.C4 | SquareFlag.B5 | SquareFlag.A6;
        public const SquareFlag dde = SquareFlag.E1 | SquareFlag.D2 | SquareFlag.C3 | SquareFlag.B4 | SquareFlag.A5;
        public const SquareFlag ddd = SquareFlag.D1 | SquareFlag.C2 | SquareFlag.B3 | SquareFlag.A4;
        public const SquareFlag ddc = SquareFlag.C1 | SquareFlag.B2 | SquareFlag.A3;
        public const SquareFlag ddb = SquareFlag.B1 | SquareFlag.A2;
        public const SquareFlag dda = SquareFlag.A1;

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

        public static SquareFlag HorizontalAttacks(this SquareFlag square)
        {
            var boardIndex = ToBoardIndex(square);

            if (boardIndex == -1)
                throw new System.Exception("SquareFlag is not a valid board index (presumably more than one bit is set)");

            var cross = GetRank(square) ^ GetFile(square);

            return cross;
        }

        public static SquareFlag DiagonalAttacks(this SquareFlag square)
        {
            var boardIndex = ToBoardIndex(square);

            if (boardIndex == -1)
                throw new System.Exception("SquareFlag is not a valid board index (presumably more than one bit is set)");

            var cross = GetDiagonalUp(square) ^ GetDiagonalDown(square);

            return cross;
        }

        private static SquareFlag GetRank(this SquareFlag square)
        {
            if (r1.HasFlag(square)) return r1;
            if (r2.HasFlag(square)) return r2;
            if (r3.HasFlag(square)) return r3;
            if (r4.HasFlag(square)) return r4;
            if (r5.HasFlag(square)) return r5;
            if (r6.HasFlag(square)) return r6;
            if (r7.HasFlag(square)) return r7;
            if (r8.HasFlag(square)) return r8;

            return 0;
        }

        private static SquareFlag GetFile(this SquareFlag square)
        {
            if (a.HasFlag(square)) return a;
            if (b.HasFlag(square)) return b;
            if (c.HasFlag(square)) return c;
            if (d.HasFlag(square)) return d;
            if (e.HasFlag(square)) return e;
            if (f.HasFlag(square)) return f;
            if (g.HasFlag(square)) return g;
            if (h.HasFlag(square)) return h;

            return 0;
        }

        private static SquareFlag GetDiagonalUp(this SquareFlag square)
        {
            if (du8.HasFlag(square)) return du8;
            if (du7.HasFlag(square)) return du7;
            if (du6.HasFlag(square)) return du6;
            if (du5.HasFlag(square)) return du5;
            if (du4.HasFlag(square)) return du4;
            if (du3.HasFlag(square)) return du3;
            if (du2.HasFlag(square)) return du2;
            if (du1.HasFlag(square)) return du1;
            if (dub.HasFlag(square)) return dub;
            if (duc.HasFlag(square)) return duc;
            if (dud.HasFlag(square)) return dud;
            if (due.HasFlag(square)) return due;
            if (duf.HasFlag(square)) return duf;
            if (dug.HasFlag(square)) return dug;
            if (duh.HasFlag(square)) return duh;

            return 0;
        }

        private static SquareFlag GetDiagonalDown(this SquareFlag square)
        {
            if (dd8.HasFlag(square)) return dd8;
            if (dd7.HasFlag(square)) return dd7;
            if (dd6.HasFlag(square)) return dd6;
            if (dd5.HasFlag(square)) return dd5;
            if (dd4.HasFlag(square)) return dd4;
            if (dd3.HasFlag(square)) return dd3;
            if (dd2.HasFlag(square)) return dd2;
            if (dd1.HasFlag(square)) return dd1;
            if (ddg.HasFlag(square)) return ddg;
            if (ddf.HasFlag(square)) return ddf;
            if (dde.HasFlag(square)) return dde;
            if (ddd.HasFlag(square)) return ddd;
            if (ddc.HasFlag(square)) return ddc;
            if (ddb.HasFlag(square)) return ddb;
            if (dda.HasFlag(square)) return dda;

            return 0;
        }

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

        //public static RankFile ToRankFile(this SquareFlag square)
        //{
        //    var file = File.a;
        //    var rank = 0;

        //    if (a.HasFlag(square))
        //        file = File.a;
        //    else if (b.HasFlag(square))
        //        file = File.b;
        //    else if (c.HasFlag(square))
        //        file = File.c;
        //    else if (d.HasFlag(square))
        //        file = File.d;
        //    else if (e.HasFlag(square))
        //        file = File.e;
        //    else if (f.HasFlag(square))
        //        file = File.f;
        //    else if (g.HasFlag(square))
        //        file = File.g;
        //    else if (h.HasFlag(square))
        //        file = File.h;

        //    if (r1.HasFlag(square))
        //        rank = 1;
        //    else if (r2.HasFlag(square))
        //        rank = 2;
        //    else if (r3.HasFlag(square))
        //        rank = 3;
        //    else if (r4.HasFlag(square))
        //        rank = 4;
        //    else if (r5.HasFlag(square))
        //        rank = 5;
        //    else if (r6.HasFlag(square))
        //        rank = 6;
        //    else if (r7.HasFlag(square))
        //        rank = 7;
        //    else if (r8.HasFlag(square))
        //        rank = 8;

        //    return RankFile.Get(rank, file);
        //}
    }
}
