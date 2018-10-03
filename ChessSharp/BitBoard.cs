using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System;
using System.Collections.Generic;

namespace ChessSharp
{
    public class BitBoard
    {
        private const BoardState DefaultState =
            BoardState.WhiteCanCastleKingSide
            | BoardState.WhiteCanCastleQueenSide
            | BoardState.BlackCanCastleKingSide
            | BoardState.BlackCanCastleQueenSide;

        public static BitBoard FromFen(Fen fen)
        {
            var squares = fen.GetSquaresStates();

            var whitePawns = (SquareFlag)0;
            var whiteRooks = (SquareFlag)0;
            var whiteKnights = (SquareFlag)0;
            var whiteBishops = (SquareFlag)0;
            var whiteQueens = (SquareFlag)0;
            var whiteKing = (SquareFlag)0;
            var blackPawns = (SquareFlag)0;
            var blackRooks = (SquareFlag)0;
            var blackKnights = (SquareFlag)0;
            var blackBishops = (SquareFlag)0;
            var blackQueens = (SquareFlag)0;
            var blackKing = (SquareFlag)0;

            foreach (var square in squares)
            {
                if (square.Colour == Colour.None)
                    continue;

                if (square.Colour == Colour.White)
                {
                    switch (square.Type)
                    {
                        case PieceType.Pawn: whitePawns |= square.Square; break;
                        case PieceType.Rook: whiteRooks |= square.Square; break;
                        case PieceType.Knight: whiteKnights |= square.Square; break;
                        case PieceType.Bishop: whiteBishops |= square.Square; break;
                        case PieceType.Queen: whiteQueens |= square.Square; break;
                        case PieceType.King: whiteKing |= square.Square; break;
                    }
                }
                else
                {
                    switch (square.Type)
                    {
                        case PieceType.Pawn: blackPawns |= square.Square; break;
                        case PieceType.Rook: blackRooks |= square.Square; break;
                        case PieceType.Knight: blackKnights |= square.Square; break;
                        case PieceType.Bishop: blackBishops |= square.Square; break;
                        case PieceType.Queen: blackQueens |= square.Square; break;
                        case PieceType.King: blackKing |= square.Square; break;
                    }
                }
            }

            return new BitBoard(whitePawns, whiteRooks, whiteKnights, whiteBishops, whiteQueens, whiteKing,
                blackPawns, blackRooks, blackKnights, blackBishops, blackQueens, blackKing, fen.BoardState);
        }

        private Stack<BoardState> _boardStates { get; } = new Stack<BoardState>(256);

        public BitBoard()
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

            _boardStates.Push(DefaultState);
        }

        public BitBoard(SquareFlag whitePawns,
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
            BoardState state)
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

            _boardStates.Push(state);
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
            _boardStates.Peek().GetEnPassantSquare();

        public bool WhiteCanCastleKingSide =>
            _boardStates.Peek().HasFlag(BoardState.WhiteCanCastleKingSide);

        public bool WhiteCanCastleQueenSide =>
            _boardStates.Peek().HasFlag(BoardState.WhiteCanCastleQueenSide);

        public bool BlackCanCastleKingSide =>
            _boardStates.Peek().HasFlag(BoardState.BlackCanCastleKingSide);

        public bool BlackCanCastleQueenSide =>
            _boardStates.Peek().HasFlag(BoardState.BlackCanCastleQueenSide);

        public SquareFlag White =>
            WhitePawns | WhiteRooks | WhiteKnights | WhiteBishops | WhiteQueens | WhiteKing;

        public SquareFlag Black =>
            BlackPawns | BlackRooks | BlackKnights | BlackBishops | BlackQueens | BlackKing;

        public SquareFlag GetOccupiedSquares(Colour colour) =>
            colour == Colour.White ? White : Black;

        public SquareFlag GetPawnSquares(Colour colour) =>
            colour == Colour.White ? WhitePawns : BlackPawns;

        public SquareFlag GetRookSquares(Colour colour) =>
            colour == Colour.White ? WhiteRooks : BlackRooks;

        public SquareFlag GetKnightSquares(Colour colour) =>
            colour == Colour.White ? WhiteKnights : BlackKnights;

        public SquareFlag GetBishopSquares(Colour colour) =>
            colour == Colour.White ? WhiteBishops : BlackBishops;

