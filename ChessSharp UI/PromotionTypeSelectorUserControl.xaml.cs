using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using System.Windows;
using System.Windows.Controls;

namespace ChessSharp_UI
{
    public partial class PromotionTypeSelectorUserControl : UserControl
    {      
        private PromotionTypeRequiredEventArgs args;

        public PromotionTypeSelectorUserControl()
        {
            this.InitializeComponent();
        }

        public delegate void PromotionTypeSelectedEventDelegate(object sender, PromotionTypeSelectedEventArgs args);

        public event PromotionTypeSelectedEventDelegate PromotionTypeSelected;

        public void Open(PromotionTypeRequiredEventArgs args)
        {
            this.Visibility = Visibility.Visible;

            this.args = args;
        }

        private void ButtonQueen_Click(object sender, RoutedEventArgs e) => this.Close(PieceType.Queen);

        private void ButtonRook_Click(object sender, RoutedEventArgs e) => this.Close(PieceType.Rook);

        private void ButtonBishop_Click(object sender, RoutedEventArgs e) => this.Close(PieceType.Bishop);

        private void ButtonKnight_Click(object sender, RoutedEventArgs e) => this.Close(PieceType.Knight);

        private void Close(PieceType pieceType)
        {
            this.Visibility = Visibility.Collapsed;

            this.PromotionTypeSelected?.Invoke(this, new PromotionTypeSelectedEventArgs(this.args.FromSquareIndex, this.args.ToSquareIndex, pieceType));
        }
    }
}
