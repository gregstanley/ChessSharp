using ChessSharp.Enums;
using ChessSharp.Extensions;

namespace ChessSharp.Models
{
    public struct BoardStateInfo
    {
        public BoardStateInfo(ulong key, uint move, StateFlag state, SquareFlag enPassant) : this()
        {
            Key = key;
            Move = move;
            StateFlags = state;
            EnPassant = enPassant;

            var movePieceType = move.GetPieceType();
            var moveType = move.GetMoveType();

            if (movePieceType == PieceType.Pawn
                || moveType == MoveType.CastleKing
                || moveType == MoveType.CastleQueen)
                IsIrreversible = true;
        }

        public ulong Key { get; }

        public uint Move { get; }

        public StateFlag StateFlags { get; }

        public SquareFlag EnPassant { get; }

        public bool IsIrreversible { get; }
    }
}
