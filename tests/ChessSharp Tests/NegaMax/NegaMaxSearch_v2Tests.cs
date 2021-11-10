
using System;
using ChessSharp.Common;
using ChessSharp.Common.Helpers;
using ChessSharp.Engine.NegaMax;
using Xunit;

namespace ChessSharp.Tests.NegaMax
{
    public class NegaMaxSearch_v2Tests : IClassFixture<NegaMaxFixture>
    {
        readonly Board board1;

        public NegaMaxSearch_v2Tests(NegaMaxFixture fixture)
        {
            if (fixture is null) throw new ArgumentNullException(nameof(fixture));

            Fixture = fixture;
            board1 = Fixture.GetBoard1();
            Fixture.LogNextTest();
        }

        public NegaMaxFixture Fixture { get; }

        [Fact]
        public void Go_Fen_Simple_Depth_1()
        {
            var x = Fixture.NegaMaxSearch_v2.GoNoIterativeDeepening(board1, Fixture.GameState1.ToPlay, 1);

            Assert.Equal(6268u, x.BestMove);
            Assert.Equal(8, x.PositionCount);
        }

        [Fact]
        public void Go_Fen_Simple_Depth_2()
        {
            var x = Fixture.NegaMaxSearch_v2.GoNoIterativeDeepening(board1, Fixture.GameState1.ToPlay, 2);

            Assert.NotEqual(uint.MinValue, x.BestMove);
        }

        [Fact]
        public void Go_Fen_Simple_Depth_4()
        {
            var x = Fixture.NegaMaxSearch_v2.GoNoIterativeDeepening(board1, Fixture.GameState1.ToPlay, 4);

            Fixture.LogResult(x);

            Assert.NotEqual(uint.MinValue, x.BestMove);
        }

        [Fact]
        public void Go_Fen_Simple_Depth_6()
        {
            var x = Fixture.NegaMaxSearch_v2.GoNoIterativeDeepening(board1, Fixture.GameState1.ToPlay, 6);

            Assert.NotEqual(uint.MinValue, x.BestMove);
        }
    }
}