        public SquareFlag GetQueenSquares(Colour colour) =>
            colour == Colour.White ? WhiteQueens : BlackQueens;

        public SquareFlag GetKingSquare(Colour colour) =>
            colour == Colour.White ? WhiteKing : BlackKing;

        public Colour GetPieceColour(SquareFlag square)
        {
            if (White.HasFlag(square))
                return Colour.White;

            if (Black.HasFlag(square))
                return Colour.Black;

            return Colour.None;
        }

        public PieceType GetPieceType(SquareFlag square)
        {
            var colour = GetPieceColour(square);

            if (colour == Colour.None)
                return PieceType.None;

            if (colour == Colour.White)
            {
                if (WhitePawns.HasFlag(square)) return PieceType.Pawn;
                if (WhiteRooks.HasFlag(square)) return PieceType.Rook;
                if (WhiteKnights.HasFlag(square)) return PieceType.Knight;
                if (WhiteBishops.HasFlag(square)) return PieceType.Bishop;
                if (WhiteQueens.HasFlag(square)) return PieceType.Queen;
                if (WhiteKing.HasFlag(square)) return PieceType.King;
            }
            else
            {
                if (BlackPawns.HasFlag(square)) return PieceType.Pawn;
                if (BlackRooks.HasFlag(square)) return PieceType.Rook;
                if (BlackKnights.HasFlag(square)) return PieceType.Knight;
                if (BlackBishops.HasFlag(square)) return PieceType.Bishop;
                if (BlackQueens.HasFlag(square)) return PieceType.Queen;
                if (BlackKing.HasFlag(square)) return PieceType.King;
            }

            throw new Exception($"Failed to find piece for {square}");
        }

        public RelativeBitBoard ToRelative(Colour colour)
        {
            var opponentColour = colour.Opposite();

            var relativeBitBoard = new RelativeBitBoard
                (colour,
                 GetPawnSquares(colour),
                 GetRookSquares(colour),
                 GetKnightSquares(colour),
                 GetBishopSquares(colour),
                 GetQueenSquares(colour),
                 GetKingSquare(colour),
                 GetPawnSquares(opponentColour),
                 GetRookSquares(opponentColour),
                 GetKnightSquares(opponentColour),
                 GetBishopSquares(opponentColour),
                 GetQueenSquares(opponentColour),
                 GetKingSquare(opponentColour),
                 _boardStates.Peek());

            return relativeBitBoard;
        }

        public void UnMakeMove(uint move)
        {
            var colour = move.GetColour();
            var fromSquare = move.GetFrom();
            var toSquare = move.GetTo();
            var moveType = move.GetMoveType();

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
            else if (move.GetPieceType() == PieceType.Pawn)
            {
                if (moveType == MoveType.EnPassant)
                {
                    // Capturing behind the opponent pawn so shift as if we are opponent
                    var captureSquare = move.GetTo()
                        .PawnForward(colour.Opposite(), 1);

                    MovePiece(colour.Opposite(), PieceType.Pawn, captureSquare, captureSquare);
                }
                else if (moveType == MoveType.PromotionQueen)
                {
                    DemotePiece(colour, PieceType.Queen, toSquare);
                }
                else if (moveType == MoveType.PromotionRook)
                {
                    DemotePiece(colour, PieceType.Rook, toSquare);
                }
                else if (moveType == MoveType.PromotionBishop)
                {
                    DemotePiece(colour, PieceType.Bishop, toSquare);
                }
                else if (moveType == MoveType.PromotionKnight)
                {
                    DemotePiece(colour, PieceType.Knight, toSquare);
                }
            }
            else
            {
                MovePiece(colour, move.GetPieceType(), toSquare, fromSquare);

                if (move.GetCapturePieceType() != PieceType.None)
                    MovePiece(colour.Opposite(), move.GetCapturePieceType(), toSquare, toSquare);
            }

            _boardStates.Pop();
        }

