using static ChessSharp.Engine.Game;

namespace ChessSharp.Engine.Events
{
    public interface IGameEventBroadcaster
    {
        event MoveAppliedEventDelegate MoveApplied;

        event PromotionTypeRequiredEventDelegate PromotionTypeRequired;
    }
}
