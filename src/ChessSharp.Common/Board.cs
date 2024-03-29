﻿using System;
using System.Collections.Generic;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Extensions;
using ChessSharp.Common.Helpers;
using ChessSharp.Common.Keys;
using ChessSharp.Common.Models;

namespace ChessSharp.Common
{
    public class Board : IPieceMap
    {
        private const StateFlag DefaultState =
            StateFlag.WhiteCanCastleKingSide
            | StateFlag.WhiteCanCastleQueenSide
            | StateFlag.BlackCanCastleKingSide
            | StateFlag.BlackCanCastleQueenSide;

        private readonly Zobrist keyGen = new();

        private readonly Stack<BoardStateInfo> history = new(256);

        public Board()
        {
            WhitePawns = SquareFlag.A2 | SquareFlag.B2 | SquareFlag.C2 | SquareFlag.D2
                | SquareFlag.E2 | SquareFlag.F2 | SquareFlag.G2 | SquareFlag.H2;
            WhiteRooks = SquareFlag.A1 | SquareFlag.H1;
            WhiteKnights = SquareFlag.B1 | SquareFlag.G1;
            WhiteBishops = SquareFlag.C1 | SquareFlag.F1;
            WhiteQueens = SquareFlag.D1;
            WhiteKing = SquareFlag.E1;
            BlackPawns = SquareFlag.A7 | SquareFlag.B7 | SquareFlag.C7 | SquareFlag.D7
                | SquareFlag.E7 | SquareFlag.F7 | SquareFlag.G7 | SquareFlag.H7;
            BlackRooks = SquareFlag.A8 | SquareFlag.H8;
            BlackKnights = SquareFlag.B8 | SquareFlag.G8;
            BlackBishops = SquareFlag.C8 | SquareFlag.F8;
            BlackQueens = SquareFlag.D8;
            BlackKing = SquareFlag.E8;

            keyGen.Init();

            Key = keyGen.Hash(this, Colour.White);

            history.Push(new BoardStateInfo(Key, 0, DefaultState, 0));
        }

        public Board(
            SquareFlag whitePawns,
            SquareFlag whiteRooks,
            SquareFlag whiteKnights,
            SquareFlag whiteBishops,
            SquareFlag whiteQueens,
            SquareFlag whiteKing,
            SquareFlag blackPawns,
            SquareFlag blackRooks,
            SquareFlag blackKnights,
            SquareFlag blackBishops,
            SquareFlag blackQueens,
            SquareFlag blackKing,
            StateFlag state,
            SquareFlag enPassant)
        {
            WhitePawns = whitePawns;
            WhiteRooks = whiteRooks;
            WhiteKnights = whiteKnights;
            WhiteBishops = whiteBishops;
            WhiteQueens = whiteQueens;
            WhiteKing = whiteKing;
            BlackPawns = blackPawns;
            BlackRooks = blackRooks;
            BlackKnights = blackKnights;
            BlackBishops = blackBishops;
            BlackQueens = blackQueens;
            BlackKing = blackKing;

            keyGen.Init();

            Key = keyGen.Hash(this, Colour.White);

            history.Push(new BoardStateInfo(Key, 0, state, enPassant));
        }

        public SquareFlag WhitePawns { get; private set; }

        public SquareFlag WhiteRooks { get; private set; }

        public SquareFlag WhiteKnights { get; private set; }

        public SquareFlag WhiteBishops { get; private set; }

        public SquareFlag WhiteQueens { get; private set; }

        public SquareFlag WhiteKing { get; private set; }

        public SquareFlag BlackPawns { get; private set; }

        public SquareFlag BlackRooks { get; private set; }

        public SquareFlag BlackKnights { get; private set; }

        public SquareFlag BlackBishops { get; private set; }

        public SquareFlag BlackQueens { get; private set; }

        public SquareFlag BlackKing { get; private set; }

        public SquareFlag EnPassant =>
            CurrentState.EnPassant;

        public bool WhiteCanCastleKingSide =>
            CurrentState.StateFlags.HasFlag(StateFlag.WhiteCanCastleKingSide);

        public bool WhiteCanCastleQueenSide =>
            CurrentState.StateFlags.HasFlag(StateFlag.WhiteCanCastleQueenSide);

        public bool BlackCanCastleKingSide =>
            CurrentState.StateFlags.HasFlag(StateFlag.BlackCanCastleKingSide);

