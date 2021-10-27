using ChessSharp.Common.Enums;
using ChessSharp.Common.Models;

namespace ChessSharp.Common
{
    public static class MoveBuilder
    {
        public static uint Create(
            Colour colour,
            PieceType pieceType,
            Square fromSquare,
            Square toSquare,
            PieceType capturePieceType,
            MoveType moveType)
        {
            return Create(colour, pieceType, fromSquare, toSquare, capturePieceType, moveType, 0);
        }

        public static uint Create(
            Colour colour,
            PieceType pieceType,
            Square fromSquare,
            Square toSquare,
            PieceType capturePieceType,
            MoveType moveType,
            byte numCheckers)
        {
            var colourShift = colour == Colour.White ? (uint)0 : 1;
            var pieceTypeShift = (uint)pieceType << 1;
            var fromShift = fromSquare.Index != 0 ? (uint)fromSquare.Index << 4 : 0;
            var toShift = toSquare.Index != 0 ? (uint)toSquare.Index << 10 : 0;
            var capturePieceTypeShift = (uint)capturePieceType << 16;
            var moveTypeShift = (uint)moveType << 19;
            var numCheckersShift = (uint)numCheckers << 22;

            return colourShift | pieceTypeShift | fromShift | toShift
                | capturePieceTypeShift | moveTypeShift | numCheckersShift;
        }

        public static uint CreateCastle(Colour colour, MoveType moveType)
        {
            var colourShift = colour == Colour.White ? (uint)0 : 1;

            var moveTypeShift = (uint)moveType << 19;

            return colourShift | moveTypeShift;
        }
    }
}
