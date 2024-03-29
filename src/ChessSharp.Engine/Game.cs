﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChessSharp.Common;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Extensions;
using ChessSharp.Common.Models;
using ChessSharp.Engine.Events;
using ChessSharp.MoveGeneration;


namespace ChessSharp.Engine
{
    public class Game : IGame
    {
        private readonly Board board;

        private readonly MoveGenerator moveGenerator;

        private readonly Search search;

        private readonly PositionEvaluator positionEvaluator;

        private readonly Stack<GameHistoryNode> history = new();

        private readonly List<GameHistoryNode> drawBuffer = new(256);

        public Game(Board board, TranspositionTable transpositionTable, Colour humanColour = Colour.None)
        {
            this.board = board;

            HumanColour = humanColour;
            CpuColour = humanColour.Opposite();

            moveGenerator = new MoveGenerator(16);

            positionEvaluator = new PositionEvaluator();

            search = new Search(moveGenerator, positionEvaluator, transpositionTable);

            search.Info += Search_Info;

            var moves = new List<uint>();

            moveGenerator.Generate(board, Colour.White, moves);

            AvailableMoves = moves.Select(x => new MoveViewer(x));

            history.Push(new GameHistoryNode(board.History.First(), GetGameState()));
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

        public Colour ToPlay => Ply % 2 == 1 ? Colour.White : Colour.Black;

        public int HalfMoveClock { get; private set; } = 0;

        public int FullTurn => (Ply + 1) / 2;

        public Colour HumanColour { get; private set; } = Colour.None;

        public Colour CpuColour { get; private set; } = Colour.None;

        public bool IsHumanTurn => HumanColour != Colour.None && ToPlay == HumanColour;

        public IEnumerable<MoveViewer> AvailableMoves { get; private set; }

        public int SearchedPositionsCount => search.PositionCount;

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
                move = TryCastles(board, HumanColour, fromSquare, toSquare, AvailableMoves);

            if (move.Value == 0)
                move = AvailableMoves.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare) ?? new MoveViewer(0);

            return move;
        }

        public MoveViewer CpuMoveFirst()
        {
            var chosenMove = AvailableMoves.FirstOrDefault();

            if (chosenMove == null)
                return new MoveViewer(0);

            ApplyMove(board, CpuColour, chosenMove);

            return chosenMove;
        }

        public async Task<MoveViewer> CpuMove(int maxDepth)
        {
            SearchStarted?.Invoke(this, new EventArgs());

            var searchResults = await Task.Run(() => search.Go(board, CpuColour, maxDepth));

            SearchCompleted?.Invoke(this, new SearchCompleteEventArgs(searchResults));

            var chosenMove = searchResults.MoveEvaluations
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            if (chosenMove == null)
                return new MoveViewer(0);

            ApplyMove(board, CpuColour, chosenMove.Move);

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

                ApplyMove(board, HumanColour, move);

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

            ApplyMove(board, HumanColour, move);

            return move;
        }

        private void Search_Info(object sender, InfoEventArgs args)
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

