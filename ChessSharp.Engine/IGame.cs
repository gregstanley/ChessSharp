using ChessSharp.Engine.Events;
using ChessSharp.Models;

namespace ChessSharp.Engine
{
    public interface IGame : IGameEventBroadcaster
    {
        GameHistoryNode CurrentState { get; }
    }
}
