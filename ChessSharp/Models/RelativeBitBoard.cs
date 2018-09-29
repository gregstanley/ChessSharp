using ChessSharp.Enums;
using ChessSharp.Extensions;
using System;

namespace ChessSharp.Models
{
    public class RelativeBitBoard
    {
        public RelativeBitBoard(Colour colour, SquareFlag myPawns, SquareFlag myRooks, 
            SquareFlag myKnights, SquareFlag myBishops, SquareFlag myQueens, SquareFlag myKing, 
            SquareFlag opponentPawns, SquareFlag opponentRooks, SquareFlag opponentKnights, 
            SquareFlag opponentBishops, SquareFlag opponentQueens, SquareFlag opponentKing, 
            SquareFlag enPassant)
        {
            Colour = colour;
            MyPawns = myPawns;
            MyRooks = myRooks;
            MyKnights = myKnights;
            MyBishops = myBishops;
            MyQueens = myQueens;
            MyKing = myKing;
            OpponentPawns = opponentPawns;
            OpponentRooks = opponentRooks;
            OpponentKnights = opponentKnights;
            OpponentBishops = opponentBishops;
            OpponentQueens = opponentQueens;
            OpponentKing = opponentKing;
            EnPassant = enPassant;
        }

        public Colour Colour { get; }

        public SquareFlag MyPawns { get; }

        public SquareFlag MyRooks { get; }

        public SquareFlag MyKnights { get; }

        public SquareFlag MyBishops { get; }

        public SquareFlag MyQueens { get; }

        public SquareFlag MyKing { get; }

        public SquareFlag OpponentPawns { get; }

        public SquareFlag OpponentRooks { get; }

        public SquareFlag OpponentKnights { get; }

        public SquareFlag OpponentBishops { get; }

        public SquareFlag OpponentQueens { get; }

        public SquareFlag OpponentKing { get; }

        public SquareFlag EnPassant { get; }

        public SquareFlag MySquares =>
            MyPawns | MyKnights | MyRaySquares | MyKing;

        public SquareFlag OpponentSquares =>
            OpponentPawns | OpponentKnights | OpponentRaySquares | OpponentKing;

        public SquareFlag MyRaySquares =>
            MyRooks | MyBishops | MyQueens;

        public SquareFlag MyDiagonalRaySquares =>
            MyBishops | MyQueens;

        public SquareFlag MyNonDiagonalRaySquares =>
            MyRooks | MyQueens;

        public SquareFlag OpponentRaySquares =>
            OpponentRooks | OpponentBishops | OpponentQueens;

        public SquareFlag OpponentDiagonalRaySquares =>
            OpponentBishops | OpponentQueens;

        public SquareFlag OpponentNonDiagonalRaySquares =>
            OpponentRooks | OpponentQueens;

        public SquareFlag OccupiedSquares =>
            MySquares | OpponentSquares;

        public Colour GetPieceColour(SquareFlag square)
        {
            if (MySquares.HasFlag(square))
                return Colour;

            if (OpponentSquares.HasFlag(square))
                return Colour.Opposite();

            return Colour.None;
        }

        public PieceType GetPieceType(SquareFlag square)
        {
            var colour = GetPieceColour(square);

            if (colour == Colour.None)
                return PieceType.None;

            if (colour == Colour.White)
            {
                if (MyPawns.HasFlag(square)) return PieceType.Pawn;
                if (MyRooks.HasFlag(square)) return PieceType.Rook;
                if (MyKnights.HasFlag(square)) return PieceType.Knight;
                if (MyBishops.HasFlag(square)) return PieceType.Bishop;
                if (MyQueens.HasFlag(square)) return PieceType.Queen;
                if (MyKing.HasFlag(square)) return PieceType.King;
            }
            else
            {
                if (OpponentPawns.HasFlag(square)) return PieceType.Pawn;
                if (OpponentRooks.HasFlag(square)) return PieceType.Rook;
                if (OpponentKnights.HasFlag(square)) return PieceType.Knight;
                if (OpponentBishops.HasFlag(square)) return PieceType.Bishop;
                if (OpponentQueens.HasFlag(square)) return PieceType.Queen;
                if (OpponentKing.HasFlag(square)) return PieceType.King;
            }

            throw new Exception($"Failed to find piece for {square}");
        }
    }
}
