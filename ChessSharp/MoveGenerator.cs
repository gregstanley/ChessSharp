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
        private SquareFlag[] PawnCapturesWhite = new SquareFlag[56];
        private SquareFlag[] PawnCapturesBlack = new SquareFlag[56];
        private SquareFlag[] KnightAttacks = new SquareFlag[64];
        private SquareFlag[] KingAttacks = new SquareFlag[64];
        private SquareFlag[][] RookAttacks = new SquareFlag[64][];
        private SquareFlag[][] BishopAttacks = new SquareFlag[64][];

        public MoveGenerator()
        {
            InitPawnCaptures();
            InitKnightAttacks();
            InitKingAttacks();
            InitRookAttacks();
            InitBishopAttacks();
        }

        public void Generate(BitBoard bitBoard)
        {
            
        }

        private void GetCheckers(BitBoard bitBoard, Colour colour)
        {

        }

        public SquareFlag GetKingMoves(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            var mySquares = bitBoard.FindPieceSquares(colour);
            var opponentSquares = bitBoard.FindPieceSquares(colour.Opposite());

            var kingSquare = bitBoard.FindKingSquare(colour).ToList().First();

            var kingSquareIndex = kingSquare.ToBoardIndex();

            var checkersRook = GetCheckers(bitBoard, colour, kingSquare, PieceType.Rook, PieceType.Rook);
            var checkersBishop = GetCheckers(bitBoard, colour, kingSquare, PieceType.Bishop, PieceType.Bishop);
            var checkersQueenAsRook = GetCheckers(bitBoard, colour, kingSquare, PieceType.Rook, PieceType.Queen);
            var checkersQueenAsBishop = GetCheckers(bitBoard, colour, kingSquare, PieceType.Bishop, PieceType.Queen);

            var attackableSquaresIncludingSelfCaptures = AttackGenerator.GeneratePotentialKingAttacks(kingSquareIndex);

            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~mySquares;

            var attackableSquaresAsList = attackableSquares.ToList();

            var safeSquares = (SquareFlag)0;

            foreach(var attackableSquare in attackableSquaresAsList)
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

            var safeSquaresAsList = safeSquares.ToList();

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

            return checkersRook | checkersBishop | checkersQueenAsRook | checkersQueenAsBishop;
        }

        private SquareFlag GetAttackableSquares(BitBoard bitBoard, Colour colour, SquareFlag square, PieceType rayType)
        {
            var mySquares = bitBoard.FindPieceSquares(colour);
            var opponentSquares = bitBoard.FindPieceSquares(colour.Opposite());

            var squareIndex = square.ToBoardIndex();

            var occupiedSquares = mySquares | opponentSquares;

            var occupancyMask = GetOccupancyMask(rayType, squareIndex);

            var occupancyMasked = occupiedSquares & occupancyMask;

            var magicIndex = GetMagicIndex(rayType, squareIndex, occupancyMasked);

            var attackableSquaresIncludingSelfCaptures = GetAttacks(rayType, squareIndex, magicIndex);

            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~mySquares;

            return attackableSquares;
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

        public void GetPawnMoves(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            var opponentSquares = bitBoard.FindPieceSquares(colour.Opposite());

            var pawnSquares = (ulong)bitBoard.FindPawnSquares(colour);

            var pawnSquaresAsList = ((SquareFlag)pawnSquares).ToList();

            foreach (var fromSquare in pawnSquaresAsList)
            {
                if (colour == Colour.White)
                {
                    var toSquare = (SquareFlag)((ulong)fromSquare << 8);

                    if (!opponentSquares.HasFlag(toSquare))
                    {
                        if (SquareFlagExtensions.r8.HasFlag(toSquare))
                            GetPromotions(bitBoard, colour, moves, fromSquare, toSquare, PieceType.None);
                        else
                            moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
                    }
                }
                else if (colour == Colour.Black)
                {
                    var toSquare = (SquareFlag)((ulong)fromSquare >> 8);

                    if (!opponentSquares.HasFlag(toSquare))
                    {
                        if (SquareFlagExtensions.r1.HasFlag(toSquare))
                            GetPromotions(bitBoard, colour, moves, fromSquare, toSquare, PieceType.None);
                        else
                            moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
                    }
                }
            }

            foreach (var fromSquare in pawnSquaresAsList)
            {
                var fromSquareIndex = fromSquare.ToBoardIndex();

                if (colour == Colour.White)
                {
                    var captureSquares = PawnCapturesWhite[fromSquareIndex].ToList();

                    foreach (var toSquare in captureSquares)
                    {
                        if (opponentSquares.HasFlag(toSquare))
                        {
                            var capturePieceType = bitBoard.GetPieceType(toSquare);

                            if (SquareFlagExtensions.r8.HasFlag(toSquare))
                                GetPromotions(bitBoard, colour, moves, fromSquare, toSquare, capturePieceType);
                            else
                                moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.Ordinary));
                        }
                    }
                }
                else if (colour == Colour.Black)
                {
                    var captureSquares = PawnCapturesBlack[fromSquareIndex].ToList();

                    foreach (var toSquare in captureSquares)
                    {
                        if (opponentSquares.HasFlag(toSquare))
                        {
                            var capturePieceType = bitBoard.GetPieceType(toSquare);

                            if (SquareFlagExtensions.r1.HasFlag(toSquare))
                                GetPromotions(bitBoard, colour, moves, fromSquare, toSquare, capturePieceType);
                            else
                                moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.Ordinary));
                        }
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

            var occupiedSquares = mySquares | opponentSquares;

            foreach (var square in squares)
            {
                var squareIndex = square.ToBoardIndex();

                var occupancyMask = GetOccupancyMask(rayType, squareIndex);

                var occupancyMasked = occupiedSquares & occupancyMask;

                var magicIndex = GetMagicIndex(rayType, squareIndex, occupancyMasked);

                var attackableSquaresIncludingSelfCaptures = GetAttacks(rayType, squareIndex, magicIndex);

                var attackableSquares = attackableSquaresIncludingSelfCaptures & ~mySquares;

                var attackableSquaresAsList = attackableSquares.ToList();

                foreach (var to in attackableSquaresAsList)
                {
                    if (opponentSquares.HasFlag(to))
                    {
                        var capturePieceType = bitBoard.GetPieceType(to);

                        moves.Add(MoveConstructor.CreateMove(colour, pieceType, square, to, capturePieceType, MoveType.Ordinary));
                    }
                    else
                    {
                        moves.Add(MoveConstructor.CreateMove(colour, pieceType, square, to, PieceType.None, MoveType.Ordinary));
                    }
                }
            }
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
