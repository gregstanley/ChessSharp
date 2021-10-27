using ChessSharp.Common.Models;
using ChessSharp.Engine.Events;

namespace ChessSharp.Engine
{
    public interface IGame : IGameEventBroadcaster
    {
        GameHistoryNode CurrentState { get; }
    }
}
