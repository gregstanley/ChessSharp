using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ChessSharp.Common;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Extensions;
using ChessSharp.Engine;
using ChessSharp.Engine.Events;

namespace ChessSharp_UI
{
    public partial class BoardUserControl : UserControl
    {
        private const int BorderSizeInPixels = 5;

        private const int GridSizeInPixels = 42;

        private const int ImageSizeInPixels = 40;

        private IGameEventBroadcaster gameEvents;

        private GameState currentGameState;

        private bool isThinking = false;

        private readonly DispatcherTimer dispatcherTimer = new();

        public BoardUserControl()
        {
            this.InitializeComponent();

            IllegalMoveLabel.Visibility = Visibility.Collapsed;
            ThinkingLabel.Visibility = Visibility.Collapsed;
            DrawLabel.Visibility = Visibility.Collapsed;
            CheckmateLabel.Visibility = Visibility.Collapsed;

            PromotionUserControl.Visibility = Visibility.Collapsed;

            this.dispatcherTimer.Tick += this.DispatcherTimer_Tick;

            this.dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        }

        public delegate void UserMovedPieceEventDelegate(object sender, UserMovedPieceEventArgs args);

        public delegate void PromotionTypeSelectedEventDelegate(object sender, PromotionTypeSelectedEventArgs args);

        public event UserMovedPieceEventDelegate PieceMoved;

        public event PromotionTypeSelectedEventDelegate PromotionTypeSelected;

        public int FromSquareIndex { get; private set; }

        public int ToSquareIndex { get; private set; }

        public void Load(IGame game)
        {
            this.gameEvents = game ?? throw new ArgumentNullException(nameof(game));

            this.gameEvents.InvalidMove += this.GameEvents_InvalidMove;
            this.gameEvents.PromotionTypeRequired += this.GameEvents_PromotionTypeRequired;
            this.gameEvents.SearchStarted += this.GameEvents_SearchStarted;
            this.gameEvents.SearchCompleted += this.GameEvents_SearchCompleted;
            this.gameEvents.MoveApplied += this.GameEvents_MoveApplied;
            this.gameEvents.Draw += this.GameEvents_Draw;
            this.gameEvents.Checkmate += this.GameEvents_Checkmate;

            this.PromotionUserControl.PromotionTypeSelected += this.PromotionUserControl_PromotionTypeSelected;

            this.Update(game.CurrentState.GameState);
        }

        private void GameEvents_InvalidMove(object sender, InvalidMoveEventArgs args)
        {
            IllegalMoveLabel.Visibility = Visibility.Visible;

            this.dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.dispatcherTimer.Stop();

            IllegalMoveLabel.Visibility = Visibility.Collapsed;
        }

        private void GameEvents_PromotionTypeRequired(object sender, PromotionTypeRequiredEventArgs args)
        {
            PromotionUserControl.Open(args);
        }

        private void GameEvents_SearchStarted(object sender, EventArgs args)
        {
            this.isThinking = true;

            ThinkingLabel.Visibility = Visibility.Visible;
        }

        private void GameEvents_SearchCompleted(object sender, EventArgs args)
        {
            this.isThinking = false;

            ThinkingLabel.Visibility = Visibility.Collapsed;
        }

        private void GameEvents_MoveApplied(object sender, MoveAppliedEventArgs args)
        {
            this.Update(args.GameState);
        }

        private void GameEvents_Draw(object sender, MoveAppliedEventArgs args)
        {
            this.Update(args.GameState);

            DrawLabel.Visibility = Visibility.Visible;
        }

        private void GameEvents_Checkmate(object sender, MoveAppliedEventArgs args)
        {
            this.Update(args.GameState);

            CheckmateLabel.Visibility = Visibility.Visible;
        }

        private void PromotionUserControl_PromotionTypeSelected(object sender, PromotionTypeSelectedEventArgs args)
        {
            this.PromotionTypeSelected?.Invoke(this, args);
        }

