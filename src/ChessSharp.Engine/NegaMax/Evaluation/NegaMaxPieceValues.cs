using System.Collections.Generic;
using System.Linq;

namespace ChessSharp.Engine.NegaMax
{
    internal class NegaMaxPieceValues
    {
        public const int CheckmateValue = -short.MaxValue / 2;

        private static readonly int[][] WhitePawn = new int[][]
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

        private static readonly int[][] Knight = new int[][]
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

        private static readonly int[][] WhiteBishop = new int[][]
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

        private static readonly int[][] WhiteRook = new int[][]
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

        private static readonly int[][] Queen = new int[][]
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

        private static readonly int[][] WhiteKing = new int[][]
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

        public static int[] PawnModifiers { get; private set; }

        public static int[] RookModifiers { get; private set; }

        public static int[] KnightModifiers { get; private set; }

        public static int[] BishopModifiers { get; private set; }

        public static int[] QueenModifiers { get; private set; }

        public static int[] KingModifiers { get; private set; }

        public static void Init()
        {
            PawnModifiers = ToArray(WhitePawn);
            RookModifiers = ToArray(WhiteRook);
            KnightModifiers = ToArray(Knight);
            BishopModifiers = ToArray(WhiteBishop);
            QueenModifiers = ToArray(Queen);
            KingModifiers = ToArray(WhiteKing);
        }

        private static int[] ToArray(int[][] source)
        {
            // TODO: Could probably be something like (need to check):
            // return source.Reverse().SelectMany(x => x).ToArray();
            var output = new List<int>(64);

            var sourceReversed = source.Reverse();

            foreach (var rank in sourceReversed)
                output.AddRange(rank);

            return output.ToArray();
        }
    }
}
