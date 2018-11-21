using ChessSharp;
using ChessSharp.Enums;
using ChessSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChessSharp_UI
{
    /// <summary>
    /// Interaction logic for Board.xaml
    /// </summary>
    public partial class BoardUserControl : UserControl
    {
        public delegate void UserMovedPieceEventDelegate(object sender, UserMovedPieceEventArgs args);

        public event UserMovedPieceEventDelegate PieceMoved;

        public int FromSquareIndex { get; private set; }

        public int ToSquareIndex { get; private set; }

        private int BorderSizeInPixels = 5;

        private int GridSizeInPixels = 42;

        private int ImageSizeInPixels = 40;

        private Image[] _pieces = new Image[64];

        private Game _currentGame;

        public BoardUserControl()
        {
            InitializeComponent();
        }

        public void Load(Game game)
        {
            _currentGame = game ?? throw new ArgumentNullException(nameof(game));

            _currentGame.MoveApplied += _currentGame_MoveApplied;

            Update(new MoveViewer(0));
        }

        private void Update(MoveViewer move)
        {
            BoardCanvas.Children.Clear();

            var board = _currentGame.GetBitBoard();

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

        private void _currentGame_MoveApplied(object sender, MoveAppliedEventArgs args)
        {
            Update(args.Move);
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
            if (_currentGame == null)
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
            if (_currentGame == null)
                return;

            Point p = e.GetPosition(this);

            double x = p.X - BorderSizeInPixels;
            double y = p.Y - BorderSizeInPixels;

            var file = (int)(x / GridSizeInPixels);
            var rank = (int)(8 - (y / GridSizeInPixels));

            ToSquareIndex = ConvertToSquareIndex(rank, file);

            ToLabel.Content = ToSquareIndex;

            var piece = _currentGame.GetPiece(FromSquareIndex);

            if (piece.Colour != _currentGame.HumanColour)
                return;

            var xy = GetScreenPosition(GridSizeInPixels, (int)rank, (int)file);

            PieceMoved?.Invoke(this, new UserMovedPieceEventArgs(
                FromSquareIndex, ToSquareIndex, UserMovedPieceEventArgs.MoveResult.Ordinary));
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
            rank * 8 + file;

        private int IndexToRank(int squareIndex) =>
            (squareIndex / 8) + 1;

        private int IndexToFile(int squareIndex) =>
            (squareIndex % 8) + 1;
    }
}
