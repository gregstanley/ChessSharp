using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using System.Windows;
using System.Windows.Controls;

namespace ChessSharp_UI
{
    public delegate void PieceSelectedEventDelegate(object sender, PromotionTypeSelectedEventArgs args);

    public partial class PromotionTypeUserControl : UserControl
    {
        public PromotionTypeUserControl()
        {
            InitializeComponent();
        }

        public event PieceSelectedEventDelegate PieceSelected;

        private void ButtonQueen_Click(object sender, RoutedEventArgs e)
        {
            PieceSelected?.Invoke(this, new PromotionTypeSelectedEventArgs(0, 0, PieceType.Queen));
        }

        private void ButtonBishop_Click(object sender, RoutedEventArgs e)
        {
            PieceSelected?.Invoke(this, new PromotionTypeSelectedEventArgs(0, 0, PieceType.Bishop));
        }

        private void ButtonKnight_Click(object sender, RoutedEventArgs e)
        {
            PieceSelected?.Invoke(this, new PromotionTypeSelectedEventArgs(0, 0, PieceType.Knight));
        }

        private void ButtonRook_Click(object sender, RoutedEventArgs e)
        {
            PieceSelected?.Invoke(this, new PromotionTypeSelectedEventArgs(0, 0, PieceType.Rook));
        }
    }
}
