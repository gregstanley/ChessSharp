using System.Collections.Generic;
using System.Linq;

namespace ChessSharp.Engine
{
    internal class PieceValues
    {
        public static double[] PawnModifiers;
        public static double[] RookModifiers;
        public static double[] KnightModifiers;
        public static double[] BishopModifiers;
        public static double[] QueenModifiers;
        public static double[] KingModifiers;

        public static void Init()
        {
            PawnModifiers = ToArray(WhitePawn);
            RookModifiers = ToArray(WhiteRook);
            KnightModifiers = ToArray(Knight);
            BishopModifiers = ToArray(WhiteBishop);
            QueenModifiers = ToArray(Queen);
            KingModifiers = ToArray(WhiteKing);
        }

        public static readonly double[][] WhitePawn = new double[][]
        {
            new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 },
            new[] { 5.0, 5.0, 5.0, 5.0, 5.0, 5.0, 5.0, 5.0 },
            new[] { 1.0, 1.0, 2.0, 3.0, 3.0, 2.0, 1.0, 1.0 },
            new[] { 0.5, 0.5, 1.0, 2.5, 2.5, 1.0, 0.5, 0.5 },
            new[] { 0.0, 0.0, 0.0, 2.0, 2.0, 0.0, 0.0, 0.0 },
            new[] { 0.5, -0.5, -1.0, 0.0, 0.0, -1.0, -0.5, 0.5 },
            new[] { 0.5, 1.0, 1.0, -2.0, -2.0, 1.0, 1.0, 0.5 },
            new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }
        };

        public static readonly double[][] Knight = new double[][]
        {
            new[] { -5.0, -4.0, -3.0, -3.0, -3.0, -3.0, -4.0, -5.0 },
            new[] { -4.0, -2.0, 0.0, 0.0, 0.0, 0.0, -2.0, -4.0 },
            new[] { -3.0, 0.0, 1.0, 1.5, 1.5, 1.0, 0.0, -3.0 },
            new[] { -3.0, 0.5, 1.5, 2.0, 2.0, 1.5, 0.5, -3.0 },
            new[] { -3.0, 0.0, 1.5, 2.0, 2.0, 1.5, 0.0, -3.0 },
            new[] { -3.0, 0.5, 1.0, 1.5, 1.5, 1.0, 0.5, -3.0 },
            new[] { -4.0, -2.0, 0.0, 0.5, 0.5, 0.0, -2.0, -4.0 },
            new[] { -5.0, -4.0, -3.0, -3.0, -3.0, -3.0, -4.0, -5.0 }
        };

        public static readonly double[][] WhiteBishop = new double[][]
        {
            new[] { -2.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -2.0 },
            new[] { -1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -1.0 },
            new[] { -1.0, 0.0, 0.5, 1.0, 1.0, 0.5, 0.0, -1.0 },
            new[] { -1.0, 0.5, 0.5, 1.0, 1.0, 0.5, 0.5, -1.0 },
            new[] { -1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 0.0, -1.0 },
            new[] { -1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, -1.0 },
            new[] { -1.0, 0.5, 0.0, 0.0, 0.0, 0.0, 0.5, -1.0 },
            new[] { -2.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -2.0 }
        };

        public static readonly double[][] WhiteRook = new double[][]
        {
            new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 },
            new[] { 0.5, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.5 },
            new[] { -0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5 },
            new[] { -0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5 },
            new[] { -0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5 },
            new[] { -0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5 },
            new[] { -0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5 },
            new[] { 0.0, 0.0, 0.0, 0.5, 0.5, 0.0, 0.0, 0.0 }
        };

        public static readonly double[][] Queen = new double[][]
        {
            new[] { -2.0, -1.0, -1.0, -0.5, -0.5, -1.0, -1.0, -2.0 },
            new[] { -1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -1.0 },
            new[] { -1.0, 0.0, 0.5, 0.5, 0.5, 0.5, 0.0, -1.0 },
            new[] { -0.5, 0.0, 0.5, 0.5, 0.5, 0.5, 0.0, -0.5 },
            new[] { 0.0, 0.0, 0.5, 0.5, 0.5, 0.5, 0.0, -0.5 },
            new[] { -1.0, 0.5, 0.5, 0.5, 0.5, 0.5, 0.0, -1.0 },
            new[] { -1.0, 0.0, 0.5, 0.0, 0.0, 0.0, 0.0, -1.0 },
            new[] { -2.0, -1.0, -1.0, -0.5, -0.5, -1.0, -1.0, -2.0 }
        };

        public static readonly double[][] WhiteKing = new double[][]
        {
            new[] { -3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0 },
            new[] { -3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0 },
            new[] { -3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0 },
            new[] { -3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0 },
            new[] { -2.0, -3.0, -3.0, -4.0, -4.0, -3.0, -3.0, -2.0 },
            new[] { -1.0, -2.0, -2.0, -2.0, -2.0, -2.0, -2.0, -1.0 },
            new[] { 2.0, 2.0, 0.0, 0.0, 0.0, 0.0, 2.0, 2.0 },
            new[] { 2.0, 3.0, 1.0, 0.0, 0.0, 1.0, 3.0, 2.0 }
        };

        private static double[][] Reverse(double[][] array)
        {
            var clone = array.Reverse();

            return clone.ToArray();
        }

        private static double[] ToArray(double[][] source)
        {
            var output = new List<double>(64);

            var sourceReversed = source.Reverse();

            foreach (var rank in sourceReversed)
                output.AddRange(rank);

            return output.ToArray();
        }
    }
}
