using ChessSharp;
using ChessSharp.Engine;
using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using ChessSharp.MoveGeneration;
using System.Collections.Generic;
using System.Linq;

namespace ChessSharp_UI
{
    public class Game
    {
        public int Ply { get; private set; } = 1;

        public Colour ToPlay { get { return Ply % 2 == 1 ? Colour.White : Colour.Black; } }

        public int HalfTurnCounter { get; private set; } = 0;

        public int FullTurnNumber { get { return (Ply + 1) / 2; } }

        public Colour HumanColour { get; private set; } = Colour.None;

        public bool IsHumanTurn { get { return HumanColour != Colour.None && ToPlay == HumanColour; } }

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

            var fromSquare = fromSquareIndex.ToSquareFlag();
            var toSquare = toSquareIndex.ToSquareFlag();

            var promotionMoveMatches = promotionMoves.Where(x => x.From == fromSquare && x.To == toSquare);

            if (!promotionMoveMatches.Any())
                return false;

            return true;
        }

        public MoveViewer TryApplyMove(int fromSquareIndex, int toSquareIndex, PieceType promotionPieceType = PieceType.None)
        {
            if (fromSquareIndex == toSquareIndex)
                return new MoveViewer(0);

            var fromSquare = fromSquareIndex.ToSquareFlag();
            var toSquare = toSquareIndex.ToSquareFlag();

            var move = TryPromotions(fromSquare, toSquare, AvailableMoves, promotionPieceType);

            if (move.Value == 0)
                move = TryCastles(_workspace, fromSquare, toSquare, AvailableMoves);

            if (move.Value == 0)
                move = AvailableMoves.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare)
                    ?? new MoveViewer(0);

            if (move.Value == 0)
                return move;

            DoMove(move);

            return move;
        }

        public MoveViewer CpuMove()
        {
            var moves = _search.Go(_workspace, 1);

            var chosenMove = moves.FirstOrDefault();

            if (chosenMove == null)
                return new MoveViewer(0);

            DoMove(chosenMove.Move);

            return chosenMove.Move;
        }

        public Piece GetPiece(int squareIndex) =>
            _workspace.BitBoard.GetPiece(squareIndex.ToSquareFlag());

        public byte GetInstanceNumber(Piece piece, SquareFlag square) =>
            _workspace.BitBoard.GetInstanceNumber(piece, square);

        public SquareFlag GetSquaresWithPieceOn() =>
            _workspace.BitBoard.White | _workspace.BitBoard.Black;

        private void DoMove(MoveViewer move)
        {
            _workspace.MakeMove(move.Value);

            Ply++;

            // Seems odd that separate things are tracking current colour, not sure how better to handle it though
            if (_workspace.Colour != ToPlay)
                throw new System.Exception("Game and Workspace out of sync");

            var moves = new List<uint>();

            _moveGenerator.Generate(_workspace, moves);

            AvailableMoves = moves.Select(x => new MoveViewer(x));
        }

        private MoveViewer TryCastles(MoveGenerationWorkspace workspace, SquareFlag fromSquare, SquareFlag toSquare, IEnumerable<MoveViewer> availableMoves)
        {
            if (workspace.Colour == Colour.White && fromSquare == SquareFlagConstants.WhiteKingStartSquare)
            {
                if (toSquare == SquareFlagConstants.WhiteKingSideRookStartSquare && workspace.BitBoard.WhiteCanCastleKingSide)
                {
                    return availableMoves.SingleOrDefault(x => x.MoveType == MoveType.CastleKing)
                        ?? new MoveViewer(0);
                }
                else if (toSquare == SquareFlagConstants.WhiteQueenSideRookStartSquare && workspace.BitBoard.WhiteCanCastleQueenSide)
                {
                    return availableMoves.SingleOrDefault(x => x.MoveType == MoveType.CastleQueen)
                        ?? new MoveViewer(0);
                }
            }

            if (workspace.Colour == Colour.Black && fromSquare == SquareFlagConstants.BlackKingStartSquare)
            {
                if (toSquare == SquareFlagConstants.BlackKingSideRookStartSquare && _workspace.BitBoard.BlackCanCastleKingSide)
                {
                    return availableMoves.SingleOrDefault(x => x.MoveType == MoveType.CastleKing)
                        ?? new MoveViewer(0);
                }
                else if (toSquare == SquareFlagConstants.BlackQueenSideRookStartSquare && _workspace.BitBoard.BlackCanCastleQueenSide)
                {
                    return availableMoves.SingleOrDefault(x => x.MoveType == MoveType.CastleQueen)
                        ?? new MoveViewer(0);
                }
            }

            return new MoveViewer(0);
        }

        private MoveViewer TryPromotions(SquareFlag fromSquare, SquareFlag toSquare, IEnumerable<MoveViewer> availableMoves, PieceType promotionPieceType = PieceType.None)
        {
            switch (promotionPieceType)
            {
                case PieceType.Queen:
                    return availableMoves.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare && x.MoveType == MoveType.PromotionQueen)
                        ?? new MoveViewer(0);
                case PieceType.Rook:
                    return availableMoves.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare && x.MoveType == MoveType.PromotionRook)
                        ?? new MoveViewer(0);
                case PieceType.Bishop:
                    return availableMoves.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare && x.MoveType == MoveType.PromotionBishop)
                        ?? new MoveViewer(0);
                case PieceType.Knight:
                    return availableMoves.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare && x.MoveType == MoveType.PromotionKnight)
                        ?? new MoveViewer(0);
                default:
                    return new MoveViewer(0);
            }
        }
    }
}
