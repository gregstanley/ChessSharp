using ChessSharp.Engine.Events;
using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using ChessSharp.MoveGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessSharp.Engine
{
    public class Game : IGameEventBroadcaster
    {
        public delegate void InvalidMoveEventDelegate(object sender, InvalidMoveEventArgs args);

        public delegate void PromotionTypeRequiredEventDelegate(object sender, PromotionTypeRequiredEventArgs args);

        public delegate void SearchStartedEventDelegate(object sender, EventArgs args);

        public delegate void SearchCompletedEventDelegate(object sender, SearchCompleteEventArgs args);

        public delegate void MoveAppliedEventDelegate(object sender, MoveAppliedEventArgs args);

        public delegate void CheckmateEventDelegate(object sender, EventArgs args);

        public event InvalidMoveEventDelegate InvalidMove;

        public event PromotionTypeRequiredEventDelegate PromotionTypeRequired;

        public event SearchStartedEventDelegate SearchStarted;

        public event SearchCompletedEventDelegate SearchCompleted;

        public event MoveAppliedEventDelegate MoveApplied;

        public event CheckmateEventDelegate Checkmate;

        public int Ply { get; private set; } = 1;

        public Colour ToPlay { get { return Ply % 2 == 1 ? Colour.White : Colour.Black; } }

        public int HalfTurnCounter { get; private set; } = 0;

        public int FullTurnNumber { get { return (Ply + 1) / 2; } }

        public Colour HumanColour { get; private set; } = Colour.None;

        public bool IsHumanTurn { get { return HumanColour != Colour.None && ToPlay == HumanColour; } }

        public IEnumerable<MoveViewer> AvailableMoves { get; private set; }

        public int SearchedPositionsCount => _search.PositionCount;

        private readonly BitBoard _bitBoard;

        private readonly MoveGenerationWorkspace _workspace;

        private readonly MoveGenerator _moveGenerator;

        private readonly PositionEvaluator _positionEvaluator;

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

            _positionEvaluator = new PositionEvaluator();

            _search = new Search(_moveGenerator, _positionEvaluator);

            var moves = new List<uint>();

            _moveGenerator.Generate(_workspace, moves);

            AvailableMoves = moves.Select(x => new MoveViewer(x));
        }

        public MoveViewer TryFindMove(int fromSquareIndex, int toSquareIndex, PieceType promotionPieceType = PieceType.None)
        {
            if (fromSquareIndex == toSquareIndex)
                return new MoveViewer(0);

            var fromSquare = fromSquareIndex.ToSquareFlag();
            var toSquare = toSquareIndex.ToSquareFlag();

            var move = TryPromotions(fromSquare, toSquare, AvailableMoves, promotionPieceType);

            if (move.Value == 0)
                move = TryCastles(_workspace, fromSquare, toSquare, AvailableMoves);

            if (move.Value == 0)
                move = AvailableMoves.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare) ?? new MoveViewer(0);

            return move;
        }

        public MoveViewer CpuMoveFirst()
        {
            var chosenMove = AvailableMoves.FirstOrDefault();

            if (chosenMove == null)
                return new MoveViewer(0);

            ApplyMove(chosenMove);

            return chosenMove;
        }

        public async Task<MoveViewer> CpuMove(int maxDepth)
        {
            SearchStarted?.Invoke(this, new EventArgs());

            var searchResults = await Task.Run(() => _search.Go(_workspace, maxDepth));

            SearchCompleted?.Invoke(this, new SearchCompleteEventArgs(searchResults));

            var chosenMove = searchResults.MoveEvaluations
                .OrderByDescending(x => x.Score).FirstOrDefault();

            //var chosenMove = moves.OrderByDescending(x => x.Score).FirstOrDefault();

            if (chosenMove == null)
                return new MoveViewer(0);

            ApplyMove(chosenMove.Move);

            return chosenMove.Move;
        }

        public double Evaluate() => _positionEvaluator.Evaluate(_bitBoard);

        public GameState GetGameState() => GameState.From(this);

        public BitBoard GetBitBoard() =>
            _bitBoard;

        public Piece GetPiece(int squareIndex) =>
            _bitBoard.GetPiece(squareIndex.ToSquareFlag());

        public byte GetInstanceNumber(Piece piece, SquareFlag square) =>
            _bitBoard.GetInstanceNumber(piece, square);

        public SquareFlag GetSquaresWithPieceOn() =>
            _bitBoard.White | _workspace.BitBoard.Black;

        public MoveViewer TryMove(int fromSquareIndex, int toSquareIndex, PieceType promotionType = PieceType.None)
        {
            MoveViewer move = null;

            // Second entry into function where the promotion type has now been defined
            if (promotionType != PieceType.None)
            {
                move = TryFindMove(fromSquareIndex, toSquareIndex, promotionType);

                if (move.Value == 0)
                {
                    InvalidMove?.Invoke(this, new InvalidMoveEventArgs(fromSquareIndex, toSquareIndex));

                    return new MoveViewer(0);
                }

                ApplyMove(move);

                return move;
            }

            var isPawnPromotion = IsMovePromotion(fromSquareIndex, toSquareIndex);

            if (isPawnPromotion)
            {
                // If it is then we have to stop and get the desired promotion type before continuing
                PromotionTypeRequired?.Invoke(this, new PromotionTypeRequiredEventArgs(fromSquareIndex, toSquareIndex));

                return new MoveViewer(0);
            }

            move = TryFindMove(fromSquareIndex, toSquareIndex);

            if (move.Value == 0)
            {
                InvalidMove?.Invoke(this, new InvalidMoveEventArgs(fromSquareIndex, toSquareIndex));

                return new MoveViewer(0);
            }

            ApplyMove(move);

            return move;
        }

        private bool IsMovePromotion(int fromSquareIndex, int toSquareIndex)
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

        private void ApplyMove(MoveViewer move)
        {
            _workspace.MakeMove(move.Value);

            Ply++;

            // Seems odd that separate things are tracking current colour, not sure how better to handle it though
            if (_workspace.Colour != ToPlay)
                throw new System.Exception("Game and Workspace out of sync");

            MoveApplied?.Invoke(this, new MoveAppliedEventArgs(move, GetGameState()));

            var moves = new List<uint>();

            _moveGenerator.Generate(_workspace, moves);

            AvailableMoves = moves.Select(x => new MoveViewer(x));

            if (!AvailableMoves.Any())
                Checkmate?.Invoke(this, new EventArgs());
        }

        private void _search_SearchCompleted(object sender, SearchCompleteEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
