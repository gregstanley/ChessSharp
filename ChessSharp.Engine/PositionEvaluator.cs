using ChessSharp.Extensions;
using System;
using System.Numerics;

namespace ChessSharp.Engine
{
    public class PositionEvaluator
    {
        public const double PawnValue = 10;
        public const double KnightValue = 30;
        public const double BishopValue = 30;
        public const double RookValue = 50;
        public const double QueenValue = 90;
        public const double KingValue = 900;

        public PositionEvaluator()
        {
            PieceValues.Init();
        }

        public double Evaluate(BitBoard bitBoard)
        {
            var whiteScore = 0d;
            var blackScore = 0d;

            var isPawn = 0ul;
            var isRook = 0ul;
            var isKnight = 0ul;
            var isBishop = 0ul;

            var buffer1 = new ulong[4];
            var buffer2 = new ulong[4];

            foreach (var squareIndex in bitBoard.White.ToSquareIndexList())
            {
                var square = squareIndex.ToSquareFlagUlong();

                if (Vector<ulong>.Count == 4)
                {
                    buffer1[0] = (ulong)bitBoard.WhitePawns;
                    buffer1[1] = (ulong)bitBoard.WhiteRooks;
                    buffer1[2] = (ulong)bitBoard.WhiteKnights;
                    buffer1[3] = (ulong)bitBoard.WhiteBishops;

                    buffer2[0] = square;
                    buffer2[1] = square;
                    buffer2[2] = square;
                    buffer2[3] = square;

                    var vector1 = new Vector<ulong>(buffer1);
                    var vector2 = new Vector<ulong>(buffer2);

                    var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                    isPawn = vectorOut[0];
                    isRook = vectorOut[1];
                    isKnight = vectorOut[2];
                    isKnight = vectorOut[2];
                }
                else
                {
                    isPawn = (ulong)bitBoard.WhitePawns & square;
                    isRook = (ulong)bitBoard.WhiteRooks & square;
                    isKnight = (ulong)bitBoard.WhiteKnights & square;
                    isBishop = (ulong)bitBoard.WhiteBishops & square;
                }

                if (isPawn > 0)
                {
                    whiteScore += PawnValue + PieceValues.PawnModifiers[squareIndex];
                    continue;
                }

                if (isRook > 0)
                {
                    whiteScore += RookValue + PieceValues.RookModifiers[squareIndex];
                    continue;
                }

                if (isKnight > 0)
                {
                    whiteScore += KnightValue + PieceValues.KnightModifiers[squareIndex];
                    continue;
                }

                if (isBishop > 0)
                {
                    whiteScore += BishopValue + PieceValues.BishopModifiers[squareIndex];
                    continue;
                }

                var isQueen = (ulong)bitBoard.WhiteQueens & square;
                
                if (isQueen > 0)
                {
                    whiteScore += QueenValue + PieceValues.QueenModifiers[squareIndex];
                    continue;
                }

                var isKing = (ulong)bitBoard.WhiteKing & square;

                if (isKing > 0)
                {
                    whiteScore += KingValue + PieceValues.KingModifiers[squareIndex];
                    continue;
                }
            }

            foreach (var squareIndex in bitBoard.Black.ToSquareIndexList())
            {
                var square = squareIndex.ToSquareFlagUlong();

                if (Vector<ulong>.Count == 4)
                {
                    buffer1[0] = (ulong)bitBoard.BlackPawns;
                    buffer1[1] = (ulong)bitBoard.BlackRooks;
                    buffer1[2] = (ulong)bitBoard.BlackKnights;
                    buffer1[3] = (ulong)bitBoard.BlackBishops;

                    buffer2[0] = square;
                    buffer2[1] = square;
                    buffer2[2] = square;
                    buffer2[3] = square;

                    var vector1 = new Vector<ulong>(buffer1);
                    var vector2 = new Vector<ulong>(buffer2);

                    var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                    isPawn = vectorOut[0];
                    isRook = vectorOut[1];
                    isKnight = vectorOut[2];
                    isKnight = vectorOut[2];
                }
                else
                {
                    isPawn = (ulong)bitBoard.BlackPawns & square;
                    isRook = (ulong)bitBoard.BlackRooks & square;
                    isKnight = (ulong)bitBoard.BlackKnights & square;
                    isBishop = (ulong)bitBoard.BlackBishops & square;
                }

                if (isPawn > 0)
                {
                    blackScore += PawnValue + PieceValues.PawnModifiers[63 - squareIndex];
                    continue;
                }

                if (isRook > 0)
                {
                    blackScore += RookValue + PieceValues.RookModifiers[63 - squareIndex];
                    continue;
                }

                if (isKnight > 0)
                {
                    blackScore += KnightValue + PieceValues.KnightModifiers[63 - squareIndex];
                    continue;
                }

                if (isBishop > 0)
                {
                    blackScore += BishopValue + PieceValues.BishopModifiers[63 - squareIndex];
                    continue;
                }

                var isQueen = (ulong)bitBoard.BlackQueens & square;

                if (isQueen > 0)
                {
                    blackScore += QueenValue + PieceValues.QueenModifiers[63 - squareIndex];
                    continue;
                }

                var isKing = (ulong)bitBoard.BlackKing & square;

                if (isKing > 0)
                {
                    blackScore += KingValue + PieceValues.KingModifiers[63 - squareIndex];
                    continue;
                }
            }

            double eval = whiteScore - blackScore;

            return Math.Round(eval * 0.1, 2);
        }
    }
}
