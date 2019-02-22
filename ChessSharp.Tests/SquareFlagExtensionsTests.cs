using ChessSharp.Enums;
using ChessSharp.Extensions;
using Xunit;

namespace ChessSharp.Tests
{
    public class SquareFlagExtensionsTests
    {
        [Fact]
        public void ToList_1_Correct()
        {
            SquareFlag square = SquareFlag.H1;

            var asList = square.ToList();

            Assert.Collection(asList, item => Assert.Equal(SquareFlag.H1, item));
        }

        [Fact]
        public void ToList_2_Correct()
        {
            SquareFlag square = SquareFlag.H1 | SquareFlag.H8;

            var asList = square.ToList();

            Assert.Collection(
                asList,
                item => Assert.Equal(SquareFlag.H1, item),
                item => Assert.Equal(SquareFlag.H8, item));
        }

        [Fact]
        public void ToList_3_Correct()
        {
            SquareFlag square = SquareFlag.H1 | SquareFlag.A5 | SquareFlag.H8;

            var asList = square.ToList();

            Assert.Collection(
                asList,
                item => Assert.Equal(SquareFlag.H1, item),
                item => Assert.Equal(SquareFlag.A5, item),
                item => Assert.Equal(SquareFlag.H8, item));
        }

        [Fact]
        public void ToList_4_Correct()
        {
            SquareFlag square = SquareFlag.H1 | SquareFlag.A5 | SquareFlag.E6 | SquareFlag.H8;

            var asList = square.ToList();

            Assert.Collection(
                asList,
                item => Assert.Equal(SquareFlag.H1, item),
                item => Assert.Equal(SquareFlag.A5, item),
                item => Assert.Equal(SquareFlag.E6, item),
                item => Assert.Equal(SquareFlag.H8, item));
        }
    }
}
