using System;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Models;

namespace ChessSharp.Common.Helpers
{
    public class PieceMapHelpers
    {
        public static Piece GetPiece(IPieceMap board, SquareFlag square)
        {
            var colour = GetPieceColour(board, square);

            if (colour == Colour.None)
                return new Piece(colour, PieceType.None);

            if (colour == Colour.White)
            {
                if (board.WhitePawns.HasFlag(square)) return new Piece(colour, PieceType.Pawn);
                if (board.WhiteRooks.HasFlag(square)) return new Piece(colour, PieceType.Rook);
                if (board.WhiteKnights.HasFlag(square)) return new Piece(colour, PieceType.Knight);
                if (board.WhiteBishops.HasFlag(square)) return new Piece(colour, PieceType.Bishop);
                if (board.WhiteQueens.HasFlag(square)) return new Piece(colour, PieceType.Queen);
                if (board.WhiteKing.HasFlag(square)) return new Piece(colour, PieceType.King);
            } else
            {
                if (board.BlackPawns.HasFlag(square)) return new Piece(colour, PieceType.Pawn);
                if (board.BlackRooks.HasFlag(square)) return new Piece(colour, PieceType.Rook);
                if (board.BlackKnights.HasFlag(square)) return new Piece(colour, PieceType.Knight);
                if (board.BlackBishops.HasFlag(square)) return new Piece(colour, PieceType.Bishop);
                if (board.BlackQueens.HasFlag(square)) return new Piece(colour, PieceType.Queen);
                if (board.BlackKing.HasFlag(square)) return new Piece(colour, PieceType.King);
            }

            throw new Exception($"Failed to find piece for {square}");
        }

        private static Colour GetPieceColour(IPieceMap board, SquareFlag square)
        {
            if (board.White.HasFlag(square))
                return Colour.White;

            if (board.Black.HasFlag(square))
                return Colour.Black;

            return Colour.None;
        }
    }
}