        public bool BlackCanCastleQueenSide =>
            CurrentState.StateFlags.HasFlag(StateFlag.BlackCanCastleQueenSide);

        public SquareFlag White =>
            WhitePawns | WhiteRooks | WhiteKnights | WhiteBishops | WhiteQueens | WhiteKing;

        public SquareFlag Black =>
            BlackPawns | BlackRooks | BlackKnights | BlackBishops | BlackQueens | BlackKing;

        public ulong Key { get; private set; }

        public IReadOnlyCollection<BoardStateInfo> History => history;

        public BoardStateInfo CurrentState => history.Peek();

        public static Board FromGameState(GameState gameState)
        {
            var boardState = (StateFlag)0;

            if (gameState.WhiteCanCastleKingSide)
                boardState |= StateFlag.WhiteCanCastleKingSide;

            if (gameState.WhiteCanCastleQueenSide)
                boardState |= StateFlag.WhiteCanCastleQueenSide;

            if (gameState.BlackCanCastleKingSide)
                boardState |= StateFlag.BlackCanCastleKingSide;

            if (gameState.BlackCanCastleQueenSide)
                boardState |= StateFlag.BlackCanCastleQueenSide;

            return new Board(
                gameState.WhitePawns,
                gameState.WhiteRooks,
                gameState.WhiteKnights,
                gameState.WhiteBishops,
                gameState.WhiteQueens,
                gameState.WhiteKing,
                gameState.BlackPawns,
                gameState.BlackRooks,
                gameState.BlackKnights,
                gameState.BlackBishops,
                gameState.BlackQueens,
                gameState.BlackKing,
                boardState,
                gameState.EnPassant);
        }

        public SquareFlag GetOccupiedSquares(Colour colour)
        {
            return colour == Colour.White ? White : Black;
        }

        public SquareFlag GetPawnSquares(Colour colour)
        {
            return colour == Colour.White ? WhitePawns : BlackPawns;
        }

        public SquareFlag GetRookSquares(Colour colour)
        {
            return colour == Colour.White ? WhiteRooks : BlackRooks;
        }

        public SquareFlag GetKnightSquares(Colour colour)
        {
            return colour == Colour.White ? WhiteKnights : BlackKnights;
        }

        public SquareFlag GetBishopSquares(Colour colour)
        {
            return colour == Colour.White ? WhiteBishops : BlackBishops;
        }

        public SquareFlag GetQueenSquares(Colour colour)
        {
            return colour == Colour.White ? WhiteQueens : BlackQueens;
        }

        public SquareFlag GetKingSquare(Colour colour)
        {
            return colour == Colour.White ? WhiteKing : BlackKing;
        }

        public byte GetInstanceNumber(Piece piece, SquareFlag square)
        {
            if (piece.Colour == Colour.White)
            {
                if (piece.Type == PieceType.Pawn) return WhitePawns.GetInstanceNumber(square);
                if (piece.Type == PieceType.Rook) return WhiteRooks.GetInstanceNumber(square);
                if (piece.Type == PieceType.Knight) return WhiteKnights.GetInstanceNumber(square);
                if (piece.Type == PieceType.Bishop) return WhiteBishops.GetInstanceNumber(square);
                if (piece.Type == PieceType.Queen) return WhiteQueens.GetInstanceNumber(square);
                if (piece.Type == PieceType.King) return WhiteKing.GetInstanceNumber(square);
            }
            else
            {
                if (piece.Type == PieceType.Pawn) return BlackPawns.GetInstanceNumber(square);
                if (piece.Type == PieceType.Rook) return BlackRooks.GetInstanceNumber(square);
                if (piece.Type == PieceType.Knight) return BlackKnights.GetInstanceNumber(square);
                if (piece.Type == PieceType.Bishop) return BlackBishops.GetInstanceNumber(square);
                if (piece.Type == PieceType.Queen) return BlackQueens.GetInstanceNumber(square);
                if (piece.Type == PieceType.King) return BlackKing.GetInstanceNumber(square);
            }

            return 0;
        }

