﻿using Chess.Engine.Bit;
using Chess.Engine.Extensions;
using Chess.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Engine
{
    public class Board
    {
        private bool _isParallel = false;

        public Board ParentBoard { get; private set; }

        public Colour Turn { get; }

        public ICollection<Board> ChildBoards { get; private set; } = new List<Board>();

        public SquareFlag EnPassantSquare => _move == null ? 0 : _move.EnPassantSquare;

        public SquareFlag EnPassantCaptureSquare => _move == null ? 0 : _move.EnPassantCaptureSquare;

        public bool WhiteCanCastleKingSide => _bitBoard.WhiteCanCastleKingSide();

        public bool WhiteCanCastleQueenSide => _bitBoard.WhiteCanCastleQueenSide();

        public bool BlackCanCastleKingSide => _bitBoard.BlackCanCastleKingSide();

        public bool BlackCanCastleQueenSide => _bitBoard.BlackCanCastleQueenSide();

        public bool WhiteIsInCheck => _state.HasFlag(BoardState.WhiteIsInCheck);

        public bool BlackIsInCheck => _state.HasFlag(BoardState.BlackIsInCheck);

        public bool WhiteIsInCheckmate => _state.HasFlag(BoardState.WhiteIsInCheckmate);

        public bool BlackIsInCheckmate => _state.HasFlag(BoardState.BlackIsInCheckmate);

        public bool IsInCheck(Colour colour) =>
            colour == Colour.White ? WhiteIsInCheck : BlackIsInCheck;

        public bool IsCheckmate => WhiteIsInCheckmate || BlackIsInCheckmate;

        public bool IsGameOver => _bitBoard.FindKingSquare(Colour.White) == 0 || _bitBoard.FindKingSquare(Colour.Black) == 0;

        public byte WhiteScore => GetScore(_bitBoard, Colour.White);

        public byte BlackScore => GetScore(_bitBoard, Colour.Black);

        public double Evaluation { get; private set; }

        public double ProjectedEvaluation { get; set; }

        public BoardMetrics WhiteMetrics { get; private set; }

        public BoardMetrics BlackMetrics { get; private set; }

        public string Code { get { return _move == null ? string.Empty : _move.UiCode; } }

        public Move Move => _move;

        private BoardState _state = BoardState.None;

        private BitBoard _bitBoard { get; }

        private Move _move { get; set; }

        private BitBoardMoveFinder _bitBoardMoveFinder;

        public static Board FromFen(Fen fen)
        {
            var bitBoard = new BitBoard();

            var bitBoardMoveFinder = new BitBoardMoveFinder();

            return new Board(null, null, bitBoard, bitBoardMoveFinder);
        }

        public Board()
        {
            _bitBoard = new BitBoard();

            _bitBoardMoveFinder = new BitBoardMoveFinder();

            Turn = Colour.White;
        }

        public Board(Board parentBoard, Move move, BitBoard bitBoard, BitBoardMoveFinder moveFinder)
        {
            ParentBoard = parentBoard ?? throw new ArgumentNullException("parentBoard must be specified");

            Turn = parentBoard.Turn.Opposite();

            _move = move;

            _bitBoard = bitBoard;
            _bitBoardMoveFinder = moveFinder;
        }

        public Board ApplyMove(Move move)
        {
            var childBitBoard = _bitBoard.ApplyMove(move, _bitBoardMoveFinder);

            return new Board(this, move, childBitBoard, _bitBoardMoveFinder);
        }

        public double Evaluate(Colour colour)
        {
            Evaluation = EvaluateWeighted(colour);

            var who2move = colour == Colour.White ? 1d : -1d;

            return Evaluation * who2move;
        }

        public int GetScore(Colour colour) =>
            colour == Colour.White ? WhiteScore : BlackScore;

        public BoardMetrics GetMetrics(Colour colour) =>
            Colour.White == colour ? WhiteMetrics : BlackMetrics;

        public IList<Move> FindMoves() =>
            _bitBoardMoveFinder.FindMoves(_bitBoard, EnPassantCaptureSquare, Turn);

        public bool CheckForPawnPromotion(RankFile startPosition, RankFile endPosition) =>
            _bitBoardMoveFinder.CheckForPawnPromotion(_bitBoard, startPosition.ToSquareFlag(), endPosition.ToSquareFlag());

        public bool CanCastle(Colour colour) =>
            _bitBoard.CanCastle(colour);

        public SquareState GetSquareState(RankFile rankFile)
        {
            var square = rankFile.ToSquareFlag();

            return _bitBoard.GetSquareState(square);
        }

        public SquareFlag GetKingSquare(Colour colour) =>
            _bitBoard.FindKingSquare(colour);

        public SquareFlag GetSquaresWithPieceOn() =>
            _bitBoard.White | _bitBoard.Black;

        public SquareFlag GetSquaresWithPieceOn(Colour colour) =>
            _bitBoard.FindPieceSquares(colour);

        public PieceType GetPieceOnSquare(RankFile rankFile) =>
            _bitBoard.GetPieceType(rankFile.ToSquareFlag());

        public Colour GetPieceOnSquareColour(RankFile rankFile) =>
            _bitBoard.GetPieceColour(rankFile.ToSquareFlag());

        public byte GetInstanceNumber(Colour colour, PieceType type, SquareFlag square) =>
            _bitBoard.GetInstanceNumber(colour, type, square);

        public SquareFlag GetCoveredSquares(Colour colour) =>
            _bitBoard.FindCoveredSquares(colour);

        public SquareFlag GetProtectedPieces(Colour colour) =>
            _bitBoard.FindPieceSquares(colour) & _bitBoard.FindCoveredSquares(colour);

        public SquareFlag GetSquaresUnderThreat(Colour colour) =>
            _bitBoard.FindCoveredSquares(colour.Opposite());

        public bool IsCapture
        {
            get
            {
                var bbis = _bitBoard.IsCapture();

                if (_move != null)
                {
                    var mis = _move.CapturePieceType != PieceType.None;

                    if (bbis != mis)
                    { var bp = true; }
                }

                return bbis;
            }
        }

        public string BoardToString() =>
            _bitBoard.ToString();

        public string GetFriendlyCode() =>
            _move.GetFriendlyCode();

        public RankFile GetMovedFrom() =>
            _move.StartPosition;

        public RankFile GetMovedTo() =>
            _move.EndPosition;

        public PieceType GetMovedPieceType() =>
            _move.Type;

        public Move GetMove() =>
            _move;

        public string MoveToString() =>
            _move != null ? _move.ToString() : string.Empty;

        public IReadOnlyCollection<Board> GetBoardsWithCheckmate(Colour colour) =>
            ChildBoards.Where(x => colour == Colour.White ? x.WhiteIsInCheckmate : x.BlackIsInCheckmate).ToList();

        public bool IsInCheckmate(Colour colour) =>
            colour == Colour.White ? WhiteIsInCheckmate : BlackIsInCheckmate;

        public void OrphanOtherChildBoardSiblingBoards(Board chosenChild)
        {
            var childBoardsToOrphan = ChildBoards.Where(x => x != chosenChild);

            foreach (var childBoard in childBoardsToOrphan)
                childBoard.Orphan();

            ChildBoards = new List<Board> { chosenChild };
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

        public void Orphan()
        {
            ParentBoard = null;

            foreach(var childBoard in ChildBoards)
                childBoard.Orphan();
        }

        public IEnumerable<Board> GetLegalMoves() =>
            ChildBoards.Where(x => !x.IsInCheck(Turn));

        public void UpdateStateInfo()
        {
            if (ChildBoards == null || !ChildBoards.Any())
                return;

            //var stationaryKingMoves = ChildBoards.Where(x => x.Move.Type != PieceType.King);

            var inCheckWhite = _bitBoardMoveFinder.FindPiecesAttackingThisSquare(_bitBoard, Colour.White, _bitBoard.FindKingSquare(Colour.White));
            var inCheckBlack = _bitBoardMoveFinder.FindPiecesAttackingThisSquare(_bitBoard, Colour.Black, _bitBoard.FindKingSquare(Colour.Black));
            //if (stationaryKingMoves.Any(x => x.IsInCheck(Turn)))

            if (inCheckWhite.Any())
                _state |= BoardState.WhiteIsInCheck;

            if (inCheckBlack.Any())
                _state |= BoardState.BlackIsInCheck;

            if (!GetLegalMoves().Any())
                _state |= Turn == Colour.White ? BoardState.WhiteIsInCheckmate : BoardState.BlackIsInCheckmate;
        }

        public string MoveHistory
        {
            get
            {
                var parentBoard = ParentBoard;

                var sb = new StringBuilder();

                while (parentBoard != null)
                {
                    sb.Append($"{parentBoard.GetFriendlyCode()} ");

                    parentBoard = parentBoard.ParentBoard;
                }

                return sb.ToString();
            }
        }

        public void GenerateChildBoards(Colour colour, int depth)
        {
            if (ChildBoards == null)
                ChildBoards = new List<Board>();

            if (!ChildBoards.Any())
            {
                var moves = _bitBoardMoveFinder.FindMoves(_bitBoard, EnPassantSquare, colour);

                foreach (var move in moves)
                {
                    var childBitBoard = _bitBoard.ApplyMove(move, _bitBoardMoveFinder);

                    var childBoard = new Board(this, move, childBitBoard, _bitBoardMoveFinder);

                    ChildBoards.Add(childBoard);

                    var kingSquare = childBitBoard.FindKingSquare(colour.Opposite());

                    if (kingSquare == 0)
                        _state |= colour.Opposite() == Colour.White ? BoardState.WhiteIsInCheck : BoardState.BlackIsInCheck;
                }
            }

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

        public override string ToString() =>
            $"{Move.ToString()}";

        public string GetMetricsString() =>
            $"{Move.ToString()} Eval: {Evaluation} ProjE: {ProjectedEvaluation} WC: {WhiteIsInCheck} BC: {BlackIsInCheck}";

        public string ToPartialFen()
        {
            var sb = new StringBuilder();

            var consecutiveEmpty = 0;
            var ranks = new List<string>();

            ulong bit = 1;

            for (var i = 0; i < 64; ++i)
            {
                var squareNotation = _bitBoard.GetSquareNotation((SquareFlag)bit);

                if (string.IsNullOrEmpty(squareNotation))
                {
                    ++consecutiveEmpty;
                }
                else
                {
                    if (consecutiveEmpty > 0)
                    {
                        sb.Append(consecutiveEmpty);
                        consecutiveEmpty = 0;
                    }

                    sb.Append(squareNotation);
                }

                if (i > 0 && (i + 1) % 8 == 0)
                {
                    if (consecutiveEmpty > 0)
                    {
                        sb.Append(consecutiveEmpty);
                        consecutiveEmpty = 0;
                    }

                    if (i > 8)
                        sb.Append("/");

                    ranks.Add(sb.ToString());

                    sb.Clear();
                }

                bit = bit << 1;
            }

            sb.Clear();

            ranks.Reverse();

            foreach (var rank in ranks)
                sb.Append(rank);

            sb.Append(" ");
            sb.Append(Turn == Colour.White ? "w" : "b");

            sb.Append(" ");

            var castleSubString = new StringBuilder();

            if (WhiteCanCastleKingSide) castleSubString.Append("K");
            if (WhiteCanCastleQueenSide) castleSubString.Append("Q");
            if (BlackCanCastleKingSide) castleSubString.Append("k");
            if (BlackCanCastleQueenSide) castleSubString.Append("q");
            if (castleSubString.Length == 0) castleSubString.Append("-");

            sb.Append(castleSubString);

            sb.Append(" ");

            if (_move != null && _move.EnPassantSquare != 0)
                sb.Append(_move.EnPassantSquare.ToString().ToLower());
            else
                sb.Append("-");

            return sb.ToString();
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

        private double EvaluateWeighted(Colour colour)
        {
            var eval = 0d;

            var squaresWithPieces = (_bitBoard.White | _bitBoard.Black).ToList();

            foreach (var squareWithPiece in squaresWithPieces)
            {
                var pieceColour = _bitBoard.GetPieceColour(squareWithPiece);

                var absoluteValue = GetAbsolutePieceValue(squareWithPiece, pieceColour);

                eval += pieceColour == Colour.White ? absoluteValue : -absoluteValue;
            }

            return Math.Round(eval * 0.1, 2);
        }

        private double GetAbsolutePieceValue(SquareFlag squareWithPiece, Colour pieceColour)
        {
            var pieceType = _bitBoard.GetPieceType(squareWithPiece);
            var rf = squareWithPiece.ToRankFile();
            var r = rf.Rank - 1;
            var f = (int)rf.File;

            if (pieceType == PieceType.Pawn)
            {
                return 10d + (pieceColour == Colour.White ? PieceValues.WhitePawn[r][f] : PieceValues.BlackPawn[r][f]);
            }
            else if (pieceType == PieceType.Rook)
            {
                return 50d + (pieceColour == Colour.White ? PieceValues.WhiteRook[r][f] : PieceValues.BlackRook[r][f]);
            }
            else if (pieceType == PieceType.Knight)
            {
                return 30d + PieceValues.Knight[r][f];
            }
            else if (pieceType == PieceType.Bishop)
            {
                return 30d + (pieceColour == Colour.White ? PieceValues.WhiteBishop[r][f] : PieceValues.BlackBishop[r][f]);
            }
            else if (pieceType == PieceType.Queen)
            {
                return 90d + PieceValues.Queen[r][f];
            }
            else if (pieceType == PieceType.King)
            {
                return 900d + (pieceColour == Colour.White ? PieceValues.WhiteKing[r][f] : PieceValues.BlackKing[r][f]);
            }

            return 0d;
        }

        private double EvaluateBasic(Colour colour, BoardMetrics whiteMetrics, BoardMetrics blackMetrics)
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

            var whiteKingCount = _bitBoard.FindKingSquare(Colour.White).Count();
            var blackKingCount = _bitBoard.FindKingSquare(Colour.Black).Count();

            var whiteQueenCount = _bitBoard.FindQueenSquares(Colour.White).Count();
            var blackQueenCount = _bitBoard.FindQueenSquares(Colour.Black).Count();

            var whiteRookCount = _bitBoard.FindRookSquares(Colour.White).Count();
            var blackRookCount = _bitBoard.FindRookSquares(Colour.Black).Count();

            var whiteKnightCount = _bitBoard.FindKnightSquares(Colour.White).Count();
            var blackKnightCount = _bitBoard.FindKnightSquares(Colour.Black).Count();

            var whiteBishopCount = _bitBoard.FindBishopSquares(Colour.White).Count();
            var blackBishopCount = _bitBoard.FindBishopSquares(Colour.Black).Count();

            var whitePawnCount = _bitBoard.FindPawnSquares(Colour.White).Count();
            var blackPawnCount = _bitBoard.FindPawnSquares(Colour.Black).Count();

            if (whiteKingCount != 1 || blackKingCount != 1)
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

            return (materialScore + mobilityScore + threatendScore);
        }
    }
}
