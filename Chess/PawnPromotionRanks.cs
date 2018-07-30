namespace Chess
{
    public class PawnPromotionRanks
    {
        public PawnPromotionRanks(byte white, byte black)
        {
            White = white;
            Black = black;
        }

        public byte White { get; }

        public byte Black { get; }
    }
}
