using ChessSharp.Enums;
using System.Windows;
using System.Windows.Controls;

namespace ChessSharp_UI
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
            PieceSelected?.Invoke(this, new PromotionTypeEventArgs(PieceType.Queen));
        }

        private void ButtonBishop_Click(object sender, RoutedEventArgs e)
        {
            PieceSelected?.Invoke(this, new PromotionTypeEventArgs(PieceType.Bishop));
        }

        private void ButtonKnight_Click(object sender, RoutedEventArgs e)
        {
            PieceSelected?.Invoke(this, new PromotionTypeEventArgs(PieceType.Knight));
        }

        private void ButtonRook_Click(object sender, RoutedEventArgs e)
        {
            PieceSelected?.Invoke(this, new PromotionTypeEventArgs(PieceType.Rook));
        }
    }
}
