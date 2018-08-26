﻿using Chess.Engine.Extensions;
using Chess.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Engine.Bit
{
    public class BitBoardMoveFinder
    {
        private static SquareFlag WhitePromotionRank = SquareFlagExtensions.r8;
        private static SquareFlag BlackPromotionRank = SquareFlagExtensions.r1;

        public IList<Move> FindMoves(BitBoard board, SquareFlag enPassantSquare, Colour colour, bool withCastles = true)
        {
            var squares = board.FindPieceSquares(colour).ToList();

            var moves = new List<Move>();

            foreach (var square in squares)
            {
                var pieceMoves = FindMoves(board, enPassantSquare, colour, square, withCastles);

                if (pieceMoves.Any())
                    moves.AddRange(pieceMoves);
            };

            return moves;
        }

        private IList<Move> FindMoves(BitBoard board, SquareFlag enPassantSquare, Colour colour, SquareFlag square, bool withCastles = true)
        {
            var pieceType = board.GetPiece(square);
            var possibleSquares = GetStandardCoveredSquares(board, colour, square, pieceType);
            var moves = new List<Move>();
            var startRankFile = square.ToRankFile();

            if (pieceType == PieceType.Pawn)
            {
                var pawnMoves = PawnMoves(board, enPassantSquare, colour, square);

                moves.AddRange(pawnMoves);
            }

            if (pieceType == PieceType.Rook && withCastles)
            {
                var castle = CanCastle(board, colour, square);

                if (castle != null)
                    moves.Add(castle);
            }

            foreach (var possibleSquare in possibleSquares)
            {
                var rankFile = possibleSquare.Square.ToRankFile();

                if (possibleSquare.Colour != colour)
                    moves.Add(new Move(colour, pieceType, RankFile.Get(startRankFile.Rank, startRankFile.File), RankFile.Get(rankFile.Rank, rankFile.File), possibleSquare.Type));
                else
                    moves.Add(new Move(colour, pieceType, RankFile.Get(startRankFile.Rank, startRankFile.File), RankFile.Get(rankFile.Rank, rankFile.File)));
            }

            return moves;
        }

        private List<Move> PawnMoves(BitBoard board, SquareFlag enPassantSquare, Colour colour, SquareFlag square)
        {
            var moves = new List<Move>();

            var startRankFile = square.ToRankFile();
            var stride = board.GetPieceColour(square) == Colour.White ? 8 : -8;
            var captureStride1 = stride + 1;
            var captureStride2 = stride - 1;
            var availableSquare1 = Next(square, stride);
            var availableCaptureSquare1 = Next(square, captureStride1);
            var availableCaptureSquare2 = Next(square, captureStride2);
            var promotionRank = board.GetPieceColour(square) == Colour.White ? WhitePromotionRank : BlackPromotionRank;

            if (availableSquare1 != 0)
            {
                if (board.GetPiece(availableSquare1) == PieceType.None)
                {
                    var as1rf = availableSquare1.ToRankFile();

                    moves.AddRange(CreatePawnMove(board, colour, startRankFile, availableSquare1, PieceType.None, promotionRank, 0));

                    var rankFile = square.ToRankFile();

                    // Has this pawn moved?
                    if (colour == Colour.White && SquareFlagExtensions.r2.HasFlag(square) || colour == Colour.Black && SquareFlagExtensions.r7.HasFlag(square))
                    {
                        // This must be 'safe' as it is on starting position
                        var availableSquare2 = Next(availableSquare1, stride);

                        var as2rf = availableSquare2.ToRankFile();

                        if (board.GetPiece(availableSquare2) == PieceType.None)
                        {
                            moves.Add(new Move(colour,
                                PieceType.Pawn,
                                RankFile.Get(startRankFile.Rank, startRankFile.File),
                                RankFile.Get(as2rf.Rank, as2rf.File),
                                PieceType.None,
                                availableSquare1,
                                0
                                ));
                        }
                    }
                }
            }

            if (availableCaptureSquare1 != 0)
            {
                // I don't *think* you can see you're own en pasant squares as it must be opponent turn
                var isEnpassantCapture = enPassantSquare != 0 && availableCaptureSquare1 == enPassantSquare;

                var isCapture = board.GetPiece(availableCaptureSquare1) != PieceType.None && board.GetPieceColour(availableCaptureSquare1) != colour;

                if ((isCapture || isEnpassantCapture) && !HasWrapped(captureStride1, To(square, availableCaptureSquare1)))
                {
                    var enPassantCaptureSquare = Next(availableCaptureSquare1, -stride);
                    moves.AddRange(CreatePawnMove(board, colour, startRankFile, availableCaptureSquare1, PieceType.Pawn, promotionRank, isEnpassantCapture ? enPassantCaptureSquare : 0));
                }
            }

            if (availableCaptureSquare2 != 0)
            {
                // I don't *think* you can see you're own en pasant squares as it must be opponent turn
                var isEnpassantCapture = enPassantSquare != 0 && availableCaptureSquare2 == enPassantSquare;

                var isCapture = board.GetPiece(availableCaptureSquare2) != PieceType.None && board.GetPieceColour(availableCaptureSquare2) != colour;

                if ((isCapture || isEnpassantCapture) && !HasWrapped(captureStride2, To(square, availableCaptureSquare2)))
                {
                    var enPassantCaptureSquare = Next(availableCaptureSquare2, -stride);
                    moves.AddRange(CreatePawnMove(board, colour, startRankFile, availableCaptureSquare2, PieceType.Pawn, promotionRank, isEnpassantCapture ? enPassantCaptureSquare : 0));
                }
            }

            return moves;
        }

        private List<Move> CreatePawnMove(BitBoard board, Colour colour, RankFile startRankFile, SquareFlag endSquare, PieceType capturePieceType, SquareFlag promotionRank, SquareFlag enPassantCaptureSquare)
        {
            var isPromotion = promotionRank.HasFlag(endSquare);

            var rankFile = endSquare.ToRankFile();

            if (!isPromotion)
            {
                if (enPassantCaptureSquare == 0)
                {
                    return new List<Move>
                    {
                        new Move(colour,
                            PieceType.Pawn,
                            RankFile.Get(startRankFile.Rank, startRankFile.File),
                            RankFile.Get(rankFile.Rank, rankFile.File),
                            capturePieceType
                            )
                    };
                }
                else
                {
                    return new List<Move>
                    {
                        new Move(colour,
                            PieceType.Pawn,
                            RankFile.Get(startRankFile.Rank, startRankFile.File),
                            RankFile.Get(rankFile.Rank, rankFile.File),
                            capturePieceType,
                            0,
                            enPassantCaptureSquare
                            )
                    };
                }
            }

            var moves = new List<Move>
            {
                new Move(colour,
                    PieceType.Pawn,
                    RankFile.Get(startRankFile.Rank, startRankFile.File),
                    RankFile.Get(rankFile.Rank, rankFile.File),
                    capturePieceType,
                    PieceType.Queen),

                new Move(colour,
                    PieceType.Pawn,
                    RankFile.Get(startRankFile.Rank, startRankFile.File),
                    RankFile.Get(rankFile.Rank, rankFile.File),
                    capturePieceType,
                    PieceType.Rook),

                new Move(colour,
                    PieceType.Pawn,
                    RankFile.Get(startRankFile.Rank, startRankFile.File),
                    RankFile.Get(rankFile.Rank, rankFile.File),
                    capturePieceType,
                    PieceType.Bishop),

                new Move(colour,
                    PieceType.Pawn,
                    RankFile.Get(startRankFile.Rank, startRankFile.File),
                    RankFile.Get(rankFile.Rank, rankFile.File),
                    capturePieceType,
                    PieceType.Knight)
            };

            return moves;
        }

        private List<SquareFlag> PawnMovesOrig(BitBoard board, Colour colour, SquareFlag square)
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

        private Move CanCastle(BitBoard board, Colour colour, SquareFlag square)
        {
            if (!board.CanCastle(colour))
                return null;

            if (board.GetPieceColour(square) != colour)
                return null;

            var type = board.GetPiece(square);

            if (type != PieceType.Rook)
                return null;

            var kingSquare = board.FindKingSquare(colour);

            var kingRankFile = kingSquare.ToRankFile();
            var rookRankFile = square.ToRankFile();

            if (rookRankFile.File != File.a && rookRankFile.File != File.h)
                return null;

            var side = Math.Abs(kingRankFile.File - rookRankFile.File) == 3 ? PieceType.King : PieceType.Queen;

            if (side == PieceType.King && !board.CanCastleKingSide(colour))
                return null;

            if (side == PieceType.Queen && !board.CanCastleQueenSide(colour))
                return null;

            var attackingThisSquare = FindPiecesAttackingThisSquare(board, colour, kingSquare);

            if (attackingThisSquare.Any())
                return null;

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
                else
                {
                    attackingThisSquare = FindPiecesAttackingThisSquare(board, colour, currentSquare);

                    // This is the square next to the Rook and CAN be under attack as the King does not pass through
                    if (attackingThisSquare.Any() && side == PieceType.Queen && currentSquare != Next(square, stride))
                        stillCan = false;
                }
            }

            if (!stillCan)
                return null;

            var targetKingSquare = RankFile.Get(kingRankFile.Rank, kingRankFile.File + (stride * -2));

            if (targetKingSquare == null)
                throw new Exception($"{colour} King is not in correct square");

            var targetRookSquare = RankFile.Get(rookRankFile.Rank, targetKingSquare.File + stride);

            return new MoveCastle(colour,
                type,
                RankFile.Get(rookRankFile.Rank, rookRankFile.File),
                targetRookSquare,
                RankFile.Get(kingRankFile.Rank, kingRankFile.File),
                targetKingSquare,
                side);
        }


        private List<SquareState> FindPiecesAttackingThisSquare(BitBoard board, Colour colour, SquareFlag square)
        {
            var squares = new List<SquareState>();

            // In theory this will hunt for Bishops and Rooks as wells as Queens
            var attackedByRay = GetStandardCoveredSquares(board, colour, square, PieceType.Queen)
                .Where(x => x.Colour == colour.Opposite()
                && (x.Type == PieceType.Rook || x.Type == PieceType.Bishop || x.Type == PieceType.Queen));

            if (attackedByRay.Any())
                squares.AddRange(attackedByRay);

            var attackedByPawn = AttackedByPawn(board, colour, square);

            if (attackedByPawn.Any())
                squares.AddRange(attackedByPawn);

            var attackedByKnight = AvailableKnightSquares(board, colour, square)
                .Where(x => x.Colour == colour.Opposite());

            if (attackedByKnight.Any())
                squares.AddRange(attackedByKnight);

            return squares;
        }

        private List<SquareState> GetStandardCoveredSquares(BitBoard board, Colour colour, SquareFlag square, PieceType pieceType)
        {
            var outSquares = new List<SquareState>();

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
        
        private List<SquareState> AttackedByPawn(BitBoard board, Colour colour, SquareFlag square)
        {
            var outSquares = new List<SquareState>();

            var stride = board.GetPieceColour(square) == Colour.White ? 8 : -8;
            var captureStride1 = stride + 1;
            var captureStride2 = stride - 1;
            var availableCaptureSquare1 = Next(square, captureStride1);
            var availableCaptureSquare2 = Next(square, captureStride2);

            if (availableCaptureSquare1 != 0)
            {
                var squareState = board.GetSquareState(availableCaptureSquare1);

                if (squareState.Colour == colour.Opposite()
                    && squareState.Type == PieceType.Pawn
                    && !HasWrapped(captureStride1, To(square, availableCaptureSquare1)))
                    outSquares.Add(squareState);
            }

            if (availableCaptureSquare2 != 0)
            {
                var squareState = board.GetSquareState(availableCaptureSquare2);

                if (squareState.Colour == colour.Opposite()
                    && squareState.Type == PieceType.Pawn
                    && !HasWrapped(captureStride2, To(square, availableCaptureSquare2)))
                    outSquares.Add(squareState);
            }

            return outSquares;
        }
        
        private IList<SquareState> AvailableKnightSquares(BitBoard board, Colour colour, SquareFlag square)
        {
            var strides = new[] { -17, -15, -10, -6, 6, 10, 15, 17 };

            var outSquares = new List<SquareState>();

            foreach (var stride in strides)
            {
                var directionSquares = CheckKnight(board, colour, square, stride);

                outSquares.AddRange(directionSquares);
            }

            return outSquares;
        }

        private IList<SquareState> AvailableHorizontalSquares(BitBoard board, Colour colour, SquareFlag square, int maxSteps = int.MaxValue)
        {
            var strides = new[] { -8, -1, 1, 8 };

            return CheckAllDirections(board, colour, square, strides, maxSteps);
        }

        private IList<SquareState> AvailableDiagonalSquares(BitBoard board, Colour colour, SquareFlag square, int maxSteps = int.MaxValue)
        {
            var strides = new[] { -9, -7, 7, 9 };

            return CheckAllDirections(board, colour, square, strides, maxSteps);
        }

        private IList<SquareState> CheckKnight(BitBoard board, Colour colour, SquareFlag square, int stride)
        {
            var outSquares = new List<SquareState>();
            var mySquares = colour == Colour.White ? board.White : board.Black;
            var opponentSquares = colour == Colour.White ? board.Black : board.White;
            var currentSquare = square;
            var opponentColour = colour.Opposite();

            currentSquare = Next(currentSquare, stride);

            var relativePosition = To(square, currentSquare);

            if (HasWrapped(stride, relativePosition))
                return outSquares;

            if (mySquares.HasFlag(currentSquare))
                return outSquares;

            if (opponentSquares.HasFlag(currentSquare))
            {
                var pieceType = board.GetPiece(currentSquare);

                outSquares.Add(new SquareState(currentSquare, opponentColour, pieceType));
            }

            outSquares.Add(new SquareState(currentSquare));

            return outSquares;
        }

        private IList<SquareState> CheckAllDirections(BitBoard board, Colour colour, SquareFlag square, int[] strides, int maxSteps)
        {
            var outSquares = new List<SquareState>();

            foreach (var stride in strides)
            {
                var directionSquares = CheckDirection(board, colour, square, stride, maxSteps);

                outSquares.AddRange(directionSquares);
            }

            return outSquares;
        }

        private IList<SquareState> CheckDirection(BitBoard board, Colour colour, SquareFlag square, int stride, int maxSteps)
        {
            var outSquares = new List<SquareState>();
            var mySquares = colour == Colour.White ? board.White : board.Black;
            var opponentSquares = colour == Colour.White ? board.Black : board.White;
            var currentStep = 1;
            var currentSquare = square;
            var startRankFile = square.ToRankFile();
            var opponentColour = colour.Opposite();

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
                    var pieceType = board.GetPiece(currentSquare);
                    
                    outSquares.Add(new SquareState(currentSquare, opponentColour, pieceType));

                    return outSquares;
                }

                outSquares.Add(new SquareState(currentSquare));

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
