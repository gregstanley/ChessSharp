namespace ChessSharp.Engine.NegaMax
{
    // C# 7.2 readonly struct
    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct#readonly-struct
    public readonly struct NegaMaxMoveEvaluation
    {
        public NegaMaxMoveEvaluation(uint move, double score)
        {
            Move = move;
            Score = score;
        }

        public uint Move { get; }

        public double Score { get; }
    }
}
