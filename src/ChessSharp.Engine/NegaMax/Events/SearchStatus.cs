namespace ChessSharp.Engine.NegaMax.Events
{
    public struct SearchStatus
    {
        public SearchStatus(int depth, int ply, long elapsedMilliseconds, int positionCount, int eval)
        {
            Depth = depth;
            Ply = ply;
            ElapsedMilliseconds = elapsedMilliseconds;
            PositionCount = positionCount;
            Eval = eval;
        }

        public int Depth { get; }

        public int Ply { get; }

        public long ElapsedMilliseconds { get; }

        public int PositionCount { get; }

        public int Eval { get; }

        public override string ToString()
        {
            return $"depth {Ply} {ElapsedMilliseconds}ms nodes {PositionCount} eval {Eval}";
        }
    }
}
