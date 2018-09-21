using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Chess.Engine
{
    public class TranspositionTable
    {
        public long AccessCount { get => _accessCount; }

        public long HitCount { get => _hitCount; }

        //private ConcurrentDictionary<int, Transposition> _table = new ConcurrentDictionary<int, Transposition>();
        private const int Size = 0xFFFFF;

        private long _accessCount = 0;

        private long _hitCount = 0;

        private Transposition[] _table = new Transposition[Size + 1];

        public void Reset()
        {
            Interlocked.Exchange(ref _accessCount, 0);
            Interlocked.Exchange(ref _hitCount, 0);
        }

        public void Add(Transposition transposition)
        {
            var index = (int) transposition.Key & Size;

            _table[index] = transposition;
        }

        public Transposition Find(ulong key)
        {
            Interlocked.Increment(ref _accessCount);

            var index = (int) key & 0xFFFF;

            //if (_table.ContainsKey(index))
            //    return _table[index];

            if (_table[index] != null)
                Interlocked.Increment(ref _hitCount);

            return _table[index];
        }
    }
}
