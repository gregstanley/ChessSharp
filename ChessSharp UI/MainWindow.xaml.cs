using ChessSharp;
using ChessSharp.Engine;
using ChessSharp.Enums;
using System.Threading.Tasks;
using System.Windows;

namespace ChessSharp_UI
{
    public partial class MainWindow : Window
    {
        private TranspositionTable _transpositionTable = new TranspositionTable();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnNewGameWhite_Click(object sender, RoutedEventArgs e) =>
            NewGameWhite();

        private async void BtnNewGameBlack_Click(object sender, RoutedEventArgs e) =>
            await NewGameBlack();

        private void NewGameWhite()
        {
            _transpositionTable.Reset();

            var game = new Game(new BitBoard(), _transpositionTable, Colour.White);

            GameUserControl.NewGameWhite(game);
        }

        private Task NewGameBlack()
        {
            _transpositionTable.Reset();

            var game = new Game(new BitBoard(), _transpositionTable, Colour.Black);

            return GameUserControl.NewGameBlack(game);
        }
    }
}
