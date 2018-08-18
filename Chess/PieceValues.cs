using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess
{
    public class PieceValues
    {
        private static double[][] _blackPawn = null;
        private static double[][] _blackBishop = null;
        private static double[][] _blackRook = null;
        private static double[][] _blackKing = null;

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

        public static double[][] BlackPawn
        {
            get
            {
                if (_blackPawn == null)
                    _blackPawn = Reverse(WhitePawn);

                return _blackPawn;
            }
        }


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

        public static double[][] BlackBishop
        {
            get
            {
                if (_blackBishop == null)
                    _blackBishop = Reverse(WhiteBishop);

                return _blackBishop;
            }
        }

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

        public static double[][] BlackRook
        {
            get
            {
                if (_blackRook == null)
                    _blackRook = Reverse(WhiteRook);

                return _blackRook;
            }
        }

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

        public static double[][] BlackKing
        {
            get
            {
                if (_blackKing == null)
                    _blackKing = Reverse(WhiteKing);

                return _blackKing;
            }
        }

        private static double[][] Reverse(double[][] array)
        {
            var clone = array.Reverse();

            return clone.ToArray();
        }
    }
}
