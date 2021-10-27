using static ChessSharp.Engine.Game;

namespace ChessSharp.Engine.Events
{
    public interface IGameEventBroadcaster
    {
        event InvalidMoveEventDelegate InvalidMove;

        event PromotionTypeRequiredEventDelegate PromotionTypeRequired;

        event SearchStartedEventDelegate SearchStarted;

        event SearchCompletedEventDelegate SearchCompleted;

        event MoveAppliedEventDelegate MoveApplied;

        event DrawEventDelegate Draw;

        event CheckmateEventDelegate Checkmate;
    }
}
