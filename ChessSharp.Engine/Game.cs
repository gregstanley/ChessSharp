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
    public class Game : IGame
    {
        private readonly BitBoard bitBoard;

        private readonly MoveGenerator moveGenerator;

        private readonly Search search;

        private readonly PositionEvaluator positionEvaluator;

        private readonly Stack<GameHistoryNode> history = new Stack<GameHistoryNode>();

        public Game(BitBoard board, TranspositionTable transpositionTable, Colour humanColour = Colour.None)
        {
            bitBoard = board;

            HumanColour = humanColour;
            CpuColour = humanColour.Opposite();

            moveGenerator = new MoveGenerator(16);

            positionEvaluator = new PositionEvaluator();

            search = new Search(moveGenerator, positionEvaluator, transpositionTable);

            search.Info += _search_Info;

            var moves = new List<uint>();

            moveGenerator.Generate(bitBoard, Colour.White, moves);

            AvailableMoves = moves.Select(x => new MoveViewer(x));

            history.Push(new GameHistoryNode(bitBoard.History.First(), GetGameState()));
        }

        public delegate void InvalidMoveEventDelegate(object sender, InvalidMoveEventArgs args);

        public delegate void PromotionTypeRequiredEventDelegate(object sender, PromotionTypeRequiredEventArgs args);

        public delegate void SearchStartedEventDelegate(object sender, EventArgs args);

        public delegate void SearchCompletedEventDelegate(object sender, SearchCompleteEventArgs args);

        public delegate void MoveAppliedEventDelegate(object sender, MoveAppliedEventArgs args);

        public delegate void DrawEventDelegate(object sender, MoveAppliedEventArgs args);

        public delegate void CheckmateEventDelegate(object sender, MoveAppliedEventArgs args);

        public delegate void InfoEventDelegate(object sender, InfoEventArgs args);

        public event InvalidMoveEventDelegate InvalidMove;

        public event PromotionTypeRequiredEventDelegate PromotionTypeRequired;

        public event SearchStartedEventDelegate SearchStarted;

        public event SearchCompletedEventDelegate SearchCompleted;

        public event MoveAppliedEventDelegate MoveApplied;

        public event DrawEventDelegate Draw;

        public event CheckmateEventDelegate Checkmate;

        public event InfoEventDelegate Info;

        public int Ply { get; private set; } = 1;

        public Colour ToPlay
        {
            get { return Ply % 2 == 1 ? Colour.White : Colour.Black; }
        }

        public int HalfMoveClock { get; private set; } = 0;

        public int FullTurn
        {
            get { return (Ply + 1) / 2; }
        }

        public Colour HumanColour { get; private set; } = Colour.None;

        public Colour CpuColour { get; private set; } = Colour.None;

        public bool IsHumanTurn
        {
            get { return HumanColour != Colour.None && ToPlay == HumanColour; }
        }

        public IEnumerable<MoveViewer> AvailableMoves { get; private set; }

        public int SearchedPositionsCount => search.PositionCount;

        //public IReadOnlyCollection<HistoryState> History => bitBoard.History;

        public IReadOnlyCollection<GameHistoryNode> History => history;

        public GameHistoryNode CurrentState => history.Last();

        public MoveViewer TryFindMove(int fromSquareIndex, int toSquareIndex, PieceType promotionPieceType = PieceType.None)
        {
            if (fromSquareIndex == toSquareIndex)
                return new MoveViewer(0);

            var fromSquare = fromSquareIndex.ToSquareFlag();
            var toSquare = toSquareIndex.ToSquareFlag();

            var move = TryPromotions(fromSquare, toSquare, AvailableMoves, promotionPieceType);

            if (move.Value == 0)
                move = TryCastles(bitBoard, HumanColour, fromSquare, toSquare, AvailableMoves);

            if (move.Value == 0)
                move = AvailableMoves.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare) ?? new MoveViewer(0);

            return move;
        }

        public MoveViewer CpuMoveFirst()
        {
            var chosenMove = AvailableMoves.FirstOrDefault();

            if (chosenMove == null)
                return new MoveViewer(0);

            ApplyMove(bitBoard, CpuColour, chosenMove);

            return chosenMove;
        }

        public async Task<MoveViewer> CpuMove(int maxDepth)
        {
            SearchStarted?.Invoke(this, new EventArgs());

            var searchResults = await Task.Run(() => search.Go(bitBoard, CpuColour, maxDepth));

            SearchCompleted?.Invoke(this, new SearchCompleteEventArgs(searchResults));

            var chosenMove = searchResults.MoveEvaluations
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            if (chosenMove == null)
                return new MoveViewer(0);

            ApplyMove(bitBoard, CpuColour, chosenMove.Move);

            return chosenMove.Move;
        }

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

                ApplyMove(bitBoard, HumanColour, move);

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

            ApplyMove(bitBoard, HumanColour, move);

            return move;
        }

        private void _search_Info(object sender, InfoEventArgs args)
        {
            Info?.Invoke(this, args);
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

        private MoveViewer TryCastles(BitBoard bitBoard, Colour colour, SquareFlag fromSquare, SquareFlag toSquare, IEnumerable<MoveViewer> availableMoves)
        {
            if (colour == Colour.White && fromSquare == SquareFlagConstants.WhiteKingStartSquare)
            {
                if (toSquare == SquareFlagConstants.WhiteKingSideRookStartSquare && bitBoard.WhiteCanCastleKingSide)
                {
                    return availableMoves.SingleOrDefault(x => x.MoveType == MoveType.CastleKing)
                        ?? new MoveViewer(0);
                }
                else if (toSquare == SquareFlagConstants.WhiteQueenSideRookStartSquare && bitBoard.WhiteCanCastleQueenSide)
                {
                    return availableMoves.SingleOrDefault(x => x.MoveType == MoveType.CastleQueen)
                        ?? new MoveViewer(0);
                }
            }

            if (colour == Colour.Black && fromSquare == SquareFlagConstants.BlackKingStartSquare)
            {
                if (toSquare == SquareFlagConstants.BlackKingSideRookStartSquare && bitBoard.BlackCanCastleKingSide)
                {
                    return availableMoves.SingleOrDefault(x => x.MoveType == MoveType.CastleKing)
                        ?? new MoveViewer(0);
                }
                else if (toSquare == SquareFlagConstants.BlackQueenSideRookStartSquare && bitBoard.BlackCanCastleQueenSide)
                {
                    return availableMoves.SingleOrDefault(x => x.MoveType == MoveType.CastleQueen)
                        ?? new MoveViewer(0);
                }
            }

            return new MoveViewer(0);
        }

        private void ApplyMove(BitBoard bitBoard, Colour colour, MoveViewer move)
        {
            bitBoard.MakeMove(move.Value);

            Ply++;

            if (move.PieceType == PieceType.Pawn || move.CapturePieceType != PieceType.None)
                HalfMoveClock = 0;
            else
                ++HalfMoveClock;

            var currentState = GetGameState();

            history.Push(new GameHistoryNode(bitBoard.History.First(), currentState));

            if (HalfMoveClock > 50)
            {
                Draw?.Invoke(this, new MoveAppliedEventArgs(move, currentState, Evaluate()));

                return;
            }

            var moves = new List<uint>();

            moveGenerator.Generate(bitBoard, colour.Opposite(), moves);

            AvailableMoves = moves.Select(x => new MoveViewer(x));

            if (!AvailableMoves.Any())
            {
                Checkmate?.Invoke(this, new MoveAppliedEventArgs(move, currentState, Evaluate()));

                return;
            }

            MoveApplied?.Invoke(this, new MoveAppliedEventArgs(move, currentState, Evaluate()));
        }

        private int Evaluate() => positionEvaluator.Evaluate(bitBoard);

        private GameState GetGameState() =>
            new GameState(
                Ply,
                ToPlay,
                HalfMoveClock,
                FullTurn,
                bitBoard.WhiteCanCastleKingSide,
                bitBoard.WhiteCanCastleQueenSide,
                bitBoard.BlackCanCastleKingSide,
                bitBoard.BlackCanCastleQueenSide,
                bitBoard.WhitePawns,
                bitBoard.WhiteRooks,
                bitBoard.WhiteKnights,
                bitBoard.WhiteBishops,
                bitBoard.WhiteQueens,
                bitBoard.WhiteKing,
                bitBoard.BlackPawns,
                bitBoard.BlackRooks,
                bitBoard.BlackKnights,
                bitBoard.BlackBishops,
                bitBoard.BlackQueens,
                bitBoard.BlackKing,
                bitBoard.EnPassant);
    }
}
