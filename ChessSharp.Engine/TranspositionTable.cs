using ChessSharp.Enums;
using System.Threading;

namespace ChessSharp.Engine
{
    public class TranspositionTable
    {
        public long AccessCount => _accessCount;

        public long HitCount => _hitCount;

        public long ConflictCount => _conflictCount;

        private const int Size = 0xFFFFF;

        private long _accessCount = 0;

        private long _hitCount = 0;

        private long _conflictCount = 0;

        private Transposition[] _table = new Transposition[Size + 1];

        public void ResetCounters()
        {
            Interlocked.Exchange(ref _accessCount, 0);
            Interlocked.Exchange(ref _hitCount, 0);
            Interlocked.Exchange(ref _conflictCount, 0);
        }

        public TranspositionTable()
        {
            for (var i = 0; i < Size + 1; ++i)
            {
                _table[i] = new Transposition();
            }
        }

        public void Set(ulong key, int depth, Colour colour, double evaluation, uint bestMove)
        {
            var index = (int) key & Size;

            var existingTranspostion = _table[index];

            if (existingTranspostion.Key == 0)
            {
                _table[index].Set(key, depth, colour, evaluation, bestMove);
                return;
            }

            if (existingTranspostion.Key != 0)
            {
                if (existingTranspostion.Key == key)
                {
                    if (existingTranspostion.Depth < depth)
                        _table[index].Set(key, depth, colour, evaluation, bestMove);
                }
                else
                {
                    Interlocked.Increment(ref _conflictCount);
                }
            }
        }

        public Transposition Find(ulong key)
        {
            Interlocked.Increment(ref _accessCount);

            var index = (int) key & Size;

            var existingTranspostion = _table[index];

            if (existingTranspostion.Key != 0)
            {
                if (existingTranspostion.Key == key)
                {
                    Interlocked.Increment(ref _hitCount);

                    return existingTranspostion;
                }

                var conflict = true;
            }

            return null;
        }
    }
}
