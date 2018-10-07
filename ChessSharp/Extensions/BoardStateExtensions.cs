using ChessSharp.Enums;

namespace ChessSharp.Extensions
{
    public static class BoardStateExtensions
    {
        public static SquareFlag GetEnPassantSquare(this BoardState value)
        {
            // Empty will be negative
            var enPassantIndex = value.GetEnPassantIndex();

            return enPassantIndex < 0 ? 0 : (SquareFlag)(1ul << enPassantIndex);
        }

        public static BoardState AddEnPassantSquare(this BoardState value, SquareFlag enPassantSquare) =>
            value.AddEnPassantIndex(enPassantSquare.ToSquareIndex());

        private static int GetEnPassantIndex(this BoardState value) =>
            (int)(((uint)value & _enPassantIndexMask) >> 8) - 1;

        private static BoardState AddEnPassantIndex(this BoardState value, int enPassantIndex)
        {
            if (enPassantIndex < 0 || enPassantIndex > 63)
                return value;

            // Internally we store as 1 based - otherwise we can't distinguished empty from A1
            value |= (BoardState)((enPassantIndex + 1) << 8);

            return value;
        }

        private static readonly uint _enPassantIndexMask = 0b00000000_00000000_01111111_00000000;
    }
}
