using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    public class Board
    {
        private static List<File> _files = new List<File> { File.a, File.b, File.c, File.d, File.e, File.f, File.g, File.h };

        public Board ParentBoard { get; private set; }

        public ICollection<Board> ChildBoards { get; private set; } = new List<Board>();

        public IList<Piece> OffBoard { get; private set; } = new List<Piece>();

        public bool IsCapture => _state.HasFlag(BoardState.IsCapture); 

        public bool IsPawnPromotion => _state.HasFlag(BoardState.IsPawnPromotion);

        public bool WhiteIsInCheck => _state.HasFlag(BoardState.WhiteIsInCheck);

        public bool BlackIsInCheck => _state.HasFlag(BoardState.BlackIsInCheck);

        public bool WhiteIsInCheckmate => _state.HasFlag(BoardState.WhiteIsInCheckmate);

        public bool BlackIsInCheckmate => _state.HasFlag(BoardState.BlackIsInCheckmate);

        public bool IsCheckmate => _state.HasFlag(BoardState.WhiteIsInCheckmate) || _state.HasFlag(BoardState.BlackIsInCheckmate);

        public byte WhiteScore { get; private set; }

        public byte BlackScore { get; private set; }

        public OptionsStats OptionsStats { get; private set; } = new OptionsStats(0, 0, 0, 0, 0);

        public BoardMetrics WhiteMetrics { get; private set; }

        public BoardMetrics BlackMetrics { get; private set; }

        public string Code { get; private set; } = string.Empty;

        private IList<Square> _squares { get; }

        private Move _move { get; set; }

        private IList<Board> _boardsInCheck { get; set; }

        private MoveFinder _moveFinder;

        private BoardState _state = BoardState.None;

        public Board(Board parentBoard, MoveFinder moveFinder)
        {
            ParentBoard = parentBoard;

            _squares = CloneSquaresAndPieces(parentBoard);

            OffBoard = parentBoard.OffBoard.Select(p => p.Clone()).ToList();

            _moveFinder = moveFinder;

            UpdateScores();
        }

        public Board(MoveFinder moveFinder)
        {
            _squares = ResetBoard().ToArray();

            _moveFinder = moveFinder;

            UpdateScores();
        }

        public void MovePiece(Move move)
        {
            _move = move;

            var startSquare = _squares.Single(x => x.Rank == move.StartPosition.Rank && x.File == move.StartPosition.File);
            var targetSquare = _squares.Single(x => x.Rank == move.EndPosition.Rank && x.File == move.EndPosition.File);
            var piece = startSquare.Piece;

            if (targetSquare.Piece != null)
            {
                _state |= BoardState.IsCapture;

                Capture(targetSquare.Piece);
            }

            targetSquare.SetPiece(piece);

            piece.Move();

            startSquare.RemovePiece();

            if (move.Type == PieceType.Pawn)
            {
                var promotionType = move.PromotionType;

                if (promotionType != PieceType.None)
                {
                    _state |= BoardState.IsPawnPromotion;

                    targetSquare.SetPiece(Promote(piece, promotionType));
                }
            }

            if (move is MoveCastle castle)
            {
                //var castle = move as MoveCastle;

                var kingStartSquare = _squares.Single(x => x.Rank == castle.KingStartPosition.Rank && x.File == castle.KingStartPosition.File);
                var targetKingSquare = _squares.Single(x => x.Rank == castle.KingEndPosition.Rank && x.File == castle.KingEndPosition.File);
                var king = kingStartSquare.Piece;

                targetKingSquare.SetPiece(king);

                king.Move();

                kingStartSquare.RemovePiece();
            }

            SetCoveredSquares();

            var colour = piece.Colour;

            var whiteSquaresUnderAttack = GetSquaresUnderThreat(Colour.White);
            var blackSquaresUnderAttack = GetSquaresUnderThreat(Colour.Black);

            var isWhiteCheck = whiteSquaresUnderAttack.Any(x => x.Piece.Type == PieceType.King);
            var isBlackCheck = blackSquaresUnderAttack.Any(x => x.Piece.Type == PieceType.King);

            if (isWhiteCheck)
                _state |= BoardState.WhiteIsInCheck;

            if (isBlackCheck)
                _state |= BoardState.BlackIsInCheck;

            UpdateScores();

            var whiteMetrics = new BoardMetrics();
            var blackMetrics = new BoardMetrics();

            whiteMetrics.PointsChange = WhiteScore - ParentBoard.WhiteScore;
            blackMetrics.PointsChange = BlackScore - ParentBoard.BlackScore;

            whiteMetrics.NumPiecesUnderThreat = (byte)blackSquaresUnderAttack.Count();
            blackMetrics.NumPiecesUnderThreat = (byte)whiteSquaresUnderAttack.Count();

            var whiteCheckBoost = isBlackCheck ? 9 : 0;
            var blackCheckBoost = isWhiteCheck ? 9 : 0;

            whiteMetrics.NumPiecesUnderThreatValue = (byte)(whiteSquaresUnderAttack.Sum(x => x.Piece.Value) + blackCheckBoost);
            blackMetrics.NumPiecesUnderThreatValue = (byte)(blackSquaresUnderAttack.Sum(x => x.Piece.Value) + whiteCheckBoost);

            whiteMetrics.NumAccessibleSquares = (byte)GetCoveredSquares(colour).Count();
            blackMetrics.NumAccessibleSquares = (byte)GetCoveredSquares(colour.Opposite()).Count();

            whiteMetrics.NumProtectedPieces = (byte)GetProtectedPieces(colour).Count();
            blackMetrics.NumProtectedPieces = (byte)GetProtectedPieces(colour.Opposite()).Count();

            WhiteMetrics = whiteMetrics;
            BlackMetrics = blackMetrics;

            Code = GetCode();
        }

        public BoardMetrics GetMetrics(Colour colour) =>
            Colour.White == colour ? WhiteMetrics : BlackMetrics;

        public bool CheckForPawnPromotion(RankFile startPosition, RankFile endPosition) =>
            _moveFinder.CheckForPawnPromotion(GetSquare(startPosition), endPosition);

        public string GetCode() =>
            _move.GetCode();

        public RankFile GetMovedFrom() =>
            _move.StartPosition;

        public RankFile GetMovedTo() =>
            _move.EndPosition;

        public PieceType GetMovedPieceType() =>
            _move.Type;

        public Move GetMove() =>
            _move;

        public Square GetSquare(RankFile rankFile) =>
            _squares.Single(x => x.Rank == rankFile.Rank && x.File == rankFile.File);

        public Square GetKingSquare(Colour colour) =>
            _squares.Single(x => x.Piece?.Colour == colour && x.Piece?.Type == PieceType.King);

        public IEnumerable<Square> GetSquaresWithPieceOn() =>
            _squares.Where(x => x.Piece != null);

        public IEnumerable<Square> GetSquaresWithPieceOn(Colour colour) =>
            _squares.Where(x => x.Piece?.Colour == colour);

        public Piece GetPieceOnSquare(RankFile rankFile) =>
            _squares.Single(x => x.Rank == rankFile.Rank && x.File == rankFile.File).Piece;

        public IQueryable<Square> GetCoveredSquares(Colour colour) =>
            _squares.AsQueryable().Where(x => x.CoveredBy.Any(s => s.Colour == colour));

        public IReadOnlyCollection<Square> GetProtectedPieces(Colour colour) =>
            GetCoveredSquares(colour).Where(x => x.Piece != null).ToList();

        public IReadOnlyCollection<Square> GetSquaresUnderThreat(Colour colour) =>
            _squares.Where(x => x.Piece?.Colour == colour && x.CoveredBy.Any(s => s.Colour == colour.Opposite()))
            .OrderByDescending(x => x.Piece?.Value)
            .ToList();

        public IReadOnlyCollection<Board> GetBoardsWithCheckmate(Colour colour) =>
            ChildBoards.Where(x => colour == Colour.White ? x.WhiteIsInCheckmate : x.BlackIsInCheckmate).ToList();

        public bool IsInCheck(Colour colour) =>
            colour == Colour.White ? WhiteIsInCheck : BlackIsInCheck;

        public bool IsInCheckmate(Colour colour) =>
            colour == Colour.White ? WhiteIsInCheckmate : BlackIsInCheckmate;

        public IReadOnlyCollection<Board> GetBoardsWithCheck() =>
            _boardsInCheck.ToList();

        public string MoveToString() =>
            _move != null ? _move.ToString() : string.Empty;

        public void OrphanOtherChildBoardSiblingBoards(Board chosenChild)
        {
            var childBoardsToOrphan = ChildBoards.Where(x => x != chosenChild);

            foreach (var childBoard in childBoardsToOrphan)
                childBoard.Orphan();

            ChildBoards = new List<Board> { chosenChild };
        }

        public string GetSquaresUnderAttackDebugString(Colour colour)
        {
            var sb = new StringBuilder();

            var squaresUnderAttack = GetSquaresUnderThreat(colour);

            foreach(var sua in squaresUnderAttack)
            {
                sb.AppendLine($"{sua.File}{sua.Rank} covered by:");

                foreach (var p in sua.CoveredBy)
                    sb.AppendLine($" - {p.Colour} {p.Type}");
            }

            return sb.ToString();
        }

        public string Diff(Board sourceBoard)
        {
            var sb = new StringBuilder();
            
            var toString = string.Empty;
            Piece movingPiece = null;

            foreach (var sourceSquare in sourceBoard._squares)
            {
                var square = _squares.Single(x => x.Rank == sourceSquare.Rank && x.File == sourceSquare.File);

                if (sourceSquare.Piece != null && square.Piece == null)
                    movingPiece = sourceSquare.Piece;
            }

            if (movingPiece == null)
            {
                sb.AppendLine($"Error - no moving piece :-(");

                return sb.ToString();
            }

            var targetSquare = _squares.Single(x => 
                x.Piece?.Type == movingPiece.Type
                && x.Piece?.StartPosition.Rank == movingPiece.StartPosition.Rank
                && x.Piece?.StartPosition.File == movingPiece.StartPosition.File);

            toString = string.Concat(targetSquare.File, targetSquare.Rank);

            var moveText = _move != null ? _move.ToString() : string.Empty;

            sb.AppendLine($"{movingPiece.Colour} {movingPiece.Type} to {toString} (Move: {moveText})");

            return sb.ToString();
        }

        public ICollection<Board> FindLeaves()
        {
            var leafBoards = new List<Board>();

            var toVisit = new Stack<Board>(ChildBoards);

            while (toVisit.Count > 0)
            {
                var current = toVisit.Pop();

                foreach (var child in current.ChildBoards)
                {
                    if (child.ChildBoards.Count == 0)
                        leafBoards.Add(child);
                    else
                        toVisit.Push(child);
                }
            }

            return leafBoards;
        }

        public int GetScore(Colour colour) =>
            colour == Colour.White ? WhiteScore : BlackScore;

        public string BoardToString()
        {
            var sb = new StringBuilder();

            var ranks = new List<string>();

            foreach (var square in _squares)
            {
                if (square.Piece == null) sb.Append("-");
                if (square.Piece?.Type == PieceType.Pawn) sb.Append("p");
                if (square.Piece?.Type == PieceType.Rook) sb.Append("R");
                if (square.Piece?.Type == PieceType.Knight) sb.Append("N");
                if (square.Piece?.Type == PieceType.Bishop) sb.Append("B");
                if (square.Piece?.Type == PieceType.Queen) sb.Append("Q");
                if (square.Piece?.Type == PieceType.King) sb.Append("K");

                var index = _squares.IndexOf(square);

                if (index > 0 && (index + 1) % 8 == 0)
                {
                    ranks.Add(sb.ToString());

                    sb.Clear();
                }
            }

            sb.Clear();

            ranks.Reverse();

            foreach (var rank in ranks)
                sb.AppendLine(rank);

            return sb.ToString();
        }

        public void GenerateChildBoards(Colour colour, int depth)
        {
            var boardsWhereKingIsInCheck = new List<Board>();

            if (!ChildBoards.Any())
            {
                var childBoards = new List<Board>();

                var colourSquares = GetSquaresWithPieceOn(colour);

                foreach(var square in colourSquares)
                {
                    var boardsFromSquare = GenerateChildBoardsFromSquare(_squares, square);

                    if (boardsFromSquare.Any())
                    {
                        boardsWhereKingIsInCheck.AddRange(boardsFromSquare.Where(x => x.IsInCheck(colour)));
                        childBoards.AddRange(boardsFromSquare.Where(x => !x.IsInCheck(colour)));
                    }
                };

                //Parallel.ForEach(colourSquares, (square) =>
                //{
                //    var boardsFromSquare = GenerateChildBoardsFromSquare(_squares, square);

                //    if (boardsFromSquare.Any())
                //    {
                //        boardsWhereKingIsInCheck.AddRange(boardsFromSquare.Where(x => x.IsInCheck(colour)));
                //        childBoards.AddRange(boardsFromSquare.Where(x => !x.IsInCheck(colour)));
                //    }
                //});


                ChildBoards = childBoards;
            }

            if (boardsWhereKingIsInCheck.Any())
                _boardsInCheck = boardsWhereKingIsInCheck;

            if (!ChildBoards.Any())
                _state |= colour == Colour.White ? BoardState.WhiteIsInCheckmate : BoardState.BlackIsInCheckmate;
            else
                CalculateStats();

            if (--depth <= 0)
                return;

            //foreach (var childBoard in ChildBoards)
            //    childBoard.GenerateChildBoards(colour.Opposite(), depth);
            Parallel.ForEach(ChildBoards, (childBoard) =>
            {
                childBoard.GenerateChildBoards(colour.Opposite(), depth);
            });
        }

        public string GetCheckDebugString()
        {
            var sb = new StringBuilder();

            foreach (var board in _boardsInCheck)
                sb.AppendLine($"{board._move.PieceColour} {board._move.Type} to {board._move.EndPosition.File}{board._move.EndPosition.Rank}");

            return sb.ToString();
        }

        public string GetMetricsString() =>
            $"{GetCode()} " +
            $"WP+-: {WhiteMetrics.PointsChange} " +
            $"Mob: {WhiteMetrics.NumAccessibleSquares} " +
            $"PP: {WhiteMetrics.NumProtectedPieces} " +
            $"Th: {WhiteMetrics.NumPiecesUnderThreat} " +
            $"ThV: {WhiteMetrics.NumPiecesUnderThreatValue} " +
            $"BP+-: {BlackMetrics.PointsChange} " +
            $"Mob: {BlackMetrics.NumAccessibleSquares} " +
            $"PP: {WhiteMetrics.NumProtectedPieces} " +
            $"Th: {BlackMetrics.NumPiecesUnderThreat} " +
            $"ThV: {BlackMetrics.NumPiecesUnderThreatValue}";

        public void Orphan()
        {
            ParentBoard = null;

            foreach(var childBoard in ChildBoards)
                childBoard.Orphan();
        }

        private void Capture(Piece piece)
        {
            piece.Capture();

            OffBoard.Add(piece);
        }

        private Piece Promote(Piece piece, PieceType promotionType)
        {
            OffBoard.Add(piece);

            Piece replacementPiece = replacementPiece = OffBoard.FirstOrDefault(x => x.Colour == piece.Colour && x.Type == promotionType);

            if (replacementPiece == null)
                throw new Exception("Haven't programmed enough spare pieces for 3+ promotions. D'oh :-(");

            OffBoard.Remove(replacementPiece);

            // Just in case this was a captured piece set it to be not captured
            replacementPiece.PlaceOnBoard();

            return replacementPiece;
        }

        private IList<Board> GenerateChildBoardsFromSquare(IList<Square> squares, Square square)
        {
            var moves = _moveFinder.GetMoves(squares, square);

            var childBoards = new List<Board>();

            foreach (var move in moves)
            {
                var board = new Board(this, _moveFinder);

                board.MovePiece(move);

                childBoards.Add(board);
            }

            return childBoards;
        }

        private void SetCoveredSquares()
        {
            var squaresWithPieces = GetSquaresWithPieceOn();

            foreach (var square in squaresWithPieces)
            {
                var coveredSquares = _moveFinder.GetCoveredSquares(_squares, square);

                foreach (var coveredSquare in coveredSquares)
                    coveredSquare.AddCoveredBy(square.Piece);
            }
        }

        private void CalculateStats()
        {
            var numBoards = ChildBoards.Count();
            var avgWhiteScore = ChildBoards.Average(x => x.WhiteScore);
            var avgBlackScore = ChildBoards.Average(x => x.BlackScore);

            var avgWhitePercentChange = Math.Round(((avgWhiteScore - WhiteScore) / avgWhiteScore) * 10000) * 100;
            var avgBlackPercentChange = Math.Round(((avgBlackScore - BlackScore) / avgBlackScore) * 10000) * 100;

            OptionsStats = new OptionsStats(numBoards, avgWhiteScore, avgBlackScore, avgWhitePercentChange, avgBlackPercentChange);
        }

        private byte CalculateScore(Colour colour) =>
            (byte) GetSquaresWithPieceOn(colour).Sum(x => x.Piece.Value);

        private void UpdateScores()
        {
            WhiteScore = CalculateScore(Colour.White);
            BlackScore = CalculateScore(Colour.Black);
        }

        private IList<Square> CloneSquaresAndPieces(Board board)
        {
            var squares = new List<Square>();

            foreach (var sourceSquare in board._squares)
            {
                var newSquare = new Square(RankFile.Get(sourceSquare.Rank, sourceSquare.File));

                if (sourceSquare.Piece != null)
                    newSquare.SetPiece(sourceSquare.Piece.Clone());

                squares.Add(newSquare);
            }

            return squares;
        }

        private IList<Square> ResetBoard()
        {
            var squares = new List<Square>();

            // Put some spare pieces in the box
            OffBoard.Add(new Piece(Colour.White, PieceType.Rook, 5, RankFile.A3));
            OffBoard.Add(new Piece(Colour.White, PieceType.Rook, 5, RankFile.B3));
            OffBoard.Add(new Piece(Colour.White, PieceType.Knight, 3, RankFile.C3));
            OffBoard.Add(new Piece(Colour.White, PieceType.Knight, 3, RankFile.D3));
            OffBoard.Add(new Piece(Colour.White, PieceType.Bishop, 3, RankFile.E3));
            OffBoard.Add(new Piece(Colour.White, PieceType.Bishop, 3, RankFile.F3));
            OffBoard.Add(new Piece(Colour.White, PieceType.Queen, 9, RankFile.G3));
            OffBoard.Add(new Piece(Colour.White, PieceType.Queen, 9, RankFile.H3));
            OffBoard.Add(new Piece(Colour.Black, PieceType.Rook, 5, RankFile.A6));
            OffBoard.Add(new Piece(Colour.Black, PieceType.Rook, 5, RankFile.B6));
            OffBoard.Add(new Piece(Colour.Black, PieceType.Knight, 3, RankFile.C6));
            OffBoard.Add(new Piece(Colour.Black, PieceType.Knight, 3, RankFile.D6));
            OffBoard.Add(new Piece(Colour.Black, PieceType.Bishop, 3, RankFile.E6));
            OffBoard.Add(new Piece(Colour.Black, PieceType.Bishop, 3, RankFile.F6));
            OffBoard.Add(new Piece(Colour.Black, PieceType.Queen, 9, RankFile.G6));
            OffBoard.Add(new Piece(Colour.Black, PieceType.Queen, 9, RankFile.H6));

            for (var rank = 1; rank < 9; rank++)
            {
                foreach (var file in _files)
                {
                    var square = new Square(RankFile.Get(rank, file));

                    if (rank == 1 && file == File.a)
                        square.SetPiece(new Piece(Colour.White, PieceType.Rook, 5, RankFile.A1));

                    if (rank == 1 && file == File.b)
                        square.SetPiece(new Piece(Colour.White, PieceType.Knight, 3, RankFile.B1));

                    if (rank == 1 && file == File.c)
                        square.SetPiece(new Piece(Colour.White, PieceType.Bishop, 3, RankFile.C1));

                    if (rank == 1 && file == File.d)
                        square.SetPiece(new Piece(Colour.White, PieceType.Queen, 9, RankFile.D1));

                    if (rank == 1 && file == File.e)
                        square.SetPiece(new Piece(Colour.White, PieceType.King, 0, RankFile.E1));

                    if (rank == 1 && file == File.f)
                        square.SetPiece(new Piece(Colour.White, PieceType.Bishop, 3, RankFile.F1));

                    if (rank == 1 && file == File.g)
                        square.SetPiece(new Piece(Colour.White, PieceType.Knight, 3, RankFile.G1));

                    if (rank == 1 && file == File.h)
                        square.SetPiece(new Piece(Colour.White, PieceType.Rook, 5, RankFile.H1));

                    if (rank == 2 && file == File.a)
                        square.SetPiece(new Piece(Colour.White, PieceType.Pawn, 1, RankFile.A2));

                    if (rank == 2 && file == File.b)
                        square.SetPiece(new Piece(Colour.White, PieceType.Pawn, 1, RankFile.B2));

                    if (rank == 2 && file == File.c)
                        square.SetPiece(new Piece(Colour.White, PieceType.Pawn, 1, RankFile.C2));

                    if (rank == 2 && file == File.d)
                        square.SetPiece(new Piece(Colour.White, PieceType.Pawn, 1, RankFile.D2));

                    if (rank == 2 && file == File.e)
                        square.SetPiece(new Piece(Colour.White, PieceType.Pawn, 1, RankFile.E2));

                    if (rank == 2 && file == File.f)
                        square.SetPiece(new Piece(Colour.White, PieceType.Pawn, 1, RankFile.F2));

                    if (rank == 2 && file == File.g)
                        square.SetPiece(new Piece(Colour.White, PieceType.Pawn, 1, RankFile.G2));

                    if (rank == 2 && file == File.h)
                        square.SetPiece(new Piece(Colour.White, PieceType.Pawn, 1, RankFile.H2));

                    if (rank == 7 && file == File.a)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Pawn, 1, RankFile.A7));

                    if (rank == 7 && file == File.b)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Pawn, 1, RankFile.B7));

                    if (rank == 7 && file == File.c)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Pawn, 1, RankFile.C7));

                    if (rank == 7 && file == File.d)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Pawn, 1, RankFile.D7));

                    if (rank == 7 && file == File.e)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Pawn, 1, RankFile.E7));

                    if (rank == 7 && file == File.f)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Pawn, 1, RankFile.F7));

                    if (rank == 7 && file == File.g)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Pawn, 1, RankFile.G7));

                    if (rank == 7 && file == File.h)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Pawn, 1, RankFile.H7));

                    if (rank == 8 && file == File.a)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Rook, 5, RankFile.A8));

                    if (rank == 8 && file == File.b)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Knight, 3, RankFile.B8));

                    if (rank == 8 && file == File.c)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Bishop, 3, RankFile.C8));

                    if (rank == 8 && file == File.d)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Queen, 9, RankFile.D8));

                    if (rank == 8 && file == File.e)
                        square.SetPiece(new Piece(Colour.Black, PieceType.King, 0, RankFile.E8));

                    if (rank == 8 && file == File.f)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Bishop, 3, RankFile.F8));

                    if (rank == 8 && file == File.g)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Knight, 3, RankFile.G8));

                    if (rank == 8 && file == File.h)
                        square.SetPiece(new Piece(Colour.Black, PieceType.Rook, 5, RankFile.H8));

                    squares.Add(square);
                }
            }

            return squares;
        }
    }
}
