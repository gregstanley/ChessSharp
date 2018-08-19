namespace Chess.Engine.Models
{
    public class RelativePosition
    {
        public int Rank { get; }

        public int File { get; }

        public RelativePosition(int rank, int file)
        {
            Rank = rank;
            File = file;
        }
    }
}
