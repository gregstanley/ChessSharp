using ChessSharp;
using ChessSharp.Engine;
using ChessSharp.Enums;
using System.Threading.Tasks;
using System.Windows;

namespace ChessSharp_UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnNewGameWhite_Click(object sender, RoutedEventArgs e) =>
            NewGameWhite();

        private async void BtnNewGameBlack_Click(object sender, RoutedEventArgs e) =>
            await NewGameBlack();

        private void NewGameWhite() =>
            GameUserControl.NewGameWhite(new Game(new BitBoard(), Colour.White));

        private Task NewGameBlack() =>
            GameUserControl.NewGameBlack(new Game(new BitBoard(), Colour.Black));
    }
}