        public void MakeMove(uint move)
        {
            var colour = move.GetColour();
            var fromSquare = move.GetFrom();
            var toSquare = move.GetTo();
            var moveType = move.GetMoveType();

            // Copy current state
            var state = _boardStates.Peek();

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
            else if (move.GetPieceType() == PieceType.Pawn)
            {
                if (moveType == MoveType.EnPassant)
                {
                    // Capturing behind the opponent pawn so shift as if we are opponent
                    var captureSquare = move.GetTo()
                        .PawnForward(colour.Opposite(), 1);

                    RemovePiece(colour.Opposite(), captureSquare);
                }
                else if (moveType == MoveType.PromotionQueen)
                {
                    PromotePiece(colour, PieceType.Queen, toSquare);
                }
                else if (moveType == MoveType.PromotionRook)
                {
                    PromotePiece(colour, PieceType.Rook, toSquare);
                }
                else if (moveType == MoveType.PromotionBishop)
                {
                    PromotePiece(colour, PieceType.Bishop, toSquare);
                }
                else if (moveType == MoveType.PromotionKnight)
                {
                    PromotePiece(colour, PieceType.Knight, toSquare);
                }
                else
                {
                    // We can only move forward two from start square
                    var toSquareTemp = move.GetFrom()
                        .PawnForward(colour, 2);

                    // So if the 'to' square is the same it might set en passant flag
                    if (toSquareTemp == move.GetTo())
                    {
                        var targetSquare = (SquareFlag)((ulong)toSquare >> Math.Abs((int)MoveDirection.West));
                        
                        var pieceColour = GetPieceColour(targetSquare);
                        var pieceType = GetPieceType(targetSquare);

                        if (pieceColour == colour.Opposite() && pieceType == PieceType.Pawn)
                        {
                            //EnPassant = move.GetFrom().PawnForward(colour, 1);
                            var enPassantSquare = move.GetFrom().PawnForward(colour, 1);
                            state = state.AddEnPassantSquare(enPassantSquare);
                        }
                        else
                        {
                            targetSquare = (SquareFlag)((ulong)toSquare << (int)MoveDirection.East);

                            pieceColour = GetPieceColour(targetSquare);
                            pieceType = GetPieceType(targetSquare);

                            if (pieceColour == colour.Opposite() && pieceType == PieceType.Pawn)
                            {
                                //EnPassant = move.GetFrom().PawnForward(colour, 1);
                                var enPassantSquare = move.GetFrom().PawnForward(colour, 1);
                                state = state.AddEnPassantSquare(enPassantSquare);
                            }
                        }
                    }
                }
            }
            else
            {
                MovePiece(colour, move.GetPieceType(), fromSquare, toSquare);

                if (move.GetCapturePieceType() != PieceType.None)
                    RemovePiece(colour.Opposite(), toSquare);

                var pieceType = move.GetPieceType();

                // Castling rights management
                if (colour == Colour.White)
                {
                    if (state.HasFlag(BoardState.WhiteCanCastleKingSide))
                    {
                        if (pieceType == PieceType.Rook)
                        {
                            if (fromSquare == SquareFlag.H1)
                                state = RemoveCastleAvailability(colour, MoveType.CastleKing, state);
                        }
                        else if (pieceType == PieceType.King)
                        {
                            state = RemoveCastleAvailability(colour, state);
                        }
                    }

                    if (state.HasFlag(BoardState.WhiteCanCastleQueenSide))
                    {
                        if (pieceType == PieceType.Rook)
                        {
                            if (fromSquare == SquareFlag.A1)
                                state = RemoveCastleAvailability(colour, MoveType.CastleQueen, state);
                        }
                        else if (pieceType == PieceType.King)
                        {
                            state = RemoveCastleAvailability(colour, state);
                        }
                    }
                }
                else
                {
                    if (state.HasFlag(BoardState.BlackCanCastleKingSide))
                    {
                        if (pieceType == PieceType.Rook)
                        {
                            if (fromSquare == SquareFlag.H8)
                                state = RemoveCastleAvailability(colour, MoveType.CastleKing, state);
                        }
                        else if (pieceType == PieceType.King)
                        {
                            state = RemoveCastleAvailability(colour, state);
                        }
                    }

                    if (state.HasFlag(BoardState.BlackCanCastleQueenSide))
                    {
                        if (pieceType == PieceType.Rook)
                        {
                            if (fromSquare == SquareFlag.A8)
                                state = RemoveCastleAvailability(colour, MoveType.CastleQueen, state);
                        }
                        else if (pieceType == PieceType.King)
                        {
                            state = RemoveCastleAvailability(colour, state);
                        }
                    }
                }
            }

            _boardStates.Push(state);
        }