        public void MakeMove(uint move)
        {
            var colour = move.GetColour();
            var fromSquare = move.GetFrom();
            //var fromSquareIndex = move.GetFromIndex();
            var toSquare = move.GetTo();
            //var toSquareIndex = move.GetToIndex();
            var moveType = move.GetMoveType();
            var pieceType = move.GetPieceType();
            var capturePieceType = move.GetCapturePieceType();

            // Copy current state
            var state = CurrentState.StateFlags;

            var enPassantSquare = (SquareFlag)0;

            if (moveType == MoveType.CastleKing)
            {
                if (colour == Colour.White)
                    MakeWhiteKingSideCastle();
                else
                    MakeBlackKingSideCastle();

                state = RemoveCastleAvailability(colour, state);
            }
            else if (moveType == MoveType.CastleQueen)
            {
                if (colour == Colour.White)
                    MakeWhiteQueenSideCastle();
                else
                    MakeBlackQueenSideCastle();

                state = RemoveCastleAvailability(colour, state);
            }
            else if (moveType == MoveType.EnPassant)
            {
                MovePiece(colour, pieceType, fromSquare, toSquare);

                // Capturing behind the opponent pawn so shift as if we are opponent
                var captureSquare = toSquare.PawnForward(colour.Opposite(), 1);

                RemovePiece(colour.Opposite(), captureSquare);
            }
            else if (moveType == MoveType.PromotionQueen)
            {
                if (capturePieceType != PieceType.None)
                    RemovePiece(colour.Opposite(), toSquare);

                MovePiece(colour, pieceType, fromSquare, toSquare);

                PromotePiece(colour, PieceType.Queen, toSquare);
            }
            else if (moveType == MoveType.PromotionRook)
            {
                if (capturePieceType != PieceType.None)
                    RemovePiece(colour.Opposite(), toSquare);

                MovePiece(colour, pieceType, fromSquare, toSquare);

                PromotePiece(colour, PieceType.Rook, toSquare);
            }
            else if (moveType == MoveType.PromotionBishop)
            {
                if (capturePieceType != PieceType.None)
                    RemovePiece(colour.Opposite(), toSquare);

                MovePiece(colour, pieceType, fromSquare, toSquare);

                PromotePiece(colour, PieceType.Bishop, toSquare);
            }
            else if (moveType == MoveType.PromotionKnight)
            {
                if (capturePieceType != PieceType.None)
                    RemovePiece(colour.Opposite(), toSquare);

                MovePiece(colour, pieceType, fromSquare, toSquare);

                PromotePiece(colour, PieceType.Knight, toSquare);
            }
            else if (pieceType == PieceType.Pawn)
            {
                // We can only move forward two from start square
                var toSquareTemp = fromSquare.PawnForward(colour, 2);

                // So if the 'to' square is the same it might set en passant flag
                if (toSquareTemp == toSquare)
                {
                    var targetSquare = (SquareFlag)((ulong)toSquare >> Math.Abs((int)MoveDirection.West));

                    var targetPiece = PieceMapHelpers.GetPiece(this, targetSquare);

                    if (targetPiece.Colour == colour.Opposite() && targetPiece.Type == PieceType.Pawn)
                    {
                        enPassantSquare = fromSquare.PawnForward(colour, 1);
                    }
                    else
                    {
                        targetSquare = (SquareFlag)((ulong)toSquare << (int)MoveDirection.East);

                        targetPiece = PieceMapHelpers.GetPiece(this, targetSquare);

                        if (targetPiece.Colour == colour.Opposite() && targetPiece.Type == PieceType.Pawn)
                        {
                            enPassantSquare = fromSquare.PawnForward(colour, 1);
                        }
                    }
                }

                MovePiece(colour, pieceType, fromSquare, toSquare);

                if (capturePieceType != PieceType.None)
                    RemovePiece(colour.Opposite(), toSquare);
            }
            else
            {
                MovePiece(colour, pieceType, fromSquare, toSquare);

                if (pieceType == PieceType.Rook)
                {
                    if (colour == Colour.White)
                    {
                        if (fromSquare == SquareFlagConstants.WhiteKingSideRookStartSquare)
                            state = RemoveCastleAvailability(colour, MoveType.CastleKing, state);
                        else if (fromSquare == SquareFlagConstants.WhiteQueenSideRookStartSquare)
                            state = RemoveCastleAvailability(colour, MoveType.CastleQueen, state);
                    }
                    else
                    {
                        if (fromSquare == SquareFlagConstants.BlackKingSideRookStartSquare)
                            state = RemoveCastleAvailability(colour, MoveType.CastleKing, state);
                        else if (fromSquare == SquareFlagConstants.BlackQueenSideRookStartSquare)
                            state = RemoveCastleAvailability(colour, MoveType.CastleQueen, state);
                    }
                }
                else if (pieceType == PieceType.King)
                {
                    state = RemoveCastleAvailability(colour, state);
                }

                if (capturePieceType != PieceType.None)
                {
                    RemovePiece(colour.Opposite(), toSquare);

                    if (capturePieceType == PieceType.Rook)
                    {
                        if (colour == Colour.White)
                        {
                            if (toSquare == SquareFlagConstants.BlackKingSideRookStartSquare)
                                state = RemoveCastleAvailability(Colour.Black, MoveType.CastleKing, state);
                            else if (toSquare == SquareFlagConstants.BlackQueenSideRookStartSquare)
                                state = RemoveCastleAvailability(Colour.Black, MoveType.CastleQueen, state);
                        }
                        else
                        {
                            if (toSquare == SquareFlagConstants.WhiteKingSideRookStartSquare)
                                state = RemoveCastleAvailability(Colour.White, MoveType.CastleKing, state);
                            else if (toSquare == SquareFlagConstants.WhiteQueenSideRookStartSquare)
                                state = RemoveCastleAvailability(Colour.White, MoveType.CastleQueen, state);
                        }
                    }
                }
            }

            Key = keyGen.UpdateHash(Key, move);

            var historyItem = new BoardStateInfo(Key, move, state, enPassantSquare);

            history.Push(historyItem);
        }

