
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
        [InlineData(FenHelpers.Default)]
        public void Go_Fen_Default(string fen)
        {
            var gameState = FenHelpers.Parse(fen);
            var board = Board.FromGameState(gameState);
            var x = Fixture.NegaMaxSearch.Go(board, gameState.ToPlay, 2);

            Assert.Equal(23654u, x);
        }
    }
}