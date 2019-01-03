namespace ChessSharp.Models
{
    public class GameHistoryNode
    {
        public GameHistoryNode(HistoryState historyState, GameState gameState)
        {
            HistoryState = historyState;
            GameState = gameState;
        }

        public HistoryState HistoryState { get; }

        public GameState GameState { get; }
    }
}
