using ChessSharp.Engine;
using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChessSharp_UI
{
    public partial class GameUserControl : UserControl
    {
        public Game _game;

        public GameUserControl()
        {
            InitializeComponent();
        }

        public void NewGameWhite(Game game)
        {
            _game = game;

            _game.MoveApplied += _game_MoveApplied;
            _game.SearchCompleted += _game_SearchCompleted;

            WhiteCastleKingSide.Visibility = Visibility.Visible;
            WhiteCastleQueenSide.Visibility = Visibility.Visible;
            BlackCastleKingSide.Visibility = Visibility.Visible;
            BlackCastleQueenSide.Visibility = Visibility.Visible;

            FullTurnNumberLabel.Content = _game.FullTurnNumber;

            BoardUserControl.Load(_game, _game.GetGameState());
        }

        public Task NewGameBlack(Game game)
        {
            _game = game;

            _game.MoveApplied += _game_MoveApplied;

            WhiteCastleKingSide.Visibility = Visibility.Visible;
            WhiteCastleQueenSide.Visibility = Visibility.Visible;
            BlackCastleKingSide.Visibility = Visibility.Visible;
            BlackCastleQueenSide.Visibility = Visibility.Visible;

            FullTurnNumberLabel.Content = _game.FullTurnNumber;

            BoardUserControl.Load(_game, _game.GetGameState());

            return DoSearch();
        }

        private void _game_MoveApplied(object sender, MoveAppliedEventArgs args)
        {
            FullTurnNumberLabel.Content = args.GameState.FullTurnNumber;

            EvaluationLabel.Content = args.GameState.Evaluation;

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
            PositionCountLabel.Content = args.SearchResults.SearchedPositionCount;

            OutputTextBox.Text = args.SearchResults.ToResultsString();
        }

        private Task DoSearch() => _game.CpuMove(5);

        private async void BoardUserControl_PieceMoved(object sender, UserMovedPieceEventArgs args) =>
            await OnPieceMoved(args); 

        private async void BoardUserControl_PieceSelected(object sender, PromotionTypeSelectedEventArgs args) =>
            await OnPieceSelected(args);

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
    }
}
