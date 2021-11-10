
using System;
using ChessSharp.Common;
using ChessSharp.Engine.NegaMax;
using Xunit;

namespace ChessSharp.Tests.NegaMax
{
    public class NegaMaxSearchTests : IClassFixture<NegaMaxFixture>
    {
        readonly Board board1;

        public NegaMaxSearchTests(NegaMaxFixture fixture)
        {
            if (fixture is null) throw new ArgumentNullException(nameof(fixture));

            Fixture = fixture;
            board1 = Fixture.GetBoard1();
            Fixture.LogNextTest();
        }

        public NegaMaxFixture Fixture { get; }

        [Fact]
        public void Go_Fen_Simple_Depth_6()
        {
            var x = Fixture.NegaMaxSearch.GoNoIterativeDeepening(board1, Fixture.GameState1.ToPlay, 6);

            Assert.NotEqual(uint.MinValue, x.BestMove);
        }
    }
}