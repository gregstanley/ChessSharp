using ChessSharp.Enums;
using ChessSharp.Models;

namespace ChessSharp
{
    public class GameState : IPieceMap
    {
        public GameState(
            int ply,
            Colour toPlay,
            int halfMoveClock,
            int fullTurn,
            bool whiteCanCastleKingSide,
            bool whiteCanCastleQueenSide,
            bool blackCanCastleKingSide,
            bool blackCanCastleQueenSide,
            SquareFlag whitePawns,
            SquareFlag whiteRooks,
            SquareFlag whiteKnights,
            SquareFlag whiteBishops,
            SquareFlag whiteQueens,
            SquareFlag whiteKing,
            SquareFlag blackPawns,
            SquareFlag blackRooks,
            SquareFlag blackKnights,
            SquareFlag blackBishops,
            SquareFlag blackQueens,
            SquareFlag blackKing,
            SquareFlag enPassant)
        {
            Ply = ply;
            ToPlay = toPlay;
            HalfMoveClock = halfMoveClock;
            FullTurn = fullTurn;
            WhiteCanCastleKingSide = whiteCanCastleKingSide;
            WhiteCanCastleQueenSide = whiteCanCastleQueenSide;
            BlackCanCastleKingSide = blackCanCastleKingSide;
            BlackCanCastleQueenSide = blackCanCastleQueenSide;
            WhitePawns = whitePawns;
            WhiteRooks = whiteRooks;
            WhiteKnights = whiteKnights;
            WhiteBishops = whiteBishops;
            WhiteQueens = whiteQueens;
            WhiteKing = whiteKing;
            BlackPawns = blackPawns;
            BlackRooks = blackRooks;
            BlackKnights = blackKnights;
            BlackBishops = blackBishops;
            BlackQueens = blackQueens;
            BlackKing = blackKing;
            EnPassant = enPassant;
            White = WhitePawns | WhiteRooks | WhiteKnights | WhiteBishops | WhiteQueens | WhiteKing;
            Black = BlackPawns | BlackRooks | BlackKnights | BlackBishops | BlackQueens | BlackKing;
        }

        public int Ply { get; }

        public Colour ToPlay { get; }

        public int HalfMoveClock { get; }

        public int FullTurn { get; }

        public SquareFlag WhitePawns { get; }

        public SquareFlag WhiteRooks { get; }

        public SquareFlag WhiteKnights { get; }

        public SquareFlag WhiteBishops { get; }

        public SquareFlag WhiteQueens { get; }

        public SquareFlag WhiteKing { get; }

        public SquareFlag BlackPawns { get; }

        public SquareFlag BlackRooks { get; }

        public SquareFlag BlackKnights { get; }

        public SquareFlag BlackBishops { get; }

        public SquareFlag BlackQueens { get; }

        public SquareFlag BlackKing { get; }

        public bool WhiteCanCastleKingSide { get; }

        public bool WhiteCanCastleQueenSide { get; }

        public bool BlackCanCastleKingSide { get; }

        public bool BlackCanCastleQueenSide { get; }

        public SquareFlag EnPassant { get; }

        public SquareFlag White { get; }

        public SquareFlag Black { get; }
    }
}
