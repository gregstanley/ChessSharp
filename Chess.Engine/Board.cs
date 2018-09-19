using Chess.Engine.Bit;
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

        public double Evaluation { get; private set; }

        public double ProjectedEvaluation { get; set; }

        public ICollection<Board> ChildBoards { get; private set; } = new List<Board>();

        public ulong Key { get; private set; } = 0;

        private BitBoard _bitBoard { get; set; }

        private BitBoardMoveFinder _bitBoardMoveFinder { get; set; }

        private Move _move { get; set; }

        private IList<Move> _moves { get; set; }

        private BoardState _state = BoardState.None;

        private SquareFlag _invalidFlags { get; set; } = 0;

        private BoardState _previousState = BoardState.None;

        private SquareFlag _previousInvalidFlags { get; set; } = 0;

        private Zobrist _keyGen { get; set; }

        public static Board FromFen(Fen fen)
        {
            Move move = null;

            if (fen.EnPassantSquare != 0)
            {
                if (SquareFlagExtensions.r3.HasFlag(fen.EnPassantSquare))
                {
                    // TODO: See if we can get away with the nulls
                    move = new Move(Colour.White, PieceType.Pawn, null, null);
                }
                else if (SquareFlagExtensions.r6.HasFlag(fen.EnPassantSquare))
                {
                    move = new Move(Colour.Black, PieceType.Pawn, null, null);
                }
            }

            var bitBoard = BitBoard.FromFen(fen);

            return new Board(fen.ToPlay, move, bitBoard);
        }

        public Board()
            : this(Colour.White, null, new BitBoard())
        {
        }

        public Board(Colour turn, Move move, BitBoard bitBoard)
        {
            _bitBoard = bitBoard;

            _bitBoardMoveFinder = new BitBoardMoveFinder();

            Turn = turn;

            _move = move;

            _keyGen = new Zobrist();
            _keyGen.Init();

            Key = _keyGen.Hash(_bitBoard, turn);
        }

        public Board(Board parentBoard, Colour turn)
        {
            ParentBoard = parentBoard ?? throw new ArgumentNullException("parentBoard must be specified");

            _bitBoardMoveFinder = ParentBoard._bitBoardMoveFinder;

            Turn = turn;

            _keyGen = ParentBoard._keyGen;

            Key = ParentBoard.Key;

            //if (_bitBoard != null)
            //    Key = _keyGen.Hash(_bitBoard);
        }

        public Board(Board parentBoard, Move move, BitBoard bitBoard, BitBoardMoveFinder moveFinder)
        {
            ParentBoard = parentBoard ?? throw new ArgumentNullException("parentBoard must be specified");

            Turn = parentBoard.Turn.Opposite();

            _move = move;

            _bitBoard = bitBoard;
            _bitBoardMoveFinder = moveFinder;

            _keyGen = ParentBoard._keyGen;

            Key = _keyGen.Hash(_bitBoard, Turn);

            _invalidFlags = CalculateInvalidSquares(move);
        }

        public bool WhiteIsInCheck => _state.HasFlag(BoardState.WhiteIsInCheck);

        public bool BlackIsInCheck => _state.HasFlag(BoardState.BlackIsInCheck);

        public bool WhiteIsInCheckmate => _state.HasFlag(BoardState.WhiteIsInCheckmate);

        public bool BlackIsInCheckmate => _state.HasFlag(BoardState.BlackIsInCheckmate);

        public bool IsInCheck(Colour colour) =>
            colour == Colour.White ? WhiteIsInCheck : BlackIsInCheck;

        public bool IsCheckmate => WhiteIsInCheckmate || BlackIsInCheckmate;

        public bool IsInCheckmate(Colour colour) =>
            colour == Colour.White ? WhiteIsInCheckmate : BlackIsInCheckmate;

        public IReadOnlyCollection<Board> GetBoardsWithCheckmate(Colour colour) =>
            ChildBoards.Where(x => colour == Colour.White
                ? x.WhiteIsInCheckmate
                : x.BlackIsInCheckmate)
            .ToList();

        public IEnumerable<Board> GetLegalMoves() =>
            ChildBoards.Where(x => !x.IsInCheck(Turn));

        public string MoveHistory
        {
            get
            {
                var parentBoard = ParentBoard;

                var sb = new StringBuilder();

                while (parentBoard != null)
                {
                    if (sb.Length > 0)
                        sb.Append($" ");

                    sb.Append($"{parentBoard.GetFriendlyCode()}");

                    parentBoard = parentBoard.ParentBoard;
                }

                return sb.ToString();
            }
        }

        // TODO: Hide again - just using for AlphaBeta tests
        //public BitBoard BitBoard => _bitBoard;

        public bool WhiteCanCastleKingSide => _bitBoard.WhiteCanCastleKingSide();

        public bool WhiteCanCastleQueenSide => _bitBoard.WhiteCanCastleQueenSide();

        public bool BlackCanCastleKingSide => _bitBoard.BlackCanCastleKingSide();

        public bool BlackCanCastleQueenSide => _bitBoard.BlackCanCastleQueenSide();

        public bool CanCastle(Colour colour) => _bitBoard.CanCastle(colour);

        public byte WhiteScore => GetScore(_bitBoard, Colour.White);

        public byte BlackScore => GetScore(_bitBoard, Colour.Black);

        public byte GetScore(Colour colour) =>
                    colour == Colour.White ? WhiteScore : BlackScore;

        public SquareState GetSquareState(RankFile rankFile) =>
            _bitBoard.GetSquareState(rankFile.ToSquareFlag());

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

        public string BoardToString() => _bitBoard.ToString();

        public bool CheckForPawnPromotion(RankFile startPosition, RankFile endPosition) =>
            _bitBoardMoveFinder.CheckForPawnPromotion(_bitBoard, startPosition.ToSquareFlag(), endPosition.ToSquareFlag());

        public string UiCode => _move == null ? string.Empty : _move.UiCode;

        public string Notation => _move == null ? string.Empty : _move.Notation;

        public SquareFlag EnPassantSquare => _move == null ? 0 : _move.EnPassantSquare;

        public SquareFlag EnPassantCaptureSquare => _move == null ? 0 : _move.EnPassantCaptureSquare;

        public PieceType MovePieceType => _move == null ? PieceType.None : _move.Type;

        public bool IsCapture => _move == null ? false : _move.CapturePieceType != PieceType.None;

        public string GetFriendlyCode() => _move == null ? string.Empty : _move.GetFriendlyCode();

        public RankFile GetMovedFrom() => _move.StartPosition;

        public RankFile GetMovedTo() => _move.EndPosition;

        public bool IsCastle => _move as MoveCastle != null;

        public string MoveToString() => _move != null ? _move.ToString() : string.Empty;

        public Board CreateChildBoardFromMove(Move move)
        {
            var childBitBoard = _bitBoard.ApplyMove(move);

            return new Board(this, move, childBitBoard, _bitBoardMoveFinder);
        }

        public void MakeMove(Move move)
        {
            _previousState = _state;
            _previousInvalidFlags = _invalidFlags;

            _move = move;
            _bitBoard = ParentBoard._bitBoard.ApplyMove(move);

            //Key = _keyGen.Hash(_bitBoard);
            if (move is MoveCastle || move.PromotionType != PieceType.None)
                Key = _keyGen.Hash(_bitBoard, move.PieceColour.Opposite());
            else
                Key = _keyGen.Update(Key, move);

            _state = CheckForCheck(move.PieceColour, _bitBoard, _bitBoardMoveFinder, _state);

            _invalidFlags = CalculateInvalidSquares(move);
        }

        public void UnMakeMove(Move move)
        {
            _state = _previousState;
            _invalidFlags = _previousInvalidFlags;

            if (move is MoveCastle || move.PromotionType != PieceType.None)
                Key = _keyGen.Hash(ParentBoard._bitBoard, move.PieceColour);
            else
                Key = _keyGen.Update(Key, move);

            // TODO: Bug - Castle doesn't undo properly
            if (ParentBoard.Key != Key)
            { var bp = true; }
        }

        public double Evaluate(Colour colour)
        {
            Evaluation = EvaluateWeighted(colour);

            var who2move = colour == Colour.White ? 1d : -1d;

            return Evaluation * who2move;
        }

        public bool ParentMoveWasCastle()
        {
            if (ParentBoard == null)
                return false;

            return ParentBoard.Notation.StartsWith("0-0");
        }

        public bool IsInCheckOrImmediatePostCastle(Colour colour)
        {
            if (ParentBoard == null
                || IsInCheck(colour)
                || ParentMoveWasCastle())
            //|| ParentBoard.Notation.StartsWith("0-0"))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<Move> GetUnplayedParentMoves(Colour colour, IEnumerable<SquareFlag> invalidSquares)
        {
            if (ParentBoard == null || ParentBoard._move == null || ParentBoard._moves == null)
                return new List<Move>();

            var unplayedMoves = ParentBoard._moves
                .Where(x => x.Notation != _move.Notation
                && !invalidSquares.Contains(x.StartPositionSquareFlag));

            // If we're in Check then drop any Castle moves
            if (IsInCheck(colour))
                unplayedMoves = unplayedMoves.Where(x => !x.Notation.StartsWith("0-0")).ToList();

            return unplayedMoves;
        }

        public IList<Move> FindMoves(Colour colour) =>
            _bitBoardMoveFinder.FindMoves(_bitBoard, EnPassantSquare, colour);

        public IEnumerable<Move> FindPreviousMoves(Colour colour)
        {
            var invalidSquares = GetInvalidFlags();
            var invalidSquaresArray = invalidSquares.ToList();

            return ParentBoard.GetUnplayedParentMoves(colour, invalidSquaresArray);
        }

        //private bool NeedsReCalc(Colour colour)
        //{
        //    if (ParentBoard?.ParentBoard == null
        //        || IsInCheck(colour)
        //        || ParentMoveWasCastle())
        //    //|| ParentBoard.Notation.StartsWith("0-0"))
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        public IEnumerable<Move> FindNewMoves(Colour colour)
        {
            //if (ParentBoard?.ParentBoard == null
            //    || IsInCheck(colour)
            //    || PreviousMoveWasCastle())
            //    //|| ParentBoard.Notation.StartsWith("0-0"))
            if (IsInCheckOrImmediatePostCastle(colour))
            {
                //_moves = FindMoves(colour);
                //return _moves;
                return FindMoves(colour);
            }

            var squareStates = GetInvalidSquares(colour);
            var newMoves2 = new List<Move>(256);
            //var newMoves = 
                _bitBoardMoveFinder.FindMoves(newMoves2, _bitBoard, EnPassantSquare, colour, squareStates);

            return newMoves2;
        }

        private List<SquareState> GetInvalidSquares(Colour colour)
        {
            var invalidSquares = GetInvalidFlags();
            var invalidSquaresArray = invalidSquares.ToList();

            return invalidSquaresArray
                .Select(x => _bitBoard.GetSquareState(x))
                .Where(x => x.Colour == colour)
                .ToList();
        }

        public IEnumerable<Move> FindMoves2(Colour colour)
        {
            //if (ParentBoard?.ParentBoard == null
            //    || IsInCheck(colour)
            //    || PreviousMoveWasCastle())
            //    //|| ParentBoard.Notation.StartsWith("0-0"))
            if (IsInCheckOrImmediatePostCastle(colour))
            {
                _moves = FindMoves(colour);
                return _moves;
            }

            //if (EnPassantSquare == SquareFlag.A6)
            //{ var bp = true; }
            //if (EnPassantSquare == SquareFlag.B6)
            //{ var bp = true; }
            //if (EnPassantSquare == SquareFlag.C6)
            //{ var bp = true; }
            //if (EnPassantSquare == SquareFlag.D6)
            //{ var bp = true; }
            //if (EnPassantSquare == SquareFlag.E6)
            //{ var bp = true; }
            //if (EnPassantSquare == SquareFlag.F6)
            //{ var bp = true; }

            //var invalidSquares = GetInvalidFlags();
            //var invalidSquaresArray = invalidSquares.ToList();
            //var validUnplayedParentMoves = ParentBoard.GetUnplayedParentMoves(colour, invalidSquaresArray);

            var validUnplayedParentMoves = FindPreviousMoves(colour);

            if (!validUnplayedParentMoves.Any())
            {
                _moves = FindMoves(colour);
                return _moves;
            }

            //var squareStates = invalidSquaresArray
            //    .Select(x => _bitBoard.GetSquareState(x))
            //    .Where(x => x.Colour == colour)
            //    .ToList();

            //var squareStates = GetInvalidSquares(colour);

            //var newMoves2 = new List<Move>(256);
            //var newMoves = _bitBoardMoveFinder.FindMoves(newMoves2, _bitBoard, EnPassantSquare, colour, squareStates);

            var newMoves = FindNewMoves(colour);

            var availableMoves = validUnplayedParentMoves.Concat(newMoves);

            var validateMoves = false;

            if (validateMoves)
            {
                var generatedMoves = FindMoves(colour);

                var a = availableMoves.Select(x => x.Notation);
                var g = generatedMoves.Select(x => x.Notation);

                var missing = g.Except(a);
                var extra = a.Except(g);

                if (missing.Any() || extra.Any())
                {
                    var bp = true;
                }
            }

            _moves = availableMoves.ToList();

            return _moves;
        }

        public void OrphanOtherChildBoardSiblingBoards(Board chosenChild)
        {
            var childBoardsToOrphan = ChildBoards.Where(x => x != chosenChild);

            foreach (var childBoard in childBoardsToOrphan)
                childBoard.Orphan();

            ChildBoards = new List<Board> { chosenChild };
        }

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

            if (!ChildBoards.Any())
            {
                var moves = FindMoves2(colour);

                _moves = moves.ToList();

                foreach (var move in moves)
                {
                    var childBitBoard = _bitBoard.ApplyMove(move);

                    var childBoard = new Board(this, move, childBitBoard, _bitBoardMoveFinder);

                    var whiteKingSquare = childBitBoard.FindKingSquare(Colour.White);
                    var blackKingSquare = childBitBoard.FindKingSquare(Colour.Black);
                    var whiteChecks = _bitBoardMoveFinder.FindPiecesAttackingThisSquare(childBitBoard, Colour.White, whiteKingSquare);
                    var blackChecks = _bitBoardMoveFinder.FindPiecesAttackingThisSquare(childBitBoard, Colour.Black, blackKingSquare);

                    if (whiteChecks.Any())
                    {
                        if (colour == Colour.Black && !blackChecks.Any())
                        {
                            childBoard._state |= BoardState.WhiteIsInCheck;
                            ChildBoards.Add(childBoard);
                        }
                    }
                    else if (blackChecks.Any())
                    {
                        if (colour == Colour.White && !whiteChecks.Any())
                        {
                            childBoard._state |= BoardState.BlackIsInCheck;
                            ChildBoards.Add(childBoard);
                        }
                    }
                    else
                    {
                        ChildBoards.Add(childBoard);
                    }
                }

                if (!GetLegalMoves().Any())
                    _state |= Turn == Colour.White ? BoardState.WhiteIsInCheckmate : BoardState.BlackIsInCheckmate;
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
            $"{_move.ToString()}";

        public string GetMetricsString() =>
            $"{_move.ToString()} Eval: {Evaluation} ProjE: {ProjectedEvaluation} WC: {WhiteIsInCheck} BC: {BlackIsInCheck}";

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

        private BoardState CheckForCheck(Colour colour, BitBoard bitBoard, BitBoardMoveFinder bitBoardMoveFinder, BoardState state)
        {
            var whiteKingSquare = bitBoard.FindKingSquare(Colour.White);
            var blackKingSquare = bitBoard.FindKingSquare(Colour.Black);
            var whiteChecks = bitBoardMoveFinder.FindPiecesAttackingThisSquare(bitBoard, Colour.White, whiteKingSquare);
            var blackChecks = bitBoardMoveFinder.FindPiecesAttackingThisSquare(bitBoard, Colour.Black, blackKingSquare);

            if (whiteChecks.Any())
            {
                //if (colour == Colour.Black && !blackChecks.Any())
                    state |= BoardState.WhiteIsInCheck;
            }
            else if (blackChecks.Any())
            {
                //if (colour == Colour.White && !whiteChecks.Any())
                    state |= BoardState.BlackIsInCheck;
            }

            return state;
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

        private SquareFlag CalculateInvalidSquares(Move move)
        {
            var invalidMoves = _bitBoardMoveFinder.FindPiecesAttackingThisSquare(_bitBoard, Colour.None, move.StartPositionSquareFlag);
            var invalidMovesEnd = _bitBoardMoveFinder.FindPiecesAttackingThisSquare(_bitBoard, Colour.None, move.EndPositionSquareFlag);
            var unblockedPawns = _bitBoardMoveFinder.FindBlockedPawns(_bitBoard, move.PieceColour, move.StartPositionSquareFlag);
            var blockedPawns = _bitBoardMoveFinder.FindBlockedPawns(_bitBoard, move.PieceColour, move.EndPositionSquareFlag);

            invalidMoves.AddRange(invalidMovesEnd);
            invalidMoves.AddRange(unblockedPawns);
            invalidMoves.AddRange(blockedPawns);

            invalidMoves.Add(_bitBoard.GetSquareState(move.StartPositionSquareFlag));
            invalidMoves.Add(_bitBoard.GetSquareState(move.EndPositionSquareFlag));

            if (move.EnPassantSquare != 0)
            {
                var enPassantInvalidsOrig = _bitBoardMoveFinder.FindPiecesAttackingThisSquare(_bitBoard, move.PieceColour, move.EnPassantSquare)
                    .Where(x => x.Type == PieceType.Pawn);

                var enPassantInvalids = _bitBoardMoveFinder.FindPawnsAttackingThisSquare(_bitBoard, move.PieceColour, move.EnPassantSquare);

                if (enPassantInvalidsOrig.Count() != enPassantInvalids.Count())
                { var bp = true; }

                if (enPassantInvalids.Any())
                    invalidMoves.AddRange(enPassantInvalids);
            }

            if (WhiteCanCastleKingSide || WhiteCanCastleQueenSide)
            {
                var rooks = _bitBoard.WhiteRooks.ToList();

                foreach (var rook in rooks)
                    invalidMoves.Add(_bitBoard.GetSquareState(rook));

                var whiteKingSquare = _bitBoard.FindKingSquare(Colour.White);

                invalidMoves.Add(_bitBoard.GetSquareState(whiteKingSquare));
            }

            if (BlackCanCastleKingSide || BlackCanCastleQueenSide)
            {
                var rooks = _bitBoard.BlackRooks.ToList();

                foreach (var rook in rooks)
                    invalidMoves.Add(_bitBoard.GetSquareState(rook));

                var blackKingSquare = _bitBoard.FindKingSquare(Colour.Black);

                invalidMoves.Add(_bitBoard.GetSquareState(blackKingSquare));
            }

            SquareFlag invalidFlags = 0;

            foreach (var invalidMove in invalidMoves)
                invalidFlags |= invalidMove.Square;

            return invalidFlags;
        }

        private SquareFlag GetInvalidFlags() =>
            ParentBoard._invalidFlags |= _invalidFlags;

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
