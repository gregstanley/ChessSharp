using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using ChessSharp.MoveGeneration;
using System.Collections.Generic;
using System.Linq;

namespace ChessSharp.Engine
{
    public class Game : IGameEventBroadcaster
    {
        public delegate void MoveAppliedEventDelegate(object sender, MoveAppliedEventArgs args);

        public delegate void PromotionTypeRequiredEventDelegate(object sender, PromotionTypeRequiredEventArgs args);

        public event MoveAppliedEventDelegate MoveApplied;

        public event PromotionTypeRequiredEventDelegate PromotionTypeRequired; 

        public int Ply { get; private set; } = 1;

        public Colour ToPlay { get { return Ply % 2 == 1 ? Colour.White : Colour.Black; } }

        public int HalfTurnCounter { get; private set; } = 0;

        public int FullTurnNumber { get { return (Ply + 1) / 2; } }

        public Colour HumanColour { get; private set; } = Colour.None;

        public bool IsHumanTurn { get { return HumanColour != Colour.None && ToPlay == HumanColour; } }

        public IEnumerable<MoveViewer> AvailableMoves { get; private set; }

        private readonly BitBoard _bitBoard;

        private readonly MoveGenerationWorkspace _workspace;

        private readonly MoveGenerator _moveGenerator;

        private readonly Search _search;

        public static Game FromFen(Fen fen, Colour humanColour = Colour.None)
        {
            var bitBoard = BitBoard.FromFen(fen);

            return new Game(bitBoard, humanColour);
        }

        public Game(BitBoard board, Colour humanColour = Colour.None)
        {
            _bitBoard = board;

            HumanColour = humanColour;

            _workspace = new MoveGenerationWorkspace(board, Colour.White);

            _moveGenerator = new MoveGenerator();

            _search = new Search(_moveGenerator, new PositionEvaluator());

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

            PromotionTypeRequired?.Invoke(this, new PromotionTypeRequiredEventArgs(fromSquareIndex, toSquareIndex));

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
            var chosenMove = AvailableMoves.FirstOrDefault();

            if (chosenMove == null)
                return new MoveViewer(0);

            DoMove(chosenMove);

            return chosenMove;
        }

        public MoveViewer CpuMoveSmart()
        {
            var moves = _search.Go(_workspace, 3, HumanColour == Colour.White);

            var chosenMove = moves.OrderByDescending(x => x.Score).FirstOrDefault();

            if (chosenMove == null)
                return new MoveViewer(0);

            DoMove(chosenMove.Move);

            return chosenMove.Move;
        }

        public BitBoard GetBitBoard() =>
            _bitBoard;

        public Piece GetPiece(int squareIndex) =>
            _bitBoard.GetPiece(squareIndex.ToSquareFlag());

        public byte GetInstanceNumber(Piece piece, SquareFlag square) =>
            _bitBoard.GetInstanceNumber(piece, square);

        public SquareFlag GetSquaresWithPieceOn() =>
            _bitBoard.White | _workspace.BitBoard.Black;

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

            MoveApplied?.Invoke(this, new MoveAppliedEventArgs(move, GetGameState()));
        }

        public GameState GetGameState() => GameState.From(this);

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
