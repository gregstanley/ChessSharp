using System;

namespace ChessSharp.Engine.Events
{
    public class MoveAppliedEventArgs : EventArgs
    {
        public MoveAppliedEventArgs(MoveViewer move, GameState gameState, int evaluation)
        {
            this.Move = move;
            this.GameState = gameState;
            this.Evaluation = evaluation;
        }

        public MoveViewer Move { get; }

        public GameState GameState { get; }

        public int Evaluation { get; }
    }
}
