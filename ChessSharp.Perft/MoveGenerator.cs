using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ChessSharp.MoveGeneration
{
    /*
     * Using ideas from Pradyumna Kannan.
     */
    public class MoveGenerator
    {
        private static readonly AttackBitmaps AttackBitmaps = new AttackBitmaps();

        public void Generate(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            var relativeBitBoard = bitBoard.RelativeTo(colour);

            var kingSquare = relativeBitBoard.MyKing;
            var kingSquareIndex = kingSquare.ToSquareIndex();

            var checkersPawn = relativeBitBoard.Colour == Colour.White
                ? AttackBitmaps.PawnCapturesWhite[kingSquareIndex] & relativeBitBoard.OpponentPawns
                : AttackBitmaps.PawnCapturesBlack[kingSquareIndex] & relativeBitBoard.OpponentPawns;

            var kingRayAttackSquaresRook = GetAttackableSquares(kingSquareIndex, PieceType.Rook, relativeBitBoard.OccupiedSquares);
            var kingRayAttackSquaresBishop = GetAttackableSquares(kingSquareIndex, PieceType.Bishop, relativeBitBoard.OccupiedSquares);
            var kingRayAttackSquares = kingRayAttackSquaresRook | kingRayAttackSquaresBishop;

            var checkersKnight = (SquareFlag)0;
            var checkersRook = (SquareFlag)0;
            var checkersBishop = (SquareFlag)0;
            var checkersQueen = (SquareFlag)0;

            var buffer1 = new ulong[4];
            var buffer2 = new ulong[4];

            if (Vector<ulong>.Count == 4)
            {
                buffer1[0] = (ulong)AttackBitmaps.KnightAttacks[kingSquareIndex];
                buffer1[1] = (ulong)kingRayAttackSquaresRook;
                buffer1[2] = (ulong)kingRayAttackSquaresBishop;
                buffer1[3] = (ulong)kingRayAttackSquares;

                buffer2[0] = (ulong)relativeBitBoard.OpponentKnights;
                buffer2[1] = (ulong)relativeBitBoard.OpponentRooks;
                buffer2[2] = (ulong)relativeBitBoard.OpponentBishops;
                buffer2[3] = (ulong)relativeBitBoard.OpponentQueens;

                var vector1 = new Vector<ulong>(buffer1);
                var vector2 = new Vector<ulong>(buffer2);

                var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                checkersKnight = (SquareFlag)vectorOut[0];
                checkersRook = (SquareFlag)vectorOut[1];
                checkersBishop = (SquareFlag)vectorOut[2];
                checkersQueen = (SquareFlag)vectorOut[3];
            }
            else
            {
                checkersKnight = AttackBitmaps.KnightAttacks[kingSquareIndex] & relativeBitBoard.OpponentKnights;

                checkersRook = relativeBitBoard.OpponentRooks == 0 ? 0
                    : kingRayAttackSquaresRook & relativeBitBoard.OpponentRooks;

                checkersBishop = relativeBitBoard.OpponentBishops == 0 ? 0
                    : kingRayAttackSquaresBishop & relativeBitBoard.OpponentBishops;

                checkersQueen = relativeBitBoard.OpponentQueens == 0 ? 0
                    : kingRayAttackSquares & relativeBitBoard.OpponentQueens;
            }

            // Start on King moves - if we're in Check then that might be all we need
            var attackableSquaresIncludingSelfCaptures = AttackBitmaps.KingAttacks[kingSquareIndex];

            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;

            var unsafeSquares = (SquareFlag)0;

            // As we have information on Checks use that to find dangerous squares
            if (checkersRook > 0)
                unsafeSquares |= AttackBitmaps.Paths[kingSquareIndex][checkersRook.ToSquareIndex()] & ~checkersRook;

            if (checkersBishop > 0)
                unsafeSquares |= AttackBitmaps.Paths[kingSquareIndex][checkersBishop.ToSquareIndex()] & ~checkersBishop;

            if (checkersQueen > 0)
                unsafeSquares |= AttackBitmaps.Paths[kingSquareIndex][checkersQueen.ToSquareIndex()] & ~checkersQueen;

            var checkers = checkersPawn | checkersKnight | checkersBishop | checkersRook | checkersQueen;

            var semiSafeAttackableSquares = attackableSquares & ~unsafeSquares;

            var semiSafeAttackableSquaresAsList = semiSafeAttackableSquares.ToList();

            var safeSquaresAsList = FindSafeSquares(relativeBitBoard, semiSafeAttackableSquaresAsList);

            foreach (var toSquare in safeSquaresAsList)
            {
                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                {
                    var capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                    moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.King, kingSquare, toSquare, capturePieceType, MoveType.Ordinary));
                }
                else
                {
                    moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.King, kingSquare, toSquare, PieceType.None, MoveType.Ordinary));
                }
            }

            var numCheckers = checkers.Count();

            // If in Check by more than one piece then only options are King moves - which we already have
            if (numCheckers > 1)
                return;

            // https://peterellisjones.com/posts/generating-legal-chess-moves-efficiently/
            // By default - whole board. If in Check it becomes limited to the Checker (no other captures count)
            var captureMask = SquareFlagConstants.All;

            // By default - whole board. If in Check by ray piece it becomes limited to popsitions between King and Checker
            var pushMask = SquareFlagConstants.All;

            if (numCheckers == 1)
            {
                captureMask = checkers;

                var rayChecker = checkers & relativeBitBoard.OpponentRaySquares;

                if (rayChecker > 0)
                {
                    var checkerSquareIndex = rayChecker.ToSquareIndex();

                    var pathFromCheckerToKing = AttackBitmaps.Paths[checkerSquareIndex][kingSquareIndex];

                    // In Check by ray piece so non capture moves must end on this line
                    pushMask = pathFromCheckerToKing & ~kingSquare & ~rayChecker;
                }
                else
                {
                    // Not a ray piece so force captures only
                    pushMask = 0;
                }
            }
            else
            {
                // Only run castle logic if we are not in check
                AddCastles(relativeBitBoard, kingSquareIndex, moves);
            }

            var kingRayAttackSquaresWithoutKing = kingRayAttackSquares & ~kingSquare;

            var pinnedSquares = AddPinnedMoves(bitBoard, relativeBitBoard, kingSquareIndex, kingRayAttackSquaresWithoutKing, pushMask, captureMask, moves);

            var legalMask = pushMask | captureMask;

            var remainingSquares = ~pinnedSquares;

            var remainingKnights = (SquareFlag)0;
            var remainingPawns = (SquareFlag)0;
            var remainingBishops = (SquareFlag)0;
            var remainingQueens = (SquareFlag)0;

            if (Vector<ulong>.Count == 4)
            {
                buffer1[0] = (ulong)relativeBitBoard.MyKnights;
                buffer1[1] = (ulong)relativeBitBoard.MyPawns;
                buffer1[2] = (ulong)relativeBitBoard.MyBishops;
                buffer1[3] = (ulong)relativeBitBoard.MyQueens;

                buffer2[0] = (ulong)remainingSquares;
                buffer2[1] = (ulong)remainingSquares;
                buffer2[2] = (ulong)remainingSquares;
                buffer2[3] = (ulong)remainingSquares;

                var vector1 = new Vector<ulong>(buffer1);
                var vector2 = new Vector<ulong>(buffer2);

                var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                remainingKnights = (SquareFlag)vectorOut[0];
                remainingPawns = (SquareFlag)vectorOut[1];
                remainingBishops = (SquareFlag)vectorOut[2];
                remainingQueens = (SquareFlag)vectorOut[3];
            }
            else
            {
                remainingKnights = relativeBitBoard.MyKnights & remainingSquares;
                remainingPawns = relativeBitBoard.MyPawns & remainingSquares;
                remainingBishops = relativeBitBoard.MyBishops & remainingSquares;
                remainingQueens = relativeBitBoard.MyQueens & remainingSquares;
            }

            if (relativeBitBoard.MyKnights > 0)
            {
                foreach (var pieceSquare in remainingKnights.ToList())
                    AddSingleKnightMoves(relativeBitBoard, pieceSquare, legalMask, moves);
            }

            if (relativeBitBoard.MyPawns > 0)
            {
                foreach (var pieceSquare in remainingPawns.ToList())
                {
                    AddSinglePawnPushes(relativeBitBoard, pieceSquare, pushMask, moves);
                    AddSinglePawnCaptures(relativeBitBoard, pieceSquare, pushMask, captureMask, moves);
                }
            }

            if (relativeBitBoard.MyRooks > 0)
            {
                var remainingRooks = relativeBitBoard.MyRooks & remainingSquares;

                foreach (var pieceSquare in remainingRooks.ToList())
                    AddSingleRookMoves(relativeBitBoard, pieceSquare, legalMask, moves);
            }

            if (relativeBitBoard.MyBishops > 0)
            {
                foreach (var pieceSquare in remainingBishops.ToList())
                    AddSingleBishopMoves(relativeBitBoard, pieceSquare, legalMask, moves);
            }

            if (relativeBitBoard.MyQueens > 0)
            {
                foreach (var pieceSquare in remainingQueens.ToList())
                    AddSingleQueenMoves(relativeBitBoard, pieceSquare, legalMask, moves);
            }
        }

        private IList<SquareFlag> FindSafeSquares(RelativeBitBoard relativeBitBoard, IEnumerable<SquareFlag> attackableSquares)
        {
            //var safeSquares = (SquareFlag)0;
            var safeSquares = new List<SquareFlag>(32);
            var unsafeSquares = (SquareFlag)0;
 
            foreach (var attackableSquare in attackableSquares)
            {
                if (unsafeSquares.HasFlag(attackableSquare))
                    continue;

                var squareIndex = attackableSquare.ToSquareIndex();

                var potentialCheckersKing = AttackBitmaps.KingAttacks[squareIndex] & relativeBitBoard.OpponentKing;

                if (potentialCheckersKing > 0)
                    continue;

                var potentialCheckersPawn = relativeBitBoard.Colour == Colour.White
                    ? AttackBitmaps.PawnCapturesWhite[squareIndex] & relativeBitBoard.OpponentPawns
                    : AttackBitmaps.PawnCapturesBlack[squareIndex] & relativeBitBoard.OpponentPawns;

                if (potentialCheckersPawn > 0)
                    continue;

                var potentialCheckersKnight = AttackBitmaps.KnightAttacks[squareIndex] & relativeBitBoard.OpponentKnights;

                if (potentialCheckersKnight > 0)
                    continue;

                var potentialCheckersRook = GetRayCheckers(relativeBitBoard, squareIndex, PieceType.Rook, PieceType.Rook);

                if (potentialCheckersRook > 0)
                {
                    foreach (var potentialCheckerRook in potentialCheckersRook.ToList())
                        unsafeSquares |= AttackBitmaps.Paths[squareIndex][potentialCheckerRook.ToSquareIndex()] & ~potentialCheckerRook;

                    continue;
                }

                var potentialCheckersBishop = GetRayCheckers(relativeBitBoard, squareIndex, PieceType.Bishop, PieceType.Bishop);

                if (potentialCheckersBishop > 0)
                {
                    foreach (var potentialCheckerBishop in potentialCheckersBishop.ToList())
                        unsafeSquares |= AttackBitmaps.Paths[squareIndex][potentialCheckerBishop.ToSquareIndex()] & ~potentialCheckerBishop;

                    continue;
                }

                var potentialCheckersQueenAsRook = GetRayCheckers(relativeBitBoard, squareIndex, PieceType.Rook, PieceType.Queen);

                if (potentialCheckersQueenAsRook > 0)
                {
                    foreach (var potentialCheckerQueenAsRook in potentialCheckersQueenAsRook.ToList())
                        unsafeSquares |= AttackBitmaps.Paths[squareIndex][potentialCheckerQueenAsRook.ToSquareIndex()] & ~potentialCheckerQueenAsRook;

                    continue;
                }

                var potentialCheckersQueenAsBishop = GetRayCheckers(relativeBitBoard, squareIndex, PieceType.Bishop, PieceType.Queen);

                if (potentialCheckersQueenAsBishop > 0)
                {
                    foreach (var potentialCheckerQueenAsBishop in potentialCheckersQueenAsBishop.ToList())
                        unsafeSquares |= AttackBitmaps.Paths[squareIndex][potentialCheckerQueenAsBishop.ToSquareIndex()] & ~potentialCheckerQueenAsBishop;

                    continue;
                }

                //safeSquares |= attackableSquare;
                safeSquares.Add(attackableSquare);
            }

            return safeSquares;
        }

        private SquareFlag GetRayCheckers(RelativeBitBoard relativeBitBoard, int fromSquareIndex, PieceType rayType, PieceType pieceType)
        {
            var opponentSquares = pieceType == PieceType.Queen
                ? relativeBitBoard.OpponentQueens
                : pieceType == PieceType.Rook
                    ? relativeBitBoard.OpponentRooks
                    : relativeBitBoard.OpponentBishops;

            var attackableSquaresWithoutKing = relativeBitBoard.OccupiedSquares & ~relativeBitBoard.MyKing;

            var attackableSquares = GetAttackableSquares(fromSquareIndex, rayType, attackableSquaresWithoutKing);

            return attackableSquares & opponentSquares;
        }

        private SquareFlag AddPinnedMoves(BitBoard bitBoard, RelativeBitBoard relativeBitBoard, int kingSquareIndex, SquareFlag kingRayAttackSquares,
            SquareFlag pushMask, SquareFlag captureMask, IList<uint> moves)
        {
            var potentialPins = kingRayAttackSquares & relativeBitBoard.MySquares;

            var occupancyWithoutPotentialPins = relativeBitBoard.OccupiedSquares & ~potentialPins;

            var attackableSquaresBeyondPinsRook = GetAttackableSquares(kingSquareIndex, PieceType.Rook, occupancyWithoutPotentialPins);
            var attackableSquaresBeyondPinsBishop = GetAttackableSquares(kingSquareIndex, PieceType.Bishop, occupancyWithoutPotentialPins);
            var attackableSquaresBeyondPins = attackableSquaresBeyondPinsRook | attackableSquaresBeyondPinsBishop;

            var pinningRooks = attackableSquaresBeyondPinsRook & relativeBitBoard.OpponentRooks;
            var pinningBishops = attackableSquaresBeyondPinsBishop & relativeBitBoard.OpponentBishops;
            var pinningQueensNonDiagonal = attackableSquaresBeyondPinsRook & relativeBitBoard.OpponentQueens;
            var pinningQueensDiagonal = attackableSquaresBeyondPinsBishop & relativeBitBoard.OpponentQueens;

            var pinnedPieces = (SquareFlag)0;

            if (pinningRooks > 0)
                pinnedPieces |= AddPinnedMovesInternal(bitBoard, relativeBitBoard, kingSquareIndex, potentialPins, pinningRooks, false, pushMask, captureMask, moves);

            if (pinningBishops > 0)
                pinnedPieces |= AddPinnedMovesInternal(bitBoard, relativeBitBoard, kingSquareIndex, potentialPins, pinningBishops, true, pushMask, captureMask, moves);

            if (pinningQueensNonDiagonal > 0)
                pinnedPieces |= AddPinnedMovesInternal(bitBoard, relativeBitBoard, kingSquareIndex, potentialPins, pinningQueensNonDiagonal, false, pushMask, captureMask, moves);

            if (pinningQueensDiagonal > 0)
                pinnedPieces |= AddPinnedMovesInternal(bitBoard, relativeBitBoard, kingSquareIndex, potentialPins, pinningQueensDiagonal, true, pushMask, captureMask, moves);

            return pinnedPieces;
        }

        private SquareFlag AddPinnedMovesInternal(BitBoard bitBoard, RelativeBitBoard relativeBitBoard, int kingSquareIndex, SquareFlag potentialPins,
            SquareFlag pinners, bool diagonal, SquareFlag pushMask, SquareFlag captureMask, IList<uint> moves)
        {
            var pinnedSquares = (SquareFlag)0;
            var pinnersAsList = pinners.ToList();

            foreach (var pinner in pinnersAsList)
            {
                var path = AttackBitmaps.Paths[kingSquareIndex][pinner.ToSquareIndex()];

                var squarePinnedByThisPiece = path & potentialPins;

                if (squarePinnedByThisPiece == 0)
                    continue;

                pinnedSquares |= squarePinnedByThisPiece;

                var pushPath = path & pushMask;
                var capturePath = path & captureMask;

                var pieceType = relativeBitBoard.GetPieceType(squarePinnedByThisPiece);

                switch (pieceType)
                {
                    case PieceType.Pawn when !diagonal:
                        AddSinglePawnPushes(relativeBitBoard, squarePinnedByThisPiece, pushPath, moves);
                        break;
                    case PieceType.Pawn when diagonal:
                        AddSinglePawnCaptures(relativeBitBoard, squarePinnedByThisPiece, pushPath, capturePath, moves);
                        break;
                    case PieceType.Rook:
                        AddSingleRookMoves(relativeBitBoard, squarePinnedByThisPiece, pushPath | capturePath, moves);
                        break;
                    case PieceType.Bishop:
                        AddSingleBishopMoves(relativeBitBoard, squarePinnedByThisPiece, pushPath | capturePath, moves);
                        break;
                    case PieceType.Queen:
                        AddSingleQueenMoves(relativeBitBoard, squarePinnedByThisPiece, pushPath | capturePath, moves);
                        break;
                }
            }

            return pinnedSquares;
        }
        
        private void AddSinglePawnPushes(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, SquareFlag pushMask, IList<uint> moves)
        {
            var toSquare = fromSquare.PawnForward(relativeBitBoard.Colour, 1);

            if (relativeBitBoard.OccupiedSquares.HasFlag(toSquare))
                return;

            if (pushMask.HasFlag(toSquare))
            {
                if (relativeBitBoard.PromotionRank.HasFlag(toSquare))
                    AddPromotions(relativeBitBoard, fromSquare, toSquare, PieceType.None, moves);
                else
                    moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
            }

            // Possible that we can block check with double push
            if (relativeBitBoard.StartRank.HasFlag(fromSquare))
            {
                toSquare = fromSquare.PawnForward(relativeBitBoard.Colour, 2);

                if (!pushMask.HasFlag(toSquare))
                    return;

                // Promotions not possible from start rank
                if (!relativeBitBoard.OccupiedSquares.HasFlag(toSquare))
                    moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
            }
        }

        private void AddSinglePawnCaptures(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, SquareFlag pushMask, SquareFlag captureMask, IList<uint> moves)
        {
            var fromSquareIndex = fromSquare.ToSquareIndex();

            var captureSquares = relativeBitBoard.Colour == Colour.White
                ? AttackBitmaps.PawnCapturesWhite[fromSquareIndex].ToList()
                : AttackBitmaps.PawnCapturesBlack[fromSquareIndex].ToList();

            foreach (var toSquare in captureSquares)
            {
                var moveType = relativeBitBoard.EnPassant.HasFlag(toSquare) ? MoveType.EnPassant : MoveType.Ordinary;

                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare) || moveType == MoveType.EnPassant)
                {
                    var capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                    var discoveredCheck = false;

                    if (moveType != MoveType.EnPassant)
                    {
                        if (!captureMask.HasFlag(toSquare))
                            continue;
                    }
                    else
                    {
                        // Abuse the push system - push an imaginary pawn from the en passant square as opponent to find piece
                        var enPassantCaptureSquare = relativeBitBoard.EnPassant.PawnForward(relativeBitBoard.OpponentColour, 1);

                        var blockWithPush = pushMask.HasFlag(relativeBitBoard.EnPassant);

                        var evadeWithCapture = captureMask.HasFlag(enPassantCaptureSquare);

                        if (!blockWithPush && !evadeWithCapture)
                            continue;

                        capturePieceType = PieceType.Pawn;

                        // Looking for DISCOVERED CHECK (not spotted by pinned pieces as there are 2 pawns in the way)
                        var kingSquare = relativeBitBoard.MyKing;

                        var enPassantDiscoveredCheckRank = relativeBitBoard.EnPassantDiscoveredCheckRank;

                        // This should be super rare. Performing an en passant capture with our King on same rank.
                        if (enPassantDiscoveredCheckRank.HasFlag(kingSquare))
                        {
                            if ((enPassantDiscoveredCheckRank & relativeBitBoard.OpponentRooks) > 0
                                || (enPassantDiscoveredCheckRank & relativeBitBoard.OpponentQueens) > 0)
                            {
                                var kingSquareIndex = kingSquare.ToSquareIndex();

                                var occupancyMask = GetOccupancyMask(PieceType.Rook, kingSquareIndex);

                                var occupancyBeforeCapture = relativeBitBoard.OccupiedSquares & occupancyMask;

                                var occupancyAfterCapture = occupancyBeforeCapture
                                    & ~enPassantCaptureSquare
                                    & ~fromSquare;

                                // Search for magic moves using just the occupancy of rank (the rest is not relevant)
                                var magicIndex = MagicIndexHelpers.GetMagicIndex(PieceType.Rook, kingSquareIndex, occupancyAfterCapture);

                                var kingRayAttacks = AttackBitmaps.RookAttacks[kingSquareIndex][magicIndex];

                                var kingRayAttacksOnRank = kingRayAttacks & enPassantDiscoveredCheckRank;

                                discoveredCheck = (kingRayAttacksOnRank & relativeBitBoard.OpponentRooks) > 0
                                    || (kingRayAttacksOnRank & relativeBitBoard.OpponentQueens) > 0;
                            }
                        }
                    }

                    if (relativeBitBoard.PromotionRank.HasFlag(toSquare))
                        AddPromotions(relativeBitBoard, fromSquare, toSquare, capturePieceType, moves);
                    else if (!discoveredCheck)
                        moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, moveType));
                }
            }
        }

        private void AddSingleKnightMoves(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, SquareFlag legalMask, IList<uint> moves)
        {
            var squareIndex = fromSquare.ToSquareIndex();

            var attackableSquaresIncludingSelfCaptures = AttackBitmaps.KnightAttacks[squareIndex];
            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
            var legalAttackableSquares = attackableSquares & legalMask;

            ToOrdinaryMoves(relativeBitBoard, PieceType.Knight, fromSquare, legalAttackableSquares, moves);
        }

        private void AddSingleRookMoves(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, SquareFlag legalMask, IList<uint> moves) =>
            AddSingleRayMoves(relativeBitBoard, fromSquare, PieceType.Rook, PieceType.Rook, legalMask, moves);

        private void AddSingleBishopMoves(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, SquareFlag legalMask, IList<uint> moves) =>
            AddSingleRayMoves(relativeBitBoard, fromSquare, PieceType.Bishop, PieceType.Bishop, legalMask, moves);

        private void AddSingleQueenMoves(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, SquareFlag legalMask, IList<uint> moves)
        {
            AddSingleRayMoves(relativeBitBoard, fromSquare, PieceType.Rook, PieceType.Queen, legalMask, moves);
            AddSingleRayMoves(relativeBitBoard, fromSquare, PieceType.Bishop, PieceType.Queen, legalMask, moves);
        }

        private void AddSingleRayMoves(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, PieceType rayType, PieceType pieceType, SquareFlag legalMask, IList<uint> moves)
        {
            var attackableSquaresIncludingSelfCaptures = GetAttackableSquares(fromSquare.ToSquareIndex(), rayType, relativeBitBoard.OccupiedSquares);
            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
            var legalAttackableSquares = attackableSquares & legalMask;

            ToOrdinaryMoves(relativeBitBoard, pieceType, fromSquare, legalAttackableSquares, moves);
        }

        private void AddCastles(RelativeBitBoard relativeBitBoard, int kingSquareIndex, IList<uint> moves)
        {
            if (!relativeBitBoard.CanCastleKingSide && !relativeBitBoard.CanCastleQueenSide)
                return;

            // We lose castle rights if any piece moves so they MUST be in correct locations
            if (relativeBitBoard.CanCastleKingSide)
            {
                var kingSideRookIndex = relativeBitBoard.KingSideRookStartSquare.ToSquareIndex();

                var kingToRook = AttackBitmaps.Paths[kingSquareIndex][kingSideRookIndex];

                var squaresBetween = kingToRook & ~relativeBitBoard.KingStartSquare & ~relativeBitBoard.KingSideRookStartSquare;

                if ((squaresBetween & relativeBitBoard.OccupiedSquares) == 0)
                {
                    var stepSquares = relativeBitBoard.KingSideCastleStep1 | relativeBitBoard.KingSideCastleStep2;
                    var safeSquaresAsList = FindSafeSquares(relativeBitBoard, stepSquares.ToList());

                    var safeSquares = (SquareFlag)0;

                    foreach (var safeSquare in safeSquaresAsList)
                        safeSquares |= safeSquare;

                    if (squaresBetween == safeSquares)
                    {
                        moves.Add(MoveBuilder.CreateCastle(relativeBitBoard.Colour, MoveType.CastleKing));
                    }
                }
            }

            if (relativeBitBoard.CanCastleQueenSide)
            {
                var queenSideRookIndex = relativeBitBoard.QueenSideRookStartSquare.ToSquareIndex();

                var kingToRook = AttackBitmaps.Paths[kingSquareIndex][queenSideRookIndex];

                var squaresBetween = kingToRook & ~relativeBitBoard.KingStartSquare & ~relativeBitBoard.QueenSideRookStartSquare;

                if ((squaresBetween & relativeBitBoard.OccupiedSquares) == 0)
                {
                    var stepSquares = relativeBitBoard.QueenSideCastleStep1 | relativeBitBoard.QueenSideCastleStep2;
                    var safeSquaresAsList = FindSafeSquares(relativeBitBoard, stepSquares.ToList());

                    var safeSquares = (SquareFlag)0;

                    foreach (var safeSquare in safeSquaresAsList)
                        safeSquares |= safeSquare;

                    // On Queen side the King doesn't pass through B file so we don't look for Check there
                    var squaresBetweenMinusFirstRookStep = squaresBetween & ~relativeBitBoard.QueenSideRookStep1Square;

                    if (squaresBetweenMinusFirstRookStep == safeSquares)
                        moves.Add(MoveBuilder.CreateCastle(relativeBitBoard.Colour, MoveType.CastleQueen));
                }
            }
        }

        private void AddPromotions(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, SquareFlag toSquare, PieceType capturePieceType, IList<uint> moves)
        {
            moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionQueen));
            moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionRook));
            moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionKnight));
            moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionBishop));
        }

        private SquareFlag GetAttackableSquares(int fromSquareIndex, PieceType rayType, SquareFlag occupiedSquares)
        {
            var occupancyMask = GetOccupancyMask(rayType, fromSquareIndex);

            var occupancyMasked = occupiedSquares & occupancyMask;

            var magicIndex = MagicIndexHelpers.GetMagicIndex(rayType, fromSquareIndex, occupancyMasked);

            var attackableSquaresIncludingSelfCaptures = rayType == PieceType.Rook
                ? AttackBitmaps.RookAttacks[fromSquareIndex][magicIndex]
                : AttackBitmaps.BishopAttacks[fromSquareIndex][magicIndex];

            return attackableSquaresIncludingSelfCaptures;
        }

        private SquareFlag GetOccupancyMask(PieceType pieceType, int squareIndex) =>
            pieceType == PieceType.Rook
                    ? MagicNumbers.RookOccupancyMasks[squareIndex]
                    : MagicNumbers.BishopOccupancyMasks[squareIndex];

        private void ToOrdinaryMoves(RelativeBitBoard relativeBitBoard, PieceType pieceType, SquareFlag fromSquare, SquareFlag attackableSquares, IList<uint> moves)
        {
            var attackableSquaresAsList = attackableSquares.ToList();

            foreach (var toSquare in attackableSquaresAsList)
            {
                var capturePieceType = PieceType.None;

                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                    capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, pieceType, fromSquare, toSquare, capturePieceType, MoveType.Ordinary));
            }
        }
    }
}
