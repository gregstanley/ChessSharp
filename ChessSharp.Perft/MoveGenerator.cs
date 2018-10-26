using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System;
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
        private SquareFlag[][] Paths = new SquareFlag[64][];
        private SquareFlag[] PawnCapturesWhite = new SquareFlag[64];
        private SquareFlag[] PawnCapturesBlack = new SquareFlag[64];
        private SquareFlag[] KnightAttacks = new SquareFlag[64];
        private SquareFlag[] KingAttacks = new SquareFlag[64];
        private readonly SquareFlag[][] RookAttacks = new SquareFlag[64][];
        private readonly SquareFlag[][] BishopAttacks = new SquareFlag[64][];

        public MoveGenerator()
        {
            InitPaths();
            InitPawnCaptures();
            InitKnightAttacks();
            InitKingAttacks();
            InitRookAttacks();
            InitBishopAttacks();
        }

        public void Generate(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            var relativeBitBoard = bitBoard.RelativeTo(colour);

            var kingSquare = relativeBitBoard.MyKing;
            var kingSquareIndex = kingSquare.ToSquareIndex();

            var checkersPawn = relativeBitBoard.Colour == Colour.White
                ? PawnCapturesWhite[kingSquareIndex] & relativeBitBoard.OpponentPawns
                : PawnCapturesBlack[kingSquareIndex] & relativeBitBoard.OpponentPawns;

            var kingRayAttackSquaresRook = GetAttackableSquares(kingSquareIndex, PieceType.Rook, relativeBitBoard.OccupiedSquares);
            var kingRayAttackSquaresBishop = GetAttackableSquares(kingSquareIndex, PieceType.Bishop, relativeBitBoard.OccupiedSquares);
            var kingRayAttackSquares = kingRayAttackSquaresRook | kingRayAttackSquaresBishop;

            var checkersKnight = (SquareFlag)0;
            var checkersRook = (SquareFlag)0;
            var checkersBishop = (SquareFlag)0;
            var checkersQueen = (SquareFlag)0;

            if (Vector<ulong>.Count == 4)
            {
                var buf1 = new ulong[] { (ulong)KnightAttacks[kingSquareIndex], (ulong)kingRayAttackSquaresRook,
                    (ulong)kingRayAttackSquaresBishop, (ulong)kingRayAttackSquares };

                var buf2 = new ulong[] { (ulong)relativeBitBoard.OpponentKnights, (ulong)relativeBitBoard.OpponentRooks,
                    (ulong)relativeBitBoard.OpponentBishops, (ulong)relativeBitBoard.OpponentQueens };

                var vec1 = new Vector<ulong>(buf1);
                var vec2 = new Vector<ulong>(buf2);

                var vecOut = Vector.BitwiseAnd(vec1, vec2);

                checkersKnight = (SquareFlag)vecOut[0];
                checkersRook = (SquareFlag)vecOut[1];
                checkersBishop = (SquareFlag)vecOut[2];
                checkersQueen = (SquareFlag)vecOut[3];
            }
            else
            {
                checkersKnight = KnightAttacks[kingSquareIndex] & relativeBitBoard.OpponentKnights;

                checkersRook = relativeBitBoard.OpponentRooks == 0 ? 0
                    : kingRayAttackSquaresRook & relativeBitBoard.OpponentRooks;

                checkersBishop = relativeBitBoard.OpponentBishops == 0 ? 0
                    : kingRayAttackSquaresBishop & relativeBitBoard.OpponentBishops;

                checkersQueen = relativeBitBoard.OpponentQueens == 0 ? 0
                    : kingRayAttackSquares & relativeBitBoard.OpponentQueens;
            }

            // Start on King moves - if we're in Check then that might be all we need
            var attackableSquaresIncludingSelfCaptures = KingAttacks[kingSquareIndex];

            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;

            var unsafeSquares = (SquareFlag)0;

            // As we have information on Checks use that to find dangerous squares
            if (checkersRook > 0)
                unsafeSquares |= Paths[kingSquareIndex][checkersRook.ToSquareIndex()] & ~checkersRook;

            if (checkersBishop > 0)
                unsafeSquares |= Paths[kingSquareIndex][checkersBishop.ToSquareIndex()] & ~checkersBishop;

            if (checkersQueen > 0)
                unsafeSquares |= Paths[kingSquareIndex][checkersQueen.ToSquareIndex()] & ~checkersQueen;

            var checkers = checkersPawn | checkersKnight | checkersBishop | checkersRook | checkersQueen;

            var semiSafeAttackableSquares = attackableSquares & ~unsafeSquares;

            var semiSafeAttackableSquaresAsList = semiSafeAttackableSquares.ToList();

            // TODO: Investigate performance here (should we just generate the attacked squares from opponent perspective?)
            var safeSquaresAsList = FastFilterOutCoveredSquares(relativeBitBoard, semiSafeAttackableSquaresAsList);

            foreach (var toSquare in safeSquaresAsList)
            {
                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                {
                    var capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                    moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.King, kingSquare, toSquare, capturePieceType, MoveType.Ordinary));
                }
                else
                {
                    moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.King, kingSquare, toSquare, PieceType.None, MoveType.Ordinary));
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

                    var pathFromCheckerToKing = Paths[checkerSquareIndex][kingSquareIndex];

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

            if (relativeBitBoard.MyKnights > 0)
            {
                foreach (var pieceSquare in relativeBitBoard.MyKnights.ToList())
                {
                    if (pinnedSquares.HasFlag(pieceSquare))
                        continue;

                    AddSingleKnightMoves(relativeBitBoard, pieceSquare, legalMask, moves);
                }
            }

            if (relativeBitBoard.MyPawns > 0)
            {
                foreach (var pieceSquare in relativeBitBoard.MyPawns.ToList())
                {
                    if (pinnedSquares.HasFlag(pieceSquare))
                        continue;

                    AddSinglePawnPushes(relativeBitBoard, pieceSquare, pushMask, moves);
                    AddSinglePawnCaptures(relativeBitBoard, pieceSquare, pushMask, captureMask, moves);
                }
            }

            if (relativeBitBoard.MyRooks > 0)
            {
                foreach (var pieceSquare in relativeBitBoard.MyRooks.ToList())
                {
                    if (pinnedSquares.HasFlag(pieceSquare))
                        continue;

                    AddSingleRookMoves(relativeBitBoard, pieceSquare, legalMask, moves);
                }
            }

            if (relativeBitBoard.MyBishops > 0)
            {
                foreach (var pieceSquare in relativeBitBoard.MyBishops.ToList())
                {
                    if (pinnedSquares.HasFlag(pieceSquare))
                        continue;
                    
                    AddSingleBishopMoves(relativeBitBoard, pieceSquare, legalMask, moves);
                }
            }

            if (relativeBitBoard.MyQueens > 0)
            {
                foreach (var pieceSquare in relativeBitBoard.MyQueens.ToList())
                {
                    if (pinnedSquares.HasFlag(pieceSquare))
                        continue;
                    
                    AddSingleQueenMoves(relativeBitBoard, pieceSquare, legalMask, moves);
                }
            }
        }

        private IList<SquareFlag> FastFilterOutCoveredSquares(RelativeBitBoard relativeBitBoard, IEnumerable<SquareFlag> attackableSquares)
        {
            //var safeSquares = (SquareFlag)0;
            var safeSquares = new List<SquareFlag>(32);
            var unsafeSquares = (SquareFlag)0;
 
            foreach (var attackableSquare in attackableSquares)
            {
                if (unsafeSquares.HasFlag(attackableSquare))
                    continue;

                var squareIndex = attackableSquare.ToSquareIndex();

                var potentialCheckersKing = KingAttacks[squareIndex] & relativeBitBoard.OpponentKing;

                if (potentialCheckersKing > 0)
                    continue;

                var potentialCheckersPawn = relativeBitBoard.Colour == Colour.White
                    ? PawnCapturesWhite[squareIndex] & relativeBitBoard.OpponentPawns
                    : PawnCapturesBlack[squareIndex] & relativeBitBoard.OpponentPawns;

                if (potentialCheckersPawn > 0)
                    continue;

                var potentialCheckersKnight = KnightAttacks[squareIndex] & relativeBitBoard.OpponentKnights;

                if (potentialCheckersKnight > 0)
                    continue;

                var potentialCheckersRook = GetRayCheckers(relativeBitBoard, squareIndex, PieceType.Rook, PieceType.Rook);

                if (potentialCheckersRook > 0)
                {
                    foreach (var potentialCheckerRook in potentialCheckersRook.ToList())
                        unsafeSquares |= Paths[squareIndex][potentialCheckerRook.ToSquareIndex()] & ~potentialCheckerRook;

                    continue;
                }

                var potentialCheckersBishop = GetRayCheckers(relativeBitBoard, squareIndex, PieceType.Bishop, PieceType.Bishop);

                if (potentialCheckersBishop > 0)
                {
                    foreach (var potentialCheckerBishop in potentialCheckersBishop.ToList())
                        unsafeSquares |= Paths[squareIndex][potentialCheckerBishop.ToSquareIndex()] & ~potentialCheckerBishop;

                    continue;
                }

                var potentialCheckersQueenAsRook = GetRayCheckers(relativeBitBoard, squareIndex, PieceType.Rook, PieceType.Queen);

                if (potentialCheckersQueenAsRook > 0)
                {
                    foreach (var potentialCheckerQueenAsRook in potentialCheckersQueenAsRook.ToList())
                        unsafeSquares |= Paths[squareIndex][potentialCheckerQueenAsRook.ToSquareIndex()] & ~potentialCheckerQueenAsRook;

                    continue;
                }

                var potentialCheckersQueenAsBishop = GetRayCheckers(relativeBitBoard, squareIndex, PieceType.Bishop, PieceType.Queen);

                if (potentialCheckersQueenAsBishop > 0)
                {
                    foreach (var potentialCheckerQueenAsBishop in potentialCheckersQueenAsBishop.ToList())
                        unsafeSquares |= Paths[squareIndex][potentialCheckerQueenAsBishop.ToSquareIndex()] & ~potentialCheckerQueenAsBishop;

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
                var path = Paths[kingSquareIndex][pinner.ToSquareIndex()];

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
                    AddPromotions(relativeBitBoard, moves, fromSquare, toSquare, PieceType.None);
                else
                    moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
            }

            // Possible that we can block check with double push
            if (relativeBitBoard.StartRank.HasFlag(fromSquare))
            {
                toSquare = fromSquare.PawnForward(relativeBitBoard.Colour, 2);

                if (!pushMask.HasFlag(toSquare))
                    return;

                // Promotions not possible from start rank
                if (!relativeBitBoard.OccupiedSquares.HasFlag(toSquare))
                    moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
            }
        }

        private void AddSinglePawnCaptures(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, SquareFlag pushMask, SquareFlag captureMask, IList<uint> moves)
        {
            var fromSquareIndex = fromSquare.ToSquareIndex();

            var captureSquares = relativeBitBoard.Colour == Colour.White
                ? PawnCapturesWhite[fromSquareIndex].ToList()
                : PawnCapturesBlack[fromSquareIndex].ToList();

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
                                var magicIndex = GetMagicIndex(PieceType.Rook, kingSquareIndex, occupancyAfterCapture);

                                var kingRayAttacks = RookAttacks[kingSquareIndex][magicIndex];

                                var kingRayAttacksOnRank = kingRayAttacks & enPassantDiscoveredCheckRank;

                                discoveredCheck = (kingRayAttacksOnRank & relativeBitBoard.OpponentRooks) > 0
                                    || (kingRayAttacksOnRank & relativeBitBoard.OpponentQueens) > 0;
                            }
                        }
                    }

                    if (relativeBitBoard.PromotionRank.HasFlag(toSquare))
                        AddPromotions(relativeBitBoard, moves, fromSquare, toSquare, capturePieceType);
                    else if (!discoveredCheck)
                        moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, moveType));
                }
            }
        }

        private void AddPawnPushes(RelativeBitBoard relativeBitBoard, SquareFlag squareFilter, SquareFlag pushMask, SquareFlag pinnedSquares, IList<uint> moves)
        {
            var squares = relativeBitBoard.MyPawns & squareFilter;

            var pawnSquaresAsList = squares.ToList();

            foreach (var fromSquare in pawnSquaresAsList)
            {
                var toSquare = fromSquare.PawnForward(relativeBitBoard.Colour, 1);

                if (pinnedSquares.HasFlag(fromSquare))
                {
                    if (!pushMask.HasFlag(toSquare))
                        continue;
                }

                if (relativeBitBoard.OccupiedSquares.HasFlag(toSquare))
                    continue;

                if (pushMask.HasFlag(toSquare))
                {
                    if (relativeBitBoard.PromotionRank.HasFlag(toSquare))
                        AddPromotions(relativeBitBoard, moves, fromSquare, toSquare, PieceType.None);
                    else
                        moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
                }

                // Possible that we can block check with double push
                if (relativeBitBoard.StartRank.HasFlag(fromSquare))
                {
                    toSquare = fromSquare.PawnForward(relativeBitBoard.Colour, 2);

                    if (!pushMask.HasFlag(toSquare))
                        continue;

                    // Promotions not possible from start rank
                    if (!relativeBitBoard.OccupiedSquares.HasFlag(toSquare))
                        moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
                }
            }
        }

        private void AddPawnCaptures(RelativeBitBoard relativeBitBoard, SquareFlag squareFilter, SquareFlag pushMask, SquareFlag captureMask, SquareFlag pinnedSquares, IList<uint> moves)
        {
            var squares = relativeBitBoard.MyPawns & squareFilter;

            var pawnSquaresAsList = squares.ToList();

            foreach (var fromSquare in pawnSquaresAsList)
            {
                var fromSquareIndex = fromSquare.ToSquareIndex();

                var captureSquares = relativeBitBoard.Colour == Colour.White 
                    ? PawnCapturesWhite[fromSquareIndex].ToList()
                    : PawnCapturesBlack[fromSquareIndex].ToList();

                foreach (var toSquare in captureSquares)
                {
                    if (pinnedSquares.HasFlag(fromSquare))
                    {
                        if (!captureMask.HasFlag(toSquare))
                            continue;
                    }

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
                                    var magicIndex = GetMagicIndex(PieceType.Rook, kingSquareIndex, occupancyAfterCapture);

                                    var kingRayAttacks = RookAttacks[kingSquareIndex][magicIndex];

                                    var kingRayAttacksOnRank = kingRayAttacks & enPassantDiscoveredCheckRank;

                                    discoveredCheck = (kingRayAttacksOnRank & relativeBitBoard.OpponentRooks) > 0
                                        || (kingRayAttacksOnRank & relativeBitBoard.OpponentQueens) > 0;
                                }
                            }
                        }

                        if (relativeBitBoard.PromotionRank.HasFlag(toSquare))
                            AddPromotions(relativeBitBoard, moves, fromSquare, toSquare, capturePieceType);
                        else if (!discoveredCheck)
                            moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, moveType));
                    }
                }
            }
        }

        private void AddSingleKnightMoves(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, SquareFlag legalMask, IList<uint> moves)
        {
            var squareIndex = fromSquare.ToSquareIndex();

            var attackableSquaresIncludingSelfCaptures = KnightAttacks[squareIndex];
            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
            var legalAttackableSquares = attackableSquares & legalMask;

            ToOrdinaryMoves(relativeBitBoard, moves, PieceType.Knight, fromSquare, legalAttackableSquares);
        }

        private void AddKnightMoves(RelativeBitBoard relativeBitBoard, SquareFlag legalMask, SquareFlag pinnedSquares, IList<uint> moves)
        {
            var knightSquares = relativeBitBoard.MyKnights.ToList();

            foreach(var fromSquare in knightSquares)
            {
                // Knights can't move at all if pinned
                if (pinnedSquares.HasFlag(fromSquare))
                    continue;

                var squareIndex = fromSquare.ToSquareIndex();

                var attackableSquaresIncludingSelfCaptures = KnightAttacks[squareIndex];
                var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
                var legalAttackableSquares = attackableSquares & legalMask;

                ToOrdinaryMoves(relativeBitBoard, moves, PieceType.Knight, fromSquare, legalAttackableSquares);
            }
        }

        private void AddSingleRookMoves(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, SquareFlag legalMask, IList<uint> moves) =>
            AddSingleRayMoves(relativeBitBoard, fromSquare, PieceType.Rook, PieceType.Rook, legalMask, moves);

        private void AddRookMoves(RelativeBitBoard relativeBitBoard, SquareFlag squareFilter, SquareFlag legalMask, SquareFlag pinnedSquares, IList<uint> moves) =>
            AddRayMoves(relativeBitBoard, squareFilter, PieceType.Rook, PieceType.Rook, legalMask, pinnedSquares, moves);

        private void AddSingleBishopMoves(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, SquareFlag legalMask, IList<uint> moves) =>
            AddSingleRayMoves(relativeBitBoard, fromSquare, PieceType.Bishop, PieceType.Bishop, legalMask, moves);

        private void AddBishopMoves(RelativeBitBoard relativeBitBoard, SquareFlag squareFilter, SquareFlag legalMask, SquareFlag pinnedSquares, IList<uint> moves) =>
            AddRayMoves(relativeBitBoard, squareFilter, PieceType.Bishop, PieceType.Bishop, legalMask, pinnedSquares, moves);

        private void AddSingleQueenMoves(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, SquareFlag legalMask, IList<uint> moves)
        {
            AddSingleRayMoves(relativeBitBoard, fromSquare, PieceType.Rook, PieceType.Queen, legalMask, moves);
            AddSingleRayMoves(relativeBitBoard, fromSquare, PieceType.Bishop, PieceType.Queen, legalMask, moves);
        }

        private void AddQueenMoves(RelativeBitBoard relativeBitBoard, SquareFlag squareFilter, SquareFlag legalMask, SquareFlag pinnedSquares, IList<uint> moves)
        {
            AddRayMoves(relativeBitBoard, squareFilter, PieceType.Rook, PieceType.Queen, legalMask, pinnedSquares, moves);
            AddRayMoves(relativeBitBoard, squareFilter, PieceType.Bishop, PieceType.Queen, legalMask, pinnedSquares, moves);
        }

        private void AddSingleRayMoves(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, PieceType rayType, PieceType pieceType, SquareFlag legalMask, IList<uint> moves)
        {
            var attackableSquaresIncludingSelfCaptures = GetAttackableSquares(fromSquare.ToSquareIndex(), rayType, relativeBitBoard.OccupiedSquares);
            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
            var legalAttackableSquares = attackableSquares & legalMask;

            ToOrdinaryMoves(relativeBitBoard, moves, pieceType, fromSquare, legalAttackableSquares);
        }

        private void AddRayMoves(RelativeBitBoard relativeBitBoard, SquareFlag squareFilter, PieceType rayType, PieceType pieceType, SquareFlag legalMask, SquareFlag pinnedSquares, IList<uint> moves)
        {
            var squares = pieceType == PieceType.Queen
                ? relativeBitBoard.MyQueens & squareFilter
                : pieceType == PieceType.Rook
                    ? relativeBitBoard.MyRooks & squareFilter
                    : relativeBitBoard.MyBishops & squareFilter;

            var squaresAsList = squares.ToList();

            foreach (var fromSquare in squaresAsList)
            {
                var attackableSquaresIncludingSelfCaptures = GetAttackableSquares(fromSquare.ToSquareIndex(), rayType, relativeBitBoard.OccupiedSquares);
                var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
                var legalAttackableSquares = attackableSquares & legalMask;

                ToOrdinaryMoves(relativeBitBoard, moves, pieceType, fromSquare, legalAttackableSquares);
            }
        }

        private void AddCastles(RelativeBitBoard relativeBitBoard, int kingSquareIndex, IList<uint> moves)
        {
            if (!relativeBitBoard.CanCastleKingSide && !relativeBitBoard.CanCastleQueenSide)
                return;

            // We lose castle rights if any piece moves so they MUST be in correct locations
            if (relativeBitBoard.CanCastleKingSide)
            {
                var kingSideRookIndex = relativeBitBoard.KingSideRookStartSquare.ToSquareIndex();

                var kingToRook = Paths[kingSquareIndex][kingSideRookIndex];

                var squaresBetween = kingToRook & ~relativeBitBoard.KingStartSquare & ~relativeBitBoard.KingSideRookStartSquare;

                if ((squaresBetween & relativeBitBoard.OccupiedSquares) == 0)
                {
                    var stepSquares = relativeBitBoard.KingSideCastleStep1 | relativeBitBoard.KingSideCastleStep2;
                    var safeSquaresAsList = FastFilterOutCoveredSquares(relativeBitBoard, stepSquares.ToList());

                    var safeSquares = (SquareFlag)0;

                    foreach (var safeSquare in safeSquaresAsList)
                        safeSquares |= safeSquare;

                    if (squaresBetween == safeSquares)
                    {
                        moves.Add(MoveConstructor.CreateCastle(relativeBitBoard.Colour, MoveType.CastleKing));
                    }
                }
            }

            if (relativeBitBoard.CanCastleQueenSide)
            {
                var queenSideRookIndex = relativeBitBoard.QueenSideRookStartSquare.ToSquareIndex();

                var kingToRook = Paths[kingSquareIndex][queenSideRookIndex];

                var squaresBetween = kingToRook & ~relativeBitBoard.KingStartSquare & ~relativeBitBoard.QueenSideRookStartSquare;

                if ((squaresBetween & relativeBitBoard.OccupiedSquares) == 0)
                {
                    var stepSquares = relativeBitBoard.QueenSideCastleStep1 | relativeBitBoard.QueenSideCastleStep2;
                    var safeSquaresAsList = FastFilterOutCoveredSquares(relativeBitBoard, stepSquares.ToList());

                    var safeSquares = (SquareFlag)0;

                    foreach (var safeSquare in safeSquaresAsList)
                        safeSquares |= safeSquare;

                    // On Queen side the King doesn't pass through B file so we don't look for Check there
                    var squaresBetweenMinusFirstRookStep = squaresBetween & ~relativeBitBoard.QueenSideRookStep1Square;

                    if (squaresBetweenMinusFirstRookStep == safeSquares)
                        moves.Add(MoveConstructor.CreateCastle(relativeBitBoard.Colour, MoveType.CastleQueen));
                }
            }
        }

        private void AddPromotions(RelativeBitBoard relativeBitBoard, IList<uint> moves, SquareFlag fromSquare, SquareFlag toSquare, PieceType capturePieceType)
        {
            moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionQueen));
            moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionRook));
            moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionKnight));
            moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionBishop));
        }

        private SquareFlag GetAttackableSquares(int fromSquareIndex, PieceType rayType, SquareFlag occupiedSquares)
        {
            var occupancyMask = GetOccupancyMask(rayType, fromSquareIndex);

            var occupancyMasked = occupiedSquares & occupancyMask;

            var magicIndex = GetMagicIndex(rayType, fromSquareIndex, occupancyMasked);

            var attackableSquaresIncludingSelfCaptures = rayType == PieceType.Rook
                ? RookAttacks[fromSquareIndex][magicIndex]
                : BishopAttacks[fromSquareIndex][magicIndex];

            return attackableSquaresIncludingSelfCaptures;
        }

        private SquareFlag GetOccupancyMask(PieceType pieceType, int squareIndex) =>
            pieceType == PieceType.Rook
                    ? MagicNumbers.RookOccupancyMasks[squareIndex]
                    : MagicNumbers.BishopOccupancyMasks[squareIndex];

        private int GetMagicIndex(PieceType pieceType, int squareIndex, SquareFlag occupancy)
        {
            if (occupancy == 0)
                return 0;

            return pieceType == PieceType.Rook
                    ? GetRookMagicIndex(squareIndex, occupancy)
                    : GetBishopMagicIndex(squareIndex, occupancy);
        }

        private int GetRookMagicIndex(int square, SquareFlag occupancy)
        {
            var index = ((ulong)occupancy * MagicNumbers.RookMagicNumbers[square]) >> 50;

            return (int)index;
        }

        private int GetBishopMagicIndex(int square, SquareFlag occupancy)
        {
            var index = ((ulong)occupancy * MagicNumbers.BishopMagicNumbers[square]) >> 50;

            return (int)index;
        }

        private void ToOrdinaryMoves(RelativeBitBoard relativeBitBoard, IList<uint> moves, PieceType pieceType, SquareFlag fromSquare, SquareFlag attackableSquares)
        {
            var attackableSquaresAsList = attackableSquares.ToList();

            foreach (var toSquare in attackableSquaresAsList)
            {
                var capturePieceType = PieceType.None;

                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                    capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, pieceType, fromSquare, toSquare, capturePieceType, MoveType.Ordinary));
            }
        }

        private void InitPaths()
        {
            var paths = new SquareFlag[64][];

            for (var squareIndex = 0; squareIndex < 64; ++squareIndex)
                paths[squareIndex] = AttackGenerator.GeneratePaths(squareIndex);

            Paths = paths;
        }

        private void InitPawnCaptures()
        {
            var pawnCapturesWhite = new SquareFlag[64];
            var pawnCapturesBlack = new SquareFlag[64];

            // You could potentially use a smaller range EXCEPT that Kings use this from ranks 1 and 8
            for (var squareIndex = 0; squareIndex < 64; ++squareIndex)
            {
                pawnCapturesWhite[squareIndex] = AttackGenerator.GeneratePotentialWhitePawnCaptures(squareIndex);
                pawnCapturesBlack[squareIndex] = AttackGenerator.GeneratePotentialBlackPawnCaptures(squareIndex);
            }

            PawnCapturesWhite = pawnCapturesWhite;
            PawnCapturesBlack = pawnCapturesBlack;
        }

        private void InitKnightAttacks()
        {
            var knightAttacks = new SquareFlag[64];

            for (var squareIndex = 0; squareIndex < 64; ++squareIndex)
                knightAttacks[squareIndex] = AttackGenerator.GeneratePotentialKnightAttacks(squareIndex);

            KnightAttacks = knightAttacks;
        }

        private void InitKingAttacks()
        {
            var kingAttacks = new SquareFlag[64];

            for (var squareIndex = 0; squareIndex < 64; ++squareIndex)
                kingAttacks[squareIndex] = AttackGenerator.GeneratePotentialKingAttacks(squareIndex);

            KingAttacks = kingAttacks;
        }

        private void InitRookAttacks()
        {
            for (var squareIndex = 0; squareIndex < 64; ++squareIndex)
            {
                var dictionary = new SortedDictionary<int, SquareFlag>();
                var occupancyMask = MagicNumbers.RookOccupancyMasks[squareIndex];

                // Fill a (sorted) dictionary with index and board key value pairs
                GenerateAllOccupancyCombinations(squareIndex, occupancyMask, AddRookAttack, dictionary);

                var highestIndex = dictionary.OrderByDescending(x => x.Key).First();

                RookAttacks[squareIndex] = new SquareFlag[highestIndex.Key + 1]; // 65536

                // Copy the sorted dictionary (binary search) to an array (empty space but fast to search)
                foreach (var magicMove in dictionary)
                    RookAttacks[squareIndex][magicMove.Key] = magicMove.Value;
            }
        }

        private void InitBishopAttacks()
        {
            for (var squareIndex = 0; squareIndex < 64; ++squareIndex)
            {
                var dictionary = new SortedDictionary<int, SquareFlag>();
                var occupancyMask = MagicNumbers.BishopOccupancyMasks[squareIndex];

                GenerateAllOccupancyCombinations(squareIndex, occupancyMask, AddBishopAttack, dictionary);

                var highestIndex = dictionary.OrderByDescending(x => x.Key).First();

                BishopAttacks[squareIndex] = new SquareFlag[highestIndex.Key + 1]; // 65536

                // Copy the sorted dictionary (binary search) to an array (empty space but fast to search)
                foreach (var magicMove in dictionary)
                    BishopAttacks[squareIndex][magicMove.Key] = magicMove.Value;
            }
        }

        private void GenerateAllOccupancyCombinations(int square, SquareFlag occupancyMask, Action<SortedDictionary<int, SquareFlag>, int, SquareFlag> addToList, SortedDictionary<int, SquareFlag> dictionary)
        {
            var squares = new List<SquareFlag>();

            //var squares = occupancyMask.ToList();
            foreach (var occupancyMaskSquare in occupancyMask.ToList())
                squares.Add(occupancyMaskSquare);
            
            var numSquares = squares.Count();

            // Generate each 'length' combination
            for (var combinationLength = 0; combinationLength <= numSquares; ++combinationLength)
                CombinationUtil(squares, square, new SquareFlag[numSquares], 0, numSquares - 1, 0, combinationLength, addToList, dictionary);
        }

        // https://www.geeksforgeeks.org/print-all-possible-combinations-of-r-elements-in-a-given-array-of-size-n/
        // The function will recursively generate all combinations of length 'combinationLength'. For example,
        // if there are 10 elements to sort then a combinationLenght of 5 will find all the combinations that 
        // use exactly 5 elements. Therefore, it must be called once 'end' times to find all combinations of 
        // any length
        private void CombinationUtil(IReadOnlyList<SquareFlag> squares, int rootSquare, SquareFlag[] combination,
            int start, int end, int index, int combinationLength, Action<SortedDictionary<int, SquareFlag>, int, SquareFlag> addToList, SortedDictionary<int, SquareFlag> dictionary)
        {
            if (index == combinationLength)
            {
                SquareFlag currentOccupancy = 0;

                foreach(var square in combination)
                    currentOccupancy |= square;

                addToList(dictionary, rootSquare, currentOccupancy);

                return;
            }

            // Replace index with all possible elements. The condition  "end-i+1 >= r-index" makes sure
            // that including one element at index will make a combination with remaining elements 
            // at remaining positions 
            for (int i = start; i <= end && end - i + 1 >= combinationLength - index; i++)
            {
                combination[index] = squares.ElementAt(i);

                CombinationUtil(squares, rootSquare, combination, i + 1, end, index + 1, combinationLength, addToList, dictionary);
            }
        }

        private void AddRookAttack(SortedDictionary<int, SquareFlag> dictionary, int squareIndex, SquareFlag currentOccupancy)
        {
            var magicIndex = GetRookMagicIndex(squareIndex, currentOccupancy);

            var attack = AttackGenerator.GeneratePotentialRookAttacks(squareIndex, currentOccupancy);

            var indexTaken = dictionary.ContainsKey(magicIndex) && dictionary[magicIndex] != attack;

            if (indexTaken)
                throw new Exception($"Magic Index {magicIndex} already in use by a different attack");

            dictionary[magicIndex] = attack;
        }

        private void AddBishopAttack(SortedDictionary<int, SquareFlag> dictionary, int squareIndex, SquareFlag currentOccupancy)
        {
            var magicIndex = GetBishopMagicIndex(squareIndex, currentOccupancy);

            var attack = AttackGenerator.GeneratePotentialBishopAttacks(squareIndex, currentOccupancy);

            var indexTaken = dictionary.ContainsKey(magicIndex) && dictionary[magicIndex] != attack;

            if (indexTaken)
                throw new Exception($"Magic Index {magicIndex} already in use by a different attack");

            dictionary[magicIndex] = attack;
        }
    }
}
