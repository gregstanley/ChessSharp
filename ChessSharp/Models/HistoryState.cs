using ChessSharp.Enums;

namespace ChessSharp.Models
{
    public struct HistoryState
    {
        public HistoryState(ulong key, uint move, BoardState state) : this()
        {
            Key = key;
            Move = move;
            State = state;
        }

        public ulong Key { get; }

        public uint Move { get; }

        public BoardState State { get; }
    }
}
