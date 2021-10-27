using ChessSharp.Common.Enums;

namespace ChessSharp.Common.Models
{
    public interface IGameState : IPieceMap
    {
        int Ply { get; }

        Colour ToPlay { get; }

        int HalfMoveClock { get; }

        int FullTurn { get; }

        bool WhiteCanCastleKingSide { get; }

        bool WhiteCanCastleQueenSide { get; }

        bool BlackCanCastleKingSide { get; }

        bool BlackCanCastleQueenSide { get; }

        SquareFlag EnPassant { get; }
    }
}
