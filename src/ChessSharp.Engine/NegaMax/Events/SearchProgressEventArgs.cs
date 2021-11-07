using System;

namespace ChessSharp.Engine.NegaMax.Events
{
    public class SearchProgressEventArgs : EventArgs
    {
        public SearchProgressEventArgs(SearchStatus status) => Status = status;

        public SearchStatus Status { get; }
    }
}
