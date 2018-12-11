using ChessSharp.Enums;
using System.Collections.Generic;
using System.Threading;

namespace ChessSharp.Engine
{
    public class TranspositionTable
    {
        public long AccessCount => _accessCount;

        public long HitCount => _hitCount;

        public long MissCount => _missCount;

        public long ReplaceCount => _replaceCount;

        private const int Size = 0xFFFFF;

        private int _iteration = 0;

        private long _accessCount = 0;

        private long _hitCount = 0;

        private long _missCount = 0;

        private long _replaceCount = 0;

        private Transposition[] _tableA = new Transposition[Size + 1];
        private Transposition[] _tableB = new Transposition[Size + 1];
        private Transposition[] _tableC = new Transposition[Size + 1];
        private Transposition[] _tableD = new Transposition[Size + 1];

        private int _availableSlots = (Size + 1) * 4;

        public TranspositionTable()
        {
            for (var i = 0; i < Size + 1; ++i)
            {
                _tableA[i] = new Transposition();
                _tableB[i] = new Transposition();
                _tableC[i] = new Transposition();
                _tableD[i] = new Transposition();
            }
        }

        public void Reset()
        {
            ResetIteration();

            for (var i = 0; i < Size + 1; ++i)
            {
                _tableA[i].Set(0, 0, Colour.None, 0, 0, 0);
                _tableB[i].Set(0, 0, Colour.None, 0, 0, 0);
                _tableC[i].Set(0, 0, Colour.None, 0, 0, 0);
                _tableD[i].Set(0, 0, Colour.None, 0, 0, 0);
            }
        }

        public void ResetIteration()
        {
            Interlocked.Exchange(ref _iteration, 0);
        }

        public void NextIteration()
        {
            Interlocked.Increment(ref _iteration);
            Interlocked.Exchange(ref _accessCount, 0);
            Interlocked.Exchange(ref _hitCount, 0);
            Interlocked.Exchange(ref _missCount, 0);
            Interlocked.Exchange(ref _replaceCount, 0);
        }

        public void Set(ulong key, byte depth, Colour colour, int evaluation, uint bestMove)
        {
            //var index = (int) key & Size;
            var index = key % Size;

            Transposition[] replaceTranspositionTable = _tableA;

            foreach (var table in GetNextTable())
            {
                var transposition = table[index];

                if (transposition.Key == 0)
                {
                    table[index].Set(key, depth, colour, evaluation, bestMove, _iteration);

                    ++SetCount;

                    return;
                }

                if (transposition.Key != key)
                {
                    if (transposition.Age < replaceTranspositionTable[index].Age)
                        replaceTranspositionTable = table;

                    continue;
                }

                if (transposition.Depth >= depth)
                    return;

                table[index].Set(key, depth, colour, evaluation, bestMove, _iteration);

                return;
            }

            Interlocked.Increment(ref _replaceCount);

            //var tA = _tableA[index];
            //var tB = _tableB[index];
            //var tC = _tableC[index];
            //var tD = _tableD[index];

            replaceTranspositionTable[index].Set(key, depth, colour, evaluation, bestMove, _iteration);
        }

        public long SetCount { get; private set; }

        public double Usage =>
            SetCount == 0 ? 0 : ((double)SetCount / _availableSlots) * 100;

        public Transposition Find(ulong key)
        {
            Interlocked.Increment(ref _accessCount);

            //var index = (int) key & Size;
            var index = key % Size;

            foreach (var table in GetNextTable())
            {
                var transpostion = table[index];

                if (transpostion.Key != key)
                    continue;

                Interlocked.Increment(ref _hitCount);

                return transpostion;
            }

            Interlocked.Increment(ref _missCount);

            return null;
        }

        private IEnumerable<Transposition[]> GetNextTable()
        {
            yield return _tableA;
            yield return _tableB;
            yield return _tableC;
            yield return _tableD;
        }
    }
}
