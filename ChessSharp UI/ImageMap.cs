using ChessSharp.Enums;
using System.Windows.Controls;

namespace ChessSharp_UI
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
