using ChessSharp.Enums;
using ChessSharp.Extensions;
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
        private SquareFlag[] PawnCapturesBlack = new SquareFlag[56];
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

        public void Generate(BitBoard bitBoard)
        {
            
        }

        public SquareFlag GetPinnedPieces(BitBoard bitBoard, Colour colour)
        {
            var mySquares = bitBoard.FindPieceSquares(colour);
            var opponentSquares = bitBoard.FindPieceSquares(colour.Opposite());

            var kingSquare = bitBoard.FindKingSquare(colour).ToList().First();

            var kingSquareIndex = kingSquare.ToBoardIndex();

            var occupiedSquares = mySquares | opponentSquares;

            //var attackableSquares = GetAttackableSquares(bitBoard, colour, kingSquare, PieceType.Queen);
            var attackableSquaresRook = GetAttackableSquares(kingSquare, PieceType.Rook, occupiedSquares);
            var attackableSquaresBishop = GetAttackableSquares(kingSquare, PieceType.Bishop, occupiedSquares);

            var attackableSquares = attackableSquaresRook | attackableSquaresBishop;

            var potentialPins = attackableSquares & mySquares;

            // TODO: We may have discovered Check at this point in which case we don't need pins - I think

            var occupancyAfterRemovingMyPieces = attackableSquares & ~potentialPins;

            var attackableSquaresBeyondPinsRook = GetAttackableSquares(kingSquare, PieceType.Rook, occupancyAfterRemovingMyPieces);
            var attackableSquaresBeyondPinsBishop = GetAttackableSquares(kingSquare, PieceType.Bishop, occupancyAfterRemovingMyPieces);

            var attackableSquaresBeyondPins = attackableSquaresBeyondPinsRook | attackableSquaresBeyondPinsBishop;

            var pinningQueens = attackableSquaresBeyondPins & bitBoard.FindQueenSquares(colour.Opposite());
            var pinningRooks = attackableSquaresBeyondPins & bitBoard.FindRookSquares(colour.Opposite());
            var pinningBishops = attackableSquaresBeyondPins & bitBoard.FindBishopSquares(colour.Opposite());

            var pinnedPieces = (SquareFlag)0;

            if (pinningQueens > 0)
            {
                var pinningQueensAsList = pinningQueens.ToList();

                foreach(var pinningQueen in pinningQueensAsList)
                {
                    var path = Paths[kingSquareIndex][pinningQueen.ToBoardIndex()];

                    var piecesPinnedByThisQueen = path & potentialPins;

                    if (piecesPinnedByThisQueen > 0)
                        pinnedPieces |= piecesPinnedByThisQueen;
                }
            }

            return pinnedPieces;
        }

        public SquareFlag GetKingMoves(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            var mySquares = bitBoard.FindPieceSquares(colour);
            var opponentSquares = bitBoard.FindPieceSquares(colour.Opposite());

            var kingSquare = bitBoard.FindKingSquare(colour).ToList().First();

            var kingSquareIndex = kingSquare.ToBoardIndex();

            var checkersPawn = GetPawnCheckers(bitBoard, colour, kingSquare);
            var checkersKnight = GetKnightCheckers(bitBoard, colour, kingSquare);
            var checkersRook = GetCheckers(bitBoard, colour, kingSquare, PieceType.Rook, PieceType.Rook);
            var checkersBishop = GetCheckers(bitBoard, colour, kingSquare, PieceType.Bishop, PieceType.Bishop);
            var checkersQueenAsRook = GetCheckers(bitBoard, colour, kingSquare, PieceType.Rook, PieceType.Queen);
            var checkersQueenAsBishop = GetCheckers(bitBoard, colour, kingSquare, PieceType.Bishop, PieceType.Queen);

            var checkers = checkersPawn | checkersKnight | checkersRook | checkersBishop | checkersQueenAsRook | checkersQueenAsBishop;

            var numCheckers = checkers.Count();

            https://peterellisjones.com/posts/generating-legal-chess-moves-efficiently/
            var captureMask = (SquareFlag)ulong.MaxValue;
            var pushMask = (SquareFlag)ulong.MaxValue;

            if (numCheckers == 1)
            {
                captureMask = checkers;

                var rayChecker = checkersRook | checkersBishop | checkersQueenAsRook | checkersQueenAsBishop;

                if (rayChecker > 0)
                {
                    var checkerSquareIndex = rayChecker.ToBoardIndex();

                    var pathFromCheckerToKing = Paths[checkerSquareIndex][kingSquareIndex];

                    pushMask = pathFromCheckerToKing & ~kingSquare & ~rayChecker;

                    //if ((checkersRook | checkersQueenAsRook) > 0)
                    //{
                    //    var checkerAsRookMagicIndex = GetMagicIndex(PieceType.Rook, checkerBoardIndex, kingSquare);
                    //    var kingAsRookMagicIndex = GetMagicIndex(PieceType.Rook, kingSquareIndex, rayChecker);

                    //    var checkerAsRook = GetAttacks(PieceType.Rook, checkerBoardIndex, checkerAsRookMagicIndex);
                    //    var kingAsRook = GetAttacks(PieceType.Rook, checkerBoardIndex, kingAsRookMagicIndex);

                    //    pushMask = checkerAsRook & kingAsRook;
                    //}
                    //else if((checkersBishop | checkersQueenAsBishop) > 0)
                    //{
                    //    var checkerAsBishopMagicIndex = GetMagicIndex(PieceType.Bishop, checkerBoardIndex, kingSquare);
                    //    var kingAsBishopMagicIndex = GetMagicIndex(PieceType.Bishop, kingSquareIndex, rayChecker);

                    //    var checkerAsBishop = GetAttacks(PieceType.Bishop, checkerBoardIndex, checkerAsBishopMagicIndex);
                    //    var kingAsBishop = GetAttacks(PieceType.Bishop, checkerBoardIndex, kingAsBishopMagicIndex);

                    //    pushMask = checkerAsBishop & kingAsBishop;
                    //}
                }
            }

            var attackableSquaresIncludingSelfCaptures = AttackGenerator.GeneratePotentialKingAttacks(kingSquareIndex);

            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~mySquares;

            var safeSquaresAsList = FastFilterOutCoveredSquares(bitBoard, colour, attackableSquares.ToList()).ToList();

            foreach (var to in safeSquaresAsList)
            {
                if (opponentSquares.HasFlag(to))
                {
                    var capturePieceType = bitBoard.GetPieceType(to);

                    moves.Add(MoveConstructor.CreateMove(colour, PieceType.King, kingSquare, to, capturePieceType, MoveType.Ordinary));
                }
                else
                {
                    moves.Add(MoveConstructor.CreateMove(colour, PieceType.King, kingSquare, to, PieceType.None, MoveType.Ordinary));
                }
            }

            return checkersPawn | checkersKnight | checkersRook | checkersBishop | checkersQueenAsRook | checkersQueenAsBishop;
        }

        public SquareFlag GetPawnCheckers(BitBoard bitBoard, Colour colour, SquareFlag square)
        {
            var squareIndex = square.ToBoardIndex();

            var opponentPawnSquares = bitBoard.FindPawnSquares(colour.Opposite());

            var attackableSquares = colour == Colour.White
                ? PawnCapturesWhite[squareIndex]
                : PawnCapturesBlack[squareIndex];

            return attackableSquares & opponentPawnSquares;
        }

        public SquareFlag GetKnightCheckers(BitBoard bitBoard, Colour colour, SquareFlag square)
        {
            var squareIndex = square.ToBoardIndex();

            var opponentKnightSquares = bitBoard.FindKnightSquares(colour.Opposite());

            var attackableSquares = KnightAttacks[squareIndex];

            return attackableSquares & opponentKnightSquares;
        }

        public SquareFlag GetCheckers(BitBoard bitBoard, Colour colour, SquareFlag square, PieceType rayType, PieceType pieceType)
        {
            var opponentSquares = pieceType == PieceType.Queen
                ? bitBoard.FindQueenSquares(colour.Opposite())
                : pieceType == PieceType.Rook
                    ? bitBoard.FindRookSquares(colour.Opposite())
                    : bitBoard.FindBishopSquares(colour.Opposite());

            var attackableSquares = GetAttackableSquares(bitBoard, colour, square, rayType);

            return attackableSquares & opponentSquares;
        }

        public bool IsCheckBishop(BitBoard bitBoard, Colour colour, SquareFlag square)
        {
            var opponentRookSquares = bitBoard.FindRookSquares(colour.Opposite());

            var attackableSquares = GetAttackableSquares(bitBoard, colour, square, PieceType.Rook);

            return (attackableSquares & opponentRookSquares) > 0 ? true : false;
        }

        public void GetWhitePawnPushes(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            var promotionRank = SquareFlagExtensions.r8;

            var opponentSquares = bitBoard.Black;

            var pawnSquares = (ulong)bitBoard.WhitePawns;

            var pawnSquaresAsList = ((SquareFlag)pawnSquares).ToList();

            foreach (var fromSquare in pawnSquaresAsList)
            {
                var toSquare = (SquareFlag)((ulong)fromSquare << 8);

                if (!opponentSquares.HasFlag(toSquare))
                {
                    if (promotionRank.HasFlag(toSquare))
                        GetPromotions(bitBoard, colour, moves, fromSquare, toSquare, PieceType.None);
                    else
                        moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
                }
            }
        }

        public void GetBlackPawnPushes(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            var promotionRank = SquareFlagExtensions.r1;

            var opponentSquares = bitBoard.White;

            var pawnSquares = (ulong)bitBoard.BlackPawns;

            var pawnSquaresAsList = ((SquareFlag)pawnSquares).ToList();

            foreach (var fromSquare in pawnSquaresAsList)
            {
                var toSquare = (SquareFlag)((ulong)fromSquare >> 8);

                if (!opponentSquares.HasFlag(toSquare))
                {
                    if (promotionRank.HasFlag(toSquare))
                        GetPromotions(bitBoard, colour, moves, fromSquare, toSquare, PieceType.None);
                    else
                        moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
                }
            }
        }

        public void GetWhitePawnCaptures(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            var promotionRank = SquareFlagExtensions.r8;

            var opponentSquares = bitBoard.Black;

            var pawnSquares = (ulong)bitBoard.WhitePawns;

            var pawnSquaresAsList = ((SquareFlag)pawnSquares).ToList();

            foreach (var fromSquare in pawnSquaresAsList)
            {
                var fromSquareIndex = fromSquare.ToBoardIndex();

                var captureSquares = PawnCapturesWhite[fromSquareIndex].ToList();

                foreach (var toSquare in captureSquares)
                {
                    var moveType = bitBoard.EnPassant.HasFlag(toSquare) ? MoveType.EnPassant : MoveType.Ordinary;

                    if (opponentSquares.HasFlag(toSquare) || moveType == MoveType.EnPassant)
                    {
                        var capturePieceType = bitBoard.GetPieceType(toSquare);

                        var discoveredCheck = false;

                        if (moveType == MoveType.EnPassant)
                        {
                            capturePieceType = PieceType.Pawn;

                            // Looking for DISCOVERED CHECK (not spotted by pinned pieces as there are 2 pawns in the way)
                            var kingSquare = bitBoard.FindKingSquare(colour);

                            var enPassantRank = SquareFlagExtensions.r6;

                            if (enPassantRank.HasFlag(kingSquare))
                            {
                                if ((enPassantRank & bitBoard.BlackRooks) > 0 || (enPassantRank & bitBoard.BlackQueens) > 0)
                                {
                                    var opponentPiecesOnRank = enPassantRank & opponentSquares;

                                    // Our King is on the en passant rank with opponent Ray pieces so could be exposed after capture
                                    var rankOccupancy = (enPassantRank & bitBoard.White) | (enPassantRank & bitBoard.Black);

                                    // We're White so the en passant square is NORTH of the capturable pawn - so look SOUTH
                                    var enPassantPiece = (ulong)bitBoard.EnPassant >> Math.Abs((int)MoveDirection.South);

                                    // Remove the two pawns from the board
                                    var rankOccupancyPostCapture = rankOccupancy & ~(SquareFlag)enPassantPiece & ~fromSquare;

                                    var kingSquareIndex = kingSquare.ToBoardIndex();

                                    // Search for magic moves using just the occupancy of rank (the rest is not relevant)
                                    var magicIndex = GetMagicIndex(PieceType.Rook, kingSquareIndex, rankOccupancyPostCapture);

                                    var kingRayAttacks = RookAttacks[kingSquareIndex][magicIndex];

                                    var kingRayAttacksOnRank = kingRayAttacks & enPassantRank;

                                    discoveredCheck = (kingRayAttacksOnRank & bitBoard.BlackRooks) > 0 || (kingRayAttacksOnRank & bitBoard.BlackQueens) > 0;
                                }
                            }
                        }

                        if (promotionRank.HasFlag(toSquare))
                            GetPromotions(bitBoard, colour, moves, fromSquare, toSquare, capturePieceType);
                        else if (!discoveredCheck)
                            moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, moveType));
                    }
                }
            }
        }

        public void GetBlackPawnCaptures(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            var promotionRank = SquareFlagExtensions.r1;

            var opponentSquares = bitBoard.White;

            var pawnSquares = (ulong)bitBoard.BlackPawns;

            var pawnSquaresAsList = ((SquareFlag)pawnSquares).ToList();

            foreach (var fromSquare in pawnSquaresAsList)
            {
                var fromSquareIndex = fromSquare.ToBoardIndex();

                var captureSquares = PawnCapturesBlack[fromSquareIndex].ToList();

                foreach (var toSquare in captureSquares)
                {
                    var moveType = bitBoard.EnPassant.HasFlag(toSquare) ? MoveType.EnPassant : MoveType.Ordinary;

                    if (opponentSquares.HasFlag(toSquare) || moveType == MoveType.EnPassant)
                    {
                        var capturePieceType = bitBoard.GetPieceType(toSquare);

                        var discoveredCheck = false;

                        if (moveType == MoveType.EnPassant)
                        {
                            capturePieceType = PieceType.Pawn;

                            // Looking for DISCOVERED CHECK (not spotted by pinned pieces as there are 2 pawns in the way)
                            var kingSquare = bitBoard.FindKingSquare(colour);

                            var enPassantRank = SquareFlagExtensions.r4;

                            if (enPassantRank.HasFlag(kingSquare))
                            {
                                if ((enPassantRank & bitBoard.WhiteRooks) > 0 || (enPassantRank & bitBoard.WhiteQueens) > 0)
                                {
                                    var opponentPiecesOnRank = enPassantRank & opponentSquares;

                                    // Our King is on the en passant rank with opponent Ray pieces so could be exposed after capture
                                    var rankOccupancy = (enPassantRank & bitBoard.White) | (enPassantRank & bitBoard.Black);

                                    // We're Black so the en passant square is SOUTH of the capturable pawn - so look NORTH
                                    var enPassantPiece = (ulong)bitBoard.EnPassant >> Math.Abs((int)MoveDirection.North);

                                    // Remove the two pawns from the board
                                    var rankOccupancyPostCapture = rankOccupancy & ~(SquareFlag)enPassantPiece & ~fromSquare;

                                    var kingSquareIndex = kingSquare.ToBoardIndex();

                                    // Search for magic moves using just the occupancy of rank (the rest is not relevant)
                                    var magicIndex = GetMagicIndex(PieceType.Rook, kingSquareIndex, rankOccupancyPostCapture);

                                    var kingRayAttacks = RookAttacks[kingSquareIndex][magicIndex];

                                    var kingRayAttacksOnRank = kingRayAttacks & enPassantRank;

                                    discoveredCheck = (kingRayAttacksOnRank & bitBoard.WhiteRooks) > 0 || (kingRayAttacksOnRank & bitBoard.WhiteQueens) > 0;
                                }
                            }
                        }

                        if (promotionRank.HasFlag(toSquare))
                            GetPromotions(bitBoard, colour, moves, fromSquare, toSquare, capturePieceType);
                        else if (!discoveredCheck)
                            moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, moveType));
                    }
                }
            }
        }

        private void GetPromotions(BitBoard bitBoard, Colour colour, IList<uint> moves, SquareFlag fromSquare, SquareFlag toSquare, PieceType capturePieceType)
        {
            moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionQueen));
            moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionRook));
            moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionKnight));
            moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionBishop));
        }

        public void GetRookMoves(BitBoard bitBoard, Colour colour, IList<uint> moves) =>
            GetRayMoves(bitBoard, colour, PieceType.Rook, PieceType.Rook, moves);

        public void GetBishopMoves(BitBoard bitBoard, Colour colour, IList<uint> moves) =>
            GetRayMoves(bitBoard, colour, PieceType.Bishop, PieceType.Bishop, moves);

        public void GetQueenMoves(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            GetRayMoves(bitBoard, colour, PieceType.Rook, PieceType.Queen, moves);
            GetRayMoves(bitBoard, colour, PieceType.Bishop, PieceType.Queen, moves);
        }

        public void GetRayMoves(BitBoard bitBoard, Colour colour, PieceType rayType, PieceType pieceType, IList<uint> moves)
        {
            var squares = pieceType == PieceType.Queen
                ? bitBoard.FindQueenSquares(colour).ToList()
                : pieceType == PieceType.Rook
                    ? bitBoard.FindRookSquares(colour).ToList()
                    : bitBoard.FindBishopSquares(colour).ToList();

            var mySquares = bitBoard.FindPieceSquares(colour);
            var opponentSquares = bitBoard.FindPieceSquares(colour.Opposite());

            //var occupiedSquares = mySquares | opponentSquares;

            foreach (var fromSquare in squares)
            {
                var attackableSquares = GetAttackableSquares(bitBoard, colour, fromSquare, rayType);//, occupiedSquares);
                //var squareIndex = fromSquare.ToBoardIndex();

                //var occupancyMask = GetOccupancyMask(rayType, squareIndex);

                //var occupancyMasked = occupiedSquares & occupancyMask;

                //var magicIndex = GetMagicIndex(rayType, squareIndex, occupancyMasked);

                //var attackableSquaresIncludingSelfCaptures = GetAttacks(rayType, squareIndex, magicIndex);

                //var attackableSquares = attackableSquaresIncludingSelfCaptures & ~mySquares;

                ToOrdinaryMoves(bitBoard, colour, moves, pieceType, fromSquare, attackableSquares);
                //var attackableSquaresAsList = attackableSquares.ToList();

                //foreach (var to in attackableSquaresAsList)
                //{
                //    if (opponentSquares.HasFlag(to))
                //    {
                //        var capturePieceType = bitBoard.GetPieceType(to);

                //        moves.Add(MoveConstructor.CreateMove(colour, pieceType, square, to, capturePieceType, MoveType.Ordinary));
                //    }
                //    else
                //    {
                //        moves.Add(MoveConstructor.CreateMove(colour, pieceType, square, to, PieceType.None, MoveType.Ordinary));
                //    }
                //}
            }
        }

        private SquareFlag GetAttackableSquares(BitBoard bitBoard, Colour colour, SquareFlag fromSquare, PieceType rayType)
        {
            var mySquares = bitBoard.FindPieceSquares(colour);
            var opponentSquares = bitBoard.FindPieceSquares(colour.Opposite());

            var occupiedSquares = mySquares | opponentSquares;

            return GetAttackableSquares(fromSquare, rayType, occupiedSquares);
            //var fromSquareIndex = fromSquare.ToBoardIndex();
            
            //var occupancyMask = GetOccupancyMask(rayType, fromSquareIndex);

            //var occupancyMasked = occupiedSquares & occupancyMask;

            //var magicIndex = GetMagicIndex(rayType, fromSquareIndex, occupancyMasked);

            //var attackableSquaresIncludingSelfCaptures = GetAttacks(rayType, fromSquareIndex, magicIndex);

            //var attackableSquares = attackableSquaresIncludingSelfCaptures & ~mySquares;

            //return attackableSquares;
        }

        private SquareFlag GetAttackableSquares(SquareFlag fromSquare, PieceType rayType, SquareFlag occupiedSquares)
        {
            var squareIndex = fromSquare.ToBoardIndex();

            var occupancyMask = GetOccupancyMask(rayType, squareIndex);

            var occupancyMasked = occupiedSquares & occupancyMask;

            var magicIndex = GetMagicIndex(rayType, squareIndex, occupancyMasked);

            var attackableSquaresIncludingSelfCaptures = GetAttacks(rayType, squareIndex, magicIndex);

            return attackableSquaresIncludingSelfCaptures;
        }

        private void ToOrdinaryMoves(BitBoard bitBoard, Colour colour, IList<uint> moves, PieceType pieceType, SquareFlag fromSquare, SquareFlag attackableSquares)
        {
            var opponentSquares = bitBoard.FindPieceSquares(colour.Opposite());

            var attackableSquaresAsList = attackableSquares.ToList();

            foreach (var toSquare in attackableSquaresAsList)
            {
                if (opponentSquares.HasFlag(toSquare))
                {
                    var capturePieceType = bitBoard.GetPieceType(toSquare);

                    moves.Add(MoveConstructor.CreateMove(colour, pieceType, fromSquare, toSquare, capturePieceType, MoveType.Ordinary));
                }
                else
                {
                    moves.Add(MoveConstructor.CreateMove(colour, pieceType, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
                }
            }
        }

        private SquareFlag FastFilterOutCoveredSquares(BitBoard bitBoard, Colour colour, IReadOnlyList<SquareFlag> attackableSquaresAsList)
        {
            var safeSquares = (SquareFlag)0;

            foreach (var attackableSquare in attackableSquaresAsList)
            {
                var potentialCheckersPawn = GetPawnCheckers(bitBoard, colour, attackableSquare);

                if (potentialCheckersPawn > 0)
                    continue;

                var potentialCheckersKnight = GetKnightCheckers(bitBoard, colour, attackableSquare);

                if (potentialCheckersKnight > 0)
                    continue;

                var potentialCheckersRook = GetCheckers(bitBoard, colour, attackableSquare, PieceType.Rook, PieceType.Rook);

                if (potentialCheckersRook > 0)
                    continue;

                var potentialCheckersBishop = GetCheckers(bitBoard, colour, attackableSquare, PieceType.Bishop, PieceType.Bishop);

                if (potentialCheckersBishop > 0)
                    continue;

                var potentialCheckersQueenAsRook = GetCheckers(bitBoard, colour, attackableSquare, PieceType.Rook, PieceType.Queen);

                if (potentialCheckersQueenAsRook > 0)
                    continue;

                var potentialCheckersQueenAsBishop = GetCheckers(bitBoard, colour, attackableSquare, PieceType.Bishop, PieceType.Queen);

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
            var pawnCapturesBlack = new SquareFlag[56];

            for (var squareIndex = 8; squareIndex < 56; ++squareIndex)
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
