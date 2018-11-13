using ChessSharp.Enums;
using ChessSharp.Models;

namespace ChessSharp
{
    public static class MoveBuilder
    {
        public static uint Create(Colour colour, PieceType pieceType,
            Square fromSquare, Square toSquare, PieceType capturePieceType, MoveType moveType)
        {
            var colourShift = colour == Colour.White ? (uint)0 : 1;
            var pieceTypeShift = (uint)pieceType << 1;
            var fromShift = fromSquare.Index != 0 ? (uint)fromSquare.Index << 4 : 0;
            var toShift = toSquare.Index != 0 ? (uint)toSquare.Index << 10 : 0;
            var capturePieceTypeShift = (uint)capturePieceType << 16;
            var moveTypeShift = (uint)moveType << 19;

            return colourShift | pieceTypeShift | fromShift | toShift
                | capturePieceTypeShift | moveTypeShift;
        }

        public static uint CreateCastle(Colour colour, MoveType moveType)
        {
            var colourShift = colour == Colour.White ? (uint)0 : 1;

            var moveTypeShift = (uint)moveType << 19;

            return colourShift | moveTypeShift;
        }
    }
}
