using System;

namespace ChessSharp.Engine.Events
{
    public class SearchCompleteEventArgs : EventArgs
    {
        public SearchCompleteEventArgs(SearchResults searchResults)
        {
            SearchResults = searchResults;
        }

        public SearchResults SearchResults { get; }
    }
}