        public void UnMakeMove(uint move)
        {
            var colour = move.GetColour();
            var fromSquare = move.GetFrom();
            //var fromSquareIndex = move.GetFromIndex();
            var toSquare = move.GetTo();
            //var toSquareIndex = move.GetToIndex();
            var moveType = move.GetMoveType();
            var pieceType = move.GetPieceType();
            var capturePieceType = move.GetCapturePieceType();

            if (moveType == MoveType.CastleKing)
            {
                if (colour == Colour.White)
                    UnMakeWhiteKingSideCastle();
                else
                    UnMakeBlackKingSideCastle();
            }
            else if (moveType == MoveType.CastleQueen)
            {
                if (colour == Colour.White)
                    UnMakeWhiteQueenSideCastle();
                else
                    UnMakeBlackQueenSideCastle();
            }
            else if (pieceType == PieceType.Pawn)
            {
                if (moveType == MoveType.EnPassant)
                {
                    // Capturing behind the opponent pawn so shift as if we are opponent
                    var captureSquare = toSquare.PawnForward(colour.Opposite(), 1);

                    MovePiece(colour.Opposite(), PieceType.Pawn, captureSquare, captureSquare);

                    MovePiece(colour, pieceType, toSquare, fromSquare);
                }
                else if (moveType == MoveType.PromotionQueen)
                {
                    DemotePiece(colour, PieceType.Queen, toSquare);

                    MovePiece(colour, pieceType, toSquare, fromSquare);

                    if (capturePieceType != PieceType.None)
                        MovePiece(colour.Opposite(), capturePieceType, toSquare, toSquare);
                }
                else if (moveType == MoveType.PromotionRook)
                {
                    DemotePiece(colour, PieceType.Rook, toSquare);

                    MovePiece(colour, pieceType, toSquare, fromSquare);

                    if (capturePieceType != PieceType.None)
                        MovePiece(colour.Opposite(), capturePieceType, toSquare, toSquare);
                }
                else if (moveType == MoveType.PromotionBishop)
                {
                    DemotePiece(colour, PieceType.Bishop, toSquare);

                    MovePiece(colour, pieceType, toSquare, fromSquare);

                    if (capturePieceType != PieceType.None)
                        MovePiece(colour.Opposite(), capturePieceType, toSquare, toSquare);
                }
                else if (moveType == MoveType.PromotionKnight)
                {
                    DemotePiece(colour, PieceType.Knight, toSquare);

                    MovePiece(colour, pieceType, toSquare, fromSquare);

                    if (capturePieceType != PieceType.None)
                        MovePiece(colour.Opposite(), capturePieceType, toSquare, toSquare);
                }
                else
                {
                    MovePiece(colour, pieceType, toSquare, fromSquare);

                    if (capturePieceType != PieceType.None)
                        MovePiece(colour.Opposite(), capturePieceType, toSquare, toSquare);
                }
            }
            else
            {
                MovePiece(colour, pieceType, toSquare, fromSquare);

                if (capturePieceType != PieceType.None)
                {
                    MovePiece(colour.Opposite(), capturePieceType, toSquare, toSquare);
                }
            }

            Key = keyGen.UpdateHash(Key, move);

            history.Pop();
        }

