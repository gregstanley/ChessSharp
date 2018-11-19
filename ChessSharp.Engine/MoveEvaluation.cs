namespace ChessSharp.Engine
{
    public class MoveEvaluation
    {
        public MoveEvaluation(MoveViewer move, double score)
        {
            Move = move;
            Score = score;
        }

        public MoveViewer Move { get; }

        public double Score { get; }
    }
}
