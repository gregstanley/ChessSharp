using System;

namespace ChessSharp.Engine.Events
{
    public class MoveAppliedEventArgs : EventArgs
    {
        public MoveAppliedEventArgs(MoveViewer move, GameState gameState)
        {
            Move = move;
            GameState = gameState;
        }

        public MoveViewer Move { get; }

        public GameState GameState { get; }
    }
}
