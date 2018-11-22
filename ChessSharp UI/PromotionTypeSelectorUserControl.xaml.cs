using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using System.Windows;
using System.Windows.Controls;

namespace ChessSharp_UI
{
    /// <summary>
    /// Interaction logic for PromotionTypeSelectorUserControl.xaml
    /// </summary>
    public partial class PromotionTypeSelectorUserControl : UserControl
    {
        public delegate void PromotionTypeSelectedEventDelegate(object sender, PromotionTypeSelectedEventArgs args);

        public event PromotionTypeSelectedEventDelegate PromotionTypeSelected;

        private PromotionTypeRequiredEventArgs _args;

        public PromotionTypeSelectorUserControl()
        {
            InitializeComponent();
        }

        public void Open(PromotionTypeRequiredEventArgs args)
        {
            Visibility = Visibility.Visible;

            _args = args;
        }

        private void ButtonQueen_Click(object sender, RoutedEventArgs e) => Close(PieceType.Queen);

        private void ButtonRook_Click(object sender, RoutedEventArgs e) => Close(PieceType.Rook);

        private void ButtonBishop_Click(object sender, RoutedEventArgs e) => Close(PieceType.Bishop);

        private void ButtonKnight_Click(object sender, RoutedEventArgs e) => Close(PieceType.Knight);

        private void Close(PieceType pieceType)
        {
            Visibility = Visibility.Collapsed;

            PromotionTypeSelected?.Invoke(this, new PromotionTypeSelectedEventArgs(_args.FromSquareIndex, _args.ToSquareIndex, pieceType));
        }
    }
}
