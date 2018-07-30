using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess
{
    public class MoveFinder
    {
        private static PawnPromotionRanks _pawnPromotionRanks { get; } = new PawnPromotionRanks(8, 1);

        public bool CheckForPawnPromotion(Square square, RankFile endPosition)
        {
            if (square.Piece?.Type != PieceType.Pawn)
                return false;

            if (square.Piece.Colour == Colour.White && endPosition.Rank == _pawnPromotionRanks.White)
                return true;

            if (endPosition.Rank == _pawnPromotionRanks.Black)
                return true;

            return false;
        }

        public Move CanCastle(IList<Square> squares, Square square)
        {
            if (square.Piece?.HasMoved == true)
                return null;

            // The King must not have moved AND must not be in check
            var kingSquare = squares.SingleOrDefault(x => x.Piece?.Colour == square.Piece.Colour
                && x.Piece?.Type == PieceType.King
                && x.Piece?.HasMoved == false);
            //&& x.CoveredBy.Any(y => y.Colour != x.Piece?.Colour.Opposite()));

            // Not that the King doesn't exist, rather it has moved
            if (kingSquare == null)
                return null;

            if (kingSquare.CoveredBy.Any(x => x.Colour != kingSquare.Piece.Colour))
                return null;

            var indexOfKing = squares.IndexOf(kingSquare);
            var indexOfRook = squares.IndexOf(square);

            var side = Math.Abs(indexOfKing - indexOfRook) == 3 ? PieceType.King : PieceType.Queen;

            var step = indexOfRook < indexOfKing ? 1 : -1;

            var i = indexOfRook + step;

            var stillCan = true;

            while (i != indexOfKing && stillCan)
            {
                var passThroughSquare = squares[i];

                if (passThroughSquare.Piece != null)
                {
                    stillCan = false;
                }
                else if (passThroughSquare.CoveredBy.Any(x => x.Colour != kingSquare.Piece.Colour))
                {
                    // This is the square next to the Rook and CAN be under attack as the King does not pass through
                    if (side == PieceType.Queen && i != indexOfRook + step)
                        stillCan = false;
                }

                i += step;
            }

            if (!stillCan)
                return null;

            var targetKingSquare = RankFile.Get(square.Rank, kingSquare.File + (step * -2));
            var targetRookSquare = RankFile.Get(square.Rank, targetKingSquare.File + step);

            return new MoveCastle(square.Piece.Colour,
                square.Piece.Type, 
                RankFile.Get(square.Rank, square.File),
                targetRookSquare,
                RankFile.Get(kingSquare.Rank, kingSquare.File),
                targetKingSquare,
                side);
        }

        public IList<Move> GetMoves(IList<Square> squares, Square startSquare, bool withCastles = true)
        {
            var possibleSquares = GetStandardCoveredSquares(squares, startSquare);
            var moves = new List<Move>();

            if (startSquare.Piece.Type == PieceType.Pawn)
            {
                var pawnSquares = PawnMoves(squares, startSquare);

                var promotionRank = startSquare.Piece.Colour == Colour.White ? _pawnPromotionRanks.White : _pawnPromotionRanks.Black;

                var promotionSquares = pawnSquares.Where(x => x.Rank == promotionRank);

                if (promotionSquares.Any())
                {
                    foreach (var promotionSquare in promotionSquares)
                    {
                        moves.Add(new Move(startSquare.Piece.Colour,
                            startSquare.Piece.Type,
                            RankFile.Get(startSquare.Rank, startSquare.File),
                            RankFile.Get(promotionSquare.Rank, promotionSquare.File),
                            PieceType.Queen));

                        moves.Add(new Move(startSquare.Piece.Colour,
                            startSquare.Piece.Type,
                            RankFile.Get(startSquare.Rank, startSquare.File),
                            RankFile.Get(promotionSquare.Rank, promotionSquare.File),
                            PieceType.Rook));

                        moves.Add(new Move(startSquare.Piece.Colour,
                            startSquare.Piece.Type,
                            RankFile.Get(startSquare.Rank, startSquare.File),
                            RankFile.Get(promotionSquare.Rank, promotionSquare.File),
                            PieceType.Bishop));

                        moves.Add(new Move(startSquare.Piece.Colour,
                            startSquare.Piece.Type,
                            RankFile.Get(startSquare.Rank, startSquare.File),
                            RankFile.Get(promotionSquare.Rank, promotionSquare.File),
                            PieceType.Knight));
                    }
                }
                else
                {
                    var nonPromotionSquares = pawnSquares.Where(x => x.Rank != promotionRank);
                    possibleSquares.AddRange(nonPromotionSquares);
                }
            }

            if (startSquare.Piece.Type == PieceType.Rook && withCastles)
            {
                var castle = CanCastle(squares, startSquare);

                if (castle != null)
                    moves.Add(castle);
            }

            foreach (var possibleSquare in possibleSquares)
                moves.Add(new Move(startSquare.Piece.Colour, startSquare.Piece.Type, RankFile.Get(startSquare.Rank, startSquare.File), RankFile.Get(possibleSquare.Rank, possibleSquare.File)));

            return moves;
        }
        
        private List<Square> GetStandardCoveredSquares(IList<Square> squares, Square startSquare)
        {
            var outSquares = new List<Square>();

            var indexOfSquare = squares.IndexOf(startSquare);

            if (startSquare.Piece.Type == PieceType.Rook)
            {
                var horizontals = AvailableHorizontalSquares(squares, startSquare);

                outSquares.AddRange(horizontals);
            }
            else if (startSquare.Piece.Type == PieceType.Knight)
            {
                var knights = AvailableKnightSquares(squares, startSquare);

                outSquares.AddRange(knights);
            }
            else if (startSquare.Piece.Type == PieceType.Bishop)
            {
                var diagonals = AvailableDiagonalSquares(squares, startSquare);

                outSquares.AddRange(diagonals);
            }
            else if (startSquare.Piece.Type == PieceType.Queen)
            {
                var horizontals = AvailableHorizontalSquares(squares, startSquare);
                var diagonals = AvailableDiagonalSquares(squares, startSquare);

                outSquares.AddRange(horizontals);
                outSquares.AddRange(diagonals);
            }
            else if (startSquare.Piece.Type == PieceType.King)
            {
                var horizontals = AvailableHorizontalSquares(squares, startSquare, 1);
                var diagonals = AvailableDiagonalSquares(squares, startSquare, 1);

                outSquares.AddRange(horizontals);
                outSquares.AddRange(diagonals);
            }

            return outSquares;
        }

        public IList<Square> GetCoveredSquares(IList<Square> squares, Square startSquare)
        {
            var outSquares = GetStandardCoveredSquares(squares, startSquare);

            var indexOfSquare = squares.IndexOf(startSquare);

            if (startSquare.Piece.Type == PieceType.Pawn)
                outSquares.AddRange(PawnCovered(squares, startSquare));

            return outSquares;
        }

        private List<Square> PawnMoves(IList<Square> squares, Square square)
        {
            var outSquares = new List<Square>();
            var indexOfSquare = squares.IndexOf(square);

            var stride = square.Piece.Colour == Colour.White ? 8 : -8;
            var captureStride1 = stride + 1;
            var captureStride2 = stride - 1;

            var availableIndex = indexOfSquare + stride;
            var availableCaptureIndex1 = indexOfSquare + captureStride1;
            var availableCaptureIndex2 = indexOfSquare + captureStride2;

            if (availableIndex >= 0 && availableIndex < squares.Count())
            {
                var targetSquare = squares[availableIndex];

                if (targetSquare.Piece == null)
                {
                    outSquares.Add(squares.ElementAt(availableIndex));

                    if (square.Rank == square.Piece.StartPosition.Rank && square.File == square.Piece.StartPosition.File)
                    {
                        // This must be 'safe' as it is on starting position
                        var targetSquare2 = squares[availableIndex + stride];

                        if (targetSquare2.Piece == null)
                            outSquares.Add(squares.ElementAt(availableIndex + stride));
                    }
                }
            }

            if (availableCaptureIndex1 >= 0 && availableCaptureIndex1 < squares.Count())
            {
                var targetSquare = squares[availableCaptureIndex1];

                if (targetSquare.Piece != null && targetSquare.Piece.Colour != square.Piece.Colour && !HasWrapped(captureStride1, square.To(targetSquare)))
                    outSquares.Add(targetSquare);
            }

            if (availableCaptureIndex2 >= 0 && availableCaptureIndex2 < squares.Count())
            {
                var targetSquare = squares[availableCaptureIndex2];

                if (targetSquare.Piece != null && targetSquare.Piece.Colour != square.Piece.Colour && !HasWrapped(captureStride2, square.To(targetSquare)))
                    outSquares.Add(targetSquare);
            }

            return outSquares;
        }

        private List<Square> PawnCovered(IList<Square> squares, Square square)
        {
            var outSquares = new List<Square>();
            var indexOfSquare = squares.IndexOf(square);

            var stride = square.Piece.Colour == Colour.White ? 8 : -8;
            var captureStride1 = stride + 1;
            var captureStride2 = stride - 1;
            var availableCaptureIndex1 = indexOfSquare + captureStride1;
            var availableCaptureIndex2 = indexOfSquare + captureStride2;

            if (availableCaptureIndex1 >= 0 && availableCaptureIndex1 < squares.Count())
            {
                var targetSquare = squares[availableCaptureIndex1];

                if (!HasWrapped(captureStride1, square.To(targetSquare)))
                    outSquares.Add(targetSquare);
            }

            if (availableCaptureIndex2 >= 0 && availableCaptureIndex2 < squares.Count())
            {
                var targetSquare = squares[availableCaptureIndex2];

                if (!HasWrapped(captureStride2, square.To(targetSquare)))
                    outSquares.Add(targetSquare);
            }

            return outSquares;
        }

        private IList<Square> AvailableKnightSquares(IList<Square> squares, Square square)
        {
            var strides = new[] { -17, -15, -10, -6, 6, 10, 15, 17 };

            var outSquares = new List<Square>();

            foreach (var stride in strides)
            {
                var directionSquares = CheckKnight(squares, square, stride);

                outSquares.AddRange(directionSquares);
            }

            return outSquares;
        }

        private IList<Square> AvailableHorizontalSquares(IList<Square> squares, Square square, int maxSteps = int.MaxValue)
        {
            var strides = new[] { -8, -1, 1, 8 };

            return CheckAllDirections(squares, square, strides, maxSteps);
        }

        private IList<Square> AvailableDiagonalSquares(IList<Square> squares, Square square, int maxSteps = int.MaxValue)
        {
            var strides = new[] { -9, -7, 7, 9 };

            return CheckAllDirections(squares, square, strides, maxSteps);
        }

        private IList<Square> CheckAllDirections(IList<Square> squares, Square square, int[] strides, int maxSteps)
        {
            var outSquares = new List<Square>();

            foreach (var stride in strides)
            {
                var directionSquares = CheckDirection(squares, square, stride, maxSteps);

                outSquares.AddRange(directionSquares);
            }

            return outSquares;
        }

        private IList<Square> CheckKnight(IList<Square> squares, Square square, int stride)
        {
            var outSquares = new List<Square>();
            var indexOfSquare = squares.IndexOf(square);

            var currentIndex = indexOfSquare;

            while (OnTheBoard(currentIndex, stride, squares.Count))
            {
                var targetSquare = squares[currentIndex + stride];

                var relativePosition = square.To(targetSquare);

                if (HasWrapped(stride, relativePosition))
                    return outSquares;

                if (targetSquare.Piece != null)
                {
                    if (targetSquare.Piece.Colour == square.Piece.Colour)
                        return outSquares;

                    outSquares.Add(targetSquare);

                    return outSquares;
                }

                outSquares.Add(targetSquare);

                currentIndex = int.MaxValue;
            }

            return outSquares;
        }

        private IList<Square> CheckDirection(IList<Square> squares, Square square, int stride, int maxSteps)
        {
            var outSquares = new List<Square>();
            var indexOfSquare = squares.IndexOf(square);

            var currentStep = 1;
            var currentIndex = indexOfSquare;

            while (OnTheBoard(currentIndex, stride, squares.Count) && currentStep <= maxSteps)
            {
                var targetSquare = squares[currentIndex + stride];

                var relativePosition = square.To(targetSquare);

                if (HasWrapped(stride, relativePosition))
                    return outSquares;

                if (targetSquare.Piece != null)
                {
                    if (targetSquare.Piece.Colour == square.Piece.Colour)
                        return outSquares;

                    outSquares.Add(targetSquare);

                    return outSquares;
                }

                outSquares.Add(targetSquare);

                ++currentStep;
                currentIndex += stride;
            }

            return outSquares;
        }

        private bool OnTheBoard(int currentIndex, int increment, int max)
        {
            if (currentIndex + increment < 0)
                return false;

            if (currentIndex + increment >= max)
                return false;

            return true;
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
