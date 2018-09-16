using System;
using System.Collections.Concurrent;

namespace Chess.Engine
{
    public class TranspositionTable
    {
        //private ConcurrentDictionary<int, Transposition> _table = new ConcurrentDictionary<int, Transposition>();
        private const int Size = 0xFFFFF;

        private Transposition[] _table = new Transposition[Size + 1];

        public void Add(Transposition transposition)
        {
            var index = (int) transposition.Key & Size;

            _table[index] = transposition;
        }

        public Transposition Find(ulong key)
        {
            var index = (int) key & 0xFFFF;

            //if (_table.ContainsKey(index))
            //    return _table[index];

            return _table[index];
        }
    }
}
