using ChessSharp;
using ChessSharp.Engine;
using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using ChessSharp.Extensions;
using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace ChessSharp_UI
{
    public partial class MainWindow : Window
    {
        private Game _currentGame;

        private IReadOnlyCollection<ImageMap> _images { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnNewGameWhite_Click(object sender, RoutedEventArgs e) => NewGameWhite();

        private async void BtnNewGameBlack_Click(object sender, RoutedEventArgs e) => await NewGameBlack();

        private async Task NewGameBlack()
        {
            var bitBoard = new BitBoard();

            _currentGame = new Game(bitBoard, Colour.Black);

            BoardUserControl.Load(_currentGame, _currentGame.GetGameState());

            await DoSearch();
        }

        private void NewGameWhite()
        {
            var bitBoard = new BitBoard();

            _currentGame = new Game(bitBoard, Colour.White);

            BoardUserControl.Load(_currentGame, _currentGame.GetGameState());
        }

        private Task DoSearch() => _currentGame.CpuMove(3);

        private void BoardUserControl_PieceMoved(object sender, UserMovedPieceEventArgs args)
        {
            var move = _currentGame.TryMove(args.FromSquareIndex, args.ToSquareIndex, PieceType.None);

            if (move.Value == 0)
                return;

            DoSearch();
        }

        private void BoardUserControl_PieceSelected(object sender, PromotionTypeSelectedEventArgs args)
        {
            var move = _currentGame.TryMove(args.FromSquareIndex, args.ToSquareIndex, args.PieceType);

            if (move.Value == 0)
                return;

            DoSearch();
        }
    }
}
