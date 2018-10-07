﻿using ChessSharp.Enums;
using ChessSharp.Extensions;

namespace ChessSharp
{
    public static class MoveConstructor
    {
        public static uint CreateMove(Colour colour, PieceType pieceType,
            SquareFlag from, SquareFlag to, PieceType capturePieceType, MoveType moveType)
        {
            var colourShift = colour == Colour.White ? (uint)0 : 1;
            var pieceTypeShift = (uint)pieceType << 1;
            var fromShift = from != 0 ? (uint)from.ToSquareIndex() << 4 : 0;
            var toShift = to != 0 ? (uint)to.ToSquareIndex() << 10 : 0;
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
