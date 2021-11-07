using System.Numerics;
using ChessSharp.Common;
using ChessSharp.Common.Extensions;

namespace ChessSharp.Engine
{
    public class NegaMaxEvaluator : IPositionEvaluator
    {
        // https://www.chessprogramming.org/Point_Value
        public const int PawnValue = 100;
        public const int KnightValue = 350;
        public const int BishopValue = 350;
        public const int RookValue = 525;
        public const int QueenValue = 1000;
        public const int KingValue = 10000;

        public NegaMaxEvaluator()
        {
            PieceValues.Init();
        }

        public int Evaluate(Board board)
        {
            var whiteScore = 0;
            var blackScore = 0;
            ulong isBishop = 0;
            ulong isPawn;
            ulong isRook;
            ulong isKnight;

            var buffer1 = new ulong[4];

            foreach (var squareIndex in board.White.ToSquareIndexList())
            {
                var square = squareIndex.ToSquareFlagUlong();

                if (Vector<ulong>.Count == 4)
                {
                    buffer1[0] = (ulong)board.WhitePawns;
                    buffer1[1] = (ulong)board.WhiteRooks;
                    buffer1[2] = (ulong)board.WhiteKnights;
                    buffer1[3] = (ulong)board.WhiteBishops;

                    Vector<ulong> vector1 = new(buffer1);
                    Vector<ulong> vector2 = new(square);

                    var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                    isPawn = vectorOut[0];
                    isRook = vectorOut[1];
                    isKnight = vectorOut[2];
                    isKnight = vectorOut[2];
                }
                else
                {
                    isPawn = (ulong)board.WhitePawns & square;
                    isRook = (ulong)board.WhiteRooks & square;
                    isKnight = (ulong)board.WhiteKnights & square;
                    isBishop = (ulong)board.WhiteBishops & square;
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

                var isQueen = (ulong)board.WhiteQueens & square;

                if (isQueen > 0)
                {
                    whiteScore += QueenValue + PieceValues.QueenModifiers[squareIndex];
                    continue;
                }

                var isKing = (ulong)board.WhiteKing & square;

                if (isKing > 0)
                {
                    whiteScore += KingValue + PieceValues.KingModifiers[squareIndex];
                    continue;
                }
            }

            foreach (var squareIndex in board.Black.ToSquareIndexList())
            {
                var square = squareIndex.ToSquareFlagUlong();

                if (Vector<ulong>.Count == 4)
                {
                    buffer1[0] = (ulong)board.BlackPawns;
                    buffer1[1] = (ulong)board.BlackRooks;
                    buffer1[2] = (ulong)board.BlackKnights;
                    buffer1[3] = (ulong)board.BlackBishops;

                    Vector<ulong> vector1 = new(buffer1);
                    Vector<ulong> vector2 = new(square);

                    var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                    isPawn = vectorOut[0];
                    isRook = vectorOut[1];
                    isKnight = vectorOut[2];
                    isKnight = vectorOut[2];
                }
                else
                {
                    isPawn = (ulong)board.BlackPawns & square;
                    isRook = (ulong)board.BlackRooks & square;
                    isKnight = (ulong)board.BlackKnights & square;
                    isBishop = (ulong)board.BlackBishops & square;
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

                var isQueen = (ulong)board.BlackQueens & square;

                if (isQueen > 0)
                {
                    blackScore += QueenValue + PieceValues.QueenModifiers[63 - squareIndex];
                    continue;
                }

                var isKing = (ulong)board.BlackKing & square;

                if (isKing > 0)
                {
                    blackScore += KingValue + PieceValues.KingModifiers[63 - squareIndex];
                    continue;
                }
            }

            return whiteScore - blackScore;
        }
    }
}
