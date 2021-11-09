
using System;
using ChessSharp.Common;
using ChessSharp.Common.Helpers;
using ChessSharp.Engine.NegaMax;
using Xunit;

namespace ChessSharp.Tests.NegaMax
{
    public class NegaMaxSearchTests : IClassFixture<NegaMaxFixture>
    {
        public NegaMaxSearchTests(NegaMaxFixture fixture)
        {
            if (fixture is null) throw new ArgumentNullException(nameof(fixture));

            Fixture = fixture;
        }

        public NegaMaxFixture Fixture { get; }

        [Theory]
        [InlineData("k7/p7/8/8/8/8/7P/7K w KQkq - 0 1")]
        public void Go_Fen_Simple_Depth_1(string fen)
        {
            var gameState = FenHelpers.Parse(fen);
            var board = Board.FromGameState(gameState);
            var x = Fixture.NegaMaxSearch.GoNoIterativeDeepening(board, gameState.ToPlay, 1);

            Assert.Equal(31986u, x.BestMove);
            Assert.Equal(8, x.PositionCount);
        }

        [Theory]
        [InlineData("k7/p7/8/8/8/8/7P/7K w KQkq - 0 1")]
        public void Go_Fen_Simple_Depth_2(string fen)
        {
            var gameState = FenHelpers.Parse(fen);
            var board = Board.FromGameState(gameState);
            var x = Fixture.NegaMaxSearch.GoNoIterativeDeepening(board, gameState.ToPlay, 2);

            Assert.NotEqual(uint.MinValue, x.BestMove);
        }

        [Theory]
        [InlineData(FenHelpers.Default)]
        public void Go_Fen_Default_Depth_1(string fen)
        {
            var gameState = FenHelpers.Parse(fen);
            var board = Board.FromGameState(gameState);
            var x = Fixture.NegaMaxSearch.GoNoIterativeDeepening(board, gameState.ToPlay, 1);

            Assert.Equal(20674u, x.BestMove);
        }

        [Theory]
        [InlineData(FenHelpers.Default)]
        public void Go_Fen_Default_Depth_2(string fen)
        {
            var gameState = FenHelpers.Parse(fen);
            var board = Board.FromGameState(gameState);
            var x = Fixture.NegaMaxSearch.GoNoIterativeDeepening(board, gameState.ToPlay, 2);

            Assert.Equal(23654u, x.BestMove);
        }
    }
}