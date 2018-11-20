using ChessSharp;
using ChessSharp.Enums;
using ChessSharp.Extensions;
using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChessSharp_UI
{
    public partial class MainWindow : Window
    {
        private const int SquarePixels = 90;

        private Game _currentGame;

        private IReadOnlyCollection<ImageMap> _images { get; set; }

        private bool _isThinking { get; set; } = false;

        private int _fromSquareIndex;

        private int _toSquareIndex;

        public MainWindow()
        {
            InitializeComponent();

            _images = BuildImageMap();

            Reset();
        }

        private void Reset()
        {
            _isThinking = false;
            CheckmateUi.Visibility = Visibility.Collapsed;
            ThinkingUI.Visibility = Visibility.Collapsed;
            PromotionTypeSelector.Visibility = Visibility.Collapsed;
        }

        private void BtnNewGameWhite_Click(object sender, RoutedEventArgs e)
        {
            var bitBoard = new BitBoard();

            _currentGame = new Game(bitBoard, Colour.White);

            Reset();

            UpdateUI();

            BoardUserControl.Load(_currentGame);
        }

        private void BtnNewGameBlack_Click(object sender, RoutedEventArgs e)
        {
            var bitBoard = new BitBoard();

            _currentGame = new Game(bitBoard, Colour.Black);

            Reset();

            UpdateUI();

            BoardUserControl.Load(_currentGame);

            DoSearch();
        }

        private void BoardCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentGame == null)
                return;

            if (_isThinking)
                return;

            Point p = e.GetPosition(this);
            double x = p.X - BoardCanvas.Margin.Left;
            double y = p.Y - BoardCanvas.Margin.Top;

            var file = x / SquarePixels;
            var rank = 8 - ((y / SquarePixels) - 1);

            _fromSquareIndex = ToSquareIndex((int)rank, (int)file);

            StartPositionUI.Content = _fromSquareIndex;
        }

        private void BoardCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_currentGame == null)
                return;

            if (_isThinking)
                return;

            Point p = e.GetPosition(this);
            double x = p.X - BoardCanvas.Margin.Left;
            double y = p.Y - BoardCanvas.Margin.Top;

            var file = x / SquarePixels;
            var rank = 8 - ((y / SquarePixels) - 1);

            _toSquareIndex = ToSquareIndex((int)rank, (int)file);

            EndPositionUI.Content = _toSquareIndex;

            Image image = GetImage(_fromSquareIndex);

            if (image == null)
                return;

            var xy = GetScreenPosition(SquarePixels, (int)rank, (int)file);

            var result = ProcessNextMove(PieceType.None);

            if (result.IsFailure)
            {
                if (result.Error == "Pawn promotion")
                    PromotionTypeSelector.Visibility = Visibility.Visible;

                return;
            }

            DoSearch();
        }

        private void PromotionTypeSelector_PieceSelected(object sender, PromotionTypeEventArgs args)
        {
            PromotionTypeSelector.Visibility = Visibility.Collapsed;

            var result = ProcessNextMove(args.PieceType);

            DoSearch();
        }

        private void DoSearch()
        {
            UpdateUI();

            _isThinking = true;

            var result = ProcessNextMove(PieceType.None);

            _isThinking = false;

            UpdateUI();
        }

        private Result<MoveViewer> ProcessNextMove(PieceType promotionType)
        {
            MoveViewer move = null;

            if (!_currentGame.IsHumanTurn)
            {
                move = _currentGame.CpuMove();

                if (move.Value == 0)
                    Result.Fail<MoveViewer>("No moves available");

                return Result.Ok(move);
            }

            if (promotionType != PieceType.None)
            {
                // Second entry into function where the promotion type has now been defined
                move = _currentGame.TryApplyMove(_fromSquareIndex, _toSquareIndex, promotionType);

                if (move.Value == 0)
                    return Result.Fail<MoveViewer>("Invalid");

                return Result.Ok(move);
            }

            // Try to convert the request into a move and then look to see if that would be a pawn promotion
            var isPawnPromotion = _currentGame.IsMovePromotion(_fromSquareIndex, _toSquareIndex);

            if (isPawnPromotion)
            {
                // If it is then we have to stop and get the desired promotion type before continuing
                return Result.Fail<MoveViewer>("Pawn promotion");
            }

            move = _currentGame.TryApplyMove(_fromSquareIndex, _toSquareIndex, promotionType);

            if (move.Value == 0)
                return Result.Fail<MoveViewer>("Invalid");

            return Result.Ok(move);
        }

        private int ToSquareIndex(int rank, int file) =>
            ((rank - 1) * 8) + file;

        private int IndexToRank(int squareIndex) =>
            (squareIndex / 8) + 1;

        private int IndexToFile(int squareIndex) =>
            (squareIndex % 8) + 1;

        private void UpdateUI()
        {
            ThinkingUI.Visibility = _isThinking ? Visibility.Visible : Visibility.Collapsed;

            var squaresWithPiecesOn = _currentGame.GetSquaresWithPieceOn().ToList();

            var usedImages = new List<Image>();

            foreach (var square in squaresWithPiecesOn)
            {
                var squareIndex = square.ToSquareIndex();
                var image = GetImage(squareIndex);

                if (image == null)
                {
                    var bp = true;
                }
                else
                {
                    image.Visibility = Visibility.Visible;

                    usedImages.Add(image);

                    var rank = IndexToRank(squareIndex);
                    var file = IndexToFile(squareIndex);

                    var xy = GetScreenPosition(SquarePixels, rank, file);

                    Canvas.SetLeft(image, xy.X);
                    Canvas.SetTop(image, xy.Y);
                }
            }

            foreach (var image in _images)
            {
                if (!usedImages.Contains(image.Image))
                    image.Image.Visibility = Visibility.Collapsed;
            }

            if (!_currentGame.AvailableMoves.Any())
                CheckmateUi.Visibility = Visibility.Visible;

            BoardUserControl.Load(_currentGame);
        }

        private Image GetImage(int squareIndex)
        {
            if (_currentGame == null)
                return null;

            var piece = _currentGame.GetPiece(squareIndex);

            if (piece.Colour == Colour.None)
                return null;

            if (piece.Type == PieceType.None)
                return null;

            var square = squareIndex.ToSquareFlag();

            var instanceNumber = _currentGame.GetInstanceNumber(piece, square);

            if (instanceNumber < 1)
                throw new Exception($"Failed to find any {piece.Colour} pieces of type {piece.Type}");

            return _images.Where(x => x.Colour == piece.Colour && x.PieceType == piece.Type)
                .Skip(instanceNumber - 1)
                .First()
                .Image;
        }
        
        private Point GetScreenPosition(int squarePixels, int rank, int file)
        {
            double x = (file - 1) * squarePixels;
            double y = (8 - rank) * squarePixels;

            return new Point((int)x, (int)y);
        }

        private IReadOnlyCollection<ImageMap> BuildImageMap()
        {
            var imageMaps = new List<ImageMap>
            {
                new ImageMap(Colour.White, PieceType.Pawn, WhitePawn1),
                new ImageMap(Colour.White, PieceType.Pawn, WhitePawn2),
                new ImageMap(Colour.White, PieceType.Pawn, WhitePawn3),
                new ImageMap(Colour.White, PieceType.Pawn, WhitePawn4),
                new ImageMap(Colour.White, PieceType.Pawn, WhitePawn5),
                new ImageMap(Colour.White, PieceType.Pawn, WhitePawn6),
                new ImageMap(Colour.White, PieceType.Pawn, WhitePawn7),
                new ImageMap(Colour.White, PieceType.Pawn, WhitePawn8),
                new ImageMap(Colour.White, PieceType.Rook, WhiteRook1),
                new ImageMap(Colour.White, PieceType.Rook, WhiteRook2),
                new ImageMap(Colour.White, PieceType.Rook, WhiteRookPromotion1),
                new ImageMap(Colour.White, PieceType.Rook, WhiteRookPromotion2),
                new ImageMap(Colour.White, PieceType.Knight, WhiteKnight1),
                new ImageMap(Colour.White, PieceType.Knight, WhiteKnight2),
                new ImageMap(Colour.White, PieceType.Knight, WhiteKnightPromotion1),
                new ImageMap(Colour.White, PieceType.Knight, WhiteKnightPromotion2),
                new ImageMap(Colour.White, PieceType.Bishop, WhiteBishop1),
                new ImageMap(Colour.White, PieceType.Bishop, WhiteBishop2),
                new ImageMap(Colour.White, PieceType.Bishop, WhiteBishopPromotion1),
                new ImageMap(Colour.White, PieceType.Bishop, WhiteBishopPromotion2),
                new ImageMap(Colour.White, PieceType.Queen, WhiteQueen),
                new ImageMap(Colour.White, PieceType.Queen, WhiteQueenPromotion1),
                new ImageMap(Colour.White, PieceType.Queen, WhiteQueenPromotion2),
                new ImageMap(Colour.White, PieceType.King, WhiteKing),
                new ImageMap(Colour.Black, PieceType.Pawn, BlackPawn1),
                new ImageMap(Colour.Black, PieceType.Pawn, BlackPawn2),
                new ImageMap(Colour.Black, PieceType.Pawn, BlackPawn3),
                new ImageMap(Colour.Black, PieceType.Pawn, BlackPawn4),
                new ImageMap(Colour.Black, PieceType.Pawn, BlackPawn5),
                new ImageMap(Colour.Black, PieceType.Pawn, BlackPawn6),
                new ImageMap(Colour.Black, PieceType.Pawn, BlackPawn7),
                new ImageMap(Colour.Black, PieceType.Pawn, BlackPawn8),
                new ImageMap(Colour.Black, PieceType.Rook, BlackRook1),
                new ImageMap(Colour.Black, PieceType.Rook, BlackRook2),
                new ImageMap(Colour.Black, PieceType.Rook, BlackRookPromotion1),
                new ImageMap(Colour.Black, PieceType.Rook, BlackRookPromotion2),
                new ImageMap(Colour.Black, PieceType.Knight, BlackKnight1),
                new ImageMap(Colour.Black, PieceType.Knight, BlackKnight2),
                new ImageMap(Colour.Black, PieceType.Knight, BlackKnightPromotion1),
                new ImageMap(Colour.Black, PieceType.Knight, BlackKnightPromotion2),
                new ImageMap(Colour.Black, PieceType.Bishop, BlackBishop1),
                new ImageMap(Colour.Black, PieceType.Bishop, BlackBishop2),
                new ImageMap(Colour.Black, PieceType.Bishop, BlackBishopPromotion1),
                new ImageMap(Colour.Black, PieceType.Bishop, BlackBishopPromotion2),
                new ImageMap(Colour.Black, PieceType.Queen, BlackQueen),
                new ImageMap(Colour.Black, PieceType.Queen, BlackQueenPromotion1),
                new ImageMap(Colour.Black, PieceType.Queen, BlackQueenPromotion2),
                new ImageMap(Colour.Black, PieceType.King, BlackKing)
            };

            return imageMaps;
        }

        private void BoardUserControl_PieceMoved(object sender, UserMovedPieceEventArgs args)
        {
            _fromSquareIndex = args.FromSquareIndex;
            _toSquareIndex = args.ToSquareIndex;

            var result = ProcessNextMove(PieceType.None);

            if (result.IsFailure)
            {
                if (result.Error == "Pawn promotion")
                    PromotionTypeSelector.Visibility = Visibility.Visible;

                return;
            }

            DoSearch();
        }
    }
}
