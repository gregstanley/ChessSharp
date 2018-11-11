using ChessSharp.MoveGeneration;

namespace ChessSharp.Tests
{
    public class MoveGeneratorFixture
    {
        public MoveGeneratorFixture()
        {
            MoveGenerator = new MoveGenerator();
        }

        public MoveGenerator MoveGenerator { get; private set; }
    }
}
