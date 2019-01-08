using ChessSharp.Enums;

namespace ChessSharp.Extensions
{
    public static class StateFlagExtensions
    {
        private static readonly uint EnPassantIndexMask = 0b00000000_00000000_01111111_00000000;

        public static SquareFlag GetEnPassantSquare(this StateFlag value)
        {
            // Empty will be negative
            var enPassantIndex = value.GetEnPassantIndex();

            return enPassantIndex < 0 ? 0 : (SquareFlag)(1ul << enPassantIndex);
        }

        public static StateFlag AddEnPassantSquare(this StateFlag value, SquareFlag enPassantSquare) =>
            value.AddEnPassantIndex(enPassantSquare.ToSquareIndex());

        // Get 'next' state by copying and removing en passant data
        public static StateFlag Next(this StateFlag value) =>
            (StateFlag)((uint)value & ~EnPassantIndexMask);

        private static int GetEnPassantIndex(this StateFlag value) =>
            (int)(((uint)value & EnPassantIndexMask) >> 8) - 1;

        private static StateFlag AddEnPassantIndex(this StateFlag value, int enPassantIndex)
        {
            if (enPassantIndex < 0 || enPassantIndex > 63)
                return value;

            // Internally we store as 1 based - otherwise we can't distinguished empty from A1
            value |= (StateFlag)((enPassantIndex + 1) << 8);

            return value;
        }
    }
}
