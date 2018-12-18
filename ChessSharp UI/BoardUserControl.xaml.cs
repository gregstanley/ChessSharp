using ChessSharp;
using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using ChessSharp.Extensions;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

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

        public BoardUserControl()
        {
            InitializeComponent();

            InvalidMoveLabel.Visibility = Visibility.Collapsed;
            ThinkingLabel.Visibility = Visibility.Collapsed;
            CheckmateLabel.Visibility = Visibility.Collapsed;

            PromotionUserControl.Visibility = Visibility.Collapsed;
        }

        public delegate void UserMovedPieceEventDelegate(object sender, UserMovedPieceEventArgs args);

        public delegate void PromotionTypeSelectedEventDelegate(object sender, PromotionTypeSelectedEventArgs args);

        public event UserMovedPieceEventDelegate PieceMoved;

        public event PromotionTypeSelectedEventDelegate PromotionTypeSelected;

        public int FromSquareIndex { get; private set; }

        public int ToSquareIndex { get; private set; }

        public void Load(IGameEventBroadcaster gameEvents, GameState gameState)
        {
            this.gameEvents = gameEvents ?? throw new ArgumentNullException(nameof(gameEvents));

            this.gameEvents.InvalidMove += gameEvents_InvalidMove;
            this.gameEvents.PromotionTypeRequired += gameEvents_PromotionTypeRequired;
            this.gameEvents.SearchStarted += gameEvents_SearchStarted;
            this.gameEvents.SearchCompleted += gameEvents_SearchCompleted;
            this.gameEvents.MoveApplied += gameEvents_MoveApplied;
            this.gameEvents.Checkmate += gameEvents_Checkmate;

            PromotionUserControl.PromotionTypeSelected += PromotionUserControl_PromotionTypeSelected;

            Update(new MoveViewer(0), gameState);
        }

        private void gameEvents_InvalidMove(object sender, InvalidMoveEventArgs args) =>
            InvalidMoveLabel.Visibility = Visibility.Visible;

        private void gameEvents_PromotionTypeRequired(object sender, PromotionTypeRequiredEventArgs args) =>
            PromotionUserControl.Open(args);

        private void gameEvents_SearchStarted(object sender, EventArgs args)
        {
            isThinking = true;

            ThinkingLabel.Visibility = Visibility.Visible;
        }

        private void gameEvents_SearchCompleted(object sender, EventArgs args)
        {
            isThinking = false;

            ThinkingLabel.Visibility = Visibility.Collapsed;
        }

        private void gameEvents_MoveApplied(object sender, MoveAppliedEventArgs args) =>
            Update(args.Move, args.GameState);

        private void gameEvents_Checkmate(object sender, EventArgs args)
        {
            CheckmateLabel.Visibility = Visibility.Visible;
        }

        private void PromotionUserControl_PromotionTypeSelected(object sender, PromotionTypeSelectedEventArgs args) =>
            PromotionTypeSelected?.Invoke(this, args);

        private void Update(MoveViewer move, GameState gameState)
        {
            currentGameState = gameState ?? throw new ArgumentNullException(nameof(gameState));

            isThinking = false;

            InvalidMoveLabel.Visibility = Visibility.Collapsed;
            ThinkingLabel.Visibility = Visibility.Collapsed;
            CheckmateLabel.Visibility = Visibility.Collapsed;

            BoardCanvas.Children.Clear();

            var board = currentGameState;

            AddPieces(Colour.White, PieceType.Pawn, board.WhitePawns, GridSizeInPixels);
            AddPieces(Colour.White, PieceType.Rook, board.WhiteRooks, GridSizeInPixels);
            AddPieces(Colour.White, PieceType.Knight, board.WhiteKnights, GridSizeInPixels);
            AddPieces(Colour.White, PieceType.Bishop, board.WhiteBishops, GridSizeInPixels);
            AddPieces(Colour.White, PieceType.Queen, board.WhiteQueens, GridSizeInPixels);
            AddPieces(Colour.White, PieceType.King, board.WhiteKing, GridSizeInPixels);
            AddPieces(Colour.Black, PieceType.Pawn, board.BlackPawns, GridSizeInPixels);
            AddPieces(Colour.Black, PieceType.Rook, board.BlackRooks, GridSizeInPixels);
            AddPieces(Colour.Black, PieceType.Knight, board.BlackKnights, GridSizeInPixels);
            AddPieces(Colour.Black, PieceType.Bishop, board.BlackBishops, GridSizeInPixels);
            AddPieces(Colour.Black, PieceType.Queen, board.BlackQueens, GridSizeInPixels);
            AddPieces(Colour.Black, PieceType.King, board.BlackKing, GridSizeInPixels);
        }

        private void AddPieces(Colour colour, PieceType pieceType, SquareFlag squares, int gridSize)
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
            if (isThinking)
                return;

            Point p = e.GetPosition(this);

            double x = p.X - BorderSizeInPixels;
            double y = p.Y - BorderSizeInPixels;

            var file = (int)(x / GridSizeInPixels);
            var rank = (int)(8 - (y / GridSizeInPixels));

            FromSquareIndex = ConvertToSquareIndex(rank, file);

            FromLabel.Content = FromSquareIndex;
        }
        
        private void Board_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isThinking)
                return;

            Point p = e.GetPosition(this);

            double x = p.X - BorderSizeInPixels;
            double y = p.Y - BorderSizeInPixels;

            var file = (int)(x / GridSizeInPixels);
            var rank = (int)(8 - (y / GridSizeInPixels));

            ToSquareIndex = ConvertToSquareIndex(rank, file);

            ToLabel.Content = ToSquareIndex;

            PieceMoved?.Invoke(this, new UserMovedPieceEventArgs(FromSquareIndex, ToSquareIndex));
        }

        private Point GetScreenPosition(int gridSize, int rank, int file)
        {
            double x = (file - 1) * gridSize;
            double y = (8 - rank) * gridSize;

            var squarePadding = (GridSizeInPixels - ImageSizeInPixels) / 2;

            var padding = BorderSizeInPixels + squarePadding;

            return new Point(x + padding, y + padding);
        }

        private int ConvertToSquareIndex(int rank, int file) =>
            (rank * 8) + file;

        private int IndexToRank(int squareIndex) =>
            (squareIndex / 8) + 1;

        private int IndexToFile(int squareIndex) =>
            (squareIndex % 8) + 1;
    }
}
