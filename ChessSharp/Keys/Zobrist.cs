using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Helpers;
using ChessSharp.Models;
using Pcg;
using System.Linq;

namespace ChessSharp.Keys
{
    public class Zobrist
    {
        private readonly ulong[,] squares = new ulong[64, 12];

        private readonly ulong[] colours = new ulong[2];

        private readonly ulong[] castleRights = new ulong[4];

        private enum CastleRights
        {
            WhiteCanCastleKingSide,
            WhiteCanCastleQueenSide,
            BlackCanCastleKingSide,
            BlackCanCastleQueenSide
        }

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

        public void Init()
        {
            var x = 0;

            for (var i = 0; i < 64; ++i)
                for (var j = 0; j < 12; ++j)
                    squares[i, j] = ZobristRandomNumbers.Values[x++];

            colours[0] = 9031923140692246275;
            colours[1] = 2884421478730253157;

            castleRights[(int)CastleRights.WhiteCanCastleKingSide] = 24694895801582720;
            castleRights[(int)CastleRights.WhiteCanCastleQueenSide] = 1451876467495148394;
            castleRights[(int)CastleRights.BlackCanCastleKingSide] = 8474605413122637464;
            castleRights[(int)CastleRights.BlackCanCastleQueenSide] = 4046274221665667751;
        }

        public ulong Hash(IPieceMap board, Colour colour)
        {
            var squares = (board.White | board.Black).ToList();

            ulong hash = 0ul;

            foreach (var square in squares)
            {
                var piece = PieceMapHelpers.GetPiece(board, square);

                hash ^= this.squares[square.ToSquareIndex(), (int)Index(piece)];
            }

            if (colour == Colour.White)
                hash ^= colours[0];
            else
                hash ^= colours[1];

            return hash;
        }

