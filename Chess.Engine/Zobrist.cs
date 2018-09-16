using Chess.Engine.Bit;
using Chess.Engine.Extensions;
using Chess.Engine.Models;
using Troschuetz.Random.Generators;

namespace Chess.Engine
{
    public class Zobrist
    {
        private enum PieceIndex
        {
            WhitePawn,
            WhiteRook,
            WhiteKnight,
            WhiteBishop,
            WhiteQueen,
            WhiteKing,
            BlackPawn,
            BlackRook,
            BlackKnight,
            BlackBishop,
            BlackQueen,
            BlackKing
        }

        //private ulong[] BaseValues = new ulong[]{0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000};
        private ulong[,] _squares = new ulong[64,12];
        private ulong[] _colours = new ulong[2];

        public void Init()
        {
            var gen = new StandardGenerator();

            for (var i = 0; i < 64; ++i)
            {
                for(var j = 0; j < 12; ++j)
                {
                    _squares[i, j] = Random64(gen);
                }
            }

            _colours[0] = Random64(gen);
            _colours[1] = Random64(gen);
        }

        public ulong Update(ulong hash, Move move)
        {
            //var from = new SquareState(move.StartPositionSquareFlag, new Piece(move.PieceColour, move.Type));

            //SquareState to;

            //if (move.CapturePieceType != PieceType.None)
            //    to = new SquareState(move.EndPositionSquareFlag, new Piece(move.PieceColour.Opposite(), move.CapturePieceType));
            //else
            //    to = new SquareState(move.EndPositionSquareFlag);

            // Remove the piece on source square
            hash ^= _squares[move.StartPositionSquareFlag.ToBoardIndex(), (int)ToPieceIndex(move.PieceColour, move.Type)];

            // Remove the piece on target square
            hash ^= _squares[move.EndPositionSquareFlag.ToBoardIndex(), (int)ToPieceIndex(move.PieceColour.Opposite(), move.CapturePieceType)];

            // Add the moving piece to target square
            hash ^= _squares[move.EndPositionSquareFlag.ToBoardIndex(), (int)ToPieceIndex(move.PieceColour, move.Type)];

            hash ^= _colours[0];
            hash ^= _colours[1];

            return hash;
        }

        public ulong Hash(BitBoard bitBoard, Colour colour)
        {
            var squares = (bitBoard.White | bitBoard.Black).ToList();

            ulong hash = 0ul;

            foreach (var square in squares)
            {
                var squareState = bitBoard.GetSquareState(square);

                hash ^= _squares[square.ToBoardIndex(), (int)Index(squareState)];
            }

            if (colour == Colour.White)
                hash ^= _colours[0];
            else
                hash ^= _colours[1];

            return hash;
        }

        private ulong Random64(StandardGenerator gen)
        {
            var int1 = (ulong)gen.NextInclusiveMaxValue();
            var int2 = (ulong)gen.NextInclusiveMaxValue();

            ulong result = int1 << 32;
            result += int2;

            return result;
        }

        private PieceIndex ToPieceIndex(Colour colour, PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn when colour == Colour.White:
                    return PieceIndex.WhitePawn;
                case PieceType.Rook when colour == Colour.White:
                    return PieceIndex.WhiteRook;
                case PieceType.Knight when colour == Colour.White:
                    return PieceIndex.WhiteKnight;
                case PieceType.Bishop when colour == Colour.White:
                    return PieceIndex.WhiteBishop;
                case PieceType.Queen when colour == Colour.White:
                    return PieceIndex.WhiteQueen;
                case PieceType.King when colour == Colour.White:
                    return PieceIndex.WhiteKing;
                case PieceType.Pawn when colour == Colour.Black:
                    return PieceIndex.BlackPawn;
                case PieceType.Rook when colour == Colour.Black:
                    return PieceIndex.BlackRook;
                case PieceType.Knight when colour == Colour.Black:
                    return PieceIndex.BlackKnight;
                case PieceType.Bishop when colour == Colour.Black:
                    return PieceIndex.BlackBishop;
                case PieceType.Queen when colour == Colour.Black:
                    return PieceIndex.BlackQueen;
                case PieceType.King when colour == Colour.Black:
                    return PieceIndex.BlackKing;
                default:
                    return 0;
            }
        }

        private PieceIndex Index(SquareState squareState)
        {
            switch (squareState.Type)
            {
                case PieceType.Pawn when squareState.Colour == Colour.White:
                    return PieceIndex.WhitePawn;
                case PieceType.Rook when squareState.Colour == Colour.White:
                    return PieceIndex.WhiteRook;
                case PieceType.Knight when squareState.Colour == Colour.White:
                    return PieceIndex.WhiteKnight;
                case PieceType.Bishop when squareState.Colour == Colour.White:
                    return PieceIndex.WhiteBishop;
                case PieceType.Queen when squareState.Colour == Colour.White:
                    return PieceIndex.WhiteQueen;
                case PieceType.King when squareState.Colour == Colour.White:
                    return PieceIndex.WhiteKing;
                case PieceType.Pawn when squareState.Colour == Colour.Black:
                    return PieceIndex.BlackPawn;
                case PieceType.Rook when squareState.Colour == Colour.Black:
                    return PieceIndex.BlackRook;
                case PieceType.Knight when squareState.Colour == Colour.Black:
                    return PieceIndex.BlackKnight;
                case PieceType.Bishop when squareState.Colour == Colour.Black:
                    return PieceIndex.BlackBishop;
                case PieceType.Queen when squareState.Colour == Colour.Black:
                    return PieceIndex.BlackQueen;
                case PieceType.King when squareState.Colour == Colour.Black:
                    return PieceIndex.BlackKing;
                default:
                    return 0;
            }
        }
    }
}
