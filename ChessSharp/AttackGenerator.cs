using ChessSharp.Enums;
using System;

namespace ChessSharp
{
    public class AttackGenerator
    {
        public static SquareFlag[] GenerateIntersections(int squareIndex)
        {
            var bit = 1ul << squareIndex;

            var rankBits = AllBitsOnForRank(squareIndex);

            var currentLine = bit;

            SquareFlag[] intersections = new SquareFlag[64];

            var toSquareIndex = squareIndex;

            // North
            do
            {
                bit <<= (int)MoveDirection.North;

                if (bit != 0)
                {
                    currentLine |= bit;
                    toSquareIndex += (int)MoveDirection.North;
                    intersections[toSquareIndex] = (SquareFlag)currentLine;
                }

            } while (bit != 0);

            bit = 1ul << squareIndex;
            currentLine = bit;
            toSquareIndex = squareIndex;

            // South
            do
            {
                bit >>= Math.Abs((int)MoveDirection.South);

                if (bit != 0)
                {
                    currentLine |= bit;
                    toSquareIndex += (int)MoveDirection.South;
                    intersections[toSquareIndex] = (SquareFlag)currentLine;
                }

            } while (bit != 0);

            bit = 1ul << squareIndex;
            currentLine = bit;
            toSquareIndex = squareIndex;

            // East
            do
            {
                bit <<= (int)MoveDirection.East;

                if ((bit & rankBits) != 0)
                {
                    currentLine |= bit;
                    toSquareIndex += (int)MoveDirection.East;
                    intersections[toSquareIndex] = (SquareFlag)currentLine;
                }

            } while (bit != 0);

            bit = 1ul << squareIndex;
            currentLine = bit;
            toSquareIndex = squareIndex;

            // West
            do
            {
                bit >>= Math.Abs((int)MoveDirection.West);

                if ((bit & rankBits) != 0)
                {
                    currentLine |= bit;
                    toSquareIndex += (int)MoveDirection.West;
                    intersections[toSquareIndex] = (SquareFlag)currentLine;
                }

            } while (bit != 0);

            bit = 1ul << squareIndex;
            currentLine = bit;
            toSquareIndex = squareIndex;
            var bit2 = bit;

            // North East
            do
            {
                bit <<= (int)MoveDirection.NorthEast;
                bit2 <<= (int)MoveDirection.East;

                if ((bit != 0) && (bit2 & rankBits) != 0)
                {
                    currentLine |= bit;
                    toSquareIndex += (int)MoveDirection.NorthEast;
                    intersections[toSquareIndex] = (SquareFlag)currentLine;
                }

            } while (bit != 0);

            bit = 1ul << squareIndex;
            currentLine = bit;
            toSquareIndex = squareIndex;
            bit2 = bit;

            // South East
            do
            {
                bit >>= Math.Abs((int)MoveDirection.SouthEast);
                bit2 <<= (int)MoveDirection.East;

                if ((bit != 0) && (bit2 & rankBits) != 0)
                {
                    currentLine |= bit;
                    toSquareIndex += (int)MoveDirection.SouthEast;
                    intersections[toSquareIndex] = (SquareFlag)currentLine;
                }

            } while (bit != 0);

            // South West
            do
            {
                bit >>= Math.Abs((int)MoveDirection.SouthWest);
                bit2 >>= Math.Abs((int)MoveDirection.West);

                if ((bit != 0) && (bit2 & rankBits) != 0)
                {
                    currentLine |= bit;
                    toSquareIndex += (int)MoveDirection.SouthWest;
                    intersections[toSquareIndex] = (SquareFlag)currentLine;
                }

            } while (bit != 0);

            // North West
            do
            {
                bit <<= (int)MoveDirection.NorthWest;
                bit2 >>= Math.Abs((int)MoveDirection.West);

                if ((bit != 0) && (bit2 & rankBits) != 0)
                {
                    currentLine |= bit;
                    toSquareIndex += (int)MoveDirection.NorthWest;
                    intersections[toSquareIndex] = (SquareFlag)currentLine;
                }

            } while (bit != 0);

            return intersections;
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

            //ulong rowBits = ((ulong)0xFF) << (8 * (squareIndex / 8));
            var rankBits = AllBitsOnForRank(squareIndex);

            do
            {
                bit <<= 8;
                attackableSquares |= bit;

            } while (bit != 0 && ((bit & occupancyAsLong) == 0));

            bit = 1ul << squareIndex;

            do
            {
                bit >>= 8;
                attackableSquares |= bit;

            } while (bit != 0 && ((bit & occupancyAsLong) == 0));

            bit = 1ul << squareIndex;

            do
            {
                bit <<= 1;

                if ((bit & rankBits) != 0)
                    attackableSquares |= bit;
                else
                    break;

            } while ((bit & occupancyAsLong) == 0);

            bit = 1ul << squareIndex;

            do
            {
                bit >>= 1;

                if ((bit & rankBits) != 0)
                    attackableSquares |= bit;
                else
                    break;

            } while ((bit & occupancyAsLong) == 0);

            return (SquareFlag)attackableSquares;
        }

        public static SquareFlag GeneratePotentialBishopAttacks(int squareIndex, SquareFlag occupancy)
        {
            ulong attackableSquares = 0;

            var bit = 1ul << squareIndex;
            var bit2 = bit;

            var occupancyAsLong = (ulong)occupancy;

            //ulong rowBits = (((ulong)0xFF) << (8 * (squareIndex / 8)));
            var rankBits = AllBitsOnForRank(squareIndex);

            do
            {
                bit <<= 8 - 1;
                bit2 >>= 1;

                if ((bit2 & rankBits) != 0)
                    attackableSquares |= bit;
                else
                    break;

            } while (bit != 0 && ((bit & occupancyAsLong) == 0));

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

            } while (bit != 0 && ((bit & occupancyAsLong) == 0));

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

            } while (bit != 0 && ((bit & occupancyAsLong) == 0));

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

            } while (bit != 0 && ((bit & occupancyAsLong) == 0));

            return (SquareFlag)attackableSquares;
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

        private static ulong AllBitsOnForRank(int squareIndex) =>
            ((ulong)0xFF) << (8 * (squareIndex / 8));

        private static ulong ShiftRankUp(ulong bits, int numRanks) =>
            bits << (8 * numRanks);

        private static ulong ShiftRankDown(ulong bits, int numRanks) =>
            bits >> (8 * numRanks);

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
