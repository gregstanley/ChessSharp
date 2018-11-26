using ChessSharp.Engine;
using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using System.Threading.Tasks;
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

            HalfTurnNumberLabel.Content = _game.FullTurnNumber;

            BoardUserControl.Load(_game, _game.GetGameState());
        }

        public Task NewGameBlack(Game game)
        {
            _game = game;

            HalfTurnNumberLabel.Content = _game.FullTurnNumber;

            BoardUserControl.Load(_game, _game.GetGameState());

            return DoSearch();
        }

        private Task DoSearch() => _game.CpuMove(3);

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
