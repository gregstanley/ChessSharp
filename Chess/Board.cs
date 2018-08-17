using Chess.Bit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    public class Board
    {
        private bool _isParallel = false;

        public Board ParentBoard { get; private set; }

        public ICollection<Board> ChildBoards { get; private set; } = new List<Board>();

        public bool IsCapture => _state.HasFlag(BoardState.IsCapture); 

        public bool IsPawnPromotion => _state.HasFlag(BoardState.IsPawnPromotion);

        public bool WhiteIsInCheck => _state.HasFlag(BoardState.WhiteIsInCheck);

        public bool BlackIsInCheck => _state.HasFlag(BoardState.BlackIsInCheck);

        public bool WhiteIsInCheckmate => _state.HasFlag(BoardState.WhiteIsInCheckmate);

        public bool BlackIsInCheckmate => _state.HasFlag(BoardState.BlackIsInCheckmate);

        public bool IsCheckmate => _state.HasFlag(BoardState.WhiteIsInCheckmate) || _state.HasFlag(BoardState.BlackIsInCheckmate);

        public byte WhiteScore { get; private set; }

        public byte BlackScore { get; private set; }

        public double EvaluationScore { get; private set; }

        public BoardMetrics WhiteMetrics { get; private set; }

        public BoardMetrics BlackMetrics { get; private set; }

        public string Code { get { return _move == null ? string.Empty : _move.GetCode(); } }

        private Move _move { get; set; }

        public Move Move => _move;

        public Board PrimaryVariation { get; private set; }

        private MoveFinder _moveFinder;

        private BoardState _state = BoardState.None;

        public BitBoard BitBoard { get; }

        private BitBoardMoveFinder _bitBoardMoveFinder;

        public int PositionCounter { get; private set; } = 0;

        public Board(MoveFinder moveFinder)
        {
            _moveFinder = moveFinder;
            

            BitBoard = new BitBoard();

            _bitBoardMoveFinder = new BitBoardMoveFinder();

            WhiteScore = GetScore(BitBoard, Colour.White);
            BlackScore = GetScore(BitBoard, Colour.Black);
        }

        public Board(Board parentBoard, Move move, BitBoard bitBoard, BitBoardMoveFinder moveFinder)
        {
            ParentBoard = parentBoard;

            _move = move;
            _bitBoardMoveFinder = moveFinder;
            
            BitBoard = bitBoard;

            WhiteScore = GetScore(bitBoard, Colour.White);
            BlackScore = GetScore(bitBoard, Colour.Black);
        }

        public double Evaluate(Colour colour)
        {
            var whiteSquaresUnderThreat = GetSquaresUnderThreat(Colour.White);
            var blackSquaresUnderThreat = GetSquaresUnderThreat(Colour.Black);

            var isWhiteCheck = whiteSquaresUnderThreat.HasFlag(BitBoard.FindKingSquare(Colour.White));
            var isBlackCheck = blackSquaresUnderThreat.HasFlag(BitBoard.FindKingSquare(Colour.Black));

            if (isWhiteCheck)
                _state |= BoardState.WhiteIsInCheck;

            if (isBlackCheck)
                _state |= BoardState.BlackIsInCheck;

            var whiteMetrics = new BoardMetrics();
            var blackMetrics = new BoardMetrics();

            whiteMetrics.PointsChange = WhiteScore - ParentBoard.WhiteScore;
            blackMetrics.PointsChange = BlackScore - ParentBoard.BlackScore;

            whiteMetrics.NumPiecesUnderThreat = whiteSquaresUnderThreat.Count();
            blackMetrics.NumPiecesUnderThreat = blackSquaresUnderThreat.Count();

            //var whiteCheckBoost = isBlackCheck ? 9 : 0;
            //var blackCheckBoost = isWhiteCheck ? 9 : 0;

            //whiteMetrics.NumPiecesUnderThreatValue = (byte)(whiteSquaresUnderAttack.Sum(x => x.Piece.Value) + blackCheckBoost);
            //blackMetrics.NumPiecesUnderThreatValue = (byte)(blackSquaresUnderAttack.Sum(x => x.Piece.Value) + whiteCheckBoost);

            whiteMetrics.NumAccessibleSquares = GetCoveredSquares(Colour.White).Count();
            blackMetrics.NumAccessibleSquares = GetCoveredSquares(Colour.Black).Count();

            whiteMetrics.NumProtectedPieces = GetProtectedPieces(Colour.White).Count();
            blackMetrics.NumProtectedPieces = GetProtectedPieces(Colour.Black).Count();

            WhiteMetrics = whiteMetrics;
            BlackMetrics = blackMetrics;

            EvaluationScore = Evaluate(colour, whiteMetrics, blackMetrics);

            return EvaluationScore;
        }

        private double Evaluate(Colour colour, BoardMetrics whiteMetrics, BoardMetrics blackMetrics)
        {
            /*
             f(p) = 200(K-K')
               + 9(Q-Q')
               + 5(R-R')
               + 3(B-B' + N-N')
               + 1(P-P')
               - 0.5(D-D' + S-S' + I-I')
               + 0.1(M-M') + ...
 
            KQRBNP = number of kings, queens, rooks, bishops, knights and pawns
            D,S,I = doubled, blocked and isolated pawns
            M = Mobility (the number of legal moves)

            materialScore = kingWt * (wK - bK)
              + queenWt * (wQ - bQ)
              + rookWt * (wR - bR)
              + knightWt * (wN - bN)
              + bishopWt * (wB - bB)
              + pawnWt * (wP - bP)

            mobilityScore = mobilityWt * (wMobility - bMobility)

            return the score relative to the side to move (who2Move = +1 for white, -1 for black):
            eval  = (materialScore + mobilityScore) * who2Move
            */
            var kingWeight = 200d;
            var queenWeight = 9d;
            var rookWeight = 5d;
            var knightWeight = 3d;
            var bishopWeight = 3d;
            var pawnWeight = 1d;
            var mobilityWeight = 0.0d;// 0.1;
            var threatendWeight = 0.0d;

            var whiteKingCount = BitBoard.FindKingSquare(Colour.White).Count();
            var blackKingCount = BitBoard.FindKingSquare(Colour.Black).Count();

            var whiteQueenCount = BitBoard.FindQueenSquares(Colour.White).Count();
            var blackQueenCount = BitBoard.FindQueenSquares(Colour.Black).Count();

            var whiteRookCount = BitBoard.FindRookSquares(Colour.White).Count();
            var blackRookCount = BitBoard.FindRookSquares(Colour.Black).Count();

            var whiteKnightCount = BitBoard.FindKnightSquares(Colour.White).Count();
            var blackKnightCount = BitBoard.FindKnightSquares(Colour.Black).Count();

            var whiteBishopCount = BitBoard.FindBishopSquares(Colour.White).Count();
            var blackBishopCount = BitBoard.FindBishopSquares(Colour.Black).Count();

            var whitePawnCount = BitBoard.FindPawnSquares(Colour.White).Count();
            var blackPawnCount = BitBoard.FindPawnSquares(Colour.Black).Count();

            if(whiteKingCount != 1 || blackKingCount != 1)
            {
                var bp = true;
            }

            var materialScore = kingWeight * (whiteKingCount - blackKingCount)
              + queenWeight * (whiteQueenCount - blackQueenCount)
              + rookWeight * (whiteRookCount - blackRookCount)
              + knightWeight * (whiteKnightCount - blackKnightCount)
              + bishopWeight * (whiteBishopCount - blackBishopCount)
              + pawnWeight * (whitePawnCount - blackPawnCount);

            var mobilityScore = mobilityWeight * (whiteMetrics.NumAccessibleSquares - blackMetrics.NumAccessibleSquares);

            var threatendScore = threatendWeight * (whiteMetrics.NumPiecesUnderThreatValue - blackMetrics.NumPiecesUnderThreatValue);

            var who2move = colour == Colour.White ? 1d : -1d;

            return (materialScore + mobilityScore + threatendScore) * who2move;
        }

        //public void SetPrimaryVariation(Move move) =>
        //    PrimaryVariation = move;

        public BoardMetrics GetMetrics(Colour colour) =>
            Colour.White == colour ? WhiteMetrics : BlackMetrics;

        public bool CheckForPawnPromotion(RankFile startPosition, RankFile endPosition) =>
            _bitBoardMoveFinder.CheckForPawnPromotion(BitBoard, startPosition.ToSquareFlag(), endPosition.ToSquareFlag());

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

        public PieceType GetPiece(RankFile rankFile) =>
            BitBoard.GetPiece(rankFile.ToSquareFlag());

        public SquareFlag GetKingSquare(Colour colour) =>
            BitBoard.FindKingSquare(colour);

        public SquareFlag GetSquaresWithPieceOn() =>
            BitBoard.White | BitBoard.Black;

        public SquareFlag GetSquaresWithPieceOn(Colour colour) =>
            BitBoard.FindPieceSquares(colour);

        public PieceType GetPieceOnSquare(RankFile rankFile) =>
            BitBoard.GetPiece(rankFile.ToSquareFlag());

        public Colour GetPieceOnSquareColour(RankFile rankFile) =>
            BitBoard.GetPieceColour(rankFile.ToSquareFlag());

        public byte GetInstanceNumber(Colour colour, PieceType type, SquareFlag square) =>
            BitBoard.GetInstanceNumber(colour, type, square);

        public SquareFlag GetCoveredSquares(Colour colour) =>
           BitBoard.FindCoveredSquares(colour);

        public SquareFlag GetProtectedPieces(Colour colour) =>
            BitBoard.FindPieceSquares(colour) & BitBoard.FindCoveredSquares(colour);

        public SquareFlag GetSquaresUnderThreat(Colour colour) =>
            BitBoard.FindCoveredSquares(colour.Opposite());

        public IReadOnlyCollection<Board> GetBoardsWithCheckmate(Colour colour) =>
            ChildBoards.Where(x => colour == Colour.White ? x.WhiteIsInCheckmate : x.BlackIsInCheckmate).ToList();

        public bool IsInCheck(Colour colour) =>
            colour == Colour.White ? WhiteIsInCheck : BlackIsInCheck;

        public bool IsInCheckmate(Colour colour) =>
            colour == Colour.White ? WhiteIsInCheckmate : BlackIsInCheckmate;

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

            var squaresUnderAttack = GetSquaresUnderThreat(colour).ToList();

            foreach(var sua in squaresUnderAttack)
            {
                var rf = sua.ToRankFile();
                sb.AppendLine($"{rf.File}{rf.Rank} covered");// by:");

                //foreach (var p in sua.CoveredBy)
                //    sb.AppendLine($" - {p.Colour} {p.Type}");
            }

            return sb.ToString();
        }

        public string Diff(Board sourceBoard) =>
            _move != null ? _move.ToString() : string.Empty;

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

        public string BoardToString() =>
            BitBoard.ToString();

        public string GetMetricsString()
        {
            if (WhiteMetrics == null)
                return "No metrics for white";

            if (BlackMetrics == null)
                return "No metrics for black";

            return
            $"{GetCode()} " +
            $"E: {Math.Round(EvaluationScore, 2)} " +
            $"PV: {PvString()} " +
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
        }

        private string PvString() =>
            PrimaryVariation == null
            ? string.Empty
            : $"{PrimaryVariation.Move} {Math.Round(PrimaryVariation.EvaluationScore, 2)}";

        public void Orphan()
        {
            ParentBoard = null;

            foreach(var childBoard in ChildBoards)
                childBoard.Orphan();
        }

        public void GenerateChildBoards(Colour colour, int depth)
        {
            if (ChildBoards == null)
                ChildBoards = new List<Board>();

            var moves = _bitBoardMoveFinder.FindMoves(BitBoard, colour);

            foreach (var move in moves)
            {
                var existingChildBoard = ChildBoards.SingleOrDefault(x => x.Code == move.GetCode());

                if (existingChildBoard != null)
                    continue;

                var childBitBoard = BitBoard.Move(move, _bitBoardMoveFinder);

                if (!childBitBoard.IsCheck(colour))
                {
                    var childBoard = new Board(this, move, childBitBoard, _bitBoardMoveFinder);

                    childBoard.Evaluate(colour);

                    ChildBoards.Add(childBoard);
                }
            }

            if (!ChildBoards.Any())
                _state |= colour == Colour.White ? BoardState.WhiteIsInCheckmate : BoardState.BlackIsInCheckmate;

            if (--depth <= 0)
                return;

            if (_isParallel)
            {
                Parallel.ForEach(ChildBoards, (childBoard) =>
                {
                    childBoard.GenerateChildBoards(colour.Opposite(), depth);
                });
            }
            else
            {
                foreach (var childBoard in ChildBoards)
                    childBoard.GenerateChildBoards(colour.Opposite(), depth);
            }
        }

        private byte GetScore(BitBoard board, Colour colour)
        {
            byte score = board.FindPawnSquares(colour).Count();
            score += (byte)(board.FindRookSquares(colour).Count() * 5);
            score += (byte)(board.FindKnightSquares(colour).Count() * 3);
            score += (byte)(board.FindBishopSquares(colour).Count() * 3);
            score += (byte)(board.FindQueenSquares(colour).Count() * 9);

            return score;
        }
    }
}
