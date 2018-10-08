using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessSharp
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
        private SquareFlag[][] RookAttacks = new SquareFlag[64][];
        private SquareFlag[][] BishopAttacks = new SquareFlag[64][];

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
            var relativeBitBoard = bitBoard.ToRelative(colour);

            if (relativeBitBoard.MyKing == 0)
                return;

            var kingSquare = relativeBitBoard.MyKing;
            var kingSquareIndex = kingSquare.ToSquareIndex();

            // Start by looking for check and pins (which share a starting point of ray attacks from King)
            var kingRayAttackSquaresRook = GetAttackableSquares(kingSquareIndex, PieceType.Rook, relativeBitBoard.OccupiedSquares);
            var kingRayAttackSquaresBishop = GetAttackableSquares(kingSquareIndex, PieceType.Bishop, relativeBitBoard.OccupiedSquares);

            var kingRayAttackSquares = kingRayAttackSquaresRook | kingRayAttackSquaresBishop;

            // Start on King moves - if we're in Check then that might be all we need
            var attackableSquaresIncludingSelfCaptures = AttackGenerator.GeneratePotentialKingAttacks(kingSquareIndex);

            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;

            // TODO: Investigate performance here (should we just generate the attacked squares from opponent perspective?)
            var safeSquaresAsList = FastFilterOutCoveredSquares(relativeBitBoard, attackableSquares.ToList()).ToList();

            foreach (var to in safeSquaresAsList)
            {
                if (relativeBitBoard.OpponentSquares.HasFlag(to))
                {
                    var capturePieceType = relativeBitBoard.GetPieceType(to);

                    moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.King, kingSquare, to, capturePieceType, MoveType.Ordinary));
                }
                else
                {
                    moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.King, kingSquare, to, PieceType.None, MoveType.Ordinary));
                }
            }

            var checkersPawn = GetPawnCheckers(relativeBitBoard, kingSquare);
            var checkersKnight = GetKnightCheckers(relativeBitBoard, kingSquare);

            var checkerBishop = kingRayAttackSquaresBishop & relativeBitBoard.OpponentBishops;
            var checkersRook = kingRayAttackSquaresRook & relativeBitBoard.OpponentRooks;
            var checkersQueen = kingRayAttackSquares & relativeBitBoard.OpponentQueens;

            var checkers = checkersPawn | checkersKnight | checkerBishop | checkersRook | checkersQueen;

            var numCheckers = checkers.Count();

            // If in Check by more than one piece then only options are King moves - which we already have
            if (numCheckers > 1)
                return;

            // https://peterellisjones.com/posts/generating-legal-chess-moves-efficiently/
            // By default - whole board. If in Check it becomes limited to the Checker (no other captures count)
            var captureMask = (SquareFlag)ulong.MaxValue;

            // By default - whole board. If in Check by ray piece it becomes limited to popsitions between King and Checker
            var pushMask = (SquareFlag)ulong.MaxValue;

            if (numCheckers == 1)
            {
                captureMask = checkers;

                var rayChecker = checkers & relativeBitBoard.OpponentRaySquares;

                if (rayChecker > 0)
                {
                    var checkerSquareIndex = rayChecker.ToSquareIndex();

                    var pathFromCheckerToKing = Paths[checkerSquareIndex][kingSquareIndex];

                    // In Check by ray piece so only non capture moves must end on this line
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
                AddCastles(relativeBitBoard, moves);
            }

            // The pinned pieces will only be allowed to move along pin ray i.e. can't move to expose King
            var pinnedSquares = AddPinnedMoves(relativeBitBoard, kingSquareIndex, kingRayAttackSquares, moves);

            if (pinnedSquares > 0)
            {
                var pinnedPiecesAsList = pinnedSquares.ToList();
            }

            // Reset the legal mask
            var legalMask = pushMask | captureMask;

            AddPawnPushes(relativeBitBoard, ~pinnedSquares, pushMask, pinnedSquares, moves);
            AddPawnCaptures(relativeBitBoard, ~pinnedSquares, captureMask, pinnedSquares, moves);
            AddKnightMoves(relativeBitBoard, legalMask, moves);
            AddRookMoves(relativeBitBoard, ~pinnedSquares, legalMask, pinnedSquares, moves);
            AddBishopMoves(relativeBitBoard, ~pinnedSquares, legalMask, pinnedSquares, moves);
            AddQueenMoves(relativeBitBoard, ~pinnedSquares, legalMask, pinnedSquares, moves);
        }

        private SquareFlag GetPinned(RelativeBitBoard relativeBitBoard, int kingSquareIndex, SquareFlag potentialPins, SquareFlag pinners, IList<uint> moves)
        {
            var pinnedSquares = (SquareFlag)0;
            var pinnersAsList = pinners.ToList();

            foreach (var pinner in pinnersAsList)
            {
                var path = Paths[kingSquareIndex][pinner.ToSquareIndex()];

                var squarePinnedByThisPiece = path & potentialPins;

                if (squarePinnedByThisPiece > 0)
                {
                    pinnedSquares |= squarePinnedByThisPiece;

                    var pieceType = relativeBitBoard.GetPieceType(squarePinnedByThisPiece);

                    switch (pieceType)
                    {
                        case PieceType.Pawn:
                            // TODO: Can be more efficient here if we know if ray is diagonal or not
                            AddPawnPushes(relativeBitBoard, pinnedSquares, path, pinnedSquares, moves);
                            AddPawnCaptures(relativeBitBoard, pinnedSquares, path, pinnedSquares, moves);
                            break;
                        case PieceType.Rook:
                            AddRookMoves(relativeBitBoard, pinnedSquares, path, pinnedSquares, moves);
                            break;
                        case PieceType.Bishop:
                            AddBishopMoves(relativeBitBoard, pinnedSquares, path, pinnedSquares, moves);
                            break;
                        case PieceType.Queen:
                            AddQueenMoves(relativeBitBoard, pinnedSquares, path, pinnedSquares, moves);
                            break;
                    }
                }
            }

            return pinnedSquares;
        }

        private SquareFlag AddPinnedMoves(RelativeBitBoard relativeBitBoard, int kingSquareIndex, SquareFlag kingRayAttackSquares, IList<uint> moves)
        {
            var potentialPins = kingRayAttackSquares & relativeBitBoard.MySquares;

            var attackableSquaresBeyondPinsRook = GetAttackableSquares(kingSquareIndex, PieceType.Rook, relativeBitBoard.OpponentSquares);
            var attackableSquaresBeyondPinsBishop = GetAttackableSquares(kingSquareIndex, PieceType.Bishop, relativeBitBoard.OpponentSquares);

            var attackableSquaresBeyondPins = attackableSquaresBeyondPinsRook | attackableSquaresBeyondPinsBishop;

            var pinningRooks = attackableSquaresBeyondPins & relativeBitBoard.OpponentRooks;
            var pinningBishops = attackableSquaresBeyondPins & relativeBitBoard.OpponentBishops;
            var pinningQueens = attackableSquaresBeyondPins & relativeBitBoard.OpponentQueens;

            var pinnedPieces = (SquareFlag)0;

            if (pinningRooks > 0)
                pinnedPieces |= GetPinned(relativeBitBoard, kingSquareIndex, potentialPins, pinningRooks, moves);

            if (pinningBishops > 0)
                pinnedPieces |= GetPinned(relativeBitBoard, kingSquareIndex, potentialPins, pinningBishops, moves);

            if (pinningQueens > 0)
                pinnedPieces |= GetPinned(relativeBitBoard, kingSquareIndex, potentialPins, pinningQueens, moves);

            return pinnedPieces;
        }

        private SquareFlag GetPawnCheckers(RelativeBitBoard relativeBitBoard, SquareFlag square)
        {
            var squareIndex = square.ToSquareIndex();

            var attackableSquares = relativeBitBoard.Colour == Colour.White
                ? PawnCapturesWhite[squareIndex]
                : PawnCapturesBlack[squareIndex];

            return attackableSquares & relativeBitBoard.OpponentPawns;
        }

        private SquareFlag GetKnightCheckers(RelativeBitBoard relativeBitBoard, SquareFlag square)
        {
            var squareIndex = square.ToSquareIndex();

            var attackableSquares = KnightAttacks[squareIndex];

            return attackableSquares & relativeBitBoard.OpponentKnights;
        }

        private SquareFlag GetCheckers(RelativeBitBoard relativeBitBoard, SquareFlag square, PieceType rayType, PieceType pieceType)
        {
            var opponentSquares = pieceType == PieceType.Queen
                ? relativeBitBoard.OpponentQueens
                : pieceType == PieceType.Rook
                    ? relativeBitBoard.OpponentRooks
                    : relativeBitBoard.OpponentBishops;

            var attackableSquaresWithoutKing = relativeBitBoard.OccupiedSquares & ~relativeBitBoard.MyKing;

            var attackableSquares = GetAttackableSquares(square, rayType, attackableSquaresWithoutKing);

            return attackableSquares & opponentSquares;
        }

        public void AddPawnPushes(RelativeBitBoard relativeBitBoard, SquareFlag squareFilter, SquareFlag pushMask, SquareFlag pinnedSquares, IList<uint> moves)
        {
            var squares = relativeBitBoard.MyPawns & squareFilter;

            var pawnSquaresAsList = squares.ToList();

            foreach (var fromSquare in pawnSquaresAsList)
            {
                //if (fromSquare == SquareFlag.F4)
                //{ var bp = true; }

                var toSquare = fromSquare.PawnForward(relativeBitBoard.Colour, 1);

                if (pinnedSquares.HasFlag(fromSquare))
                {
                    if (!pushMask.HasFlag(toSquare))
                        continue;
                }

                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                    continue;

                if (!pushMask.HasFlag(toSquare))
                    continue;

                if (relativeBitBoard.PromotionRank.HasFlag(toSquare))
                    AddPromotions(relativeBitBoard, moves, fromSquare, toSquare, PieceType.None);
                else
                    moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));

                if (relativeBitBoard.StartRank.HasFlag(fromSquare))
                {
                    toSquare = fromSquare.PawnForward(relativeBitBoard.Colour, 2);

                    if (!relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                    {
                        if (!pushMask.HasFlag(toSquare))
                            continue;

                        // Promotions not possible from start rank
                        moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
                    }
                }
            }
        }

        public void AddPawnCaptures(RelativeBitBoard relativeBitBoard, SquareFlag squareFilter, SquareFlag captureMask, SquareFlag pinnedSquares, IList<uint> moves)
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

                    if (!captureMask.HasFlag(toSquare))
                        continue;

                    var moveType = relativeBitBoard.EnPassant.HasFlag(toSquare) ? MoveType.EnPassant : MoveType.Ordinary;

                    if (relativeBitBoard.OpponentSquares.HasFlag(toSquare) || moveType == MoveType.EnPassant)
                    {
                        var capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                        var discoveredCheck = false;

                        if (moveType == MoveType.EnPassant)
                        {
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
                                    var opponentPiecesOnRank = enPassantDiscoveredCheckRank & relativeBitBoard.OpponentSquares;

                                    // Our King is on the en passant rank with opponent Ray pieces so could be exposed after capture
                                    var rankOccupancy = (enPassantDiscoveredCheckRank & relativeBitBoard.MySquares)
                                        | (enPassantDiscoveredCheckRank & relativeBitBoard.OpponentSquares);

                                    // Abuse the push system - push an imaginary pawn from the en passant square as opponent to find piece
                                    var enPassantCaptureSquare = relativeBitBoard.EnPassant.PawnForward(relativeBitBoard.OpponentColour, 1);

                                    // Remove the two pawns from the board
                                    var rankOccupancyPostCapture = rankOccupancy & ~enPassantCaptureSquare & ~fromSquare;

                                    var kingSquareIndex = kingSquare.ToSquareIndex();

                                    // Search for magic moves using just the occupancy of rank (the rest is not relevant)
                                    var magicIndex = GetMagicIndex(PieceType.Rook, kingSquareIndex, rankOccupancyPostCapture);

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
        
        public void AddKnightMoves(RelativeBitBoard relativeBitBoard, SquareFlag legalMask, IList<uint> moves)
        {
            var knightSquares = relativeBitBoard.MyKnights.ToList();

            foreach(var fromSquare in knightSquares)
            {
                var squareIndex = fromSquare.ToSquareIndex();

                var attackableSquaresIncludingSelfCaptures = KnightAttacks[squareIndex];
                var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
                var legalAttackableSquares = attackableSquares & legalMask;

                ToOrdinaryMoves(relativeBitBoard, moves, PieceType.Knight, fromSquare, legalAttackableSquares);
            }
        }

        public void AddRookMoves(RelativeBitBoard relativeBitBoard, SquareFlag squareFilter, SquareFlag legalMask, SquareFlag pinnedSquares, IList<uint> moves) =>
            AddRayMoves(relativeBitBoard, squareFilter, PieceType.Rook, PieceType.Rook, legalMask, pinnedSquares, moves);

        public void AddBishopMoves(RelativeBitBoard relativeBitBoard, SquareFlag squareFilter, SquareFlag legalMask, SquareFlag pinnedSquares, IList<uint> moves) =>
            AddRayMoves(relativeBitBoard, squareFilter, PieceType.Bishop, PieceType.Bishop, legalMask, pinnedSquares, moves);

        public void AddQueenMoves(RelativeBitBoard relativeBitBoard, SquareFlag squareFilter, SquareFlag legalMask, SquareFlag pinnedSquares, IList<uint> moves)
        {
            AddRayMoves(relativeBitBoard, squareFilter, PieceType.Rook, PieceType.Queen, legalMask, pinnedSquares, moves);
            AddRayMoves(relativeBitBoard, squareFilter, PieceType.Bishop, PieceType.Queen, legalMask, pinnedSquares, moves);
        }

        public void AddRayMoves(RelativeBitBoard relativeBitBoard, SquareFlag squareFilter, PieceType rayType, PieceType pieceType, SquareFlag legalMask, SquareFlag pinnedSquares, IList<uint> moves)
        {
            var squares = pieceType == PieceType.Queen
                ? relativeBitBoard.MyQueens & squareFilter
                : pieceType == PieceType.Rook
                    ? relativeBitBoard.MyRooks & squareFilter
                    : relativeBitBoard.MyBishops & squareFilter;

            var squaresAsList = squares.ToList();

            foreach (var fromSquare in squaresAsList)
            {
                var attackableSquaresIncludingSelfCaptures = GetAttackableSquares(relativeBitBoard, fromSquare, rayType);
                var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
                var legalAttackableSquares = attackableSquares & legalMask;

                ToOrdinaryMoves(relativeBitBoard, moves, pieceType, fromSquare, legalAttackableSquares);
            }
        }

        public void AddCastles(RelativeBitBoard relativeBitBoard, IList<uint> moves)
        {
            if (!relativeBitBoard.CanCastleKingSide && !relativeBitBoard.CanCastleQueenSide)
                return;

            var kingSquareIndex = relativeBitBoard.KingStartSquare.ToSquareIndex();

            // We lose castle rights if any piece moves so they MUST be in correct locations
            if (relativeBitBoard.CanCastleKingSide)
            {
                var kingSideRookIndex = relativeBitBoard.KingSideRookStartSquare.ToSquareIndex();

                var kingToRook = Paths[kingSquareIndex][kingSideRookIndex];

                var squaresBetween = kingToRook & ~relativeBitBoard.KingStartSquare & ~relativeBitBoard.KingSideRookStartSquare;

                if ((squaresBetween & relativeBitBoard.OccupiedSquares) == 0)
                {
                    var stepsSquares = new List<SquareFlag>
                    {
                        relativeBitBoard.KingSideCastleStep1,
                        relativeBitBoard.KingSideCastleStep2
                    };

                    var safeSquares = FastFilterOutCoveredSquares(relativeBitBoard, stepsSquares);

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
                    var stepsSquares = new List<SquareFlag>
                    {
                        relativeBitBoard.QueenSideCastleStep1,
                        relativeBitBoard.QueenSideCastleStep2
                    };

                    var safeSquares = FastFilterOutCoveredSquares(relativeBitBoard, stepsSquares);

                    // On Queen side the King doesn't pass through B file so we don't look for Check there
                    var squaresBetweenMinusFirstRookStep = squaresBetween & ~relativeBitBoard.QueenSideRookStep1Square;

                    if(squaresBetweenMinusFirstRookStep == safeSquares)
                    {
                        moves.Add(MoveConstructor.CreateCastle(relativeBitBoard.Colour, MoveType.CastleQueen));
                    }
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

        private SquareFlag GetAttackableSquares(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, PieceType rayType) => 
            GetAttackableSquares(fromSquare, rayType, relativeBitBoard.OccupiedSquares);

        private SquareFlag GetAttackableSquares(SquareFlag fromSquare, PieceType rayType, SquareFlag occupiedSquares) =>
            GetAttackableSquares(fromSquare.ToSquareIndex(), rayType, occupiedSquares);

        private SquareFlag GetAttackableSquares(int fromSquareIndex, PieceType rayType, SquareFlag occupiedSquares)
        {
            var occupancyMask = GetOccupancyMask(rayType, fromSquareIndex);

            var occupancyMasked = occupiedSquares & occupancyMask;

            var magicIndex = GetMagicIndex(rayType, fromSquareIndex, occupancyMasked);

            var attackableSquaresIncludingSelfCaptures = GetAttacks(rayType, fromSquareIndex, magicIndex);

            return attackableSquaresIncludingSelfCaptures;
        }

        private void ToOrdinaryMoves(RelativeBitBoard relativeBitBoard, IList<uint> moves, PieceType pieceType, SquareFlag fromSquare, SquareFlag attackableSquares)
        {
            var attackableSquaresAsList = attackableSquares.ToList();

            foreach (var toSquare in attackableSquaresAsList)
            {
                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                {
                    var capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                    moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, pieceType, fromSquare, toSquare, capturePieceType, MoveType.Ordinary));
                }
                else
                {
                    moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, pieceType, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
                }
            }
        }

        private SquareFlag FastFilterOutCoveredSquares(RelativeBitBoard relativeBitBoard, IReadOnlyList<SquareFlag> attackableSquaresAsList)
        {
            var safeSquares = (SquareFlag)0;

            foreach (var attackableSquare in attackableSquaresAsList)
            {
                var potentialCheckersPawn = GetPawnCheckers(relativeBitBoard, attackableSquare);

                if (potentialCheckersPawn > 0)
                    continue;

                var potentialCheckersKnight = GetKnightCheckers(relativeBitBoard, attackableSquare);

                if (potentialCheckersKnight > 0)
                    continue;

                var potentialCheckersRook = GetCheckers(relativeBitBoard, attackableSquare, PieceType.Rook, PieceType.Rook);

                if (potentialCheckersRook > 0)
                    continue;

                var potentialCheckersBishop = GetCheckers(relativeBitBoard, attackableSquare, PieceType.Bishop, PieceType.Bishop);

                if (potentialCheckersBishop > 0)
                    continue;

                var potentialCheckersQueenAsRook = GetCheckers(relativeBitBoard, attackableSquare, PieceType.Rook, PieceType.Queen);

                if (potentialCheckersQueenAsRook > 0)
                    continue;

                var potentialCheckersQueenAsBishop = GetCheckers(relativeBitBoard, attackableSquare, PieceType.Bishop, PieceType.Queen);

                if (potentialCheckersQueenAsBishop > 0)
                    continue;

                safeSquares |= attackableSquare;
            }

            return safeSquares;
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

        private SquareFlag GetAttacks(PieceType pieceType, int squareIndex, int magicIndex) =>
            pieceType == PieceType.Rook 
                ? RookAttacks[squareIndex][magicIndex]
                : BishopAttacks[squareIndex][magicIndex];

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

            // You could potentially use a smaller range EXCEPT that Kings use this from where Pawns can't reach
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
            var squares = occupancyMask.ToList();
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
