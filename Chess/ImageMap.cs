using Chess.Engine.Models;
using System.Windows.Controls;

namespace Chess
{
    public class ImageMap
    {
        public ImageMap(Colour colour, PieceType pieceType, Image image)
        {
            Colour = colour;
            PieceType = pieceType;
            Image = image;
        }

        public Colour Colour { get; }

        public PieceType PieceType { get; }

        public Image Image { get; }
    }
}
