using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Bit
{
    public class BitBoardMoveFinder
    {
        private static SquareFlag WhitePromotionRank = SquareFlagExtensions.r8;
        private static SquareFlag BlackPromotionRank = SquareFlagExtensions.r1;

        public IList<Move> FindMoves(BitBoard board, Colour colour, bool withCastles = true)
        {
            var squares = board.FindPieceSquares(colour).ToList();

            var moves = new List<Move>();

            foreach (var square in squares)
            {
                var pieceMoves = FindMoves(board, colour, square, withCastles);

                if (pieceMoves.Any())
                {
                    //boardsWhereKingIsInCheck.AddRange(boardsFromSquare.Where(x => x.IsInCheck(colour)));
                    //childBoards.AddRange(boardsFromSquare.Where(x => !x.IsInCheck(colour)));

                    moves.AddRange(pieceMoves);
                }
            };

            return moves;
        }

        private IList<Move> FindMoves(BitBoard board, Colour colour, SquareFlag square, bool withCastles = true)
        {
            var possibleSquares = GetStandardCoveredSquares(board, colour, square);
            var moves = new List<Move>();

            var pieceType = board.GetPiece(square);
            var startRankFile = square.ToRankFile();

            if (pieceType == PieceType.Pawn)
            {
                var pawnSquares = PawnMoves(board, colour, square);

                var promotionRank = board.GetPieceColour(square) == Colour.White ? WhitePromotionRank : BlackPromotionRank;

                var promotionSquares = pawnSquares.Where(x => promotionRank.HasFlag(x));

                if (promotionSquares.Any())
                {
                    foreach (var promotionSquare in promotionSquares)
                    {
                        var rankFile = promotionSquare.ToRankFile();

                        moves.Add(new Move(colour,
                            pieceType,
                            RankFile.Get(startRankFile.Rank, startRankFile.File),
                            RankFile.Get(rankFile.Rank, rankFile.File),
                            PieceType.Queen));

                        moves.Add(new Move(colour,
                            pieceType,
                            RankFile.Get(startRankFile.Rank, startRankFile.File),
                            RankFile.Get(rankFile.Rank, rankFile.File),
                            PieceType.Rook));

                        moves.Add(new Move(colour,
                            pieceType,
                            RankFile.Get(startRankFile.Rank, startRankFile.File),
                            RankFile.Get(rankFile.Rank, rankFile.File),
                            PieceType.Bishop));

                        moves.Add(new Move(colour,
                            pieceType,
                            RankFile.Get(startRankFile.Rank, startRankFile.File),
                            RankFile.Get(rankFile.Rank, rankFile.File),
                            PieceType.Knight));
                    }
                }
                else
                {
                    var nonPromotionSquares = pawnSquares.Where(x => !promotionRank.HasFlag(x));
                    possibleSquares.AddRange(nonPromotionSquares);
                }
            }

            if (pieceType == PieceType.Rook && withCastles)
            {
                var castle = CanCastle(board, colour, square);

                if (castle != null)
                    moves.Add(castle);
            }

            foreach (var possibleSquare in possibleSquares)
            {
                var rankFile = possibleSquare.ToRankFile();
                moves.Add(new Move(colour, pieceType, RankFile.Get(startRankFile.Rank, startRankFile.File), RankFile.Get(rankFile.Rank, rankFile.File)));
            }

            return moves;
        }

        private List<SquareFlag> PawnMoves(BitBoard board, Colour colour, SquareFlag square)
        {
            var outSquares = new List<SquareFlag>();

            var stride = board.GetPieceColour(square) == Colour.White ? 8 : -8;
            var captureStride1 = stride + 1;
            var captureStride2 = stride - 1;
            var availableSquare1 = Next(square, stride);
            var availableCaptureSquare1 = Next(square, captureStride1);
            var availableCaptureSquare2 = Next(square, captureStride2);

            if (availableSquare1 != 0)
            {
                if (board.GetPiece(availableSquare1) == PieceType.None)
                {
                    outSquares.Add(availableSquare1);

                    var rankFile = square.ToRankFile();

                    // Has this pawn moved?
                    if (colour == Colour.White && SquareFlagExtensions.r2.HasFlag(square) || colour == Colour.Black && SquareFlagExtensions.r7.HasFlag(square))
                    {
                        // This must be 'safe' as it is on starting position
                        var availableSquare2 = Next(availableSquare1, stride);

                        if (board.GetPiece(availableSquare2) == PieceType.None)
                            outSquares.Add(availableSquare2);
                    }
                }
            }

            if (availableCaptureSquare1 != 0)
            {
                if (board.GetPiece(availableCaptureSquare1) != PieceType.None
                    && board.GetPieceColour(availableCaptureSquare1) != colour
                    && !HasWrapped(captureStride1, To(square, availableCaptureSquare1)))
                    outSquares.Add(availableCaptureSquare1);
            }

            if (availableCaptureSquare2 != 0)
            {
                if (board.GetPiece(availableCaptureSquare2) != PieceType.None
                    && board.GetPieceColour(availableCaptureSquare2) != colour
                    && !HasWrapped(captureStride2, To(square, availableCaptureSquare2)))
                    outSquares.Add(availableCaptureSquare2);
            }

            return outSquares;
        }

        public SquareFlag GetCoveredSquares(BitBoard board, Colour colour)
        {
            SquareFlag coveredSquares = 0;

            var squaresWithPieces = board.FindPieceSquares(colour).ToList();

            foreach (var square in squaresWithPieces)
                coveredSquares |= GetCoveredSquares(board, colour, square);

            return coveredSquares;
        }

        public Move CanCastle(BitBoard board, Colour colour, SquareFlag square)
        {
            if (board.GetPieceColour(square) != colour)
                return null;

            var type = board.GetPiece(square);

            if (type != PieceType.Rook)
                return null;

            var kingSquare = board.FindKingSquare(colour);

            var coveredSquares = board.FindCoveredSquares(colour.Opposite());

            if (coveredSquares.HasFlag(kingSquare))
                return null;

            var kingRankFile = kingSquare.ToRankFile();
            var rookRankFile = square.ToRankFile();

            var side = Math.Abs(kingRankFile.File - rookRankFile.File) == 3 ? PieceType.King : PieceType.Queen;

            var stride = rookRankFile.File < kingRankFile.File ? 1 : -1;

            var stillCan = true;

            var currentSquare = square;

            while (Next(currentSquare, stride) != kingSquare && stillCan)
            {
                currentSquare = Next(currentSquare, stride);

                if (board.GetPiece(currentSquare) != PieceType.None)
                {
                    stillCan = false;
                }
                else if (coveredSquares.HasFlag(currentSquare))
                {
                    // This is the square next to the Rook and CAN be under attack as the King does not pass through
                    if (side == PieceType.Queen && currentSquare != Next(square, stride))
                        stillCan = false;
                }
            }

            if (!stillCan)
                return null;

            var targetKingSquare = RankFile.Get(kingRankFile.Rank, kingRankFile.File + (stride * -2));
            var targetRookSquare = RankFile.Get(rookRankFile.Rank, targetKingSquare.File + stride);

            return new MoveCastle(colour,
                type,
                RankFile.Get(rookRankFile.Rank, rookRankFile.File),
                targetRookSquare,
                RankFile.Get(kingRankFile.Rank, kingRankFile.File),
                targetKingSquare,
                side);
        }


        private SquareFlag GetCoveredSquares(BitBoard board, Colour colour, SquareFlag square)
        {
            var outSquares = GetStandardCoveredSquares(board, colour, square);

            if (board.GetPiece(square) == PieceType.Pawn)
                outSquares.AddRange(PawnCovered(board, colour, square));

            return outSquares.ToSquareFlag();
        }

        private List<SquareFlag> GetStandardCoveredSquares(BitBoard board, Colour colour, SquareFlag square)
        {
            var outSquares = new List<SquareFlag>();

            var pieceType = board.GetPiece(square);

            if (pieceType == PieceType.Rook)
            {
                var horizontals = AvailableHorizontalSquares(board, colour, square);

                outSquares.AddRange(horizontals);
            }
            else if (pieceType == PieceType.Knight)
            {
                var knights = AvailableKnightSquares(board, colour, square);

                outSquares.AddRange(knights);
            }
            else if (pieceType == PieceType.Bishop)
            {
                var diagonals = AvailableDiagonalSquares(board, colour, square);

                outSquares.AddRange(diagonals);
            }
            else if (pieceType == PieceType.Queen)
            {
                var horizontals = AvailableHorizontalSquares(board, colour, square);
                var diagonals = AvailableDiagonalSquares(board, colour, square);

                outSquares.AddRange(horizontals);
                outSquares.AddRange(diagonals);
            }
            else if (pieceType == PieceType.King)
            {
                var horizontals = AvailableHorizontalSquares(board, colour, square, 1);
                var diagonals = AvailableDiagonalSquares(board, colour, square, 1);

                outSquares.AddRange(horizontals);
                outSquares.AddRange(diagonals);
            }

            return outSquares;
        }

        private List<SquareFlag> PawnCovered(BitBoard board, Colour colour, SquareFlag square)
        {
            var outSquares = new List<SquareFlag>();

            var stride = board.GetPieceColour(square) == Colour.White ? 8 : -8;
            var captureStride1 = stride + 1;
            var captureStride2 = stride - 1;
            var availableCaptureSquare1 = Next(square, captureStride1);
            var availableCaptureSquare2 = Next(square, captureStride2);

            if (availableCaptureSquare1 != 0)
            {
                if (!HasWrapped(captureStride1, To(square, availableCaptureSquare1)))
                    outSquares.Add(availableCaptureSquare1);
            }

            if (availableCaptureSquare2 != 0)
            {
                if (!HasWrapped(captureStride2, To(square, availableCaptureSquare2)))
                    outSquares.Add(availableCaptureSquare2);
            }

            return outSquares;
        }

        private IList<SquareFlag> AvailableKnightSquares(BitBoard board, Colour colour, SquareFlag square)
        {
            var strides = new[] { -17, -15, -10, -6, 6, 10, 15, 17 };

            var outSquares = new List<SquareFlag>();

            foreach (var stride in strides)
            {
                var directionSquares = CheckKnight(board, colour, square, stride);

                outSquares.AddRange(directionSquares);
            }

            return outSquares;
        }

        private IList<SquareFlag> AvailableHorizontalSquares(BitBoard board, Colour colour, SquareFlag square, int maxSteps = int.MaxValue)
        {
            var strides = new[] { -8, -1, 1, 8 };

            return CheckAllDirections(board, colour, square, strides, maxSteps);
        }

        private IList<SquareFlag> AvailableDiagonalSquares(BitBoard board, Colour colour, SquareFlag square, int maxSteps = int.MaxValue)
        {
            var strides = new[] { -9, -7, 7, 9 };

            return CheckAllDirections(board, colour, square, strides, maxSteps);
        }

        private IList<SquareFlag> CheckKnight(BitBoard board, Colour colour, SquareFlag square, int stride)
        {
            var outSquares = new List<SquareFlag>();
            var mySquares = colour == Colour.White ? board.White : board.Black;
            var opponentSquares = colour == Colour.White ? board.Black : board.White;
            var currentSquare = square;

            currentSquare = Next(currentSquare, stride);

            var relativePosition = To(square, currentSquare);

            if (HasWrapped(stride, relativePosition))
                return outSquares;

            if (mySquares.HasFlag(currentSquare))
                return outSquares;

            outSquares.Add(currentSquare);

            return outSquares;
        }

        private IList<SquareFlag> CheckAllDirections(BitBoard board, Colour colour, SquareFlag square, int[] strides, int maxSteps)
        {
            var outSquares = new List<SquareFlag>();

            foreach (var stride in strides)
            {
                var directionSquares = CheckDirection(board, colour, square, stride, maxSteps);

                outSquares.AddRange(directionSquares);
            }

            return outSquares;
        }

        private IList<SquareFlag> CheckDirection(BitBoard board, Colour colour, SquareFlag square, int stride, int maxSteps)
        {
            var outSquares = new List<SquareFlag>();
            var mySquares = colour == Colour.White ? board.White : board.Black;
            var opponentSquares = colour == Colour.White ? board.Black : board.White;
            var currentStep = 1;
            var currentSquare = square;
            var startRankFile = square.ToRankFile();

            while (HasNext(currentSquare, stride) && currentStep <= maxSteps)
            {
                currentSquare = Next(currentSquare, stride);

                var relativePosition = To(startRankFile, currentSquare);

                if (HasWrapped(stride, relativePosition))
                    return outSquares;

                if (mySquares.HasFlag(currentSquare))
                    return outSquares;

                if (opponentSquares.HasFlag(currentSquare))
                {
                    outSquares.Add(currentSquare);

                    return outSquares;
                }

                outSquares.Add(currentSquare);

                ++currentStep;
            }

            return outSquares;
        }

        public bool CheckForPawnPromotion(BitBoard board, SquareFlag startSquare, SquareFlag endSquare)
        {
            if (board.GetPiece(startSquare) != PieceType.Pawn)
                return false;

            if (board.GetPieceColour(startSquare) == Colour.White && WhitePromotionRank.HasFlag(endSquare))
                return true;

            if (BlackPromotionRank.HasFlag(endSquare))
                return true;

            return false;
        }

        private RelativePosition To(SquareFlag startSquare, SquareFlag targetSquare)
        {
            var startRankFile = startSquare.ToRankFile();
            return To(startRankFile, targetSquare);
        }

        private RelativePosition To(RankFile startRankFile, SquareFlag targetSquare)
        {
            var targetRankFile = targetSquare.ToRankFile();
            return startRankFile.To(targetRankFile);
        }

        private bool HasNext(SquareFlag currentSquare, int stride)
        {
            // If we are at the known first or last squares then next move will be off the board == 0
            if (currentSquare.HasFlag(SquareFlag.A1) && stride < 0)
                return false;

            if (currentSquare.HasFlag(SquareFlag.H8) && stride > 0)
                return false;

            return true;
        }

        private SquareFlag Next(SquareFlag currentSquare, int stride)
        {
            ulong nextSquare = 0;

            if (stride > 0)
                nextSquare = (ulong) currentSquare << stride;
            else
                nextSquare = (ulong) currentSquare >> Math.Abs(stride);

            return (SquareFlag) nextSquare;
        }

        private bool HasWrapped(int stride, RelativePosition relativePosition)
        {
            if (stride == -17)
            {
                if (relativePosition.Rank > 0)
                    return true;

                if (relativePosition.File > 0)
                    return true;
            }

            if (stride == -15)
            {
                if (relativePosition.Rank > 0)
                    return true;

                if (relativePosition.File < 0)
                    return true;
            }

            if (stride == -10)
            {
                if (relativePosition.Rank > 0)
                    return true;

                if (relativePosition.File > 0)
                    return true;
            }

            if (stride == -9)
            {
                if (relativePosition.Rank > 0)
                    return true;

                if (relativePosition.File > 0)
                    return true;
            }

            if (stride == -8)
            {
                if (relativePosition.Rank > 0)
                    return true;
            }

            if (stride == -7)
            {
                if (relativePosition.Rank > 0)
                    return true;

                if (relativePosition.File < 0)
                    return true;
            }

            if (stride == -6)
            {
                if (relativePosition.Rank > 0)
                    return true;

                if (relativePosition.File < 0)
                    return true;
            }

            if (stride == -1)
            {
                if (relativePosition.Rank < 0)
                    return true;

                if (relativePosition.File > 0)
                    return true;
            }

            if (stride == 1)
            {
                if (relativePosition.Rank > 0)
                    return true;

                if (relativePosition.File < 0)
                    return true;
            }

            if (stride == 6)
            {
                if (relativePosition.Rank < 0)
                    return true;

                if (relativePosition.File > 0)
                    return true;
            }

            if (stride == 7)
            {
                if (relativePosition.Rank < 0)
                    return true;

                if (relativePosition.File > 0)
                    return true;
            }

            if (stride == 8)
            {
                if (relativePosition.Rank < 0)
                    return true;
            }

            if (stride == 9)
            {
                if (relativePosition.Rank < 0)
                    return true;

                if (relativePosition.File < 0)
                    return true;
            }

            if (stride == 10)
            {
                if (relativePosition.Rank < 0)
                    return true;

                if (relativePosition.File < 0)
                    return true;
            }

            if (stride == 15)
            {
                if (relativePosition.Rank < 0)
                    return true;

                if (relativePosition.File > 0)
                    return true;
            }

            if (stride == 17)
            {
                if (relativePosition.Rank < 0)
                    return true;

                if (relativePosition.File < 0)
                    return true;
            }

            return false;
        }
    }
}
