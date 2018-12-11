using System.Collections.Generic;
using System.Linq;

namespace ChessSharp.Engine
{
    internal class PieceValues
    {
        public static int[] PawnModifiers;
        public static int[] RookModifiers;
        public static int[] KnightModifiers;
        public static int[] BishopModifiers;
        public static int[] QueenModifiers;
        public static int[] KingModifiers;

        public static void Init()
        {
            PawnModifiers = ToArray(WhitePawn);
            RookModifiers = ToArray(WhiteRook);
            KnightModifiers = ToArray(Knight);
            BishopModifiers = ToArray(WhiteBishop);
            QueenModifiers = ToArray(Queen);
            KingModifiers = ToArray(WhiteKing);
        }

        public static readonly int[][] WhitePawn = new int[][]
        {
            new[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new[] { 50, 50, 50, 50, 50, 50, 50, 50 },
            new[] { 10, 10, 20, 30, 30, 20, 10, 10 },
            new[] { 5, 5, 10, 25, 25, 10, 5, 5 },
            new[] { 0, 0, 0, 20, 20, 0, 0, 0 },
            new[] { 5, -5, -10, 0, 0, -10, -5, 5 },
            new[] { 5, 10, 10, -20, -20, 10, 10, 5 },
            new[] { 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        public static readonly int[][] Knight = new int[][]
        {
            new[] { -50, -40, -30, -30, -30, -30, -40, -50 },
            new[] { -40, -20, 0, 0, 0, 0, -20, -40 },
            new[] { -30, 0, 10, 15, 15, 10, 0, -30 },
            new[] { -30, 5, 15, 20, 20, 15, 5, -30 },
            new[] { -30, 0, 15, 20, 20, 15, 0, -30 },
            new[] { -30, 5, 10, 15, 15, 10, 5, -30 },
            new[] { -40, -20, 0, 5, 5, 0, -20, -40 },
            new[] { -50, -40, -30, -30, -30, -30, -40, -50 }
        };

        public static readonly int[][] WhiteBishop = new int[][]
        {
            new[] { -20, -10, -10, -10, -10, -10, -10, -20 },
            new[] { -10, 0, 0, 0, 0, 0, 0, -10 },
            new[] { -10, 0, 5, 10, 10, 5, 0, -10 },
            new[] { -10, 5, 5, 10, 10, 5, 5, -10 },
            new[] { -10, 0, 10, 10, 10, 10, 0, -10 },
            new[] { -10, 10, 10, 10, 10, 10, 10, -10 },
            new[] { -10, 5, 0, 0, 0, 0, 5, -10 },
            new[] { -20, -10, -10, -10, -10, -10, -10, -20 }
        };

        public static readonly int[][] WhiteRook = new int[][]
        {
            new[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new[] { 5, 10, 10, 10, 10, 10, 10, 5 },
            new[] { -5, 0, 0, 0, 0, 0, 0, -5 },
            new[] { -5, 0, 0, 0, 0, 0, 0, -5 },
            new[] { -5, 0, 0, 0, 0, 0, 0, -5 },
            new[] { -5, 0, 0, 0, 0, 0, 0, -5 },
            new[] { -5, 0, 0, 0, 0, 0, 0, -5 },
            new[] { 0, 0, 0, 5, 5, 0, 0, 0 }
        };

        public static readonly int[][] Queen = new int[][]
        {
            new[] { -20, -10, -10, -5, -5, -10, -10, -20 },
            new[] { -10, 0, 0, 0, 0, 0, 0, -10 },
            new[] { -10, 0, 5, 5, 5, 5, 0, -10 },
            new[] { -5, 0, 5, 5, 5, 5, 0, -5 },
            new[] { 0, 0, 5, 5, 5, 5, 0, -5 },
            new[] { -10, 5, 5, 5, 5, 5, 0, -10 },
            new[] { -10, 0, 5, 0, 0, 0, 0, -10 },
            new[] { -20, -10, -10, -5, -5, -10, -10, -20 }
        };

        public static readonly int[][] WhiteKing = new int[][]
        {
            new[] { -30, -40, -40, -50, -50, -40, -40, -30 },
            new[] { -30, -40, -40, -50, -50, -40, -40, -30 },
            new[] { -30, -40, -40, -50, -50, -40, -40, -30 },
            new[] { -30, -40, -40, -50, -50, -40, -40, -30 },
            new[] { -20, -30, -30, -40, -40, -30, -30, -20 },
            new[] { -10, -20, -20, -20, -20, -20, -20, -10 },
            new[] { 20, 20, 0, 0, 0, 0, 20, 20 },
            new[] { 20, 30, 10, 0, 0, 10, 30, 20 }
        };

        public static readonly double[][] WhitePawnDouble = new double[][]
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

        public static readonly double[][] KnightDouble = new double[][]
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

        public static readonly double[][] WhiteBishopDouble = new double[][]
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

        public static readonly double[][] WhiteRookDouble = new double[][]
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

        public static readonly double[][] QueenDouble = new double[][]
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

        public static readonly double[][] WhiteKingDouble = new double[][]
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

        private static int[][] Reverse(int[][] array)
        {
            var clone = array.Reverse();

            return clone.ToArray();
        }

        private static double[][] Reverse(double[][] array)
        {
            var clone = array.Reverse();

            return clone.ToArray();
        }

        private static int[] ToArray(int[][] source)
        {
            var output = new List<int>(64);

            var sourceReversed = source.Reverse();

            foreach (var rank in sourceReversed)
                output.AddRange(rank);

            return output.ToArray();
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
