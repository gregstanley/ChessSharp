using ChessSharp.Common.Enums;

namespace ChessSharp.Common.Extensions
{
    public static class ColourExtensions
    {
        public static Colour Opposite(this Colour colour)
        {
            return colour == Colour.White ? Colour.Black : Colour.White;
        }
    }
}
