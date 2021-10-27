using ChessSharp.Common.Enums;

namespace ChessSharp.MoveGeneration
{
    internal class MagicIndexHelpers
    {
        public static int GetMagicIndex(PieceType pieceType, int squareIndex, SquareFlag occupancy)
        {
            if (occupancy == 0)
                return 0;

            return pieceType == PieceType.Rook
                    ? GetRookMagicIndex(squareIndex, occupancy)
                    : GetBishopMagicIndex(squareIndex, occupancy);
        }

        public static int GetRookMagicIndex(int square, SquareFlag occupancy)
        {
            var index = ((ulong)occupancy * MagicNumbers.RookMagicNumbers[square]) >> 50;

            return (int)index;
        }

        public static int GetBishopMagicIndex(int square, SquareFlag occupancy)
        {
            var index = ((ulong)occupancy * MagicNumbers.BishopMagicNumbers[square]) >> 50;

            return (int)index;
        }
    }
}
