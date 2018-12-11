using System;

namespace ChessSharp.Engine.Events
{
    public class MoveAppliedEventArgs : EventArgs
    {
        public MoveAppliedEventArgs(MoveViewer move, GameState gameState, int evaluation)
        {
            Move = move;
            GameState = gameState;
            Evaluation = evaluation;
        }

        public MoveViewer Move { get; }

        public GameState GameState { get; }

        public int Evaluation { get; }
    }
}