        private void Update(GameState gameState)
        {
            this.currentGameState = gameState ?? throw new ArgumentNullException(nameof(gameState));

            this.isThinking = false;

            IllegalMoveLabel.Visibility = Visibility.Collapsed;
            ThinkingLabel.Visibility = Visibility.Collapsed;
            DrawLabel.Visibility = Visibility.Collapsed;
            CheckmateLabel.Visibility = Visibility.Collapsed;

            BoardCanvas.Children.Clear();

            var board = this.currentGameState;

            this.AddPieces(Colour.White, PieceType.Pawn, board.WhitePawns);
            this.AddPieces(Colour.White, PieceType.Rook, board.WhiteRooks);
            this.AddPieces(Colour.White, PieceType.Knight, board.WhiteKnights);
            this.AddPieces(Colour.White, PieceType.Bishop, board.WhiteBishops);
            this.AddPieces(Colour.White, PieceType.Queen, board.WhiteQueens);
            this.AddPieces(Colour.White, PieceType.King, board.WhiteKing);
            this.AddPieces(Colour.Black, PieceType.Pawn, board.BlackPawns);
            this.AddPieces(Colour.Black, PieceType.Rook, board.BlackRooks);
            this.AddPieces(Colour.Black, PieceType.Knight, board.BlackKnights);
            this.AddPieces(Colour.Black, PieceType.Bishop, board.BlackBishops);
            this.AddPieces(Colour.Black, PieceType.Queen, board.BlackQueens);
            this.AddPieces(Colour.Black, PieceType.King, board.BlackKing);
        }

        private void AddPieces(Colour colour, PieceType pieceType, SquareFlag squares)
        {
            var dropShadowEffect = new DropShadowEffect
            {
                BlurRadius = 2,
                Direction = 315,
                Opacity = 0.5,
                ShadowDepth = 2
            };

            foreach (var square in squares.ToList())
            {
                var squareIndex = square.ToSquareIndex();

                var pieceName = pieceType.ToString();

                var imagePath = $"Images/Icons-40x40-{colour}{pieceName}.png";

                var bitmapImage = new BitmapImage(new Uri(imagePath, UriKind.Relative));

                var image = new Image
                {
                    Source = bitmapImage,
                    Width = ImageSizeInPixels,
                    Height = ImageSizeInPixels,
                    Opacity = 0.95,
                    Effect = dropShadowEffect
                };

                BoardCanvas.Children.Add(image);

                image.Visibility = Visibility.Visible;

                var rank = IndexToRank(squareIndex);
                var file = IndexToFile(squareIndex);

                var xy = GetScreenPosition(GridSizeInPixels, rank, file);

                Canvas.SetLeft(image, xy.X);
                Canvas.SetTop(image, xy.Y);
            }
        }

        private void Board_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.isThinking)
                return;

            Point p = e.GetPosition(this);

            double x = p.X - BorderSizeInPixels;
            double y = p.Y - BorderSizeInPixels;

            var file = (int)(x / GridSizeInPixels);
            var rank = (int)(8 - (y / GridSizeInPixels));

            this.FromSquareIndex = ConvertToSquareIndex(rank, file);

            FromLabel.Content = this.FromSquareIndex;
        }

        private void Board_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.isThinking)
                return;

            Point p = e.GetPosition(this);

            double x = p.X - BorderSizeInPixels;
            double y = p.Y - BorderSizeInPixels;

            var file = (int)(x / GridSizeInPixels);
            var rank = (int)(8 - (y / GridSizeInPixels));

            this.ToSquareIndex = ConvertToSquareIndex(rank, file);

            ToLabel.Content = this.ToSquareIndex;

            this.PieceMoved?.Invoke(this, new UserMovedPieceEventArgs(this.FromSquareIndex, this.ToSquareIndex));
        }

        private static Point GetScreenPosition(int gridSize, int rank, int file)
        {
            double x = (file - 1) * gridSize;
            double y = (8 - rank) * gridSize;

            var squarePadding = (GridSizeInPixels - ImageSizeInPixels) / 2;

            var padding = BorderSizeInPixels + squarePadding;

            return new Point(x + padding, y + padding);
        }

        private static int ConvertToSquareIndex(int rank, int file)
        {
            return (rank * 8) + file;
        }

        private static int IndexToRank(int squareIndex)
        {
            return (squareIndex / 8) + 1;
        }

        private static int IndexToFile(int squareIndex)
        {
            return (squareIndex % 8) + 1;
        }
    }
}