        private void MakeWhiteKingSideCastle()
        {
            MovePiece(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingStartSquare,
                SquareFlagConstants.WhiteKingSideCastleStep2);

            MovePiece(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteKingSideRookStartSquare,
                SquareFlagConstants.WhiteKingSideCastleStep1);
        }

        private void MakeWhiteQueenSideCastle()
        {
            MovePiece(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingStartSquare,
                SquareFlagConstants.WhiteQueenSideCastleStep2);

            MovePiece(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteQueenSideRookStartSquare,
                SquareFlagConstants.WhiteQueenSideCastleStep1);
        }

        private void MakeBlackKingSideCastle()
        {
            MovePiece(Colour.Black, PieceType.King, SquareFlagConstants.BlackKingStartSquare,
                SquareFlagConstants.BlackKingSideCastleStep2);

            MovePiece(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackKingSideRookStartSquare,
                SquareFlagConstants.BlackKingSideCastleStep1);
        }

        private void MakeBlackQueenSideCastle()
        {
            MovePiece(Colour.Black, PieceType.King, SquareFlagConstants.BlackKingStartSquare,
                SquareFlagConstants.BlackQueenSideCastleStep2);

            MovePiece(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackQueenSideRookStartSquare,
                SquareFlagConstants.BlackQueenSideCastleStep1);
        }

        private void UnMakeWhiteKingSideCastle()
        {
            MovePiece(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingSideCastleStep2,
                SquareFlagConstants.WhiteKingStartSquare);

            MovePiece(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteKingSideCastleStep1,
                SquareFlagConstants.WhiteKingSideRookStartSquare);
        }

        private void UnMakeWhiteQueenSideCastle()
        {
            MovePiece(Colour.White, PieceType.King, SquareFlagConstants.WhiteQueenSideCastleStep2,
                SquareFlagConstants.WhiteKingStartSquare);

            MovePiece(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteQueenSideCastleStep1,
                SquareFlagConstants.WhiteQueenSideRookStartSquare);
        }

        private void UnMakeBlackKingSideCastle()
        {
            MovePiece(Colour.Black, PieceType.King, SquareFlagConstants.BlackKingSideCastleStep2,
                SquareFlagConstants.BlackKingStartSquare);

            MovePiece(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackKingSideCastleStep1,
                SquareFlagConstants.BlackKingSideRookStartSquare);
        }

        private void UnMakeBlackQueenSideCastle()
        {
            MovePiece(Colour.Black, PieceType.King, SquareFlagConstants.BlackQueenSideCastleStep2,
                SquareFlagConstants.BlackKingStartSquare);

            MovePiece(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackQueenSideCastleStep1,
                SquareFlagConstants.BlackQueenSideRookStartSquare);
        }

        private BoardState RemoveCastleAvailability(Colour colour, MoveType moveType, BoardState boardState)
        {
            if (colour == Colour.White)
            {
                if (moveType == MoveType.CastleKing) boardState &= ~BoardState.WhiteCanCastleKingSide;
                if (moveType == MoveType.CastleQueen) boardState &= ~BoardState.WhiteCanCastleQueenSide;
            }
            else
            {
                if (moveType == MoveType.CastleKing) boardState &= ~BoardState.BlackCanCastleKingSide;
                if (moveType == MoveType.CastleQueen) boardState &= ~BoardState.BlackCanCastleQueenSide;
            }

            return boardState;
        }

        private BoardState RemoveCastleAvailability(Colour colour, BoardState boardState)
        {
            boardState = RemoveCastleAvailability(colour, MoveType.CastleKing, boardState);
            return RemoveCastleAvailability(colour, MoveType.CastleQueen, boardState);
        }

