using Chess.Engine;
using Chess.Engine.Ai;
using Chess.Engine.Ai.Searches;
using Chess.Engine.Extensions;
using Chess.Engine.Models;
using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            NextMove.Visibility = Visibility.Collapsed;
            PromotionTypeSelector.Visibility = Visibility.Collapsed;
            CheckmateUi.Visibility = Visibility.Collapsed;

            Images = BuildImageMap();
        }

        private void StartNewGame(CpuPlayer cpuPlayer, bool isHuman = false)
        {
            BoardDebug.Clear();

            LogText.Clear();

            CheckmateUi.Visibility = Visibility.Collapsed;

            var board = new Board();

            if (isHuman)
                _currentMatch = new Match(board, cpuPlayer, Colour.White);
            else
                _currentMatch = new Match(board, cpuPlayer, Colour.None);

            UpdateUI(board);
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            NextMove.IsEnabled = true;
            NextMove.Visibility = Visibility.Visible;

            StartNewGame(new CpuPlayer(new AlphaBeta()));
        }

        private void NewGameHuman_Click(object sender, RoutedEventArgs e)
        {
            NextMove.IsEnabled = false;
            NextMove.Visibility = Visibility.Collapsed;

            StartNewGame(new CpuPlayer(new RandomMove()), true);
        }

        private void NewGameHumanHard_Click(object sender, RoutedEventArgs e)
        {
            NextMove.IsEnabled = false;
            NextMove.Visibility = Visibility.Collapsed;

            StartNewGame(new CpuPlayer(new AlphaBeta()), true);
        }

        private void NextMove_Click(object sender, RoutedEventArgs e) =>
            ProcessNextMove();

        private void PromotionTypeSelector_PieceSelected(object sender, PromotionTypeEventArgs args)
        {
            PromotionTypeSelector.Visibility = Visibility.Collapsed;

            //ProcessNextMove(args.PieceType);
            RunTasks(args.PieceType);
        }

        private Result<Board> ProcessNextMove() =>
            ProcessNextMove(PieceType.None);

        private Result<Board> ProcessNextMove(PieceType promotionType)
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
                    var isLegal = _currentMatch.IsLegalMove(StartPosition, EndPosition, PieceType.None);

                    // Illegal move
                    if (!isLegal)
                        return Result.Fail<Board>("Illegal move");

                    // Try to convert the request into a move and then look to see if that would be a pawn promotion
                    var isPawnPromotion = _currentMatch.CheckForPawnPromotion(StartPosition, EndPosition);

                    if (isPawnPromotion)
                    {
                        // If it is then we have to stop and get the desired promotion type before continuing
                        return Result.Fail<Board>("Pawn promotion");
                    }

                    chosenBoard = _currentMatch.NextTurn(StartPosition, EndPosition);
                }
            }
            else
            {
                chosenBoard = _currentMatch.NextTurn();
            }

            if (chosenBoard == null)
                return Result.Fail<Board>("Checkmate");

            return Result.Ok(chosenBoard);
        }

        private void GameOver(Colour winner)
        {
            CheckmateUi.Visibility = Visibility.Visible;
            NextMove.IsEnabled = false;

            LogText.AppendText($"***** CHECKMATE - {winner} wins! *****");
        }

        private void UpdateUI(Board board)
        {
            var checkBoards = board.ChildBoards.Where(x => x.WhiteIsInCheck);
            if (checkBoards.Any())
            {
                var p = true;
            }

            ErrorUi.Text = string.Empty;

            WhiteTurnIcon.Visibility = board.Turn == Colour.White ? Visibility.Visible : Visibility.Collapsed;
            BlackTurnIcon.Visibility = board.Turn == Colour.White ? Visibility.Collapsed : Visibility.Visible;

            WhiteIsInCheckUi.Visibility = board.IsInCheck(Colour.White) ? Visibility.Visible : Visibility.Collapsed;
            BlackIsInCheckUi.Visibility = board.IsInCheck(Colour.Black) ? Visibility.Visible : Visibility.Collapsed;

            var insertText = string.Empty;

            if (board.IsCapture)
                insertText = "CAPTURE! ";

            var moveText = board.MoveToString() + Environment.NewLine;

            LogText.AppendText($"{_currentMatch.ThisTurn - 1}: {insertText}{moveText}");

            LogicLog.Text = _currentMatch.GetLastCpuMoveLog();

            var sb = new StringBuilder();

            EvaluationUi.Text = board.Evaluation.ToString();

            WhiteCastleKingSideUI.IsChecked = board.WhiteCanCastleKingSide;
            WhiteCastleQueenSideUI.IsChecked = board.WhiteCanCastleQueenSide;
            BlackCastleKingSideUI.IsChecked = board.BlackCanCastleKingSide;
            BlackCastleQueenSideUI.IsChecked = board.BlackCanCastleQueenSide;

            if (board.IsCheckmate)
            {
                if (board.WhiteIsInCheckmate)
                    GameOver(Colour.Black);

                if (board.BlackIsInCheckmate)
                    GameOver(Colour.White);
            }

            sb.AppendLine($"== Turn {board.Turn} Board ==");

            sb.Append(board.BoardToString());

            var squaresWithPiecesOn = board.GetSquaresWithPieceOn().ToList();

            var usedImages = new List<Image>();

            foreach (var square in squaresWithPiecesOn)
            {
                var rankFile = square.ToRankFile();
                var image = GetImage(rankFile);

                if (image == null)
                {
                    var bp = true;
                }
                else
                {
                    image.Visibility = Visibility.Visible;

                    usedImages.Add(image);

                    var xy = GetScreenPosition(SquarePixels, rankFile.Rank, (int)rankFile.File);

                    Canvas.SetLeft(image, xy.X);
                    Canvas.SetTop(image, xy.Y);
                }
            }

            foreach(var image in Images)
            {
                if (!usedImages.Contains(image.Image))
                    image.Image.Visibility = Visibility.Collapsed;
            }

            WhiteScore.Text = board.WhiteScore.ToString();
            BlackScore.Text = board.BlackScore.ToString();

            BoardDebug.Text += sb.ToString();
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

            Point p = e.GetPosition(this);
            double x = p.X - BoardCanvas.Margin.Left;
            double y = p.Y - BoardCanvas.Margin.Top;

            var file = x / SquarePixels;
            var rank = 8 - ((y / SquarePixels) - 1);

            StartPosition = RankFile.Get((int)rank, (File)file);

            StartPositionUI.Content = StartPosition;
        }

        private void RunTasks(PieceType promotionType)
        {
            RunTasks(null, new Point(0, 0), promotionType);
        }

        private void RunTasks(Image image, Point xy, PieceType promotionType)
        {
            var ix = 0d; 
            var iy = 0d;

            if (image != null)
            {
                ix = Canvas.GetLeft(image);
                iy = Canvas.GetTop(image);
                Canvas.SetLeft(image, xy.X);
                Canvas.SetTop(image, xy.Y);
            }

            var taskA = Task.Run(() => ProcessNextMove(promotionType));

            taskA
                .OnFailure(() =>
                {
                    if (taskA.Result.Error == "Pawn promotion")
                    {
                        PromotionTypeSelector.Visibility = Visibility.Visible;
                    }
                    if (taskA.Result.Error == "Checkmate")
                    {
                        // TODO: This should be better
                        GameOver(Colour.Black);
                    }
                    else
                    {
                        ErrorUi.Text = taskA.Result.Error;

                        if (image != null)
                        {
                            // Reset the piece
                            Canvas.SetLeft(image, ix);
                            Canvas.SetTop(image, iy);
                        }
                    }
                })
                .OnSuccess((boardA) =>
                {
                    UpdateUI(boardA);

                    var taskB = Task.Run(() => ProcessNextMove());

                    taskB
                    .OnFailure(() =>
                    {
                        ErrorUi.Text = taskB.Result.Error;
                    })
                    .OnSuccess((boardB) =>
                    {
                        UpdateUI(boardB);
                    });
                });
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

            RunTasks(image, xy, PieceType.None);
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
    }
}
