namespace Chess
{
    public static class ColourExtensions
    {
        public static Colour Opposite(this Colour colour) =>
            colour == Colour.White ? Colour.Black : Colour.White;
    }
}
