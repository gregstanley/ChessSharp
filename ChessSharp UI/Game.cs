using ChessSharp;
using ChessSharp.Engine;
using ChessSharp.Enums;
using ChessSharp.Models;
using ChessSharp.MoveGeneration;
using System.Collections.Generic;
using System.Linq;

namespace ChessSharp_UI
{
    public class Game
    {
        public int Ply { get; private set; } = 1;

        public Colour Turn { get { return Ply % 2 == 1 ? Colour.White : Colour.Black; } }

        public int HalfTurnCounter { get; private set; } = 0;

        public int FullTurnNumber { get { return (int)(Ply + 1) / 2; } }

        public Colour HumanColour { get; private set; } = Colour.None;

        public bool IsHumanTurn { get { return HumanColour != Colour.None && Turn == HumanColour; } }

        public IEnumerable<MoveViewer> AvailableMoves { get; private set; }

        private readonly MoveGenerationWorkspace _workspace;

        private readonly MoveGenerator _moveGenerator;

        private readonly Search _search;

        public static Game FromFen(Fen fen, Colour humanColour = Colour.None)
        {
            var board = BitBoard.FromFen(fen);

            return new Game(board, humanColour);
        }

        public Game(BitBoard board, Colour humanColour = Colour.None)
        {
            HumanColour = humanColour;

            _workspace = new MoveGenerationWorkspace(board, Colour.White);

            _moveGenerator = new MoveGenerator();

            _search = new Search(_moveGenerator);

            var moves = new List<uint>();

            _moveGenerator.Generate(_workspace, moves);

            AvailableMoves = moves.Select(x => new MoveViewer(x));
        }

        public bool IsMovePromotion(int fromSquareIndex, int toSquareIndex)
        {
            var promotionMoves = AvailableMoves.Where(x =>
                   x.MoveType == MoveType.PromotionQueen
                || x.MoveType == MoveType.PromotionRook
                || x.MoveType == MoveType.PromotionBishop
                || x.MoveType == MoveType.PromotionKnight);

            if (!promotionMoves.Any())
                return false;

            var fromSquare = (SquareFlag)(1ul << fromSquareIndex);
            var toSquare = (SquareFlag)(1ul << toSquareIndex);

            var promotionMoveMatches = promotionMoves.Where(x => x.From == fromSquare && x.To == toSquare);

            if (!promotionMoveMatches.Any())
                return false;

            return true;
        }

        public MoveViewer TryApplyMove(int fromSquareIndex, int toSquareIndex, PieceType promotionPieceType = PieceType.None)
        {
            if (fromSquareIndex == toSquareIndex)
                return new MoveViewer(0);

            var moveViews = AvailableMoves;

            var fromSquare = (SquareFlag)(1ul << fromSquareIndex);
            var toSquare = (SquareFlag)(1ul << toSquareIndex);

            MoveViewer move = null;

            if (HumanColour == Colour.White && fromSquare == SquareFlagConstants.WhiteKingStartSquare)
            {
                if (toSquare == SquareFlagConstants.WhiteKingSideRookStartSquare && _workspace.BitBoard.WhiteCanCastleKingSide)
                {
                    move = moveViews.SingleOrDefault(x => x.MoveType == MoveType.CastleKing);

                    if (move == null)
                        return new MoveViewer(0);
                }
                else if (toSquare == SquareFlagConstants.WhiteQueenSideRookStartSquare && _workspace.BitBoard.WhiteCanCastleQueenSide)
                {
                    move = moveViews.SingleOrDefault(x => x.MoveType == MoveType.CastleQueen);

                    if (move == null)
                        return new MoveViewer(0);
                }
            }

            if (HumanColour == Colour.Black && fromSquare == SquareFlagConstants.BlackKingStartSquare)
            {
                if (toSquare == SquareFlagConstants.BlackKingSideRookStartSquare && _workspace.BitBoard.BlackCanCastleKingSide)
                {
                    move = moveViews.SingleOrDefault(x => x.MoveType == MoveType.CastleKing);

                    if (move == null)
                        return new MoveViewer(0);
                }
                else if (toSquare == SquareFlagConstants.BlackQueenSideRookStartSquare && _workspace.BitBoard.BlackCanCastleQueenSide)
                {
                    move = moveViews.SingleOrDefault(x => x.MoveType == MoveType.CastleQueen);

                    if (move == null)
                        return new MoveViewer(0);
                }
            }

            if (move == null)
            {
                if (promotionPieceType == PieceType.Queen)
                    move = moveViews.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare && x.MoveType == MoveType.PromotionQueen);
                else if (promotionPieceType == PieceType.Rook)
                    move = moveViews.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare && x.MoveType == MoveType.PromotionRook);
                else if (promotionPieceType == PieceType.Bishop)
                    move = moveViews.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare && x.MoveType == MoveType.PromotionBishop);
                else if (promotionPieceType == PieceType.Knight)
                    move = moveViews.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare && x.MoveType == MoveType.PromotionKnight);
                else
                    move = moveViews.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare);
            }

            if (move == null)
                return new MoveViewer(0);

            DoMove(move);

            return move;
        }

        public MoveViewer CpuMove()
        {
            var moves = _search.Go(_workspace.BitBoard, Turn, 1);

            var chosenMove = moves.FirstOrDefault();

            if (chosenMove == null)
                return new MoveViewer(0);

            DoMove(chosenMove.Move);

            return chosenMove.Move;
        }

        public Piece GetPiece(int squareIndex)
        {
            var square = (SquareFlag)(1ul << squareIndex);

            return _workspace.BitBoard.GetPiece(square);
        }

        public byte GetInstanceNumber(Piece piece, SquareFlag square) =>
            _workspace.BitBoard.GetInstanceNumber(piece, square);

        public SquareFlag GetSquaresWithPieceOn() =>
            _workspace.BitBoard.White | _workspace.BitBoard.Black;

        private void DoMove(MoveViewer move)
        {
            _workspace.MakeMove(move.Value);

            Ply++;

            // Seems odd that separate things are tracking current colour, not sure how better to handle it though
            if (_workspace.Colour != Turn)
                throw new System.Exception("Game and Workspace out of sync");

            var moves = new List<uint>();

            _moveGenerator.Generate(_workspace, moves);

            AvailableMoves = moves.Select(x => new MoveViewer(x));
        }
    }
}
