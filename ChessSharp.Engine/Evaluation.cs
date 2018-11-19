using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System;

namespace ChessSharp.Engine
{
    public class Evaluation
    {
        public double Evaluate(BitBoard bitBoard)
        {
            var eval = 0d;

            var occupiedSquares = (bitBoard.White | bitBoard.Black).ToList();

            foreach (var square in occupiedSquares)
            {
                var piece = bitBoard.GetPiece(square);

                var absoluteValue = GetAbsolutePieceValue(bitBoard, square, piece);

                eval += piece.Colour == Colour.White ? absoluteValue : -absoluteValue;
            }

            return Math.Round(eval * 0.1, 2);
        }

        private double GetAbsolutePieceValue(BitBoard bitBoard, SquareFlag square, Piece piece)
        {
            //var rf = square.ToRankFile();
            //var r = rf.Rank - 1;
            //var f = (int)rf.File;

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