        private void MakeWhiteKingSideCastle()
        {
            MovePiece(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingStartSquare, SquareFlagConstants.WhiteKingSideCastleStep2);

            MovePiece(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteKingSideRookStartSquare, SquareFlagConstants.WhiteKingSideCastleStep1);
        }

        private void MakeWhiteQueenSideCastle()
        {
            MovePiece(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingStartSquare, SquareFlagConstants.WhiteQueenSideCastleStep2);

            MovePiece(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteQueenSideRookStartSquare, SquareFlagConstants.WhiteQueenSideCastleStep1);
        }

        private void MakeBlackKingSideCastle()
        {
            MovePiece(Colour.Black, PieceType.King, SquareFlagConstants.BlackKingStartSquare, SquareFlagConstants.BlackKingSideCastleStep2);

            MovePiece(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackKingSideRookStartSquare, SquareFlagConstants.BlackKingSideCastleStep1);
        }

        private void MakeBlackQueenSideCastle()
        {
            MovePiece(Colour.Black, PieceType.King, SquareFlagConstants.BlackKingStartSquare, SquareFlagConstants.BlackQueenSideCastleStep2);

            MovePiece(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackQueenSideRookStartSquare, SquareFlagConstants.BlackQueenSideCastleStep1);
        }

        private void UnMakeWhiteKingSideCastle()
        {
            MovePiece(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingSideCastleStep2, SquareFlagConstants.WhiteKingStartSquare);

            MovePiece(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteKingSideCastleStep1, SquareFlagConstants.WhiteKingSideRookStartSquare);
        }

        private void UnMakeWhiteQueenSideCastle()
        {
            MovePiece(Colour.White, PieceType.King, SquareFlagConstants.WhiteQueenSideCastleStep2, SquareFlagConstants.WhiteKingStartSquare);

            MovePiece(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteQueenSideCastleStep1, SquareFlagConstants.WhiteQueenSideRookStartSquare);
        }

        private void UnMakeBlackKingSideCastle()
        {
            MovePiece(Colour.Black, PieceType.King, SquareFlagConstants.BlackKingSideCastleStep2, SquareFlagConstants.BlackKingStartSquare);

            MovePiece(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackKingSideCastleStep1, SquareFlagConstants.BlackKingSideRookStartSquare);
        }

        private void UnMakeBlackQueenSideCastle()
        {
            MovePiece(Colour.Black, PieceType.King, SquareFlagConstants.BlackQueenSideCastleStep2, SquareFlagConstants.BlackKingStartSquare);

            MovePiece(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackQueenSideCastleStep1, SquareFlagConstants.BlackQueenSideRookStartSquare);
        }

        private static StateFlag RemoveCastleAvailability(Colour colour, MoveType moveType, StateFlag boardState)
        {
            if (colour == Colour.White)
            {
                if (moveType == MoveType.CastleKing) boardState &= ~StateFlag.WhiteCanCastleKingSide;
                if (moveType == MoveType.CastleQueen) boardState &= ~StateFlag.WhiteCanCastleQueenSide;
            }
            else
            {
                if (moveType == MoveType.CastleKing) boardState &= ~StateFlag.BlackCanCastleKingSide;
                if (moveType == MoveType.CastleQueen) boardState &= ~StateFlag.BlackCanCastleQueenSide;
            }

            return boardState;
        }

        private static StateFlag RemoveCastleAvailability(Colour colour, StateFlag boardState)
        {
            boardState = RemoveCastleAvailability(colour, MoveType.CastleKing, boardState);
            return RemoveCastleAvailability(colour, MoveType.CastleQueen, boardState);
        }

        private void RemovePiece(Colour colour, SquareFlag square)
        {
            // TODO: Burning time here on wasted operations
            if (colour == Colour.White)
            {
                WhitePawns &= ~square;
                WhiteRooks &= ~square;
                WhiteKnights &= ~square;
                WhiteBishops &= ~square;
                WhiteQueens &= ~square;
                WhiteKing &= ~square;
            }
            else
            {
                BlackPawns &= ~square;
                BlackRooks &= ~square;
                BlackKnights &= ~square;
                BlackBishops &= ~square;
                BlackQueens &= ~square;
                BlackKing &= ~square;
            }
        }

