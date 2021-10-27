using ChessSharp.Common.Keys;

namespace ChessSharp.Tests
{
    public class KeyGeneratorFixture
    {
        public KeyGeneratorFixture()
        {
            KeyGenerator = new Zobrist();

            KeyGenerator.Init();
        }

        public Zobrist KeyGenerator { get; private set; }
    }
}
