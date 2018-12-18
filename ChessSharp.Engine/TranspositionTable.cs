using ChessSharp.Enums;
using System.Collections.Generic;
using System.Threading;

namespace ChessSharp.Engine
{
    public class TranspositionTable
    {
        private const int Size = 0xFFFFF;

        private readonly Transposition emptyTransposition = new Transposition();

        private readonly int availableSlots = (Size + 1) * 4;

        private readonly Transposition[] tableA = new Transposition[Size + 1];

        private readonly Transposition[] tableB = new Transposition[Size + 1];

        private readonly Transposition[] tableC = new Transposition[Size + 1];

        private readonly Transposition[] tableD = new Transposition[Size + 1];

        private int iteration = 0;

        private long accessCount = 0;

        private long hitCount = 0;

        private long missCount = 0;

        private long replaceCount = 0;

        public TranspositionTable()
        {
            for (var i = 0; i < Size + 1; ++i)
            {
                tableA[i] = new Transposition();
                tableB[i] = new Transposition();
                tableC[i] = new Transposition();
                tableD[i] = new Transposition();
            }
        }

        public long AccessCount => accessCount;

        public long HitCount => hitCount;

        public long MissCount => missCount;

        public long ReplaceCount => replaceCount;

        public long SetCount { get; private set; } = 0;

        public double Usage =>
            SetCount == 0 ? 0 : ((double)SetCount / availableSlots) * 100;

        public void Reset()
        {
            ResetIteration();

            for (var i = 0; i < Size + 1; ++i)
            {
                tableA[i].Set(0, 0, Colour.None, 0, 0, 0);
                tableB[i].Set(0, 0, Colour.None, 0, 0, 0);
                tableC[i].Set(0, 0, Colour.None, 0, 0, 0);
                tableD[i].Set(0, 0, Colour.None, 0, 0, 0);
            }

            SetCount = 0;
        }

        public void ResetIteration()
        {
            Interlocked.Exchange(ref iteration, 0);
        }

        public void NextIteration()
        {
            Interlocked.Increment(ref iteration);
            Interlocked.Exchange(ref accessCount, 0);
            Interlocked.Exchange(ref hitCount, 0);
            Interlocked.Exchange(ref missCount, 0);
            Interlocked.Exchange(ref replaceCount, 0);
        }

        public void Set(ulong key, byte depth, Colour colour, int evaluation, uint bestMove)
        {
            // var index = (int) key & Size;
            var index = key % Size;

            Transposition[] replaceTranspositionTable = tableA;

            foreach (var table in GetNextTable())
            {
                var transposition = table[index];

                if (transposition.Key == 0)
                {
                    table[index].Set(key, depth, colour, evaluation, bestMove, (byte)iteration);

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

                table[index].Set(key, depth, colour, evaluation, bestMove, (byte)iteration);

                return;
            }

            Interlocked.Increment(ref replaceCount);

            // var tA = _tableA[index];
            // var tB = _tableB[index];
            // var tC = _tableC[index];
            // var tD = _tableD[index];
            replaceTranspositionTable[index].Set(key, depth, colour, evaluation, bestMove, (byte)iteration);
        }

        public Transposition Find(ulong key)
        {
            Interlocked.Increment(ref accessCount);

            // var index = (int) key & Size;
            var index = key % Size;

            foreach (var table in GetNextTable())
            {
                var transpostion = table[index];

                if (transpostion.Key != key)
                    continue;

                Interlocked.Increment(ref hitCount);

                return transpostion;
            }

            Interlocked.Increment(ref missCount);

            return emptyTransposition;
        }

        public bool VerfiyUsage()
        {
            var available = 0;
            var count = 0;

            for (var i = 0; i < Size + 1; ++i)
            {
                available += 4;

                if (tableA[i].Key != 0)
                    ++count;

                if (tableB[i].Key != 0)
                    ++count;

                if (tableC[i].Key != 0)
                    ++count;

                if (tableD[i].Key != 0)
                    ++count;
            }

            var usage = ((double)count / available) * 100;

            return usage == Usage;
        }

        private IEnumerable<Transposition[]> GetNextTable()
        {
            yield return tableA;
            yield return tableB;
            yield return tableC;
            yield return tableD;
        }
    }
}
