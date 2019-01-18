using ChessSharp.Enums;
using ChessSharp.Extensions;
using System;

namespace ChessSharp.Models
{
    public class RelativeBitBoard
    {    
        public RelativeBitBoard()
        {
        }

        public RelativeBitBoard(
            Colour colour,
            SquareFlag myPawns,
            SquareFlag myRooks, 
            SquareFlag myKnights,
            SquareFlag myBishops,
            SquareFlag myQueens,
            SquareFlag myKing, 
            SquareFlag opponentPawns,
            SquareFlag opponentRooks,
            SquareFlag opponentKnights, 
            SquareFlag opponentBishops,
            SquareFlag opponentQueens,
            SquareFlag opponentKing, 
            StateFlag boardState)
        {
            Colour = colour;
            OpponentColour = colour.Opposite();
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
            _boardState = boardState;
        }

        public Colour Colour { get; private set; }

        public Colour OpponentColour { get; private set; }

        public SquareFlag MyPawns { get; private set; }

        public SquareFlag MyRooks { get; private set; }

        public SquareFlag MyKnights { get; private set; }

        public SquareFlag MyBishops { get; private set; }

        public SquareFlag MyQueens { get; private set; }

        public SquareFlag MyKing { get; private set; }

        public SquareFlag OpponentPawns { get; private set; }

        public SquareFlag OpponentRooks { get; private set; }

        public SquareFlag OpponentKnights { get; private set; }

        public SquareFlag OpponentBishops { get; private set; }

        public SquareFlag OpponentQueens { get; private set; }

        public SquareFlag OpponentKing { get; private set; }

        public SquareFlag EnPassant { get; private set; }

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

        public SquareFlag StartRank =>
            Colour == Colour.White ? SquareFlagConstants.R2 : SquareFlagConstants.R7;

        public SquareFlag SecondRank =>
            Colour == Colour.White ? SquareFlagConstants.R3 : SquareFlagConstants.R6;

        public SquareFlag EnPassantDiscoveredCheckRank =>
            Colour == Colour.White ? SquareFlagConstants.R5 : SquareFlagConstants.R4;

        public SquareFlag PrePromotionRank =>
            Colour == Colour.White ? SquareFlagConstants.R7 : SquareFlagConstants.R2;

        public SquareFlag PromotionRank =>
            Colour == Colour.White ? SquareFlagConstants.R8 : SquareFlagConstants.R1;

        public SquareFlag KingStartSquare =>
            Colour == Colour.White
            ? SquareFlagConstants.WhiteKingStartSquare
            : SquareFlagConstants.BlackKingStartSquare;

        public SquareFlag KingSideRookStartSquare =>
            Colour == Colour.White
            ? SquareFlagConstants.WhiteKingSideRookStartSquare
            : SquareFlagConstants.BlackKingSideRookStartSquare;

        public SquareFlag QueenSideRookStartSquare =>
            Colour == Colour.White
            ? SquareFlagConstants.WhiteQueenSideRookStartSquare
            : SquareFlagConstants.BlackQueenSideRookStartSquare;

        public SquareFlag QueenSideRookStep1Square =>
            Colour == Colour.White
            ? SquareFlagConstants.WhiteQueenSideRookStep1Square
            : SquareFlagConstants.BlackQueenSideRookStep1Square;

        public SquareFlag KingSideCastleStep1 =>
            Colour == Colour.White
            ? SquareFlagConstants.WhiteKingSideCastleStep1
            : SquareFlagConstants.BlackKingSideCastleStep1;

        public SquareFlag KingSideCastleStep2 =>
            Colour == Colour.White
            ? SquareFlagConstants.WhiteKingSideCastleStep2
            : SquareFlagConstants.BlackKingSideCastleStep2;

        public SquareFlag QueenSideCastleStep1 =>
            Colour == Colour.White
            ? SquareFlagConstants.WhiteQueenSideCastleStep1
            : SquareFlagConstants.BlackQueenSideCastleStep1;

        public SquareFlag QueenSideCastleStep2 =>
            Colour == Colour.White
            ? SquareFlagConstants.WhiteQueenSideCastleStep2
            : SquareFlagConstants.BlackQueenSideCastleStep2;

        public bool CanCastleKingSide =>
            Colour == Colour.White
            ? _boardState.HasFlag(StateFlag.WhiteCanCastleKingSide)
            : _boardState.HasFlag(StateFlag.BlackCanCastleKingSide);

        public bool CanCastleQueenSide =>
            Colour == Colour.White
            ? _boardState.HasFlag(StateFlag.WhiteCanCastleQueenSide)
            : _boardState.HasFlag(StateFlag.BlackCanCastleQueenSide);

        private StateFlag _boardState { get; set; }

        public void Set(
            Colour colour,
            SquareFlag myPawns,
            SquareFlag myRooks,
            SquareFlag myKnights,
            SquareFlag myBishops,
            SquareFlag myQueens,
            SquareFlag myKing,
            SquareFlag opponentPawns,
            SquareFlag opponentRooks,
            SquareFlag opponentKnights,
            SquareFlag opponentBishops,
            SquareFlag opponentQueens,
            SquareFlag opponentKing,
            StateFlag boardState,
            SquareFlag enPassantSquare)
        {
            Colour = colour;
            OpponentColour = colour.Opposite();
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
            _boardState = boardState;
            EnPassant = enPassantSquare;
        }

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

            if (Colour == colour)
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
