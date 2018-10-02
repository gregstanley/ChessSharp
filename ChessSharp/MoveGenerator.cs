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
        private SquareFlag[] PawnCapturesWhite = new SquareFlag[56];
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

            var kingSquare = relativeBitBoard.MyKing;
            var kingSquareIndex = kingSquare.ToBoardIndex();

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

            var rayCheckers = kingRayAttackSquares & relativeBitBoard.OpponentRaySquares;
            var checkersPawn = GetPawnCheckers(relativeBitBoard, kingSquare);
            var checkersKnight = GetKnightCheckers(relativeBitBoard, kingSquare);

            var checkers = checkersPawn | checkersKnight | rayCheckers;

            var numCheckers = checkers.Count();

            // If in Check by move than one piece then only options are King moves
            if (numCheckers > 1)
                return;

            // https://peterellisjones.com/posts/generating-legal-chess-moves-efficiently/
            var captureMask = (SquareFlag)ulong.MaxValue;
            var pushMask = (SquareFlag)ulong.MaxValue;

            if (numCheckers == 1)
            {
                captureMask = checkers;

                var rayChecker = checkers & relativeBitBoard.OpponentRaySquares;

                if (rayChecker > 0)
                {
                    var checkerSquareIndex = rayChecker.ToBoardIndex();

                    var pathFromCheckerToKing = Paths[checkerSquareIndex][kingSquareIndex];

                    pushMask = pathFromCheckerToKing & ~kingSquare & ~rayChecker;
                }
                else
                {
                    pushMask = 0;
                }
            }

            // The pinned pieces will only be allowed to move along pin ray i.e. can't move to expose King
            var pinnedPieces = GetPinnedPieces2(relativeBitBoard, kingSquareIndex, kingRayAttackSquares);

            var legalMask = pushMask | captureMask;

            GetPawnPushes(relativeBitBoard, pushMask, moves);
            GetPawnCaptures(relativeBitBoard, captureMask, moves);
            GetKnightMoves(relativeBitBoard, legalMask, moves);
            GetRookMoves(relativeBitBoard, legalMask, moves);
            GetBishopMoves(relativeBitBoard, legalMask, moves);
            GetQueenMoves(relativeBitBoard, legalMask, moves);

            GetCastles(relativeBitBoard, legalMask, moves);
        }

        public SquareFlag GetPinnedPieces2(RelativeBitBoard relativeBitBoard, int kingSquareIndex, SquareFlag kingRayAttackSquares)
        {
            var potentialPins = kingRayAttackSquares & relativeBitBoard.MySquares;

            var opponentOccupancy = kingRayAttackSquares & relativeBitBoard.OpponentSquares;

            var attackableSquaresBeyondPinsRook = GetAttackableSquares(kingSquareIndex, PieceType.Rook, opponentOccupancy);
            var attackableSquaresBeyondPinsBishop = GetAttackableSquares(kingSquareIndex, PieceType.Bishop, opponentOccupancy);

            var attackableSquaresBeyondPins = attackableSquaresBeyondPinsRook | attackableSquaresBeyondPinsBishop;

            var pinningRooks = attackableSquaresBeyondPins & relativeBitBoard.OpponentRooks;
            var pinningBishops = attackableSquaresBeyondPins & relativeBitBoard.OpponentBishops;
            var pinningQueens = attackableSquaresBeyondPins & relativeBitBoard.OpponentQueens;

            var pinnedPieces = (SquareFlag)0;

            if (pinningRooks > 0)
                pinnedPieces |= AddPinned(kingSquareIndex, potentialPins, pinningRooks);

            if (pinningBishops > 0)
                pinnedPieces |= AddPinned(kingSquareIndex, potentialPins, pinningBishops);

            if (pinningQueens > 0)
                pinnedPieces |= AddPinned(kingSquareIndex, potentialPins, pinningQueens);

            return pinnedPieces;
        }

        public SquareFlag GetPinnedPieces(RelativeBitBoard relativeBitBoard)
        {
            var kingSquare = relativeBitBoard.MyKing;
            var kingSquareIndex = kingSquare.ToBoardIndex();

            var attackableSquaresRook = GetAttackableSquares(kingSquare, PieceType.Rook, relativeBitBoard.OccupiedSquares);
            var attackableSquaresBishop = GetAttackableSquares(kingSquare, PieceType.Bishop, relativeBitBoard.OccupiedSquares);

            var attackableSquares = attackableSquaresRook | attackableSquaresBishop;

            var potentialPins = attackableSquares & relativeBitBoard.MySquares;

            // TODO: We may have discovered Check at this point in which case we don't need pins - I think

            var occupancyAfterRemovingMyPieces = attackableSquares & ~potentialPins;

            var attackableSquaresBeyondPinsRook = GetAttackableSquares(kingSquare, PieceType.Rook, occupancyAfterRemovingMyPieces);
            var attackableSquaresBeyondPinsBishop = GetAttackableSquares(kingSquare, PieceType.Bishop, occupancyAfterRemovingMyPieces);

            var attackableSquaresBeyondPins = attackableSquaresBeyondPinsRook | attackableSquaresBeyondPinsBishop;

            var pinningRooks = attackableSquaresBeyondPins & relativeBitBoard.OpponentRooks;
            var pinningBishops = attackableSquaresBeyondPins & relativeBitBoard.OpponentBishops;
            var pinningQueens = attackableSquaresBeyondPins & relativeBitBoard.OpponentQueens;

            var pinnedPieces = (SquareFlag)0;

            if (pinningRooks > 0)
                pinnedPieces |= AddPinned(kingSquareIndex, potentialPins, pinningRooks);

            if (pinningBishops > 0)
                pinnedPieces |= AddPinned(kingSquareIndex, potentialPins, pinningBishops);

            if (pinningQueens > 0)
                pinnedPieces |= AddPinned(kingSquareIndex, potentialPins, pinningQueens);

            return pinnedPieces;
        }

        public SquareFlag GetKingMoves(RelativeBitBoard relativeBitBoard, IList<uint> moves)
        {
            var kingSquare = relativeBitBoard.MyKing;
            var kingSquareIndex = kingSquare.ToBoardIndex();

            var checkersPawn = GetPawnCheckers(relativeBitBoard, kingSquare);
            var checkersKnight = GetKnightCheckers(relativeBitBoard, kingSquare);
            var checkersRook = GetCheckers(relativeBitBoard, kingSquare, PieceType.Rook, PieceType.Rook);
            var checkersBishop = GetCheckers(relativeBitBoard, kingSquare, PieceType.Bishop, PieceType.Bishop);
            var checkersQueenAsRook = GetCheckers(relativeBitBoard, kingSquare, PieceType.Rook, PieceType.Queen);
            var checkersQueenAsBishop = GetCheckers(relativeBitBoard, kingSquare, PieceType.Bishop, PieceType.Queen);

            var checkers = checkersPawn | checkersKnight | checkersRook | checkersBishop | checkersQueenAsRook | checkersQueenAsBishop;

            var numCheckers = checkers.Count();

            // https://peterellisjones.com/posts/generating-legal-chess-moves-efficiently/
            var captureMask = (SquareFlag)ulong.MaxValue;
            var pushMask = (SquareFlag)ulong.MaxValue;

            if (numCheckers == 1)
            {
                captureMask = checkers;

                var rayChecker = checkersRook | checkersBishop | checkersQueenAsRook | checkersQueenAsBishop;

                if (rayChecker > 0)
                {
                    var checkerSquareIndex = rayChecker.ToBoardIndex();

                    var pathFromKingToChecker = Paths[kingSquareIndex][checkerSquareIndex];

                    pushMask = pathFromKingToChecker & ~kingSquare & ~rayChecker;
                }
            }

            var attackableSquaresIncludingSelfCaptures = AttackGenerator.GeneratePotentialKingAttacks(kingSquareIndex);

            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;//~mySquares;

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

            return checkersPawn | checkersKnight | checkersRook | checkersBishop | checkersQueenAsRook | checkersQueenAsBishop;
        }

        public SquareFlag GetPawnCheckers(RelativeBitBoard relativeBitBoard, SquareFlag square)
        {
            var squareIndex = square.ToBoardIndex();

            var attackableSquares = relativeBitBoard.Colour == Colour.White
                ? PawnCapturesWhite[squareIndex]
                : PawnCapturesBlack[squareIndex];

            return attackableSquares & relativeBitBoard.OpponentPawns;
        }

        public SquareFlag GetKnightCheckers(RelativeBitBoard relativeBitBoard, SquareFlag square)
        {
            var squareIndex = square.ToBoardIndex();

            var attackableSquares = KnightAttacks[squareIndex];

            return attackableSquares & relativeBitBoard.OpponentKnights;
        }

        public SquareFlag GetCheckers(RelativeBitBoard relativeBitBoard, SquareFlag square, PieceType rayType, PieceType pieceType)
        {
            var opponentSquares = pieceType == PieceType.Queen
                ? relativeBitBoard.OpponentQueens
                : pieceType == PieceType.Rook
                    ? relativeBitBoard.OpponentRooks
                    : relativeBitBoard.OpponentBishops;

            //var attackableSquares = GetAttackableSquares(relativeBitBoard, square, rayType);
            var attackableSquaresWithoutKing = relativeBitBoard.OccupiedSquares & ~relativeBitBoard.MyKing;

            var attackableSquares = GetAttackableSquares(square, rayType, attackableSquaresWithoutKing);

            return attackableSquares & opponentSquares;
        }

        public bool IsCheckBishop(RelativeBitBoard relativeBitBoard, Colour colour, SquareFlag square)
        {
            var attackableSquares = GetAttackableSquares(relativeBitBoard, square, PieceType.Rook);

            return (attackableSquares & relativeBitBoard.OpponentRooks) > 0 ? true : false;
        }

        public void GetPawnPushes(RelativeBitBoard relativeBitBoard, IList<uint> moves) =>
            GetPawnPushes(relativeBitBoard, (SquareFlag)ulong.MaxValue, moves);

        public void GetPawnPushes(RelativeBitBoard relativeBitBoard, SquareFlag pushMask, IList<uint> moves)
        {
            var pawnSquaresAsList = relativeBitBoard.MyPawns.ToList();

            foreach (var fromSquare in pawnSquaresAsList)
            {
                //var toSquare = PushPawnOneSquare(relativeBitBoard, fromSquare);
                var toSquare = fromSquare.PawnForward(relativeBitBoard.Colour, 1);

                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                    continue;

                if (!pushMask.HasFlag(toSquare))
                    continue;

                if (relativeBitBoard.PromotionRank.HasFlag(toSquare))
                    GetPromotions(relativeBitBoard, moves, fromSquare, toSquare, PieceType.None);
                else
                    moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));

                if (relativeBitBoard.StartRank.HasFlag(fromSquare))
                {
                    //toSquare = PushPawnOneSquare(relativeBitBoard, toSquare);
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

        public void GetPawnCaptures(RelativeBitBoard relativeBitBoard, IList<uint> moves) =>
            GetPawnCaptures(relativeBitBoard, (SquareFlag)ulong.MaxValue, moves);

        public void GetPawnCaptures(RelativeBitBoard relativeBitBoard, SquareFlag captureMask, IList<uint> moves)
        {
            var pawnSquaresAsList = relativeBitBoard.MyPawns.ToList();

            foreach (var fromSquare in pawnSquaresAsList)
            {
                var fromSquareIndex = fromSquare.ToBoardIndex();

                var captureSquares = relativeBitBoard.Colour == Colour.White 
                    ? PawnCapturesWhite[fromSquareIndex].ToList()
                    : PawnCapturesBlack[fromSquareIndex].ToList();

                foreach (var toSquare in captureSquares)
                {
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
                                    var enPassantCaptureSquare = PushPawnOneSquare(relativeBitBoard.OpponentColour, relativeBitBoard.EnPassant);
                                    
                                    // Remove the two pawns from the board
                                    var rankOccupancyPostCapture = rankOccupancy & ~enPassantCaptureSquare & ~fromSquare;

                                    var kingSquareIndex = kingSquare.ToBoardIndex();

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
                            GetPromotions(relativeBitBoard, moves, fromSquare, toSquare, capturePieceType);
                        else if (!discoveredCheck)
                            moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, moveType));
                    }
                }
            }
        }
        
        private void GetPromotions(RelativeBitBoard relativeBitBoard, IList<uint> moves, SquareFlag fromSquare, SquareFlag toSquare, PieceType capturePieceType)
        {
            moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionQueen));
            moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionRook));
            moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionKnight));
            moves.Add(MoveConstructor.CreateMove(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionBishop));
        }

        public void GetKnightMoves(RelativeBitBoard relativeBitBoard, SquareFlag legalMask, IList<uint> moves)
        {
            var knightSquares = relativeBitBoard.MyKnights.ToList();

            foreach(var fromSquare in knightSquares)
            {
                var squareIndex = fromSquare.ToBoardIndex();

                var attackableSquaresIncludingSelfCaptures = KnightAttacks[squareIndex];
                var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
                var legalAttackableSquares = attackableSquares & legalMask;

                ToOrdinaryMoves(relativeBitBoard, moves, PieceType.Knight, fromSquare, legalAttackableSquares);
            }
        }

        public void GetRookMoves(RelativeBitBoard relativeBitBoard, SquareFlag legalMask, IList<uint> moves) =>
            GetRayMoves(relativeBitBoard, PieceType.Rook, PieceType.Rook, legalMask, moves);

        public void GetBishopMoves(RelativeBitBoard relativeBitBoard, SquareFlag legalMask, IList<uint> moves) =>
            GetRayMoves(relativeBitBoard, PieceType.Bishop, PieceType.Bishop, legalMask, moves);

        public void GetQueenMoves(RelativeBitBoard relativeBitBoard, SquareFlag legalMask, IList<uint> moves)
        {
            GetRayMoves(relativeBitBoard, PieceType.Rook, PieceType.Queen, legalMask, moves);
            GetRayMoves(relativeBitBoard, PieceType.Bishop, PieceType.Queen, legalMask, moves);
        }

        public void GetRayMoves(RelativeBitBoard relativeBitBoard, PieceType rayType, PieceType pieceType, SquareFlag legalMask, IList<uint> moves)
        {
            var squares = pieceType == PieceType.Queen
                ? relativeBitBoard.MyQueens.ToList()
                : pieceType == PieceType.Rook
                    ? relativeBitBoard.MyRooks.ToList()
                    : relativeBitBoard.MyBishops.ToList();

            foreach (var fromSquare in squares)
            {
                var attackableSquaresIncludingSelfCaptures = GetAttackableSquares(relativeBitBoard, fromSquare, rayType);
                var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
                var legalAttackableSquares = attackableSquares & legalMask;

                ToOrdinaryMoves(relativeBitBoard, moves, pieceType, fromSquare, legalAttackableSquares);
            }
        }

        public void GetCastles(RelativeBitBoard relativeBitBoard, SquareFlag legalMask, IList<uint> moves)
        {
            if (!relativeBitBoard.CanCastleKingSide && !relativeBitBoard.CanCastleQueenSide)
                return;

            var kingSquareIndex = relativeBitBoard.KingStartSquare.ToBoardIndex();

            // We lose castle rights if any piece moves so they MUST be in correct locations
            if (relativeBitBoard.CanCastleKingSide)
            {
                var kingSideRookIndex = relativeBitBoard.KingSideRookStartSquare.ToBoardIndex();

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
                        moves.Add(MoveConstructor.CreateCastle(relativeBitBoard.Colour, relativeBitBoard.KingStartSquare, relativeBitBoard.KingSideCastleStep2, MoveType.CastleKing));
                    }
                }
            }

            if (relativeBitBoard.CanCastleQueenSide)
            {
                var queenSideRookIndex = relativeBitBoard.QueenSideRookStartSquare.ToBoardIndex();

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
                        moves.Add(MoveConstructor.CreateCastle(relativeBitBoard.Colour, relativeBitBoard.KingStartSquare, relativeBitBoard.QueenSideCastleStep2, MoveType.CastleQueen));
                    }
                }
            }
        }

        private SquareFlag PushPawnOneSquare(Colour colour, SquareFlag fromSquare) =>
           colour == Colour.White ? (SquareFlag)((ulong)fromSquare << 8) : (SquareFlag)((ulong)fromSquare >> 8);

        private SquareFlag PushPawnOneSquare(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare) =>
            relativeBitBoard.Colour == Colour.White ? (SquareFlag)((ulong)fromSquare << 8) : (SquareFlag)((ulong)fromSquare >> 8);

        private SquareFlag GetAttackableSquares(RelativeBitBoard relativeBitBoard, SquareFlag fromSquare, PieceType rayType) => 
            GetAttackableSquares(fromSquare, rayType, relativeBitBoard.OccupiedSquares);

        private SquareFlag GetAttackableSquares(SquareFlag fromSquare, PieceType rayType, SquareFlag occupiedSquares) =>
            GetAttackableSquares(fromSquare.ToBoardIndex(), rayType, occupiedSquares);

        private SquareFlag GetAttackableSquares(int fromSquareIndex, PieceType rayType, SquareFlag occupiedSquares)
        {
            var occupancyMask = GetOccupancyMask(rayType, fromSquareIndex);

            var occupancyMasked = occupiedSquares & occupancyMask;

            var magicIndex = GetMagicIndex(rayType, fromSquareIndex, occupancyMasked);

            var attackableSquaresIncludingSelfCaptures = GetAttacks(rayType, fromSquareIndex, magicIndex);

            return attackableSquaresIncludingSelfCaptures;
        }

        private SquareFlag AddPinned(int kingSquareIndex, SquareFlag potentialPins, SquareFlag pinners)
        {
            var pinnedSquares = (SquareFlag)0;
            var pinnersAsList = pinners.ToList();

            foreach (var pinner in pinnersAsList)
            {
                var path = Paths[kingSquareIndex][pinner.ToBoardIndex()];

                var squaresPinnedByThisPiece = path & potentialPins;

                if (squaresPinnedByThisPiece > 0)
                    pinnedSquares |= squaresPinnedByThisPiece;
            }

            return pinnedSquares;
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
            var pawnCapturesWhite = new SquareFlag[56];
            var pawnCapturesBlack = new SquareFlag[64];

            for (var squareIndex = 0; squareIndex < 64; ++squareIndex)
            {
                if (squareIndex < 56)
                    pawnCapturesWhite[squareIndex] = AttackGenerator.GeneratePotentialWhitePawnCaptures(squareIndex);

                if (squareIndex > 7)
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
            for (var combinationLength = 0; combinationLength < numSquares; ++combinationLength)
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