        public ulong UpdateHash(ulong hash, uint move)
        {
            // We can just immediately flip the colour
            hash ^= colours[0];
            hash ^= colours[1];

            var colour = move.GetColour();
            var moveType = move.GetMoveType();

            if (moveType == MoveType.CastleKing && colour == Colour.White)
            {                
                var kingStartSquareIndex = SquareFlagConstants.WhiteKingStartSquare.ToSquareIndex();
                var rookStartSquareIndex = SquareFlagConstants.WhiteKingSideRookStartSquare.ToSquareIndex();
                var kingEndSquareIndex = SquareFlagConstants.WhiteKingSideCastleStep2.ToSquareIndex();
                var rookEndSquareIndex = SquareFlagConstants.WhiteKingSideCastleStep1.ToSquareIndex();

                // Add/Remove pieces from start positions
                hash ^= squares[kingStartSquareIndex, (int)PieceIndex.WhiteKing];
                hash ^= squares[rookStartSquareIndex, (int)PieceIndex.WhiteRook];

                // Add/Remove pieces from end positions
                hash ^= squares[kingEndSquareIndex, (int)PieceIndex.WhiteKing];
                hash ^= squares[rookEndSquareIndex, (int)PieceIndex.WhiteRook];

                return hash;
            }

            if (moveType == MoveType.CastleQueen && colour == Colour.White)
            {
                var kingStartSquareIndex = SquareFlagConstants.WhiteKingStartSquare.ToSquareIndex();
                var rookStartSquareIndex = SquareFlagConstants.WhiteQueenSideRookStartSquare.ToSquareIndex();
                var kingEndSquareIndex = SquareFlagConstants.WhiteQueenSideCastleStep2.ToSquareIndex();
                var rookEndSquareIndex = SquareFlagConstants.WhiteQueenSideCastleStep1.ToSquareIndex();

                // Add/Remove pieces from start positions
                hash ^= squares[kingStartSquareIndex, (int)PieceIndex.WhiteKing];
                hash ^= squares[rookStartSquareIndex, (int)PieceIndex.WhiteRook];

                // Add/Remove pieces from end positions
                hash ^= squares[kingEndSquareIndex, (int)PieceIndex.WhiteKing];
                hash ^= squares[rookEndSquareIndex, (int)PieceIndex.WhiteRook];

                return hash;
            }

            if (moveType == MoveType.CastleKing && colour == Colour.Black)
            {
                var kingStartSquareIndex = SquareFlagConstants.BlackKingStartSquare.ToSquareIndex();
                var rookStartSquareIndex = SquareFlagConstants.BlackKingSideRookStartSquare.ToSquareIndex();
                var kingEndSquareIndex = SquareFlagConstants.BlackKingSideCastleStep2.ToSquareIndex();
                var rookEndSquareIndex = SquareFlagConstants.BlackKingSideCastleStep1.ToSquareIndex();

                // Add/Remove pieces from start positions
                hash ^= squares[kingStartSquareIndex, (int)PieceIndex.BlackKing];
                hash ^= squares[rookStartSquareIndex, (int)PieceIndex.BlackRook];

                // Add/Remove pieces from end positions
                hash ^= squares[kingEndSquareIndex, (int)PieceIndex.BlackKing];
                hash ^= squares[rookEndSquareIndex, (int)PieceIndex.BlackRook];

                return hash;
            }

            if (moveType == MoveType.CastleQueen && colour == Colour.Black)
            {
                var kingStartSquareIndex = SquareFlagConstants.BlackKingStartSquare.ToSquareIndex();
                var rookStartSquareIndex = SquareFlagConstants.BlackQueenSideRookStartSquare.ToSquareIndex();
                var kingEndSquareIndex = SquareFlagConstants.BlackQueenSideCastleStep2.ToSquareIndex();
                var rookEndSquareIndex = SquareFlagConstants.BlackQueenSideCastleStep1.ToSquareIndex();

                // Add/Remove pieces from start positions
                hash ^= squares[kingStartSquareIndex, (int)PieceIndex.BlackKing];
                hash ^= squares[rookStartSquareIndex, (int)PieceIndex.BlackRook];

                // Add/Remove pieces from end positions
                hash ^= squares[kingEndSquareIndex, (int)PieceIndex.BlackKing];
                hash ^= squares[rookEndSquareIndex, (int)PieceIndex.BlackRook];

                return hash;
            }

            var fromSquareIndex = move.GetFrom().ToSquareIndex();
            var toSquareIndex = move.GetTo().ToSquareIndex();
            var pieceType = move.GetPieceType();
            var fromPieceIndex = (int)ToPieceIndex(colour, pieceType);
            var capturePieceIndex = (int)ToPieceIndex(colour.Opposite(), move.GetCapturePieceType());

            // Ahh En Passant. Need to adjust the capture square if this happens to be an En Passant capture.
            var captureSquareIndex = toSquareIndex;

            if (moveType == MoveType.EnPassant)
            {
                if (colour == Colour.White)
                    captureSquareIndex -= 8;
                else
                    captureSquareIndex += 8;
            }

            // And in the case of promotions the piece magically changes type so better do that.
            var toPieceIndex = fromPieceIndex;

            switch (moveType)
            {
                case MoveType.PromotionQueen: toPieceIndex = (int)ToPieceIndex(colour, PieceType.Queen);
                    break;
                case MoveType.PromotionRook: toPieceIndex = (int)ToPieceIndex(colour, PieceType.Rook);
                    break;
                case MoveType.PromotionBishop: toPieceIndex = (int)ToPieceIndex(colour, PieceType.Bishop);
                    break;
                case MoveType.PromotionKnight: toPieceIndex = (int)ToPieceIndex(colour, PieceType.Knight);
                    break;
                default: break;
            }

            // Add/Remove the moving piece to/from source square
            hash ^= squares[fromSquareIndex, fromPieceIndex];

            // Add/Remove a captured piece to/from target square
            if (move.GetCapturePieceType() != PieceType.None)
                hash ^= squares[captureSquareIndex, capturePieceIndex];

            // Add/Remove the moving piece to/from target square
            hash ^= squares[toSquareIndex, toPieceIndex];

            return hash;
        }

        private ulong Random64(PcgRandom gen)
        {
            var int1 = (ulong)gen.Next();
            var int2 = (ulong)gen.Next();

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

        private PieceIndex Index(Piece piece)
        {
            switch (piece.Type)
            {
                case PieceType.Pawn when piece.Colour == Colour.White:
                    return PieceIndex.WhitePawn;
                case PieceType.Rook when piece.Colour == Colour.White:
                    return PieceIndex.WhiteRook;
                case PieceType.Knight when piece.Colour == Colour.White:
                    return PieceIndex.WhiteKnight;
                case PieceType.Bishop when piece.Colour == Colour.White:
                    return PieceIndex.WhiteBishop;
                case PieceType.Queen when piece.Colour == Colour.White:
                    return PieceIndex.WhiteQueen;
                case PieceType.King when piece.Colour == Colour.White:
                    return PieceIndex.WhiteKing;
                case PieceType.Pawn when piece.Colour == Colour.Black:
                    return PieceIndex.BlackPawn;
                case PieceType.Rook when piece.Colour == Colour.Black:
                    return PieceIndex.BlackRook;
                case PieceType.Knight when piece.Colour == Colour.Black:
                    return PieceIndex.BlackKnight;
                case PieceType.Bishop when piece.Colour == Colour.Black:
                    return PieceIndex.BlackBishop;
                case PieceType.Queen when piece.Colour == Colour.Black:
                    return PieceIndex.BlackQueen;
                case PieceType.King when piece.Colour == Colour.Black:
                    return PieceIndex.BlackKing;
                default:
                    return 0;
            }
        }
    }
}
