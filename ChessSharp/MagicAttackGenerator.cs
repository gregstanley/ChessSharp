using ChessSharp.Enums;
using System;

namespace ChessSharp
{
    public class MagicAttackGenerator
    {
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

        public static SquareFlag GeneratePawnCapture(int squareIndex, Colour colour)
        {
            ulong attackableSquares = 0;

            var bit = 1ul << squareIndex;

            if (colour == Colour.White)
            {
                var rankBits = AllBitsOnForRankAtOffset(squareIndex, 1);

                attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.NorthWest, rankBits);
                attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.NorthEast, rankBits);
            }
            else if(colour == Colour.Black)
            {
                var rankBits = AllBitsOnForRankAtOffset(squareIndex, -1);

                attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.SouthWest, rankBits);
                attackableSquares |= ShiftAndMaskRank(bit, (int)MoveDirection.SouthEast, rankBits);
            }

            return (SquareFlag)attackableSquares;
        }

        public static SquareFlag GenerateKnightAttack(int squareIndex)
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

        public static SquareFlag GenerateKingAttack(int squareIndex)
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

        public static SquareFlag GenerateRookAttack(int squareIndex, SquareFlag occupancy)
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

        public static SquareFlag GenerateBishopAttack(int squareIndex, SquareFlag occupancy)
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
    }
}
