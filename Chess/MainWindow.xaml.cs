using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public partial class MainWindow : Window
    {
        private const int SquarePixels = 90;

        private RankFile StartPosition { get; set; }

        private RankFile EndPosition { get; set; }

        private Match _currentMatch { get; set; }

        private IReadOnlyCollection<ImageMap> Images { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            NextMove.IsEnabled = false;
            PromotionTypeSelector.Visibility = Visibility.Collapsed;
            GameOver.Visibility = Visibility.Collapsed;

            Images = BuildImageMap();
        }

        private void StartNewGame(bool isHuman = false)
        {
            BoardDebug.Clear();

            LogText.Clear();

            GameOver.Visibility = Visibility.Collapsed;

            var board = new Board(new MoveFinder());

            if (isHuman)
                _currentMatch = new Match(board, Colour.White);
            else
                _currentMatch = new Match(board, Colour.None);

            UpdateUI(board);
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            NextMove.IsEnabled = true;

            StartNewGame();
        }

        private void NewGameHuman_Click(object sender, RoutedEventArgs e)
        {
            NextMove.IsEnabled = true;

            StartNewGame(true);
        }

        private void NextMove_Click(object sender, RoutedEventArgs e) =>
            ProcessNextMove();

        private void PromotionTypeSelector_PieceSelected(object sender, PromotionTypeEventArgs args)
        {
            PromotionTypeSelector.Visibility = Visibility.Collapsed;

            ProcessNextMove(args.PieceType);
        }

        private void ProcessNextMove() =>
            ProcessNextMove(PieceType.None);

        private void ProcessNextMove(PieceType promotionType)
        {
            Board chosenBoard = null;

            if (_currentMatch.IsHumanTurn)
            {
                if (promotionType != PieceType.None)
                {
                    // Second entry into function where the promotion type has now been defined
                    chosenBoard = _currentMatch.NextTurn(StartPosition, EndPosition, promotionType);
                }
                else
                {
                    // Try to convert the reuest into a move ans then look to see if that would be a pawn promotion
                    var isPawnPromotion = _currentMatch.CheckForPawnPromotion(StartPosition, EndPosition);

                    if (isPawnPromotion)
                    {
                        // If it is then we have to stop and get the desired promotion type before continuing
                        PromotionTypeSelector.Visibility = Visibility.Visible;
                        return;
                    }

                    // Not pawn promotion so run regular move
                    chosenBoard = _currentMatch.NextTurn(StartPosition, EndPosition);
                }

                // Illegal move
                if (chosenBoard == null)
                    return;
            }
            else
            {
                chosenBoard = _currentMatch.NextTurn();
            }

            var insertText = string.Empty;

            if (chosenBoard.IsCapture)
                insertText = "CAPTURE! ";

            if (chosenBoard.WhiteIsInCheckmate)
                insertText = "***** CHECKMATE - Black wins! ***** ";

            if (chosenBoard.BlackIsInCheckmate)
                insertText = "***** CHECKMATE - White wins! ***** ";

            var moveText = chosenBoard.MoveToString() + Environment.NewLine;

            LogText.AppendText($"{_currentMatch.ThisTurn - 1}: {insertText}{moveText}");

            LogicLog.Text = _currentMatch.GetLastCpuMoveLog();

            UpdateUI(chosenBoard);
        }

        private void UpdateUI(Board board)
        {
            var sb = new StringBuilder();

            //var offBoardPieces = board.OffBoard;

            //if (offBoardPieces.Any()) sb.AppendLine("== Captured ==");

            //foreach(var offBoardPiece in offBoardPieces)
            //{
            //    var image = GetImage(offBoardPiece);
            //    image.Visibility = Visibility.Collapsed;

            //    if (offBoardPiece.Captured)
            //        sb.AppendLine(offBoardPiece.LongName);
            //}

            sb.AppendLine("== Board ==");

            sb.Append(board.BoardToString());

            var squaresWithPiecesOn = board.GetSquaresWithPieceOn().ToList();

            var usedImages = new List<Image>();

            foreach (var square in squaresWithPiecesOn)
            {
                var rankFile = square.ToRankFile();
                //var image = GetImage(square.Piece);
                var image = GetImage(rankFile);

                image.Visibility = Visibility.Visible;

                usedImages.Add(image);

                var xy = GetScreenPosition(SquarePixels, rankFile.Rank, (int)rankFile.File);

                Canvas.SetLeft(image, xy.X);
                Canvas.SetTop(image, xy.Y);
            }

            foreach(var image in Images)
            {
                if (!usedImages.Contains(image.Image))
                    image.Image.Visibility = Visibility.Collapsed;
            }

            WhiteScore.Text = board.WhiteScore.ToString();
            BlackScore.Text = board.BlackScore.ToString();

            BoardDebug.Text += sb.ToString();

            if (board.IsCheckmate)
            {
                GameOver.Visibility = Visibility.Visible;
                NextMove.IsEnabled = false;
            }
        }

        private Point GetScreenPosition(int squarePixels, int rank, int file)
        {
            double x = file * squarePixels;
            double y = (8 - rank) * squarePixels;

            return new Point((int)x, (int)y);
        }

        private void BoardCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_currentMatch == null)
                return;

            UpdateUI(_currentMatch.GetHeadBoard());

            Point p = e.GetPosition(this);
            double x = p.X - BoardCanvas.Margin.Left;
            double y = p.Y - BoardCanvas.Margin.Top;

            var file = x / SquarePixels;
            var rank = 8 - ((y / SquarePixels) - 1);

            StartPosition = RankFile.Get((int)rank, (File)file);

            StartPositionUI.Content = StartPosition;
        }

        private void BoardCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_currentMatch == null)
                return;

            Point p = e.GetPosition(this);
            double x = p.X - BoardCanvas.Margin.Left;
            double y = p.Y - BoardCanvas.Margin.Top;

            var file = x / SquarePixels;
            var rank = 8 - ((y / SquarePixels) - 1);

            EndPosition = RankFile.Get((int)rank, (File)file);

            EndPositionUI.Content = EndPosition;

            Image image = GetImage(StartPosition);

            if (image == null)
                return;

            var xy = GetScreenPosition(SquarePixels, (int)rank, (int)file);

            Canvas.SetLeft(image, xy.X);
            Canvas.SetTop(image, xy.Y);
        }

        private Image GetImage(RankFile rankFile)
        {
            if (_currentMatch == null)
                return null;

            var colour = _currentMatch.GetPieceOnSquareColour(rankFile);

            if (colour == Colour.None)
                return null;

            var type = _currentMatch.GetPieceOnSquareType(rankFile);

            if (type == PieceType.None)
                return null;

            var instanceNumber = _currentMatch.GetInstanceNumber(colour, type, rankFile.ToSquareFlag());

            if (instanceNumber < 1)
                throw new Exception($"Failed to find any {colour} pieces of type {type}");

            return Images.Where(x => x.Colour == colour && x.PieceType == type)
                .Skip(instanceNumber - 1)
                .First()
                .Image;
        }

        //private Image GetImage(IList<ImageMap> images, RankFile rankFile)
        //{
        //    var type = _currentMatch?.GetPieceOnSquareType(rankFile);
        //    var colour = _currentMatch?.GetPieceOnSquareColour(rankFile);

        //    GetImage(_currentMatch?.GetPieceOnSquare(rankFile));
        //}

        //private Image GetImage(IList<ImageMap> remainingImages, Colour colour, PieceType type) =>
        //    remainingImages.First(x => x.Colour == colour && x.PieceType == type).Image;

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

        //private Image GetImage(Piece piece)
        //{
        //    switch (piece?.FullName)
        //    {
        //        case "WhitePawna2": return WhitePawn1;
        //        case "WhitePawnb2": return WhitePawn2;
        //        case "WhitePawnc2": return WhitePawn3;
        //        case "WhitePawnd2": return WhitePawn4;
        //        case "WhitePawne2": return WhitePawn5;
        //        case "WhitePawnf2": return WhitePawn6;
        //        case "WhitePawng2": return WhitePawn7;
        //        case "WhitePawnh2": return WhitePawn8;
        //        case "WhiteRooka1": return WhiteRook1;
        //        case "WhiteKnightb1": return WhiteKnight1;
        //        case "WhiteBishopc1": return WhiteBishop1;
        //        case "WhiteQueend1": return WhiteQueen;
        //        case "WhiteKinge1": return WhiteKing;
        //        case "WhiteBishopf1": return WhiteBishop2;
        //        case "WhiteKnightg1": return WhiteKnight2;
        //        case "WhiteRookh1": return WhiteRook2;
        //        case "BlackPawna7": return BlackPawn1;
        //        case "BlackPawnb7": return BlackPawn2;
        //        case "BlackPawnc7": return BlackPawn3;
        //        case "BlackPawnd7": return BlackPawn4;
        //        case "BlackPawne7": return BlackPawn5;
        //        case "BlackPawnf7": return BlackPawn6;
        //        case "BlackPawng7": return BlackPawn7;
        //        case "BlackPawnh7": return BlackPawn8;
        //        case "BlackRooka8": return BlackRook1;
        //        case "BlackKnightb8": return BlackKnight1;
        //        case "BlackBishopc8": return BlackBishop1;
        //        case "BlackQueend8": return BlackQueen;
        //        case "BlackKinge8": return BlackKing;
        //        case "BlackBishopf8": return BlackBishop2;
        //        case "BlackKnightg8": return BlackKnight2;
        //        case "BlackRookh8": return BlackRook2;

        //        case "WhiteRooka3": return WhiteRookPromotion1;
        //        case "WhiteRookb3": return WhiteRookPromotion2;
        //        case "WhiteKnightc3": return WhiteKnightPromotion1;
        //        case "WhiteKnightd3": return WhiteKnightPromotion2;
        //        case "WhiteBishope3": return WhiteBishopPromotion1;
        //        case "WhiteBishopf3": return WhiteBishopPromotion2;
        //        case "WhiteQueeng3": return WhiteQueenPromotion1;
        //        case "WhiteQueenh3": return WhiteQueenPromotion2;

        //        case "BlackRooka6": return BlackRookPromotion1;
        //        case "BlackRookb6": return BlackRookPromotion2;
        //        case "BlackKnightc6": return BlackKnightPromotion1;
        //        case "BlackKnightd6": return BlackKnightPromotion2;
        //        case "BlackBishope6": return BlackBishopPromotion1;
        //        case "BlackBishopf6": return BlackBishopPromotion2;
        //        case "BlackQueeng6": return BlackQueenPromotion1;
        //        case "BlackQueenh6": return BlackQueenPromotion2;
        //    }

        //    return null;
        //}
    }
}