        private void MovePiece(Colour colour, PieceType type, SquareFlag fromSquare, SquareFlag toSquare)
        {
            switch (colour)
            {
                case Colour.White when type == PieceType.Pawn:
                    WhitePawns &= ~fromSquare;
                    WhitePawns |= toSquare;
                    return;

                case Colour.White when type == PieceType.Rook:
                    WhiteRooks &= ~fromSquare;
                    WhiteRooks |= toSquare;
                    return;

                case Colour.White when type == PieceType.Knight:
                    WhiteKnights &= ~fromSquare;
                    WhiteKnights |= toSquare;
                    return;

                case Colour.White when type == PieceType.Bishop:
                    WhiteBishops &= ~fromSquare;
                    WhiteBishops |= toSquare;
                    return;

                case Colour.White when type == PieceType.Queen:
                    WhiteQueens &= ~fromSquare;
                    WhiteQueens |= toSquare;
                    return;

                case Colour.White when type == PieceType.King:
                    WhiteKing &= ~fromSquare;
                    WhiteKing |= toSquare;
                    return;

                case Colour.Black when type == PieceType.Pawn:
                    BlackPawns &= ~fromSquare;
                    BlackPawns |= toSquare;
                    return;

                case Colour.Black when type == PieceType.Rook:
                    BlackRooks &= ~fromSquare;
                    BlackRooks |= toSquare;
                    return;

                case Colour.Black when type == PieceType.Knight:
                    BlackKnights &= ~fromSquare;
                    BlackKnights |= toSquare;
                    return;

                case Colour.Black when type == PieceType.Bishop:
                    BlackBishops &= ~fromSquare;
                    BlackBishops |= toSquare;
                    return;

                case Colour.Black when type == PieceType.Queen:
                    BlackQueens &= ~fromSquare;
                    BlackQueens |= toSquare;
                    return;

                case Colour.Black when type == PieceType.King:
                    BlackKing &= ~fromSquare;
                    BlackKing |= toSquare;
                    return;
            }
        }

        private void PromotePiece(Colour colour, PieceType promoteTo, SquareFlag square)
        {
            switch (colour)
            {
                case Colour.White when promoteTo == PieceType.Rook:
                    WhitePawns &= ~square;
                    WhiteRooks |= square;
                    return;

                case Colour.White when promoteTo == PieceType.Knight:
                    WhitePawns &= ~square;
                    WhiteKnights |= square;
                    return;

                case Colour.White when promoteTo == PieceType.Bishop:
                    WhitePawns &= ~square;
                    WhiteBishops |= square;
                    return;

                case Colour.White when promoteTo == PieceType.Queen:
                    WhitePawns &= ~square;
                    WhiteQueens |= square;
                    return;

                case Colour.Black when promoteTo == PieceType.Rook:
                    BlackPawns &= ~square;
                    BlackRooks |= square;
                    return;

                case Colour.Black when promoteTo == PieceType.Knight:
                    BlackPawns &= ~square;
                    BlackKnights |= square;
                    return;

                case Colour.Black when promoteTo == PieceType.Bishop:
                    BlackPawns &= ~square;
                    BlackBishops |= square;
                    return;

                case Colour.Black when promoteTo == PieceType.Queen:
                    BlackPawns &= ~square;
                    BlackQueens |= square;
                    return;
            }
        }

        private void DemotePiece(Colour colour, PieceType promoteTo, SquareFlag square)
        {
            switch (colour)
            {
                case Colour.White when promoteTo == PieceType.Rook:
                    WhiteRooks &= ~square;
                    WhitePawns |= square;
                    return;

                case Colour.White when promoteTo == PieceType.Knight:
                    WhiteKnights &= ~square;
                    WhitePawns |= square;
                    return;

                case Colour.White when promoteTo == PieceType.Bishop:
                    WhiteBishops &= ~square;
                    WhitePawns |= square;
                    return;

                case Colour.White when promoteTo == PieceType.Queen:
                    WhiteQueens &= ~square;
                    WhitePawns |= square;
                    return;

                case Colour.Black when promoteTo == PieceType.Rook:
                    BlackRooks &= ~square;
                    BlackPawns |= square;
                    return;

                case Colour.Black when promoteTo == PieceType.Knight:
                    BlackKnights &= ~square;
                    BlackPawns |= square;
                    return;

                case Colour.Black when promoteTo == PieceType.Bishop:
                    BlackBishops &= ~square;
                    BlackPawns |= square;
                    return;

                case Colour.Black when promoteTo == PieceType.Queen:
                    BlackQueens &= ~square;
                    BlackPawns |= square;
                    return;
            }
        }
    }
}
