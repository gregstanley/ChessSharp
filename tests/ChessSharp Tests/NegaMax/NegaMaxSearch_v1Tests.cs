
using System;
using ChessSharp.Common;
using ChessSharp.Common.Helpers;
using ChessSharp.Engine.NegaMax;
using Xunit;

namespace ChessSharp.Tests.NegaMax
{
    public class NegaMaxSearch_v1Tests : IClassFixture<NegaMaxFixture>
    {
        readonly Board board1;

        public NegaMaxSearch_v1Tests(NegaMaxFixture fixture)
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
            var x = Fixture.NegaMaxSearch_v1.GoNoIterativeDeepeningAndNoPv(board1, Fixture.GameState1.ToPlay, 1);

            Assert.Equal(6268u, x.BestMove);
            Assert.Equal(8, x.PositionCount);
        }

        [Fact]
        public void Go_Fen_Simple_Depth_2()
        {
            var x = Fixture.NegaMaxSearch_v1.GoNoIterativeDeepeningAndNoPv(board1, Fixture.GameState1.ToPlay, 2);

            Assert.NotEqual(uint.MinValue, x.BestMove);
        }

        [Theory]
        [InlineData(FenHelpers.Default)]
        public void Go_Fen_Default_Depth_1(string fen)
        {
            var gameState = FenHelpers.Parse(fen);
            var board = Board.FromGameState(gameState);

            var x = Fixture.NegaMaxSearch_v1.GoNoIterativeDeepeningAndNoPv(board, gameState.ToPlay, 1);

            Assert.Equal(31986u, x.BestMove);
        }

        [Theory]
        [InlineData(FenHelpers.Default)]
        public void Go_Fen_Default_Depth_2(string fen)
        {
            var gameState = FenHelpers.Parse(fen);
            var board = Board.FromGameState(gameState);

            var x = Fixture.NegaMaxSearch_v1.GoNoIterativeDeepeningAndNoPv(board, gameState.ToPlay, 2);

            Assert.Equal(16514u, x.BestMove);
        }
    }
}