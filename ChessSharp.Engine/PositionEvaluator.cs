using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System;

namespace ChessSharp.Engine
{
    public class PositionEvaluator
    {
        public double Evaluate(BitBoard bitBoard)
        {
            var eval = 0d;

            var occupiedSquares = (bitBoard.White | bitBoard.Black).ToList();

            var whiteScore = 0d;
            var blackScore = 0d;

            foreach (var square in bitBoard.WhitePawns.ToList())
                whiteScore += GetAbsoluteWhitePieceValue(square, PieceType.Pawn);

            foreach (var square in bitBoard.WhiteRooks.ToList())
                whiteScore += GetAbsoluteWhitePieceValue(square, PieceType.Rook);

            foreach (var square in bitBoard.WhiteKnights.ToList())
                whiteScore += GetAbsoluteWhitePieceValue(square, PieceType.Knight);

            foreach(var square in bitBoard.WhiteBishops.ToList())
                whiteScore += GetAbsoluteWhitePieceValue(square, PieceType.Bishop);

            foreach (var square in bitBoard.WhiteQueens.ToList())
                whiteScore += GetAbsoluteWhitePieceValue(square, PieceType.Queen);

            foreach (var square in bitBoard.WhiteKing.ToList())
                whiteScore += GetAbsoluteWhitePieceValue(square, PieceType.King);

            foreach (var square in bitBoard.BlackPawns.ToList())
                blackScore += GetAbsoluteBlackPieceValue(square, PieceType.Pawn);

            foreach (var square in bitBoard.BlackRooks.ToList())
                blackScore += GetAbsoluteBlackPieceValue(square, PieceType.Rook);

            foreach (var square in bitBoard.BlackKnights.ToList())
                blackScore += GetAbsoluteBlackPieceValue(square, PieceType.Knight);

            foreach (var square in bitBoard.BlackBishops.ToList())
                blackScore += GetAbsoluteBlackPieceValue(square, PieceType.Bishop);

            foreach (var square in bitBoard.BlackQueens.ToList())
                blackScore += GetAbsoluteBlackPieceValue(square, PieceType.Queen);

            foreach (var square in bitBoard.BlackKing.ToList())
                blackScore += GetAbsoluteBlackPieceValue(square, PieceType.King);

            //foreach (var square in occupiedSquares)
            //{
            //    var piece = bitBoard.GetPiece(square);

            //    var absoluteValue = GetAbsolutePieceValue(square, piece);

            //    eval += piece.Colour == Colour.White ? absoluteValue : -absoluteValue;
            //}
            eval = whiteScore - blackScore;

            return Math.Round(eval * 0.1, 2);
        }

        private double GetAbsoluteWhitePieceValue(SquareFlag square, PieceType pieceType)
        {
            var squareIndex = square.ToSquareIndex();
            var r = squareIndex / 8;
            var f = squareIndex % 8;

            switch (pieceType)
            {
                case PieceType.Pawn:
                    return 10d + PieceValues.WhitePawn[r][f];
                case PieceType.Rook:
                    return 50d + PieceValues.WhiteRook[r][f];
                case PieceType.Knight:
                    return 30d + PieceValues.Knight[r][f];
                case PieceType.Bishop:
                    return 30d + PieceValues.WhiteBishop[r][f];
                case PieceType.Queen:
                    return 90d + PieceValues.Queen[r][f];
                case PieceType.King:
                    return 900d + PieceValues.WhiteKing[r][f];
            }

            return 0d;
        }

        private double GetAbsoluteBlackPieceValue(SquareFlag square, PieceType pieceType)
        {
            var squareIndex = square.ToSquareIndex();
            var r = squareIndex / 8;
            var f = squareIndex % 8;

            switch (pieceType)
            {
                case PieceType.Pawn:
                    return 10d + PieceValues.BlackPawn[r][f];
                case PieceType.Rook:
                    return 50d + PieceValues.BlackRook[r][f];
                case PieceType.Knight:
                    return 30d + PieceValues.Knight[r][f];
                case PieceType.Bishop:
                    return 30d + PieceValues.BlackBishop[r][f];
                case PieceType.Queen:
                    return 90d + PieceValues.Queen[r][f];
                case PieceType.King:
                    return 900d + PieceValues.BlackKing[r][f];
            }

            return 0d;
        }

        private double GetAbsolutePieceValue(SquareFlag square, Piece piece)
        {
            var squareIndex = square.ToSquareIndex();
            var r = squareIndex / 8;
            var f = squareIndex % 8;

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    return 10d + (piece.Colour == Colour.White ? PieceValues.WhitePawn[r][f] : PieceValues.BlackPawn[r][f]);
                case PieceType.Rook:
                    return 50d + (piece.Colour == Colour.White ? PieceValues.WhiteRook[r][f] : PieceValues.BlackRook[r][f]);
                case PieceType.Knight:
                    return 30d + PieceValues.Knight[r][f];
                case PieceType.Bishop:
                    return 30d + (piece.Colour == Colour.White ? PieceValues.WhiteBishop[r][f] : PieceValues.BlackBishop[r][f]);
                case PieceType.Queen:
                    return 90d + PieceValues.Queen[r][f];
                case PieceType.King:
                    return 900d + (piece.Colour == Colour.White ? PieceValues.WhiteKing[r][f] : PieceValues.BlackKing[r][f]);
            }

            return 0d;
        }
    }
}
