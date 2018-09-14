using Chess.Engine.Extensions;
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

        private static int[] StridesKnight = new[] { -17, -15, -10, -6, 6, 10, 15, 17 };
        private static int[] StridesHorizontal = new[] { -8, -1, 1, 8 };
        private static int[] StridesDiagonal = new[] { -9, -7, 7, 9 };
        private static int[] StridesQueen = new[] { -9, -8, -7, -1, 1, 7, 8, 9 };

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


        public IList<Move> FindMoves(BitBoard board, SquareFlag enPassantSquare, Colour colour, IReadOnlyCollection<SquareState> squareStates, bool withCastles = true)
        {
            var moves = new List<Move>();

            foreach (var squareState in squareStates)
            {
                var pieceMoves = FindMoves(board, enPassantSquare, colour, squareState.Square, withCastles);

                if (pieceMoves.Any())
                    moves.AddRange(pieceMoves);
            };

            return moves;
        }

        private IList<Move> FindMoves(BitBoard board, SquareFlag enPassantSquare, Colour colour, SquareFlag square, bool withCastles = true)
        {
            var pieceType = board.GetPieceType(square);
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
                if (board.GetPieceType(availableSquare1) == PieceType.None)
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

                        if (board.GetPieceType(availableSquare2) == PieceType.None)
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
                var isEnpassantCapture = enPassantSquare != 0 && availableCaptureSquare1 == enPassantSquare;

                var isCapture = board.GetPieceType(availableCaptureSquare1) != PieceType.None && board.GetPieceColour(availableCaptureSquare1) != colour;

                if ((isCapture || isEnpassantCapture) && !HasWrapped(captureStride1, To(square, availableCaptureSquare1)))
                {
                    var enPassantCaptureSquare = Next(availableCaptureSquare1, -stride);
                    moves.AddRange(CreatePawnMove(board, colour, startRankFile, availableCaptureSquare1, PieceType.Pawn, promotionRank, isEnpassantCapture ? enPassantCaptureSquare : 0));
                }
            }

            if (availableCaptureSquare2 != 0)
            {
                var isEnpassantCapture = enPassantSquare != 0 && availableCaptureSquare2 == enPassantSquare;

                var isCapture = board.GetPieceType(availableCaptureSquare2) != PieceType.None && board.GetPieceColour(availableCaptureSquare2) != colour;

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
                if (board.GetPieceType(availableSquare1) == PieceType.None)
                {
                    outSquares.Add(availableSquare1);

                    var rankFile = square.ToRankFile();

                    // Has this pawn moved?
                    if (colour == Colour.White && SquareFlagExtensions.r2.HasFlag(square) || colour == Colour.Black && SquareFlagExtensions.r7.HasFlag(square))
                    {
                        // This must be 'safe' as it is on starting position
                        var availableSquare2 = Next(availableSquare1, stride);

                        if (board.GetPieceType(availableSquare2) == PieceType.None)
                            outSquares.Add(availableSquare2);
                    }
                }
            }

            if (availableCaptureSquare1 != 0)
            {
                var piece = board.GetPiece(availableCaptureSquare1);

                //if (board.GetPieceType(availableCaptureSquare1) != PieceType.None
                //    && board.GetPieceColour(availableCaptureSquare1) != colour
                if(piece.Type != PieceType.None && piece.Colour != colour
                    && !HasWrapped(captureStride1, To(square, availableCaptureSquare1)))
                    outSquares.Add(availableCaptureSquare1);
            }

            if (availableCaptureSquare2 != 0)
            {
                var piece = board.GetPiece(availableCaptureSquare2);

                //if (board.GetPieceType(availableCaptureSquare2) != PieceType.None
                //    && board.GetPieceColour(availableCaptureSquare2) != colour
                if (piece.Type != PieceType.None && piece.Colour != colour
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

            var type = board.GetPieceType(square);

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

                if (board.GetPieceType(currentSquare) != PieceType.None)
                {
                    stillCan = false;
                }
                else
                {
                    attackingThisSquare = FindPiecesAttackingThisSquare(board, colour, currentSquare);

                    var queenSideSafeSpace = side == PieceType.Queen && currentSquare == Next(square, stride);

                    // This is the square next to the Rook and CAN be under attack as the King does not pass through
                    if (attackingThisSquare.Any() && !queenSideSafeSpace)
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

        public List<SquareState> FindBlockedPawns(BitBoard board, Colour colour, SquareFlag square)
        {
            var outSquares = new List<SquareState>();

            // Stride reversed when looking for blocks
            var stride = colour == Colour.White ? -8 : 8;

            var possiblePawnMine = Next(square, stride);
            var possiblePawnOpponent = Next(square, -stride);

            if (possiblePawnMine != 0)
            {
                var squareState = board.GetSquareState(possiblePawnMine);

                if (squareState.Type == PieceType.Pawn
                    && !HasWrapped(stride, To(square, possiblePawnMine)))
                    outSquares.Add(squareState);
            }

            if (possiblePawnOpponent != 0)
            {
                var squareState = board.GetSquareState(possiblePawnOpponent);

                if (squareState.Type == PieceType.Pawn
                    && !HasWrapped(-stride, To(square, possiblePawnOpponent)))
                    outSquares.Add(squareState);
            }

            possiblePawnMine = Next(possiblePawnMine, stride);
            possiblePawnOpponent = Next(possiblePawnOpponent, -stride);

            if (possiblePawnMine != 0)
            {
                var squareState = board.GetSquareState(possiblePawnMine);

                if (squareState.Type == PieceType.Pawn
                    && !HasWrapped(stride, To(square, possiblePawnMine)))
                    outSquares.Add(squareState);
            }

            if (possiblePawnOpponent != 0)
            {
                var squareState = board.GetSquareState(possiblePawnOpponent);

                if (squareState.Type == PieceType.Pawn
                    && !HasWrapped(-stride, To(square, possiblePawnOpponent)))
                    outSquares.Add(squareState);
            }

            return outSquares;
        }

        public List<SquareState> FindPiecesAttackingThisSquare(BitBoard board, Colour colour, SquareFlag square)
        {
            var squares = new List<SquareState>();

            Func<SquareState, bool> opponentColour = ss => ss.Colour == colour.Opposite();
            Func<SquareState, bool> anyColour = ss => true;
            Func<SquareState, bool> whereColour = colour == Colour.None ? anyColour : opponentColour;
            
            Func<SquareState, bool> whereQueen = ss => whereColour(ss) && ss.Type == PieceType.Queen;
            Func<SquareState, bool> whereRook = ss => whereColour(ss) && ss.Type == PieceType.Rook;
            Func<SquareState, bool> whereBishop = ss => whereColour(ss) && ss.Type == PieceType.Bishop;
            Func<SquareState, bool> wherePawn = ss => whereColour(ss) && ss.Type == PieceType.Pawn;
            Func<SquareState, bool> whereKnight = ss => whereColour(ss) && ss.Type == PieceType.Knight;
            Func<SquareState, bool> whereKing = ss => whereColour(ss) && ss.Type == PieceType.King;

            if (colour == Colour.None)
            {
                var attackedByPawnWhite = FindPawnsAttackingThisSquare(board, Colour.White, square);

                if (attackedByPawnWhite.Any())
                    squares.AddRange(attackedByPawnWhite);

                var attackedByPawnBlack = FindPawnsAttackingThisSquare(board, Colour.Black, square);

                if (attackedByPawnBlack.Any())
                    squares.AddRange(attackedByPawnBlack);
            }
            else
            {
                var attackedByPawn = FindPawnsAttackingThisSquare(board, colour, square);

                if (attackedByPawn.Any())
                    squares.AddRange(attackedByPawn);
            }

            var attackedByRook = GetStandardCoveredSquares(board, colour, square, PieceType.Rook)
                .Where(whereRook);

            if (attackedByRook.Any())
                squares.AddRange(attackedByRook);

            var attackedByKnight = AvailableKnightSquares(board, colour, square)
                .Where(whereKnight);

            if (attackedByKnight.Any())
                squares.AddRange(attackedByKnight);

            var attackedByBishop = GetStandardCoveredSquares(board, colour, square, PieceType.Bishop)
                .Where(whereBishop);

            if (attackedByBishop.Any())
                squares.AddRange(attackedByBishop);

            var attackedByQueen = GetStandardCoveredSquares(board, colour, square, PieceType.Queen)
                .Where(whereQueen);

            if (attackedByQueen.Any())
                squares.AddRange(attackedByQueen);

            var attackedByKing = GetStandardCoveredSquares(board, colour, square, PieceType.King)
                .Where(whereKing);

            if (attackedByKing.Any())
                squares.AddRange(attackedByKing);

            return squares;
        }

        private List<SquareState> GetStandardCoveredSquares(BitBoard board, Colour colour, SquareFlag square, PieceType pieceType)
        {
            var outSquares = new List<SquareState>();

            switch (pieceType)
            {
                case PieceType.Rook:
                    {
                        var horizontals = AvailableHorizontalSquares(board, colour, square);

                        outSquares.AddRange(horizontals);
                        break;
                    }

                case PieceType.Knight:
                    {
                        var knights = AvailableKnightSquares(board, colour, square);

                        outSquares.AddRange(knights);
                        break;
                    }

                case PieceType.Bishop:
                    {
                        var diagonals = AvailableDiagonalSquares(board, colour, square);

                        outSquares.AddRange(diagonals);
                        break;
                    }

                case PieceType.Queen:
                    {
                        var horizontals = AvailableHorizontalSquares(board, colour, square);
                        var diagonals = AvailableDiagonalSquares(board, colour, square);

                        outSquares.AddRange(horizontals);
                        outSquares.AddRange(diagonals);
                        break;
                    }

                case PieceType.King:
                    {
                        var horizontals = AvailableHorizontalSquares(board, colour, square, 1);
                        var diagonals = AvailableDiagonalSquares(board, colour, square, 1);

                        outSquares.AddRange(horizontals);
                        outSquares.AddRange(diagonals);
                        break;
                    }
            }

            return outSquares;
        }
        
        public List<SquareState> FindPawnsAttackingThisSquare(BitBoard board, Colour colour, SquareFlag square)
        {
            var outSquares = new List<SquareState>();

            var stride = colour == Colour.White ? 8 : -8;
            var captureStride1 = stride + 1;
            var captureStride2 = stride - 1;
            var availableCaptureSquare1 = Next(square, captureStride1);
            var availableCaptureSquare2 = Next(square, captureStride2);

            if (availableCaptureSquare1 != 0)
            {
                var squareState = board.GetSquareState(availableCaptureSquare1);

                var colourOk = colour == Colour.None
                    ? true
                    : squareState.Colour == colour.Opposite();

                if (colourOk
                    && squareState.Type == PieceType.Pawn
                    && !HasWrapped(captureStride1, To(square, availableCaptureSquare1)))
                    outSquares.Add(squareState);
            }

            if (availableCaptureSquare2 != 0)
            {
                var squareState = board.GetSquareState(availableCaptureSquare2);

                var colourOk = colour == Colour.None
                    ? true
                    : squareState.Colour == colour.Opposite();

                if (colourOk
                    && squareState.Type == PieceType.Pawn
                    && !HasWrapped(captureStride2, To(square, availableCaptureSquare2)))
                    outSquares.Add(squareState);
            }

            return outSquares;
        }
        
        private IList<SquareState> AvailableKnightSquares(BitBoard board, Colour colour, SquareFlag square)
        {
            //var strides = new[] { -17, -15, -10, -6, 6, 10, 15, 17 };

            var outSquares = new List<SquareState>();

            foreach (var stride in StridesKnight)
            {
                var directionSquares = CheckKnight(board, colour, square, stride);

                outSquares.AddRange(directionSquares);
            }

            return outSquares;
        }

        private IList<SquareState> AvailableHorizontalSquares(BitBoard board, Colour colour, SquareFlag square, int maxSteps = int.MaxValue)
        {
            //var strides = new[] { -8, -1, 1, 8 };

            return CheckAllDirections(board, colour, square, StridesHorizontal, maxSteps);
        }

        private IList<SquareState> AvailableDiagonalSquares(BitBoard board, Colour colour, SquareFlag square, int maxSteps = int.MaxValue)
        {
            //var strides = new[] { -9, -7, 7, 9 };

            return CheckAllDirections(board, colour, square, StridesDiagonal, maxSteps);
        }

        private IList<SquareState> CheckKnight(BitBoard board, Colour colour, SquareFlag square, int stride)
        {
            var outSquares = new List<SquareState>();
            var mySquares = colour == Colour.White ? board.White : board.Black;
            //var opponentSquares = colour == Colour.White ? board.Black : board.White;
            var currentSquare = square;
            var opponentColour = colour.Opposite();
            var opponentSquares = board.FindPieceSquares(opponentColour);

            if (colour == Colour.None)
            {
                mySquares = 0;
                opponentSquares = board.White | board.Black;
            }

            currentSquare = Next(currentSquare, stride);

            var relativePosition = To(square, currentSquare);

            if (HasWrapped(stride, relativePosition))
                return outSquares;

            if (mySquares.HasFlag(currentSquare))
                return outSquares;

            var ss = board.GetSquareState(currentSquare);

            if (opponentSquares.HasFlag(currentSquare))
            {
                //var pieceType = board.GetPieceType(currentSquare);

                //outSquares.Add(new SquareState(currentSquare, new Piece(opponentColour, pieceType)));

                outSquares.Add(ss);

                return outSquares;
            }

            //outSquares.Add(new SquareState(currentSquare));
            outSquares.Add(ss);

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

            //var opponentSquares = colour == Colour.White ? board.Black : board.White;
            var currentStep = 1;
            var currentSquare = square;
            var startRankFile = square.ToRankFile();
            var opponentColour = colour.Opposite();
            var opponentSquares = board.FindPieceSquares(opponentColour);

            if (colour == Colour.None)
            {
                mySquares = 0;
                opponentSquares = board.White | board.Black;
            }

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
                    var pieceType = board.GetPieceType(currentSquare);
                    
                    outSquares.Add(new SquareState(currentSquare, new Piece(opponentColour, pieceType)));

                    return outSquares;
                }

                outSquares.Add(new SquareState(currentSquare));

                ++currentStep;
            }

            return outSquares;
        }

        public bool CheckForPawnPromotion(BitBoard board, SquareFlag startSquare, SquareFlag endSquare)
        {
            if (board.GetPieceType(startSquare) != PieceType.Pawn)
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

        // TODO: Move this to a general helper area
        public SquareFlag Next(SquareFlag currentSquare, int stride)
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
