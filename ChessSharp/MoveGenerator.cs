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
        private SquareFlag[] KingAttacks = new SquareFlag[64];
        private SquareFlag[][] RookAttacks = new SquareFlag[64][];
        private SquareFlag[][] BishopAttacks = new SquareFlag[64][];

        public MoveGenerator()
        {
            InitKingAttacks();
            InitRookAttacks();
            InitBishopAttacks();
        }

        public void Generate(BitBoard bitBoard)
        {
            
        }

        public void GetKingMoves(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            var kingSquare = bitBoard.FindKingSquare(colour).ToList().First();


        }

        public void GetPawnMoves(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            var pawnSquares = bitBoard.FindPawnSquares(colour).ToList();
            
            foreach (var fromSquare in pawnSquares)
            {
                var toSquare = colour == Colour.White ? (ulong)fromSquare << 8 : (ulong)fromSquare >> 8;
                moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, fromSquare, (SquareFlag)toSquare, PieceType.None, MoveType.Ordinary));
            }
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

        private void InitKingAttacks()
        {
            var kingAttacks = new SquareFlag[64];

            for (var square = 0; square < 64; ++square)
            {
                kingAttacks[square] = MagicAttackGenerator.GenerateKingAttack(square);
            }

            KingAttacks = kingAttacks;
        }

        private void InitRookAttacks()
        {
            for (var square = 0; square < 64; ++square)
            {
                var dictionary = new SortedDictionary<int, SquareFlag>();
                var occupancyMask = MagicNumbers.RookOccupancyMasks[square];

                // Fill a (sorted) dictionary with index and board key value pairs
                GenerateAllOccupancyCombinations(square, occupancyMask, AddRookAttack, dictionary);

                var highestIndex = dictionary.OrderByDescending(x => x.Key).First();

                RookAttacks[square] = new SquareFlag[highestIndex.Key + 1]; // 65536

                // Copy the sorted dictionary (binary search) to an array (empty space but fast to search)
                foreach (var magicMove in dictionary)
                    RookAttacks[square][magicMove.Key] = magicMove.Value;
            }
        }

        private void InitBishopAttacks()
        {
            for (var square = 0; square < 64; ++square)
            {
                var dictionary = new SortedDictionary<int, SquareFlag>();
                var occupancyMask = MagicNumbers.BishopOccupancyMasks[square];

                GenerateAllOccupancyCombinations(square, occupancyMask, AddBishopAttack, dictionary);

                var highestIndex = dictionary.OrderByDescending(x => x.Key).First();

                BishopAttacks[square] = new SquareFlag[highestIndex.Key + 1]; // 65536

                // Copy the sorted dictionary (binary search) to an array (empty space but fast to search)
                foreach (var magicMove in dictionary)
                    BishopAttacks[square][magicMove.Key] = magicMove.Value;
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

        private void AddRookAttack(SortedDictionary<int, SquareFlag> dictionary, int square, SquareFlag currentOccupancy)
        {
            var index = GetRookMagicIndex(square, currentOccupancy);

            var attack = MagicAttackGenerator.GenerateRookAttack(square, currentOccupancy);

            if (dictionary.ContainsKey(index) && dictionary[index] != attack)
            {
                var bp = true;
            }
            else
            {
                dictionary[index] = attack;
            }
        }

        private void AddBishopAttack(SortedDictionary<int, SquareFlag> dictionary, int square, SquareFlag currentOccupancy)
        {
            var index = GetBishopMagicIndex(square, currentOccupancy);

            var attack = MagicAttackGenerator.GenerateBishopAttack(square, currentOccupancy);

            if (dictionary.ContainsKey(index) && dictionary[index] != attack)
            {
                var bp = true;
            }
            else
            {
                dictionary[index] = attack;
            }
        }
    }
}
