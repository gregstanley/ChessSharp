using ChessSharp.Enums;

namespace ChessSharp.Models
{
    public struct HistoryState
    {
        public HistoryState(ulong key, uint move, StateFlag state) : this()
        {
            Key = key;
            Move = move;
            State = state;
        }

        public ulong Key { get; }

        public uint Move { get; }

        public StateFlag State { get; }
    }
}
