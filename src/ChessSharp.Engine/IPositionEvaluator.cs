using ChessSharp.Common;

namespace ChessSharp.Engine
{
    public interface IPositionEvaluator
    {
        int Evaluate(Board board);
    }
}