        private static MoveViewer TryPromotions(SquareFlag fromSquare, SquareFlag toSquare, IEnumerable<MoveViewer> availableMoves, PieceType promotionPieceType = PieceType.None)
        {
            return promotionPieceType switch
            {
                PieceType.Queen => availableMoves.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare && x.MoveType == MoveType.PromotionQueen)
                    ?? new MoveViewer(0),
                PieceType.Rook => availableMoves.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare && x.MoveType == MoveType.PromotionRook)
                    ?? new MoveViewer(0),
                PieceType.Bishop => availableMoves.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare && x.MoveType == MoveType.PromotionBishop)
                    ?? new MoveViewer(0),
                PieceType.Knight => availableMoves.SingleOrDefault(x => x.From == fromSquare && x.To == toSquare && x.MoveType == MoveType.PromotionKnight)
                    ?? new MoveViewer(0),
                _ => new MoveViewer(0),
            };
        }

        private static MoveViewer TryCastles(Board board, Colour colour, SquareFlag fromSquare, SquareFlag toSquare, IEnumerable<MoveViewer> availableMoves)
        {
            if (colour == Colour.White && fromSquare == SquareFlagConstants.WhiteKingStartSquare)
            {
                if (toSquare == SquareFlagConstants.WhiteKingSideRookStartSquare && board.WhiteCanCastleKingSide)
                {
                    return availableMoves.SingleOrDefault(x => x.MoveType == MoveType.CastleKing)
                        ?? new MoveViewer(0);
                }
                else if (toSquare == SquareFlagConstants.WhiteQueenSideRookStartSquare && board.WhiteCanCastleQueenSide)
                {
                    return availableMoves.SingleOrDefault(x => x.MoveType == MoveType.CastleQueen)
                        ?? new MoveViewer(0);
                }
            }

            if (colour == Colour.Black && fromSquare == SquareFlagConstants.BlackKingStartSquare)
            {
                if (toSquare == SquareFlagConstants.BlackKingSideRookStartSquare && board.BlackCanCastleKingSide)
                {
                    return availableMoves.SingleOrDefault(x => x.MoveType == MoveType.CastleKing)
                        ?? new MoveViewer(0);
                }
                else if (toSquare == SquareFlagConstants.BlackQueenSideRookStartSquare && board.BlackCanCastleQueenSide)
                {
                    return availableMoves.SingleOrDefault(x => x.MoveType == MoveType.CastleQueen)
                        ?? new MoveViewer(0);
                }
            }

            return new MoveViewer(0);
        }

        private void ApplyMove(Board board, Colour colour, MoveViewer move)
        {
            board.MakeMove(move.Value);

            Ply++;

            if (move.PieceType == PieceType.Pawn || move.CapturePieceType != PieceType.None)
                HalfMoveClock = 0;
            else
                ++HalfMoveClock;

            var gameState = GetGameState();

            var currentState = board.CurrentState;

            var historyItem = new GameHistoryNode(currentState, gameState);

            history.Push(historyItem);

            if (historyItem.IsIrreversible)
                drawBuffer.Clear();
            else
                drawBuffer.Add(historyItem);

            var isDrawn = IsDrawn(HalfMoveClock, drawBuffer);

            if (isDrawn)
            {
                Draw?.Invoke(this, new MoveAppliedEventArgs(move, gameState, Evaluate()));

                return;
            }

            var moves = new List<uint>();

            moveGenerator.Generate(board, colour.Opposite(), moves);

            AvailableMoves = moves.Select(x => new MoveViewer(x));

            if (!AvailableMoves.Any())
            {
                Checkmate?.Invoke(this, new MoveAppliedEventArgs(move, gameState, Evaluate()));

                return;
            }

            MoveApplied?.Invoke(this, new MoveAppliedEventArgs(move, gameState, Evaluate()));
        }

        private bool IsDrawn(int halfMoveClock, List<GameHistoryNode> gameHistoryNodes)
        {
            if (halfMoveClock > 50)
                return true;

            // TODO: Understand the 'fast' mathod here
            // https://chess.stackexchange.com/questions/17127/programming-the-three-fold-repetition-for-my-chess-engine
            // and then implement in search
            var previousCount = 0;

            // Note: We step in twos as a match must be same player to move
            for (var i = gameHistoryNodes.Count - 1; i >= 0; i -= 2)
            {
                var nextItem = gameHistoryNodes[i];

                if (nextItem.IsIrreversible)
                    break;

                if (nextItem.Key == board.Key)
                    ++previousCount;
            }

            return previousCount >= 2;
        }

        private int Evaluate()
        {
            return positionEvaluator.Evaluate(board);
        }

        private GameState GetGameState()
        {
            return new GameState(
                Ply,
                ToPlay,
                HalfMoveClock,
                FullTurn,
                board.WhiteCanCastleKingSide,
                board.WhiteCanCastleQueenSide,
                board.BlackCanCastleKingSide,
                board.BlackCanCastleQueenSide,
                board.WhitePawns,
                board.WhiteRooks,
                board.WhiteKnights,
                board.WhiteBishops,
                board.WhiteQueens,
                board.WhiteKing,
                board.BlackPawns,
                board.BlackRooks,
                board.BlackKnights,
                board.BlackBishops,
                board.BlackQueens,
                board.BlackKing,
                board.EnPassant);
        }
    }
}
