using ChessSharp.Enums;

namespace ChessSharp
{
    public class MagicAttackGenerator
    {
        public static SquareFlag GenerateKingAttack(int square)
        {
            ulong attackableSquares = 0;

            var bit = 1ul << square;

            ulong rowBits = ((ulong)0xFF) << (8 * (square / 8));

            bit <<= 8;
            attackableSquares |= bit;

            bit = 1ul << square;

            bit >>= 8;
            attackableSquares |= bit;

            bit = 1ul << (square + 1);

            if ((bit & rowBits) != 0)
            {
                attackableSquares |= bit;

                bit <<= 8;
                attackableSquares |= bit;

                bit = 1ul << (square + 1);

                bit >>= 8;
                attackableSquares |= bit;
            }

            bit = 1ul << (square - 1);

            if ((bit & rowBits) != 0)
            {
                attackableSquares |= bit;

                bit <<= 8;
                attackableSquares |= bit;

                bit = 1ul << (square - 1);

                bit >>= 8;
                attackableSquares |= bit;
            }

            return (SquareFlag)attackableSquares;
        }

        public static SquareFlag GenerateRookAttack(int square, SquareFlag occupancy)
        {
            ulong attackableSquares = 0;

            var bit = 1ul << square;

            var occupancyAsLong = (ulong)occupancy;

            ulong rowBits = ((ulong)0xFF) << (8 * (square / 8));

            do
            {
                bit <<= 8;
                attackableSquares |= bit;

            } while (bit != 0 && ((bit & occupancyAsLong) == 0));

            bit = 1ul << square;

            do
            {
                bit >>= 8;
                attackableSquares |= bit;

            } while (bit != 0 && ((bit & occupancyAsLong) == 0));

            bit = 1ul << square;

            do
            {
                bit <<= 1;

                if ((bit & rowBits) != 0)
                    attackableSquares |= bit;
                else
                    break;

            } while ((bit & occupancyAsLong) == 0);

            bit = 1ul << square;

            do
            {
                bit >>= 1;

                if ((bit & rowBits) != 0)
                    attackableSquares |= bit;
                else
                    break;

            } while ((bit & occupancyAsLong) == 0);

            return (SquareFlag)attackableSquares;
        }

        public static SquareFlag GenerateBishopAttack(int square, SquareFlag occupancy)
        {
            ulong attackableSquares = 0;

            var bit = 1ul << square;
            var bit2 = bit;

            var occupancyAsLong = (ulong)occupancy;

            ulong rowBits = (((ulong)0xFF) << (8 * (square / 8)));

            do
            {
                bit <<= 8 - 1;
                bit2 >>= 1;

                if ((bit2 & rowBits) != 0)
                    attackableSquares |= bit;
                else
                    break;

            } while (bit != 0 && ((bit & occupancyAsLong) == 0));

            bit = 1ul << square;
            bit2 = bit;

            do
            {
                bit <<= 8 + 1;
                bit2 <<= 1;

                if ((bit2 & rowBits) != 0)
                    attackableSquares |= bit;
                else
                    break;

            } while (bit != 0 && ((bit & occupancyAsLong) == 0));

            bit = 1ul << square;
            bit2 = bit;

            do
            {
                bit >>= 8 - 1;
                bit2 <<= 1;

                if ((bit2 & rowBits) != 0)
                    attackableSquares |= bit;
                else
                    break;

            } while (bit != 0 && ((bit & occupancyAsLong) == 0));

            bit = 1ul << square;
            bit2 = bit;

            do
            {
                bit >>= 8 + 1;
                bit2 >>= 1;

                if ((bit2 & rowBits) != 0)
                    attackableSquares |= bit;
                else
                    break;

            } while (bit != 0 && ((bit & occupancyAsLong) == 0));

            return (SquareFlag)attackableSquares;
        }
    }
}
