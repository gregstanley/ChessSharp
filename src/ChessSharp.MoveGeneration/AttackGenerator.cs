using System;
using ChessSharp.Common.Enums;

namespace ChessSharp.MoveGeneration
{
    internal class AttackGenerator
    {
        public static SquareFlag[] GeneratePaths(int squareIndex)
        {
            SquareFlag[] paths = new SquareFlag[64];

            GenerateNorthPath(squareIndex, paths);
            GenerateNorthEastPath(squareIndex, paths);
            GenerateEastPath(squareIndex, paths);
            GenerateSouthEastPath(squareIndex, paths);
            GenerateSouthPath(squareIndex, paths);
            GenerateSouthWestPath(squareIndex, paths);
            GenerateWestPath(squareIndex, paths);
            GenerateNorthWestPath(squareIndex, paths);

            return paths;
        }

        public static SquareFlag GeneratePotentialWhitePawnCaptures(int squareIndex)
        {
            ulong attackableSquares = 0;

            var bit = 1ul << squareIndex;

            var rankBits = AllBitsOnForRankAtOffset(squareIndex, 1);

            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.NorthWest, rankBits);
            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.NorthEast, rankBits);

            return (SquareFlag)attackableSquares;
        }

        public static SquareFlag GeneratePotentialBlackPawnCaptures(int squareIndex)
        {
            ulong attackableSquares = 0;

            var bit = 1ul << squareIndex;

            var rankBits = AllBitsOnForRankAtOffset(squareIndex, -1);

            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.SouthWest, rankBits);
            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.SouthEast, rankBits);

            return (SquareFlag)attackableSquares;
        }

        public static SquareFlag GeneratePotentialKnightAttacks(int squareIndex)
        {
            ulong attackableSquares = 0;

            var bit = 1ul << squareIndex;

            var rankBits = AllBitsOnForRankAtOffset(squareIndex, 2);

            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.NorthNorthWest, rankBits);
            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.NorthNorthEast, rankBits);

            rankBits = AllBitsOnForRankAtOffset(squareIndex, 1);

            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.WestNorthWest, rankBits);
            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.EastNorthEast, rankBits);

            rankBits = AllBitsOnForRankAtOffset(squareIndex, -1);

            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.WestSouthWest, rankBits);
            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.EastSouthEast, rankBits);

            rankBits = AllBitsOnForRankAtOffset(squareIndex, -2);

            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.SouthSouthWest, rankBits);
            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.SouthSouthEast, rankBits);

            return (SquareFlag)attackableSquares;
        }

        public static SquareFlag GeneratePotentialKingAttacks(int squareIndex)
        {
            ulong attackableSquares = 0;

            var bit = 1ul << squareIndex;

            var rankBits = AllBitsOnForRankAtOffset(squareIndex, 1);

            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.NorthWest, rankBits);
            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.North, rankBits);
            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.NorthEast, rankBits);

            rankBits = AllBitsOnForRank(squareIndex);

            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.West, rankBits);
            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.East, rankBits);

            rankBits = AllBitsOnForRankAtOffset(squareIndex, -1);

            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.SouthWest, rankBits);
            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.South, rankBits);
            attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.SouthEast, rankBits);

            return (SquareFlag)attackableSquares;
        }

        public static SquareFlag GeneratePotentialRookAttacks(int squareIndex, SquareFlag occupancy)
        {
            ulong attackableSquares = 0;

            var bit = 1ul << squareIndex;

            var occupancyAsLong = (ulong)occupancy;

            var rankBits = AllBitsOnForRank(squareIndex);

            do
            {
                bit <<= 8;
                attackableSquares |= bit;
            }
            while (bit != 0 && ((bit & occupancyAsLong) == 0));

            bit = 1ul << squareIndex;

            do
            {
                bit >>= 8;
                attackableSquares |= bit;
            }
            while (bit != 0 && ((bit & occupancyAsLong) == 0));

            bit = 1ul << squareIndex;

            do
            {
                bit <<= 1;

                if ((bit & rankBits) != 0)
                    attackableSquares |= bit;
                else
                    break;
            }
            while ((bit & occupancyAsLong) == 0);

            bit = 1ul << squareIndex;

            do
            {
                bit >>= 1;

                if ((bit & rankBits) != 0)
                    attackableSquares |= bit;
                else
                    break;
            }
            while ((bit & occupancyAsLong) == 0);

            return (SquareFlag)attackableSquares;
        }

        public static SquareFlag GeneratePotentialBishopAttacks(int squareIndex, SquareFlag occupancy)
        {
            ulong attackableSquares = 0;

            var bit = 1ul << squareIndex;
            var bit2 = bit;

            var occupancyAsLong = (ulong)occupancy;

            var rankBits = AllBitsOnForRank(squareIndex);

            do
            {
                bit <<= 8 - 1;
                bit2 >>= 1;

                if ((bit2 & rankBits) != 0)
                    attackableSquares |= bit;
                else
                    break;
            }
            while (bit != 0 && ((bit & occupancyAsLong) == 0));

            bit = 1ul << squareIndex;
            bit2 = bit;

            do
            {
                bit <<= 8 + 1;
                bit2 <<= 1;

                if ((bit2 & rankBits) != 0)
                    attackableSquares |= bit;
                else
                    break;
            }
            while (bit != 0 && ((bit & occupancyAsLong) == 0));

            bit = 1ul << squareIndex;
            bit2 = bit;

            do
            {
                bit >>= 8 - 1;
                bit2 <<= 1;

                if ((bit2 & rankBits) != 0)
                    attackableSquares |= bit;
                else
                    break;
            }
            while (bit != 0 && ((bit & occupancyAsLong) == 0));

            bit = 1ul << squareIndex;
            bit2 = bit;

            do
            {
                bit >>= 8 + 1;
                bit2 >>= 1;

                if ((bit2 & rankBits) != 0)
                    attackableSquares |= bit;
                else
                    break;
            }
            while (bit != 0 && ((bit & occupancyAsLong) == 0));

            return (SquareFlag)attackableSquares;
        }

        private static void GenerateNorthPath(int squareIndex, SquareFlag[] paths)
        {
            var bit = 1ul << squareIndex;
            var path = bit;
            var toSquareIndex = squareIndex;

            do
            {
                bit <<= (int)MoveDirection.North;

                if (bit != 0)
                {
                    toSquareIndex += (int)MoveDirection.North;
                    paths[toSquareIndex] = (SquareFlag)(path |= bit);
                }
            }
            while (bit != 0);
        }

        private static void GenerateSouthPath(int squareIndex, SquareFlag[] paths)
        {
            var bit = 1ul << squareIndex;
            var path = bit;
            var toSquareIndex = squareIndex;

            do
            {
                bit >>= Math.Abs((int)MoveDirection.South);

                if (bit != 0)
                {
                    toSquareIndex += (int)MoveDirection.South;
                    paths[toSquareIndex] = (SquareFlag)(path |= bit);
                }
            }
            while (bit != 0);
        }

        private static void GenerateEastPath(int squareIndex, SquareFlag[] paths)
        {
            var bit = 1ul << squareIndex;
            var path = bit;
            var toSquareIndex = squareIndex;

            var rankBits = AllBitsOnForRank(squareIndex);

            do
            {
                bit <<= (int)MoveDirection.East;

                if ((bit & rankBits) != 0)
                {
                    toSquareIndex += (int)MoveDirection.East;
                    paths[toSquareIndex] = (SquareFlag)(path |= bit);
                }
            }
            while (bit != 0);
        }

        private static void GenerateWestPath(int squareIndex, SquareFlag[] paths)
        {
            var bit = 1ul << squareIndex;
            var path = bit;
            var toSquareIndex = squareIndex;

            var rankBits = AllBitsOnForRank(squareIndex);

            do
            {
                bit >>= Math.Abs((int)MoveDirection.West);

                if ((bit & rankBits) != 0)
                {
                    toSquareIndex += (int)MoveDirection.West;
                    paths[toSquareIndex] = (SquareFlag)(path |= bit);
                }
            }
            while (bit != 0);
        }

        private static void GenerateNorthEastPath(int squareIndex, SquareFlag[] paths)
        {
            var bit = 1ul << squareIndex;
            var path = bit;
            var toSquareIndex = squareIndex;

            var rankBits = AllBitsOnForRank(squareIndex);
            var bit2 = bit;

            do
            {
                bit <<= (int)MoveDirection.NorthEast;
                bit2 <<= (int)MoveDirection.East;

                if ((bit != 0) && (bit2 & rankBits) != 0)
                {
                    toSquareIndex += (int)MoveDirection.NorthEast;
                    paths[toSquareIndex] = (SquareFlag)(path |= bit);
                }
            }
            while (bit != 0);
        }

        private static void GenerateSouthEastPath(int squareIndex, SquareFlag[] paths)
        {
            var bit = 1ul << squareIndex;
            var path = bit;
            var toSquareIndex = squareIndex;

            var rankBits = AllBitsOnForRank(squareIndex);
            var bit2 = bit;

            do
            {
                bit >>= Math.Abs((int)MoveDirection.SouthEast);
                bit2 <<= (int)MoveDirection.East;

                if ((bit != 0) && (bit2 & rankBits) != 0)
                {
                    toSquareIndex += (int)MoveDirection.SouthEast;
                    paths[toSquareIndex] = (SquareFlag)(path |= bit);
                }
            }
            while (bit != 0);
        }

        private static void GenerateSouthWestPath(int squareIndex, SquareFlag[] paths)
        {
            var bit = 1ul << squareIndex;
            var path = bit;
            var toSquareIndex = squareIndex;

            var rankBits = AllBitsOnForRank(squareIndex);
            var bit2 = bit;

            do
            {
                bit >>= Math.Abs((int)MoveDirection.SouthWest);
                bit2 >>= Math.Abs((int)MoveDirection.West);

                if ((bit != 0) && (bit2 & rankBits) != 0)
                {
                    toSquareIndex += (int)MoveDirection.SouthWest;
                    paths[toSquareIndex] = (SquareFlag)(path |= bit);
                }
            }
            while (bit != 0);
        }

        private static void GenerateNorthWestPath(int squareIndex, SquareFlag[] paths)
        {
            var bit = 1ul << squareIndex;
            var path = bit;
            var toSquareIndex = squareIndex;

            var rankBits = AllBitsOnForRank(squareIndex);
            var bit2 = bit;

            do
            {
                bit <<= (int)MoveDirection.NorthWest;
                bit2 >>= Math.Abs((int)MoveDirection.West);

                if ((bit != 0) && (bit2 & rankBits) != 0)
                {
                    toSquareIndex += (int)MoveDirection.NorthWest;
                    paths[toSquareIndex] = (SquareFlag)(path |= bit);
                }
            }
            while (bit != 0);
        }

        private static ulong AllBitsOnForRankAtOffset(int squareIndex, int offset)
        {
            var rankBits = AllBitsOnForRank(squareIndex);

            return offset == 0
                ? rankBits
                : offset > 0
                    ? ShiftRankUp(rankBits, offset)
                    : ShiftRankDown(rankBits, Math.Abs(offset));
        }

        private static ulong AllBitsOnForRank(int squareIndex)
        {
            return ((ulong)0xFF) << (8 * (squareIndex / 8));
        }

        private static ulong ShiftRankUp(ulong bits, int numRanks)
        {
            return bits << (8 * numRanks);
        }

        private static ulong ShiftRankDown(ulong bits, int numRanks)
        {
            return bits >> (8 * numRanks);
        }

        private static ulong ShiftAndMaskRank(ulong bit, int shiftAmount, ulong rankBits)
        {
            if (shiftAmount > 0)
                bit <<= shiftAmount;
            else if (shiftAmount < 0)
                bit >>= Math.Abs(shiftAmount);

            return bit & rankBits;
        }
    }
}
