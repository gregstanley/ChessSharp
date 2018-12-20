using ChessSharp;
using ChessSharp.Engine;
using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using ChessSharp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChessSharp_UI
{
    public partial class GameUserControl : UserControl
    {
        private TranspositionTable transpositionTable = new TranspositionTable();

        public GameUserControl()
        {
            InitializeComponent();
        }

        public Game Game { get; set; }

        public void NewGameWhite(Game game) =>
            NewGame(game);

        public Task NewGameBlack(Game game)
        {
            NewGame(game);

            return DoSearch();
        }

        private void NewGame(Game game)
        {
            if (Game != null)
            {
                Game.MoveApplied -= Game_MoveApplied;
                Game.SearchCompleted -= Game_SearchCompleted;
                Game.Info -= Game_Info;
            }

            if (game != null)
            {
                Game = game;
            }
            else
            {
                Game = new Game(new BitBoard(), transpositionTable, Colour.White);
            }

            Game.MoveApplied += Game_MoveApplied;
            Game.SearchCompleted += Game_SearchCompleted;
            Game.Info += Game_Info;

            WhiteCastleKingSide.Visibility = Visibility.Visible;
            WhiteCastleQueenSide.Visibility = Visibility.Visible;
            BlackCastleKingSide.Visibility = Visibility.Visible;
            BlackCastleQueenSide.Visibility = Visibility.Visible;

            FullTurnNumberLabel.Content = this.Game.FullTurn;

            BoardUserControl.Load(this.Game, this.Game.GetGameState());
        }

        private void Game_Info(object sender, InfoEventArgs args)
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

        private void Game_MoveApplied(object sender, MoveAppliedEventArgs args)
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

            var history = Game.History
                .Reverse()
                .Skip(1)
                .Select(x => x);

            var sb = new StringBuilder();

            var count = 0;

            MovesListBox.Items.Clear();

            TurnItemUserControl item = null;

            foreach (var snapshot in history)
            {
                var notation = new MoveViewer(snapshot.Move).GetNotation();

                if (count % 2 == 0)
                {
                    var turnNumber = (count / 2) + 1;

                    item = new TurnItemUserControl();

                    item.BlackMoveButton.Visibility = Visibility.Collapsed;

                    item.TurnNumberLabel.Content = turnNumber;

                    sb.Append($"{turnNumber}.");

                    item.WhiteMoveButton.Content = notation;

                    MovesListBox.Items.Add(item);
                }
                else
                {
                    //if (item == null)
                    //    item = new TurnItemUserControl();
                    item.BlackMoveButton.Visibility = Visibility.Visible;

                    item.BlackMoveButton.Content = notation;
                }

                sb.Append($" {notation}");

                if (count % 2 == 1)
                    sb.AppendLine();

                ++count;
            }

            MovesListBox.SelectedIndex = MovesListBox.Items.Count - 1;
            MovesListBox.ScrollIntoView(MovesListBox.SelectedItem);

            //GameHistoryTextBox.Text = sb.ToString();
        }

        private void Game_SearchCompleted(object sender, SearchCompleteEventArgs args)
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
            var move = Game.TryMove(args.FromSquareIndex, args.ToSquareIndex, PieceType.None);

            return move.Value == 0 ? Task.CompletedTask : DoSearch();
        }

        private Task OnPieceSelected(PromotionTypeSelectedEventArgs args)
        {
            var move = Game.TryMove(args.FromSquareIndex, args.ToSquareIndex, args.PieceType);

            return move.Value == 0 ? Task.CompletedTask : DoSearch();
        }

        private Task NewGameFromFen()
        {
            var gameState = FenHelpers.Parse(FenTextBox.Text);

            var board = BitBoard.FromGameState(gameState);

            var game = new Game(board, transpositionTable, Colour.White);

            if (gameState.ToPlay == Colour.White)
            {
                NewGameWhite(game);

                return Task.CompletedTask;
            }

            return NewGameBlack(game);
        }

        private async Task DoSearch()
        {
            await Game.CpuMove(5);

            FenTextBox.Text = FenHelpers.ToFen(Game.GetGameState());

            // var ttUsage = _transpositionTable.VerfiyUsage();
        }
    }
}
