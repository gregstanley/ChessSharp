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

        public void Generate(MoveGenerationWorkspace workspace, IList<uint> moves)
        {
            var relativeBitBoard = workspace.RelativeBitBoard;

            var kingSquare = relativeBitBoard.MyKing.ToSquare();

            var kingRayAttackSquaresRook = GetAttackableSquares(kingSquare, PieceType.Rook, relativeBitBoard.OccupiedSquares);
            var kingRayAttackSquaresBishop = GetAttackableSquares(kingSquare, PieceType.Bishop, relativeBitBoard.OccupiedSquares);
            var kingRayAttackSquares = kingRayAttackSquaresRook | kingRayAttackSquaresBishop;

            var checkersPawn = relativeBitBoard.Colour == Colour.White
                ? AttackBitmaps.PawnCapturesWhite[kingSquare.Index] & relativeBitBoard.OpponentPawns
                : AttackBitmaps.PawnCapturesBlack[kingSquare.Index] & relativeBitBoard.OpponentPawns;

            var checkersKnight = (SquareFlag)0;
            var checkersRook = (SquareFlag)0;
            var checkersBishop = (SquareFlag)0;
            var checkersQueen = (SquareFlag)0;

            var buffer1 = workspace.Buffer1;
            var buffer2 = workspace.Buffer2;

            var anyNonPawnCheckers = false;

            if (Vector<ulong>.Count == 4)
            {
                buffer1[0] = (ulong)AttackBitmaps.KnightAttacks[kingSquare.Index];
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

                anyNonPawnCheckers = Vector.GreaterThanAny(vectorOut, new Vector<ulong>(0));

                checkersKnight = (SquareFlag)vectorOut[0];
                checkersRook = (SquareFlag)vectorOut[1];
                checkersBishop = (SquareFlag)vectorOut[2];
                checkersQueen = (SquareFlag)vectorOut[3];
            }
            else
            {
                checkersKnight = AttackBitmaps.KnightAttacks[kingSquare.Index] & relativeBitBoard.OpponentKnights;

                checkersRook = relativeBitBoard.OpponentRooks == 0 ? 0
                    : kingRayAttackSquaresRook & relativeBitBoard.OpponentRooks;

                checkersBishop = relativeBitBoard.OpponentBishops == 0 ? 0
                    : kingRayAttackSquaresBishop & relativeBitBoard.OpponentBishops;

                checkersQueen = relativeBitBoard.OpponentQueens == 0 ? 0
                    : kingRayAttackSquares & relativeBitBoard.OpponentQueens;
            }

            // Start on King moves - if we're in Check then that might be all we need
            var attackableSquaresIncludingSelfCaptures = AttackBitmaps.KingAttacks[kingSquare.Index];

            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;

            var unsafeSquares = (SquareFlag)0;

            var checkers = (SquareFlag)0;

            if (checkersPawn > 0 || anyNonPawnCheckers)
            {
                // As we have information on Checks use that to find dangerous squares
                if (checkersRook > 0)
                    unsafeSquares |= AttackBitmaps.Paths[kingSquare.Index][checkersRook.ToSquareIndex()] & ~checkersRook;

                if (checkersBishop > 0)
                    unsafeSquares |= AttackBitmaps.Paths[kingSquare.Index][checkersBishop.ToSquareIndex()] & ~checkersBishop;

                if (checkersQueen > 0)
                    unsafeSquares |= AttackBitmaps.Paths[kingSquare.Index][checkersQueen.ToSquareIndex()] & ~checkersQueen;

                checkers = checkersPawn | checkersKnight | checkersRook | checkersBishop | checkersQueen;
            }

            var semiSafeAttackableSquares = attackableSquares & ~unsafeSquares;

            var semiSafeAttackableSquaresAsList = semiSafeAttackableSquares.ToList();

            var safeSquaresAsList = FindSafeSquares(relativeBitBoard, semiSafeAttackableSquaresAsList, workspace.SafeSquares);

            foreach (var toSquare in safeSquaresAsList)
            {
                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                {
                    var capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                    moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.King, kingSquare, toSquare.ToSquare(), capturePieceType, MoveType.Ordinary));
                }
                else
                {
                    moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.King, kingSquare, toSquare.ToSquare(), PieceType.None, MoveType.Ordinary));
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

                    var pathFromCheckerToKing = AttackBitmaps.Paths[checkerSquareIndex][kingSquare.Index];

                    // In Check by ray piece so non capture moves must end on this line
                    pushMask = pathFromCheckerToKing & ~kingSquare.Flag & ~rayChecker;
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
                AddCastles(relativeBitBoard, workspace.SafeSquares, kingSquare, moves);
            }

            var kingRayAttackSquaresWithoutKing = kingRayAttackSquares & ~kingSquare.Flag;

            // Pass the relative bitboard in separately to avoid it being re-generated
            var pinnedSquares = AddPinnedMoves(workspace, relativeBitBoard, kingSquare, kingRayAttackSquaresWithoutKing, pushMask, captureMask, moves);

            var legalMask = pushMask | captureMask;

            var remainingSquares = ~pinnedSquares;

            var remainingPawns = (SquareFlag)0;
            var remainingKnights = (SquareFlag)0;
            var remainingRooks = (SquareFlag)0;
            var remainingBishops = (SquareFlag)0;
            
            if (Vector<ulong>.Count == 4)
            {
                buffer1[0] = (ulong)relativeBitBoard.MyPawns;
                buffer1[1] = (ulong)relativeBitBoard.MyKnights;
                buffer1[2] = (ulong)relativeBitBoard.MyRooks;
                buffer1[3] = (ulong)relativeBitBoard.MyBishops;

                buffer2[0] = (ulong)remainingSquares;
                buffer2[1] = (ulong)remainingSquares;
                buffer2[2] = (ulong)remainingSquares;
                buffer2[3] = (ulong)remainingSquares;

                var vector1 = new Vector<ulong>(buffer1);
                var vector2 = new Vector<ulong>(buffer2);

                var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                remainingPawns = (SquareFlag)vectorOut[0];
                remainingKnights = (SquareFlag)vectorOut[1];
                remainingRooks = (SquareFlag)vectorOut[2];
                remainingBishops = (SquareFlag)vectorOut[3];
            }
            else
            {
                remainingPawns = relativeBitBoard.MyPawns & remainingSquares;
                remainingKnights = relativeBitBoard.MyKnights & remainingSquares;
                remainingRooks = relativeBitBoard.MyRooks & remainingSquares;
                remainingBishops = relativeBitBoard.MyBishops & remainingSquares;
            }

            if (relativeBitBoard.MyPawns > 0)
            {
                foreach (var pieceSquare in remainingPawns.ToList())
                {
                    var toSquare = pieceSquare.ToSquare();

                    AddSinglePawnPushes(relativeBitBoard, toSquare, pushMask, moves);
                    AddSinglePawnCaptures(relativeBitBoard, toSquare, pushMask, captureMask, moves);
                }
            }

            if (relativeBitBoard.MyKnights > 0)
            {
                foreach (var pieceSquareFlag in remainingKnights.ToList())
                    AddSingleKnightMoves(relativeBitBoard, pieceSquareFlag.ToSquare(), legalMask, moves);
            }

            if (relativeBitBoard.MyRooks > 0)
            {
                foreach (var pieceSquare in remainingRooks.ToList())
                    AddSingleRayMoves(relativeBitBoard, pieceSquare.ToSquare(), PieceType.Rook, PieceType.Rook, legalMask, moves);
            }

            if (relativeBitBoard.MyBishops > 0)
            {
                foreach (var pieceSquare in remainingBishops.ToList())
                    AddSingleRayMoves(relativeBitBoard, pieceSquare.ToSquare(), PieceType.Bishop, PieceType.Bishop, legalMask, moves);
            }

            if (relativeBitBoard.MyQueens > 0)
            {
                var remainingQueens = relativeBitBoard.MyQueens & remainingSquares;

                foreach (var pieceSquare in remainingQueens.ToList())
                    AddSingleQueenMoves(relativeBitBoard, pieceSquare.ToSquare(), legalMask, moves);
            }
        }

        private IList<SquareFlag> FindSafeSquares(RelativeBitBoard relativeBitBoard, IEnumerable<SquareFlag> attackableSquares, IList<SquareFlag> safeSquares)
        {
            //var safeSquares = (SquareFlag)0;
            safeSquares.Clear();

            var unsafeSquares = (SquareFlag)0;
 
            foreach (var attackableSquare in attackableSquares)
            {
                if (unsafeSquares.HasFlag(attackableSquare))
                    continue;

                var square = attackableSquare.ToSquare();

                var potentialCheckersKing = AttackBitmaps.KingAttacks[square.Index] & relativeBitBoard.OpponentKing;

                if (potentialCheckersKing > 0)
                    continue;

                var potentialCheckersPawn = relativeBitBoard.Colour == Colour.White
                    ? AttackBitmaps.PawnCapturesWhite[square.Index] & relativeBitBoard.OpponentPawns
                    : AttackBitmaps.PawnCapturesBlack[square.Index] & relativeBitBoard.OpponentPawns;

                if (potentialCheckersPawn > 0)
                    continue;

                var potentialCheckersKnight = AttackBitmaps.KnightAttacks[square.Index] & relativeBitBoard.OpponentKnights;

                if (potentialCheckersKnight > 0)
                    continue;

                var occupiedSquaresWithoutMyKing = relativeBitBoard.OccupiedSquares & ~relativeBitBoard.MyKing;

                var potentialCheckersRook = GetRayCheckers(relativeBitBoard, square, PieceType.Rook, PieceType.Rook, occupiedSquaresWithoutMyKing);

                if (potentialCheckersRook > 0)
                {
                    foreach (var potentialCheckerRook in potentialCheckersRook.ToList())
                        unsafeSquares |= AttackBitmaps.Paths[square.Index][potentialCheckerRook.ToSquareIndex()] & ~potentialCheckerRook;

                    continue;
                }

                var potentialCheckersBishop = GetRayCheckers(relativeBitBoard, square, PieceType.Bishop, PieceType.Bishop, occupiedSquaresWithoutMyKing);

                if (potentialCheckersBishop > 0)
                {
                    foreach (var potentialCheckerBishop in potentialCheckersBishop.ToList())
                        unsafeSquares |= AttackBitmaps.Paths[square.Index][potentialCheckerBishop.ToSquareIndex()] & ~potentialCheckerBishop;

                    continue;
                }

                var potentialCheckersQueenAsRook = GetRayCheckers(relativeBitBoard, square, PieceType.Rook, PieceType.Queen, occupiedSquaresWithoutMyKing);

                if (potentialCheckersQueenAsRook > 0)
                {
                    foreach (var potentialCheckerQueenAsRook in potentialCheckersQueenAsRook.ToList())
                        unsafeSquares |= AttackBitmaps.Paths[square.Index][potentialCheckerQueenAsRook.ToSquareIndex()] & ~potentialCheckerQueenAsRook;

                    continue;
                }

                var potentialCheckersQueenAsBishop = GetRayCheckers(relativeBitBoard, square, PieceType.Bishop, PieceType.Queen, occupiedSquaresWithoutMyKing);

                if (potentialCheckersQueenAsBishop > 0)
                {
                    foreach (var potentialCheckerQueenAsBishop in potentialCheckersQueenAsBishop.ToList())
                        unsafeSquares |= AttackBitmaps.Paths[square.Index][potentialCheckerQueenAsBishop.ToSquareIndex()] & ~potentialCheckerQueenAsBishop;

                    continue;
                }

                //safeSquares |= attackableSquare;
                safeSquares.Add(attackableSquare);
            }

            return safeSquares;
        }

        private SquareFlag GetRayCheckers(RelativeBitBoard relativeBitBoard, Square fromSquare, PieceType rayType, PieceType pieceType, SquareFlag occupiedSquaresWithoutMyKing)
        {
            var opponentSquares = pieceType == PieceType.Queen
                ? relativeBitBoard.OpponentQueens
                : pieceType == PieceType.Rook
                    ? relativeBitBoard.OpponentRooks
                    : relativeBitBoard.OpponentBishops;

            var attackableSquares = GetAttackableSquares(fromSquare, rayType, occupiedSquaresWithoutMyKing);

            return attackableSquares & opponentSquares;
        }

        private SquareFlag AddPinnedMoves(MoveGenerationWorkspace workspace, RelativeBitBoard relativeBitBoard, Square kingSquare,
            SquareFlag kingRayAttackSquares, SquareFlag pushMask, SquareFlag captureMask, IList<uint> moves)
        {
            var potentialPins = kingRayAttackSquares & relativeBitBoard.MySquares;

            var occupiedSquaresWithoutPotentialPins = relativeBitBoard.OccupiedSquares & ~potentialPins;

            var attackableSquaresBeyondPinsRook = GetAttackableSquares(kingSquare, PieceType.Rook, occupiedSquaresWithoutPotentialPins);
            var attackableSquaresBeyondPinsBishop = GetAttackableSquares(kingSquare, PieceType.Bishop, occupiedSquaresWithoutPotentialPins);
            var attackableSquaresBeyondPins = attackableSquaresBeyondPinsRook | attackableSquaresBeyondPinsBishop;

            var pinningRooks = (SquareFlag)0;
            var pinningBishops = (SquareFlag)0;
            var pinningQueensNonDiagonal = (SquareFlag)0;
            var pinningQueensDiagonal = (SquareFlag)0;

            var buffer1 = workspace.Buffer1;
            var buffer2 = workspace.Buffer2;

            if (Vector<ulong>.Count == 4)
            {
                buffer1[0] = (ulong)attackableSquaresBeyondPinsRook;
                buffer1[1] = (ulong)attackableSquaresBeyondPinsBishop;
                buffer1[2] = (ulong)attackableSquaresBeyondPinsRook;
                buffer1[3] = (ulong)attackableSquaresBeyondPinsBishop;

                buffer2[0] = (ulong)relativeBitBoard.OpponentRooks;
                buffer2[1] = (ulong)relativeBitBoard.OpponentBishops;
                buffer2[2] = (ulong)relativeBitBoard.OpponentQueens;
                buffer2[3] = (ulong)relativeBitBoard.OpponentQueens;

                var vector1 = new Vector<ulong>(buffer1);
                var vector2 = new Vector<ulong>(buffer2);

                var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                var anyGreaterThanZero = Vector.GreaterThanAny(vectorOut, new Vector<ulong>(0));

                if (!anyGreaterThanZero)
                    return 0;

                pinningRooks = (SquareFlag)vectorOut[0];
                pinningBishops = (SquareFlag)vectorOut[1];
                pinningQueensNonDiagonal = (SquareFlag)vectorOut[2];
                pinningQueensDiagonal = (SquareFlag)vectorOut[3];
            }
            else
            {
                pinningRooks = attackableSquaresBeyondPinsRook & relativeBitBoard.OpponentRooks;
                pinningBishops = attackableSquaresBeyondPinsBishop & relativeBitBoard.OpponentBishops;
                pinningQueensNonDiagonal = attackableSquaresBeyondPinsRook & relativeBitBoard.OpponentQueens;
                pinningQueensDiagonal = attackableSquaresBeyondPinsBishop & relativeBitBoard.OpponentQueens;
            }

            var pinnedPieces = (SquareFlag)0;

            if (pinningRooks > 0)
                pinnedPieces |= AddPinnedMovesInternal(workspace.BitBoard, relativeBitBoard, kingSquare, potentialPins, pinningRooks, false, pushMask, captureMask, moves);

            if (pinningBishops > 0)
                pinnedPieces |= AddPinnedMovesInternal(workspace.BitBoard, relativeBitBoard, kingSquare, potentialPins, pinningBishops, true, pushMask, captureMask, moves);

            if (pinningQueensNonDiagonal > 0)
                pinnedPieces |= AddPinnedMovesInternal(workspace.BitBoard, relativeBitBoard, kingSquare, potentialPins, pinningQueensNonDiagonal, false, pushMask, captureMask, moves);

            if (pinningQueensDiagonal > 0)
                pinnedPieces |= AddPinnedMovesInternal(workspace.BitBoard, relativeBitBoard, kingSquare, potentialPins, pinningQueensDiagonal, true, pushMask, captureMask, moves);

            return pinnedPieces;
        }

        private SquareFlag AddPinnedMovesInternal(BitBoard bitBoard, RelativeBitBoard relativeBitBoard, Square kingSquare, SquareFlag potentialPins,
            SquareFlag pinners, bool diagonal, SquareFlag pushMask, SquareFlag captureMask, IList<uint> moves)
        {
            var pinnedSquares = (SquareFlag)0;
            var pinnersAsList = pinners.ToList();

            foreach (var pinner in pinnersAsList)
            {
                var path = AttackBitmaps.Paths[kingSquare.Index][pinner.ToSquareIndex()];

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
                        AddSinglePawnPushes(relativeBitBoard, squarePinnedByThisPiece.ToSquare(), pushPath, moves);
                        break;
                    case PieceType.Pawn when diagonal:
                        AddSinglePawnCaptures(relativeBitBoard, squarePinnedByThisPiece.ToSquare(), pushPath, capturePath, moves);
                        break;
                    case PieceType.Rook:
                        AddSingleRayMoves(relativeBitBoard, squarePinnedByThisPiece.ToSquare(), PieceType.Rook, PieceType.Rook, pushPath | capturePath, moves);
                        break;
                    case PieceType.Bishop:
                        AddSingleRayMoves(relativeBitBoard, squarePinnedByThisPiece.ToSquare(), PieceType.Bishop, PieceType.Bishop, pushPath | capturePath, moves);
                        break;
                    case PieceType.Queen:
                        AddSingleQueenMoves(relativeBitBoard, squarePinnedByThisPiece.ToSquare(), pushPath | capturePath, moves);
                        break;
                }
            }

            return pinnedSquares;
        }
        
        private void AddSinglePawnPushes(RelativeBitBoard relativeBitBoard, Square fromSquare, SquareFlag pushMask, IList<uint> moves)
        {
            var toSquare = fromSquare.Flag
                .PawnForward(relativeBitBoard.Colour, 1)
                .ToSquare();

            if (relativeBitBoard.OccupiedSquares.HasFlag(toSquare.Flag))
                return;

            if (pushMask.HasFlag(toSquare.Flag))
            {
                if (relativeBitBoard.PromotionRank.HasFlag(toSquare.Flag))
                    AddPromotions(relativeBitBoard, fromSquare, toSquare, PieceType.None, moves);
                else
                    moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
            }

            // Possible that we can block check with double push
            if (relativeBitBoard.StartRank.HasFlag(fromSquare.Flag))
            {
                toSquare = fromSquare.Flag
                    .PawnForward(relativeBitBoard.Colour, 2)
                    .ToSquare();

                if (!pushMask.HasFlag(toSquare.Flag))
                    return;

                // Promotions not possible from start rank
                if (!relativeBitBoard.OccupiedSquares.HasFlag(toSquare.Flag))
                    moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
            }
        }

        private void AddSinglePawnCaptures(RelativeBitBoard relativeBitBoard, Square fromSquare, SquareFlag pushMask, SquareFlag captureMask, IList<uint> moves)
        {
            var captureSquares = relativeBitBoard.Colour == Colour.White
                ? AttackBitmaps.PawnCapturesWhite[fromSquare.Index].ToList()
                : AttackBitmaps.PawnCapturesBlack[fromSquare.Index].ToList();

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
                        // Slight duplication here but probably cleaner than passing in
                        var kingSquare = relativeBitBoard.MyKing.ToSquare();

                        var enPassantDiscoveredCheckRank = relativeBitBoard.EnPassantDiscoveredCheckRank;

                        // This should be super rare. Performing an en passant capture with our King on same rank.
                        if (enPassantDiscoveredCheckRank.HasFlag(kingSquare.Flag))
                        {
                            if ((enPassantDiscoveredCheckRank & relativeBitBoard.OpponentRooks) > 0
                                || (enPassantDiscoveredCheckRank & relativeBitBoard.OpponentQueens) > 0)
                            {
                                var occupancyMask = GetOccupancyMask(PieceType.Rook, kingSquare);

                                var occupancyBeforeCapture = relativeBitBoard.OccupiedSquares & occupancyMask;

                                var occupancyAfterCapture = occupancyBeforeCapture
                                    & ~enPassantCaptureSquare
                                    & ~fromSquare.Flag;

                                // Search for magic moves using just the occupancy of rank (the rest is not relevant)
                                var magicIndex = MagicIndexHelpers.GetMagicIndex(PieceType.Rook, kingSquare.Index, occupancyAfterCapture);

                                var kingRayAttacks = AttackBitmaps.RookAttacks[kingSquare.Index][magicIndex];

                                var kingRayAttacksOnRank = kingRayAttacks & enPassantDiscoveredCheckRank;

                                discoveredCheck = (kingRayAttacksOnRank & relativeBitBoard.OpponentRooks) > 0
                                    || (kingRayAttacksOnRank & relativeBitBoard.OpponentQueens) > 0;
                            }
                        }
                    }

                    if (relativeBitBoard.PromotionRank.HasFlag(toSquare))
                        AddPromotions(relativeBitBoard, fromSquare, toSquare.ToSquare(), capturePieceType, moves);
                    else if (!discoveredCheck)
                        moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare.ToSquare(), capturePieceType, moveType));
                }
            }
        }

        private void AddSingleKnightMoves(RelativeBitBoard relativeBitBoard, Square fromSquare, SquareFlag legalMask, IList<uint> moves)
        {
            var attackableSquaresIncludingSelfCaptures = AttackBitmaps.KnightAttacks[fromSquare.Index];
            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
            var legalAttackableSquares = attackableSquares & legalMask;

            ToOrdinaryMoves(relativeBitBoard, PieceType.Knight, fromSquare, legalAttackableSquares, moves);
        }

        private void AddSingleQueenMoves(RelativeBitBoard relativeBitBoard, Square fromSquare, SquareFlag legalMask, IList<uint> moves)
        {
            AddSingleRayMoves(relativeBitBoard, fromSquare, PieceType.Rook, PieceType.Queen, legalMask, moves);
            AddSingleRayMoves(relativeBitBoard, fromSquare, PieceType.Bishop, PieceType.Queen, legalMask, moves);
        }

        private void AddSingleRayMoves(RelativeBitBoard relativeBitBoard, Square fromSquare, PieceType rayType, PieceType pieceType, SquareFlag legalMask, IList<uint> moves)
        {
            var attackableSquaresIncludingSelfCaptures = GetAttackableSquares(fromSquare, rayType, relativeBitBoard.OccupiedSquares);
            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
            var legalAttackableSquares = attackableSquares & legalMask;

            ToOrdinaryMoves(relativeBitBoard, pieceType, fromSquare, legalAttackableSquares, moves);
        }

        private void AddCastles(RelativeBitBoard relativeBitBoard, IList<SquareFlag> workspaceSafeSquares, Square kingSquare, IList<uint> moves)
        {
            if (!relativeBitBoard.CanCastleKingSide && !relativeBitBoard.CanCastleQueenSide)
                return;

            // We lose castle rights if any piece moves so they MUST be in correct locations
            if (relativeBitBoard.CanCastleKingSide)
            {
                var kingSideRookIndex = relativeBitBoard.KingSideRookStartSquare.ToSquareIndex();

                var kingToRook = AttackBitmaps.Paths[kingSquare.Index][kingSideRookIndex];

                var squaresBetween = kingToRook & ~relativeBitBoard.KingStartSquare & ~relativeBitBoard.KingSideRookStartSquare;

                if ((squaresBetween & relativeBitBoard.OccupiedSquares) == 0)
                {
                    var stepSquares = relativeBitBoard.KingSideCastleStep1 | relativeBitBoard.KingSideCastleStep2;
                    var safeSquaresAsList = FindSafeSquares(relativeBitBoard, stepSquares.ToList(), workspaceSafeSquares);

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

                var kingToRook = AttackBitmaps.Paths[kingSquare.Index][queenSideRookIndex];

                var squaresBetween = kingToRook & ~relativeBitBoard.KingStartSquare & ~relativeBitBoard.QueenSideRookStartSquare;

                if ((squaresBetween & relativeBitBoard.OccupiedSquares) == 0)
                {
                    var stepSquares = relativeBitBoard.QueenSideCastleStep1 | relativeBitBoard.QueenSideCastleStep2;
                    var safeSquaresAsList = FindSafeSquares(relativeBitBoard, stepSquares.ToList(), workspaceSafeSquares);

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

        private void AddPromotions(RelativeBitBoard relativeBitBoard, Square fromSquare, Square toSquare, PieceType capturePieceType, IList<uint> moves)
        {
            moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionQueen));
            moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionRook));
            moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionKnight));
            moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionBishop));
        }

        private SquareFlag GetAttackableSquares(Square fromSquare, PieceType rayType, SquareFlag occupiedSquares)
        {
            var occupancyMask = GetOccupancyMask(rayType, fromSquare);

            var occupancyMasked = occupiedSquares & occupancyMask;

            var magicIndex = MagicIndexHelpers.GetMagicIndex(rayType, fromSquare.Index, occupancyMasked);

            var attackableSquaresIncludingSelfCaptures = rayType == PieceType.Rook
                ? AttackBitmaps.RookAttacks[fromSquare.Index][magicIndex]
                : AttackBitmaps.BishopAttacks[fromSquare.Index][magicIndex];

            return attackableSquaresIncludingSelfCaptures;
        }

        private SquareFlag GetOccupancyMask(PieceType pieceType, Square square) =>
            pieceType == PieceType.Rook
                    ? MagicNumbers.RookOccupancyMasks[square.Index]
                    : MagicNumbers.BishopOccupancyMasks[square.Index];

        private void ToOrdinaryMoves(RelativeBitBoard relativeBitBoard, PieceType pieceType, Square fromSquare, SquareFlag attackableSquares, IList<uint> moves)
        {
            var attackableSquaresAsList = attackableSquares.ToList();

            foreach (var toSquare in attackableSquaresAsList)
            {
                var capturePieceType = PieceType.None;

                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                    capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                moves.Add(MoveBuilder.Create(relativeBitBoard.Colour, pieceType, fromSquare, toSquare.ToSquare(), capturePieceType, MoveType.Ordinary));
            }
        }
    }
}
