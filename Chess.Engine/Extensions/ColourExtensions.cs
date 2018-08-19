using Chess.Engine.Models;

namespace Chess.Engine.Extensions
{
    public static class ColourExtensions
    {
        public static Colour Opposite(this Colour colour) =>
            colour == Colour.White ? Colour.Black : Colour.White;
    }
}
