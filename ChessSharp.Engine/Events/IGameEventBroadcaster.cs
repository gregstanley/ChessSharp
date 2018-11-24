using static ChessSharp.Engine.Game;

namespace ChessSharp.Engine.Events
{
    public interface IGameEventBroadcaster
    {
        event InvalidMoveEventDelegate InvalidMove;

        event PromotionTypeRequiredEventDelegate PromotionTypeRequired;

        event MoveAppliedEventDelegate MoveApplied;

        event CheckmateEventDelegate Checkmate;
    }
}