        private void RemovePiece(Colour colour, SquareFlag square)
        {
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

        private void MovePiece(Colour colour, PieceType type, SquareFlag start, SquareFlag end)
        {
            if (colour == Colour.White)
            {
                if (type == PieceType.Pawn)
                {
                    WhitePawns &= ~start;
                    WhitePawns |= end;
                    return;
                }

                if (type == PieceType.Rook)
                {
                    WhiteRooks &= ~start;
                    WhiteRooks |= end;
                    return;
                }

                if (type == PieceType.Knight)
                {
                    WhiteKnights &= ~start;
                    WhiteKnights |= end;
                    return;
                }

                if (type == PieceType.Bishop)
                {
                    WhiteBishops &= ~start;
                    WhiteBishops |= end;
                    return;
                }

                if (type == PieceType.Queen)
                {
                    WhiteQueens &= ~start;
                    WhiteQueens |= end;
                    return;
                }

                if (type == PieceType.King)
                {
                    WhiteKing &= ~start;
                    WhiteKing |= end;
                    return;
                }
            }
            else
            {
                if (type == PieceType.Pawn)
                {
                    BlackPawns &= ~start;
                    BlackPawns |= end;
                    return;
                }

                if (type == PieceType.Rook)
                {
                    BlackRooks &= ~start;
                    BlackRooks |= end;
                    return;
                }

                if (type == PieceType.Knight)
                {
                    BlackKnights &= ~start;
                    BlackKnights |= end;
                    return;
                }

                if (type == PieceType.Bishop)
                {
                    BlackBishops &= ~start;
                    BlackBishops |= end;
                    return;
                }

                if (type == PieceType.Queen)
                {
                    BlackQueens &= ~start;
                    BlackQueens |= end;
                    return;
                }

                if (type == PieceType.King)
                {
                    BlackKing &= ~start;
                    BlackKing |= end;
                    return;
                }
            }
        }

        private void PromotePiece(Colour colour, PieceType promoteTo, SquareFlag square)
        {
            if (colour == Colour.White)
            {
                if (promoteTo == PieceType.Rook)
                {
                    WhitePawns &= ~square;
                    WhiteRooks |= square;
                    return;
                }

                if (promoteTo == PieceType.Knight)
                {
                    WhitePawns &= ~square;
                    WhiteKnights |= square;
                    return;
                }

                if (promoteTo == PieceType.Bishop)
                {
                    WhitePawns &= ~square;
                    WhiteBishops |= square;
                    return;
                }

                if (promoteTo == PieceType.Queen)
                {
                    WhitePawns &= ~square;
                    WhiteQueens |= square;
                    return;
                }
            }
            else
            {
                if (promoteTo == PieceType.Rook)
                {
                    BlackPawns &= ~square;
                    BlackRooks |= square;
                    return;
                }

                if (promoteTo == PieceType.Knight)
                {
                    BlackPawns &= ~square;
                    BlackKnights |= square;
                    return;
                }

                if (promoteTo == PieceType.Bishop)
                {
                    BlackPawns &= ~square;
                    BlackBishops |= square;
                    return;
                }

                if (promoteTo == PieceType.Queen)
                {
                    BlackPawns &= ~square;
                    BlackQueens |= square;
                    return;
                }
            }
        }

        private void DemotePiece(Colour colour, PieceType promoteTo, SquareFlag square)
        {
            if (colour == Colour.White)
            {
                if (promoteTo == PieceType.Rook)
                {
                    WhiteRooks &= ~square;
                    WhitePawns |= square;
                    return;
                }

                if (promoteTo == PieceType.Knight)
                {
                    WhiteKnights &= ~square;
                    WhitePawns |= square;
                    return;
                }

                if (promoteTo == PieceType.Bishop)
                {
                    WhiteBishops &= ~square;
                    WhitePawns |= square;
                    return;
                }

                if (promoteTo == PieceType.Queen)
                {
                    WhiteQueens &= ~square;
                    WhitePawns |= square;
                    return;
                }
            }
            else
            {
                if (promoteTo == PieceType.Rook)
                {
                    BlackRooks &= ~square;
                    BlackPawns |= square;
                    return;
                }

                if (promoteTo == PieceType.Knight)
                {
                    BlackKnights &= ~square;
                    BlackPawns |= square;
                    return;
                }

                if (promoteTo == PieceType.Bishop)
                {
                    BlackBishops &= ~square;
                    BlackPawns |= square;
                    return;
                }

                if (promoteTo == PieceType.Queen)
                {
                    BlackQueens &= ~square;
                    BlackPawns |= square;
                    return;
                }
            }
        }
    }
}
