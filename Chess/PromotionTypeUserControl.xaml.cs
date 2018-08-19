using Chess.Engine.Models;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public delegate void PieceSelectedEventDelegate(object sender, PromotionTypeEventArgs args);

    public partial class PromotionTypeUserControl : UserControl
    {
        public PromotionTypeUserControl()
        {
            InitializeComponent();
        }

        public event PieceSelectedEventDelegate PieceSelected;

        private void ButtonQueen_Click(object sender, RoutedEventArgs e)
        {
            if (PieceSelected != null)
                PieceSelected(this, new PromotionTypeEventArgs(PieceType.Queen));
        }

        private void ButtonBishop_Click(object sender, RoutedEventArgs e)
        {
            if (PieceSelected != null)
                PieceSelected(this, new PromotionTypeEventArgs(PieceType.Bishop));
        }

        private void ButtonKnight_Click(object sender, RoutedEventArgs e)
        {
            if (PieceSelected != null)
                PieceSelected(this, new PromotionTypeEventArgs(PieceType.Knight));
        }

        private void ButtonRook_Click(object sender, RoutedEventArgs e)
        {
            if (PieceSelected != null)
                PieceSelected(this, new PromotionTypeEventArgs(PieceType.Rook));
        }
    }
}
