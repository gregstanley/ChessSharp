using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private Stack<uint> _moves { get; } = new Stack<uint>(256);
        private RelativeBitBoard _relativeBitBoard { get; }

        private enum PieceStateOffset
        {
            Pawn = 0,
            Rook = 8,
            Knight = 10,
            Bishop = 12,
            Queen = 14,
            King = 15
        }

        private int[] _pieceStateOffsets = new[]
        {
            0,
            (int)PieceStateOffset.Pawn,
            (int)PieceStateOffset.Rook,
            (int)PieceStateOffset.Knight,
            (int)PieceStateOffset.Bishop,
            (int)PieceStateOffset.Queen,
            (int)PieceStateOffset.King
        };

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

            _relativeBitBoard = new RelativeBitBoard(Colour.White, WhitePawns, WhiteRooks, WhiteKnights, WhiteBishops, WhiteQueens,
                WhiteKing, BlackPawns, BlackRooks, BlackKnights, BlackBishops, BlackQueens, BlackKing, _boardStates.Peek());
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

            _relativeBitBoard = new RelativeBitBoard(Colour.White, WhitePawns, WhiteRooks, WhiteKnights, WhiteBishops, WhiteQueens,
                WhiteKing, BlackPawns, BlackRooks, BlackKnights, BlackBishops, BlackQueens, BlackKing, _boardStates.Peek());
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

        public RelativeBitBoard RelativeTo(Colour colour)
        {
            var opponentColour = colour.Opposite();

            _relativeBitBoard.Set(colour,
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

            return _relativeBitBoard;
        }

        public IReadOnlyCollection<MoveViewer> MoveHistory =>
            _moves.Select(x => new MoveViewer(x)).ToList();

        public void MakeMove(uint move)
        {
            var colour = move.GetColour();
            var fromSquare = move.GetFrom();
            var fromSquareIndex = move.GetFromIndex();
            var toSquare = move.GetTo();
            var toSquareIndex = move.GetToIndex();
            var moveType = move.GetMoveType();
            var pieceType = move.GetPieceType();
            var capturePieceType = move.GetCapturePieceType();

            if (capturePieceType != PieceType.None)
            { var bp = true; }
            // Copy current state
            var state = _boardStates.Peek().Next();

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
            else if (pieceType == PieceType.Pawn)
            {
                if (moveType == MoveType.EnPassant)
                {
                    MovePiece(colour, pieceType, fromSquare, toSquare);
                    //UpdatePieceStateSquare(colour, pieceType, fromSquareIndex, toSquareIndex);

                    // Capturing behind the opponent pawn so shift as if we are opponent
                    var captureSquare = toSquare.PawnForward(colour.Opposite(), 1);

                    RemovePiece(colour.Opposite(), captureSquare);
                    //UpdatePieceStateType(colour.Opposite(), captureSquare.ToSquareIndex(), PieceType.None);
                }
                else if (moveType == MoveType.PromotionQueen)
                {
                    if (capturePieceType != PieceType.None)
                    {
                        RemovePiece(colour.Opposite(), toSquare);
                        //UpdatePieceStateType(colour.Opposite(), toSquareIndex, PieceType.None);
                    }

                    MovePiece(colour, pieceType, fromSquare, toSquare);
                    //UpdatePieceStateSquare(colour, pieceType, fromSquareIndex, toSquareIndex);

                    PromotePiece(colour, PieceType.Queen, toSquare);
                    //UpdatePieceStateType(colour, toSquareIndex, PieceType.Queen);
                }
                else if (moveType == MoveType.PromotionRook)
                {
                    if (capturePieceType != PieceType.None)
                    {
                        RemovePiece(colour.Opposite(), toSquare);
                        //UpdatePieceStateType(colour.Opposite(), toSquareIndex, PieceType.None);
                    }

                    MovePiece(colour, pieceType, fromSquare, toSquare);
                    //UpdatePieceStateSquare(colour, pieceType, fromSquareIndex, toSquareIndex);

                    PromotePiece(colour, PieceType.Rook, toSquare);
                    //UpdatePieceStateType(colour, toSquareIndex, PieceType.Rook);
                }
                else if (moveType == MoveType.PromotionBishop)
                {
                    if (capturePieceType != PieceType.None)
                    {
                        RemovePiece(colour.Opposite(), toSquare);
                        //UpdatePieceStateType(colour.Opposite(), toSquareIndex, PieceType.None);
                    }

                    MovePiece(colour, pieceType, fromSquare, toSquare);
                    //UpdatePieceStateSquare(colour, pieceType, fromSquareIndex, toSquareIndex);

                    PromotePiece(colour, PieceType.Bishop, toSquare);
                    //UpdatePieceStateType(colour, toSquareIndex, PieceType.Bishop);
                }
                else if (moveType == MoveType.PromotionKnight)
                {
                    if (capturePieceType != PieceType.None)
                    {
                        RemovePiece(colour.Opposite(), toSquare);
                        //UpdatePieceStateType(colour.Opposite(), toSquareIndex, PieceType.None);
                    }

                    MovePiece(colour, pieceType, fromSquare, toSquare);
                    //UpdatePieceStateSquare(colour, pieceType, fromSquareIndex, toSquareIndex);

                    PromotePiece(colour, PieceType.Knight, toSquare);
                    //UpdatePieceStateType(colour, toSquareIndex, PieceType.Knight);
                }
                else
                {
                    // We can only move forward two from start square
                    var toSquareTemp = fromSquare.PawnForward(colour, 2);

                    // So if the 'to' square is the same it might set en passant flag
                    if (toSquareTemp == toSquare)
                    {
                        var targetSquare = (SquareFlag)((ulong)toSquare >> Math.Abs((int)MoveDirection.West));
                        
                        var targetPieceColour = GetPieceColour(targetSquare);
                        var targetPieceType = GetPieceType(targetSquare);

                        if (targetPieceColour == colour.Opposite() && targetPieceType == PieceType.Pawn)
                        {
                            var enPassantSquare = fromSquare.PawnForward(colour, 1);

                            state = state.AddEnPassantSquare(enPassantSquare);
                        }
                        else
                        {
                            targetSquare = (SquareFlag)((ulong)toSquare << (int)MoveDirection.East);

                            targetPieceColour = GetPieceColour(targetSquare);
                            targetPieceType = GetPieceType(targetSquare);

                            if (targetPieceColour == colour.Opposite() && targetPieceType == PieceType.Pawn)
                            {
                                var enPassantSquare = fromSquare.PawnForward(colour, 1);

                                state = state.AddEnPassantSquare(enPassantSquare);
                            }
                        }
                    }

                    MovePiece(colour, pieceType, fromSquare, toSquare);
                    //UpdatePieceStateSquare(colour, pieceType, fromSquareIndex, toSquareIndex);

                    if (capturePieceType != PieceType.None)
                    {
                        RemovePiece(colour.Opposite(), toSquare);
                        //UpdatePieceStateType(colour.Opposite(), toSquareIndex, PieceType.None);
                    }
                }
            }
            else
            {
                MovePiece(colour, pieceType, fromSquare, toSquare);
                //UpdatePieceStateSquare(colour, pieceType, fromSquareIndex, toSquareIndex);

                if (capturePieceType != PieceType.None)
                {
                    RemovePiece(colour.Opposite(), toSquare);
                    //UpdatePieceStateType(colour.Opposite(), toSquareIndex, PieceType.None);

                    if (capturePieceType == PieceType.Rook)
                    {
                        if (colour == Colour.White)
                        {
                            if (BlackCanCastleKingSide && 
                                toSquare == SquareFlagConstants.BlackKingSideRookStartSquare)
                            {
                                state = RemoveCastleAvailability(Colour.Black, MoveType.CastleKing, state);
                            }
                            else if (BlackCanCastleQueenSide &&
                                toSquare == SquareFlagConstants.BlackQueenSideRookStartSquare)
                            {
                                state = RemoveCastleAvailability(Colour.Black, MoveType.CastleQueen, state);
                            }
                        }
                        else
                        {
                            if (WhiteCanCastleKingSide &&
                                toSquare == SquareFlagConstants.WhiteKingSideRookStartSquare)
                            {
                                state = RemoveCastleAvailability(Colour.White, MoveType.CastleKing, state);
                            }
                            else if (WhiteCanCastleQueenSide &&
                                toSquare == SquareFlagConstants.WhiteQueenSideRookStartSquare)
                            {
                                state = RemoveCastleAvailability(Colour.White, MoveType.CastleQueen, state);
                            }
                        }
                    }
                }

                // Castling rights management
                if (colour == Colour.White)
                {
                    if (state.HasFlag(BoardState.WhiteCanCastleKingSide))
                    {
                        if (pieceType == PieceType.Rook)
                        {
                            if (fromSquare == SquareFlagConstants.WhiteKingSideRookStartSquare)
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
                            if (fromSquare == SquareFlagConstants.WhiteQueenSideRookStartSquare)
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
                            if (fromSquare == SquareFlagConstants.BlackKingSideRookStartSquare)
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
                            if (fromSquare == SquareFlagConstants.BlackQueenSideRookStartSquare)
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
            _moves.Push(move);
        }

        public void UnMakeMove(uint move)
        {
            var colour = move.GetColour();
            var fromSquare = move.GetFrom();
            var fromSquareIndex = move.GetFromIndex();
            var toSquare = move.GetTo();
            var toSquareIndex = move.GetToIndex();
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
                    //RestorePieceStateType(colour.Opposite(), captureSquare.ToSquareIndex(), PieceType.Pawn);

                    MovePiece(colour, pieceType, toSquare, fromSquare);
                    //UpdatePieceStateSquare(colour, pieceType, toSquareIndex, fromSquareIndex);
                }
                else if (moveType == MoveType.PromotionQueen)
                {
                    DemotePiece(colour, PieceType.Queen, toSquare);
                    //UpdatePieceStateType(colour, toSquareIndex, PieceType.Pawn);

                    MovePiece(colour, pieceType, toSquare, fromSquare);
                    //UpdatePieceStateSquare(colour, pieceType, toSquareIndex, fromSquareIndex);

                    if (capturePieceType != PieceType.None)
                    {
                        MovePiece(colour.Opposite(), capturePieceType, toSquare, toSquare);
                        //RestorePieceStateType(colour.Opposite(), toSquareIndex, capturePieceType);
                    }
                }
                else if (moveType == MoveType.PromotionRook)
                {
                    DemotePiece(colour, PieceType.Rook, toSquare);
                    //UpdatePieceStateType(colour, toSquareIndex, PieceType.Pawn);

                    MovePiece(colour, pieceType, toSquare, fromSquare);
                    //UpdatePieceStateSquare(colour, pieceType, toSquareIndex, fromSquareIndex);

                    if (capturePieceType != PieceType.None)
                    {
                        MovePiece(colour.Opposite(), capturePieceType, toSquare, toSquare);
                        //RestorePieceStateType(colour.Opposite(), toSquareIndex, capturePieceType);
                    }
                }
                else if (moveType == MoveType.PromotionBishop)
                {
                    DemotePiece(colour, PieceType.Bishop, toSquare);
                    //UpdatePieceStateType(colour, toSquareIndex, PieceType.Pawn);

                    MovePiece(colour, pieceType, toSquare, fromSquare);
                    //UpdatePieceStateSquare(colour, pieceType, toSquareIndex, fromSquareIndex);

                    if (capturePieceType != PieceType.None)
                    {
                        MovePiece(colour.Opposite(), capturePieceType, toSquare, toSquare);
                        //RestorePieceStateType(colour.Opposite(), toSquareIndex, capturePieceType);
                    }
                }
                else if (moveType == MoveType.PromotionKnight)
                {
                    DemotePiece(colour, PieceType.Knight, toSquare);
                    //UpdatePieceStateType(colour, toSquareIndex, PieceType.Pawn);

                    MovePiece(colour, pieceType, toSquare, fromSquare);
                    //UpdatePieceStateSquare(colour, pieceType, toSquareIndex, fromSquareIndex);

                    if (capturePieceType != PieceType.None)
                    {
                        MovePiece(colour.Opposite(), capturePieceType, toSquare, toSquare);
                        //RestorePieceStateType(colour.Opposite(), toSquareIndex, capturePieceType);
                    }
                }
                else
                {
                    MovePiece(colour, pieceType, toSquare, fromSquare);
                    //UpdatePieceStateSquare(colour, pieceType, toSquareIndex, fromSquareIndex);

                    if (capturePieceType != PieceType.None)
                    {
                        MovePiece(colour.Opposite(), capturePieceType, toSquare, toSquare);
                        //RestorePieceStateType(colour.Opposite(), toSquareIndex, capturePieceType);
                    }
                }
            }
            else
            {
                MovePiece(colour, pieceType, toSquare, fromSquare);
                //UpdatePieceStateSquare(colour, pieceType, toSquareIndex, fromSquareIndex);

                if (capturePieceType != PieceType.None)
                {
                    MovePiece(colour.Opposite(), capturePieceType, toSquare, toSquare);
                    //RestorePieceStateType(colour.Opposite(), toSquareIndex, capturePieceType);
                }
            }

            _boardStates.Pop();
            _moves.Pop();
        }


        private void MakeWhiteKingSideCastle()
        {
            MovePiece(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingStartSquare,
                SquareFlagConstants.WhiteKingSideCastleStep2);

            MovePiece(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteKingSideRookStartSquare,
                SquareFlagConstants.WhiteKingSideCastleStep1);

            //UpdatePieceStateSquare(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingStartSquare.ToSquareIndex(),
            //    SquareFlagConstants.WhiteKingSideCastleStep2.ToSquareIndex());

            //UpdatePieceStateSquare(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteKingSideRookStartSquare.ToSquareIndex(),
            //    SquareFlagConstants.WhiteKingSideCastleStep1.ToSquareIndex());
        }

        private void MakeWhiteQueenSideCastle()
        {
            MovePiece(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingStartSquare,
                SquareFlagConstants.WhiteQueenSideCastleStep2);

            MovePiece(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteQueenSideRookStartSquare,
                SquareFlagConstants.WhiteQueenSideCastleStep1);

            //UpdatePieceStateSquare(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingStartSquare.ToSquareIndex(),
            //    SquareFlagConstants.WhiteQueenSideCastleStep2.ToSquareIndex());

            //UpdatePieceStateSquare(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteQueenSideRookStartSquare.ToSquareIndex(),
            //    SquareFlagConstants.WhiteQueenSideCastleStep1.ToSquareIndex());
        }

        private void MakeBlackKingSideCastle()
        {
            MovePiece(Colour.Black, PieceType.King, SquareFlagConstants.BlackKingStartSquare,
                SquareFlagConstants.BlackKingSideCastleStep2);

            MovePiece(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackKingSideRookStartSquare,
                SquareFlagConstants.BlackKingSideCastleStep1);

            //UpdatePieceStateSquare(Colour.Black, PieceType.King, SquareFlagConstants.BlackKingStartSquare.ToSquareIndex(),
            //    SquareFlagConstants.BlackKingSideCastleStep2.ToSquareIndex());

            //UpdatePieceStateSquare(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackKingSideRookStartSquare.ToSquareIndex(),
            //    SquareFlagConstants.BlackKingSideCastleStep1.ToSquareIndex());
        }

        private void MakeBlackQueenSideCastle()
        {
            MovePiece(Colour.Black, PieceType.King, SquareFlagConstants.BlackKingStartSquare,
                SquareFlagConstants.BlackQueenSideCastleStep2);

            MovePiece(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackQueenSideRookStartSquare,
                SquareFlagConstants.BlackQueenSideCastleStep1);

            //UpdatePieceStateSquare(Colour.Black, PieceType.King, SquareFlagConstants.BlackKingStartSquare.ToSquareIndex(),
            //    SquareFlagConstants.BlackQueenSideCastleStep2.ToSquareIndex());

            //UpdatePieceStateSquare(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackQueenSideRookStartSquare.ToSquareIndex(),
            //    SquareFlagConstants.BlackQueenSideCastleStep1.ToSquareIndex());
        }

        private void UnMakeWhiteKingSideCastle()
        {
            MovePiece(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingSideCastleStep2,
                SquareFlagConstants.WhiteKingStartSquare);

            MovePiece(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteKingSideCastleStep1,
                SquareFlagConstants.WhiteKingSideRookStartSquare);

            //UpdatePieceStateSquare(Colour.White, PieceType.King, SquareFlagConstants.WhiteKingSideCastleStep2.ToSquareIndex(),
            //    SquareFlagConstants.WhiteKingStartSquare.ToSquareIndex());

            //UpdatePieceStateSquare(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteKingSideCastleStep1.ToSquareIndex(),
            //    SquareFlagConstants.WhiteKingSideRookStartSquare.ToSquareIndex());
        }

        private void UnMakeWhiteQueenSideCastle()
        {
            MovePiece(Colour.White, PieceType.King, SquareFlagConstants.WhiteQueenSideCastleStep2,
                SquareFlagConstants.WhiteKingStartSquare);

            MovePiece(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteQueenSideCastleStep1,
                SquareFlagConstants.WhiteQueenSideRookStartSquare);

            //UpdatePieceStateSquare(Colour.White, PieceType.King, SquareFlagConstants.WhiteQueenSideCastleStep2.ToSquareIndex(),
            //    SquareFlagConstants.WhiteKingStartSquare.ToSquareIndex());

            //UpdatePieceStateSquare(Colour.White, PieceType.Rook, SquareFlagConstants.WhiteQueenSideCastleStep1.ToSquareIndex(),
            //    SquareFlagConstants.WhiteQueenSideRookStartSquare.ToSquareIndex());
        }

        private void UnMakeBlackKingSideCastle()
        {
            MovePiece(Colour.Black, PieceType.King, SquareFlagConstants.BlackKingSideCastleStep2,
                SquareFlagConstants.BlackKingStartSquare);

            MovePiece(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackKingSideCastleStep1,
                SquareFlagConstants.BlackKingSideRookStartSquare);

            //UpdatePieceStateSquare(Colour.Black, PieceType.King, SquareFlagConstants.BlackKingSideCastleStep2.ToSquareIndex(),
            //    SquareFlagConstants.BlackKingStartSquare.ToSquareIndex());

            //UpdatePieceStateSquare(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackKingSideCastleStep1.ToSquareIndex(),
            //    SquareFlagConstants.BlackKingSideRookStartSquare.ToSquareIndex());
        }

        private void UnMakeBlackQueenSideCastle()
        {
            MovePiece(Colour.Black, PieceType.King, SquareFlagConstants.BlackQueenSideCastleStep2,
                SquareFlagConstants.BlackKingStartSquare);

            MovePiece(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackQueenSideCastleStep1,
                SquareFlagConstants.BlackQueenSideRookStartSquare);

            //UpdatePieceStateSquare(Colour.Black, PieceType.King, SquareFlagConstants.BlackQueenSideCastleStep2.ToSquareIndex(),
            //    SquareFlagConstants.BlackKingStartSquare.ToSquareIndex());

            //UpdatePieceStateSquare(Colour.Black, PieceType.Rook, SquareFlagConstants.BlackQueenSideCastleStep1.ToSquareIndex(),
            //    SquareFlagConstants.BlackQueenSideRookStartSquare.ToSquareIndex());
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

        private void MovePiece(Colour colour, PieceType type, SquareFlag fromSquare, SquareFlag toSquare)
        {
            if (colour == Colour.White)
            {
                if (type == PieceType.Pawn)
                {
                    WhitePawns &= ~fromSquare;
                    WhitePawns |= toSquare;
                    return;
                }

                if (type == PieceType.Rook)
                {
                    WhiteRooks &= ~fromSquare;
                    WhiteRooks |= toSquare;
                    return;
                }

                if (type == PieceType.Knight)
                {
                    WhiteKnights &= ~fromSquare;
                    WhiteKnights |= toSquare;
                    return;
                }

                if (type == PieceType.Bishop)
                {
                    WhiteBishops &= ~fromSquare;
                    WhiteBishops |= toSquare;
                    return;
                }

                if (type == PieceType.Queen)
                {
                    WhiteQueens &= ~fromSquare;
                    WhiteQueens |= toSquare;
                    return;
                }

                if (type == PieceType.King)
                {
                    WhiteKing &= ~fromSquare;
                    WhiteKing |= toSquare;
                    return;
                }
            }
            else
            {
                if (type == PieceType.Pawn)
                {
                    BlackPawns &= ~fromSquare;
                    BlackPawns |= toSquare;
                    return;
                }

                if (type == PieceType.Rook)
                {
                    BlackRooks &= ~fromSquare;
                    BlackRooks |= toSquare;
                    return;
                }

                if (type == PieceType.Knight)
                {
                    BlackKnights &= ~fromSquare;
                    BlackKnights |= toSquare;
                    return;
                }

                if (type == PieceType.Bishop)
                {
                    BlackBishops &= ~fromSquare;
                    BlackBishops |= toSquare;
                    return;
                }

                if (type == PieceType.Queen)
                {
                    BlackQueens &= ~fromSquare;
                    BlackQueens |= toSquare;
                    return;
                }

                if (type == PieceType.King)
                {
                    BlackKing &= ~fromSquare;
                    BlackKing |= toSquare;
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
