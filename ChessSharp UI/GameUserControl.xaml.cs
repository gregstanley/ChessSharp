using ChessSharp;
using ChessSharp.Engine;
using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using ChessSharp.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChessSharp_UI
{
    public partial class GameUserControl : UserControl
    {
        private TranspositionTable _transpositionTable = new TranspositionTable();

        public Game _game;

        public GameUserControl()
        {
            InitializeComponent();
        }

        private void NewGame(Game game)
        {
            if (_game != null)
            {
                _game.MoveApplied -= _game_MoveApplied;
                _game.SearchCompleted -= _game_SearchCompleted;
                _game.Info -= _game_Info;
            }

            if (game != null)
            {
                _game = game;
            }
            else
            {
                _game = new Game(new BitBoard(), _transpositionTable, Colour.White);
            }

            _game.MoveApplied += _game_MoveApplied;
            _game.SearchCompleted += _game_SearchCompleted;
            _game.Info += _game_Info;

            WhiteCastleKingSide.Visibility = Visibility.Visible;
            WhiteCastleQueenSide.Visibility = Visibility.Visible;
            BlackCastleKingSide.Visibility = Visibility.Visible;
            BlackCastleQueenSide.Visibility = Visibility.Visible;

            FullTurnNumberLabel.Content = _game.FullTurn;

            BoardUserControl.Load(_game, _game.GetGameState());
        }

        public void NewGameWhite(Game game) =>
            NewGame(game);

        public Task NewGameBlack(Game game)
        {
            NewGame(game);

            return DoSearch();
        }

        private void _game_Info(object sender, InfoEventArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                InfoTimeTextBlock.Text = args.Info.GetTimeString();
                InfoTtTextBlock.Text = args.Info.GetTtString();

                if (args.Info is InfoDepthComplete info2)
                {
                    InfoPvTextBlock.Text = info2.GetPvString();
                    return;
                }

                if (args.Info is InfoNewPv info3)
                {
                    InfoPvTextBlock2.Text = info3.GetBestMoveString();
                    return;
                }
            }));
        }

        private void _game_MoveApplied(object sender, MoveAppliedEventArgs args)
        {
            FullTurnNumberLabel.Content = args.GameState.FullTurn;
            HalfTurnCountLabel.Content = args.GameState.HalfMoveClock;

            var score = Math.Round(args.Evaluation * 0.01, 2);

            EvaluationLabel.Content = score;

            WhiteCastleKingSide.Visibility = args.GameState.WhiteCanCastleKingSide
                ? Visibility.Visible
                : Visibility.Collapsed;

            WhiteCastleQueenSide.Visibility = args.GameState.WhiteCanCastleQueenSide
                ? Visibility.Visible
                : Visibility.Collapsed;

            BlackCastleKingSide.Visibility = args.GameState.BlackCanCastleKingSide
                ? Visibility.Visible
                : Visibility.Collapsed;

            BlackCastleQueenSide.Visibility = args.GameState.BlackCanCastleQueenSide
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void _game_SearchCompleted(object sender, SearchCompleteEventArgs args)
        {
            OutputTextBox.Text = args.SearchResults.ToResultsString();
        }
        
        private async void BoardUserControl_PieceMoved(object sender, UserMovedPieceEventArgs args) =>
            await OnPieceMoved(args); 

        private async void BoardUserControl_PieceSelected(object sender, PromotionTypeSelectedEventArgs args) =>
            await OnPieceSelected(args);

        private async void GoBtn_Click(object sender, RoutedEventArgs e) =>
            await NewGameFromFen();

        private Task OnPieceMoved(UserMovedPieceEventArgs args)
        {
            var move = _game.TryMove(args.FromSquareIndex, args.ToSquareIndex, PieceType.None);

            return move.Value == 0 ? Task.CompletedTask : DoSearch();
        }

        private Task OnPieceSelected(PromotionTypeSelectedEventArgs args)
        {
            var move = _game.TryMove(args.FromSquareIndex, args.ToSquareIndex, args.PieceType);

            return move.Value == 0 ? Task.CompletedTask : DoSearch();
        }

        private Task NewGameFromFen()
        {
            var gameState = FenHelpers.Parse(FenTextBox.Text);

            var board = BitBoard.FromGameState(gameState);

            var game = new Game(board, _transpositionTable, Colour.White);

            if (gameState.ToPlay == Colour.White)
            {
                NewGameWhite(game);

                return Task.CompletedTask;
            }

            return NewGameBlack(game);
        }

        private async Task DoSearch()
        {
            await _game.CpuMove(5);

            FenTextBox.Text = FenHelpers.ToFen(_game.GetGameState());

            var ttUsage = _transpositionTable.VerfiyUsage();
        }
    }
}
