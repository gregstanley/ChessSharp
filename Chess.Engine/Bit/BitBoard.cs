using Chess.Engine.Extensions;
using Chess.Engine.Models;
using System.Collections.Generic;
using System.Text;

namespace Chess.Engine.Bit
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
                        case PieceType.Pawn:   whitePawns |= square.Square; break;
                        case PieceType.Rook:   whiteRooks |= square.Square; break;
                        case PieceType.Knight: whiteKnights |= square.Square; break;
                        case PieceType.Bishop: whiteBishops |= square.Square; break;
                        case PieceType.Queen:  whiteQueens |= square.Square; break;
                        case PieceType.King:   whiteKing |= square.Square; break;
                    }
                }
                else
                {
                    switch (square.Type)
                    {
                        case PieceType.Pawn:   blackPawns |= square.Square; break;
                        case PieceType.Rook:   blackRooks |= square.Square; break;
                        case PieceType.Knight: blackKnights |= square.Square; break;
                        case PieceType.Bishop: blackBishops |= square.Square; break;
                        case PieceType.Queen:  blackQueens |= square.Square; break;
                        case PieceType.King:   blackKing |= square.Square; break;
                    }
                }
            }

            return new BitBoard(whitePawns, whiteRooks, whiteKnights, whiteBishops, whiteQueens, whiteKing,
                blackPawns, blackRooks, blackKnights, blackBishops, blackQueens, blackKing, fen.CastlingRights);
        }

        public BitBoard()
        {
            WhitePawns = SquareFlag.A2 | SquareFlag.B2 | SquareFlag.C2 | SquareFlag.D2 | SquareFlag.E2 | SquareFlag.F2 | SquareFlag.G2 | SquareFlag.H2;
            WhiteRooks = SquareFlag.A1 | SquareFlag.H1;
            WhiteKnights = SquareFlag.B1 | SquareFlag.G1;
            WhiteBishops = SquareFlag.C1 | SquareFlag.F1;
            WhiteQueens = SquareFlag.D1;
            WhiteKing = SquareFlag.E1;
            BlackPawns = SquareFlag.A7 | SquareFlag.B7 | SquareFlag.C7 | SquareFlag.D7 | SquareFlag.E7 | SquareFlag.F7 | SquareFlag.G7 | SquareFlag.H7;
            BlackRooks = SquareFlag.A8 | SquareFlag.H8;
            BlackKnights = SquareFlag.B8 | SquareFlag.G8;
            BlackBishops = SquareFlag.C8 | SquareFlag.F8;
            BlackQueens = SquareFlag.D8;
            BlackKing = SquareFlag.E8;
            _state = DefaultState;
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
            _state = state;
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

        public SquareFlag White =>
            WhitePawns | WhiteRooks | WhiteKnights | WhiteBishops | WhiteQueens | WhiteKing;

        public SquareFlag Black =>
            BlackPawns | BlackRooks | BlackKnights | BlackBishops | BlackQueens | BlackKing;

        private BoardState _state = BoardState.None;

        public SquareFlag FindPieceSquares(Colour colour) =>
            colour == Colour.White ? White : Black;

        public SquareFlag FindPawnSquares(Colour colour) =>
            colour == Colour.White ? WhitePawns : BlackPawns;

        public SquareFlag FindRookSquares(Colour colour) =>
            colour == Colour.White ? WhiteRooks : BlackRooks;

        public SquareFlag FindKnightSquares(Colour colour) =>
            colour == Colour.White ? WhiteKnights : BlackKnights;

        public SquareFlag FindBishopSquares(Colour colour) =>
            colour == Colour.White ? WhiteBishops : BlackBishops;

        public SquareFlag FindQueenSquares(Colour colour) =>
            colour == Colour.White ? WhiteQueens : BlackQueens;

        public SquareFlag FindKingSquare(Colour colour) =>
            colour == Colour.White ? WhiteKing : BlackKing;

        public bool WhiteCanCastleKingSide() =>
            _state.HasFlag(BoardState.WhiteCanCastleKingSide);

        public bool WhiteCanCastleQueenSide() =>
            _state.HasFlag(BoardState.WhiteCanCastleQueenSide);

        public bool BlackCanCastleKingSide() =>
            _state.HasFlag(BoardState.BlackCanCastleKingSide);

        public bool BlackCanCastleQueenSide() =>
            _state.HasFlag(BoardState.BlackCanCastleQueenSide);

        public bool IsCapture() =>
            _state.HasFlag(BoardState.IsCapture);

        public bool IsPawnPromotion(Colour colour) =>
            _state.HasFlag(BoardState.IsPawnPromotion);

        public bool CanCastle(Colour colour) =>
            colour == Colour.White
                ? WhiteCanCastleKingSide() || WhiteCanCastleQueenSide()
                : BlackCanCastleKingSide() || BlackCanCastleQueenSide();

        public bool CanCastleKingSide(Colour colour) =>
            colour == Colour.White ? WhiteCanCastleKingSide() : BlackCanCastleKingSide();

        public bool CanCastleQueenSide(Colour colour) =>
            colour == Colour.White ? WhiteCanCastleQueenSide() : BlackCanCastleQueenSide();

        public SquareState GetSquareState(SquareFlag square) =>
            new SquareState(square, GetPiece(square));

        public Piece GetPiece(SquareFlag square)
        {
            var colour = GetPieceColour(square);
            var pieceType = GetPieceType(square);

            return new Piece(colour, pieceType);
        }

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

            throw new System.Exception($"Failed to find piece for {square}");
        }

        public byte GetInstanceNumber(Colour colour, PieceType type, SquareFlag square)
        {
            if (colour == Colour.White)
            {
                if (type == PieceType.Pawn) return WhitePawns.GetInstanceNumber(square);
                if (type == PieceType.Rook) return WhiteRooks.GetInstanceNumber(square);
                if (type == PieceType.Knight) return WhiteKnights.GetInstanceNumber(square);
                if (type == PieceType.Bishop) return WhiteBishops.GetInstanceNumber(square);
                if (type == PieceType.Queen) return WhiteQueens.GetInstanceNumber(square);
                if (type == PieceType.King) return WhiteKing.GetInstanceNumber(square);
            }
            else
            {
                if (type == PieceType.Pawn) return BlackPawns.GetInstanceNumber(square);
                if (type == PieceType.Rook) return BlackRooks.GetInstanceNumber(square);
                if (type == PieceType.Knight) return BlackKnights.GetInstanceNumber(square);
                if (type == PieceType.Bishop) return BlackBishops.GetInstanceNumber(square);
                if (type == PieceType.Queen) return BlackQueens.GetInstanceNumber(square);
                if (type == PieceType.King) return BlackKing.GetInstanceNumber(square);
            }

            return 0;
        }

        public BitBoard ApplyMove(Move move)
        {
            var childBoard = Clone();

            var colour = move.PieceColour;
            var startSquareFlag = move.StartPositionSquareFlag;
            var endSquareFlag = move.EndPositionSquareFlag;

            childBoard.MovePiece(move.PieceColour, move.Type, startSquareFlag, endSquareFlag);

            if (move.CapturePieceType != PieceType.None)
            {
                childBoard.RemovePiece(colour.Opposite(), endSquareFlag);
                childBoard._state |= BoardState.IsCapture;
            }

            if (move.Type == PieceType.Pawn)
            {
                if (move.EnPassantCaptureSquare != 0)
                {
                    childBoard.RemovePiece(colour.Opposite(), move.EnPassantCaptureSquare);
                    childBoard._state |= BoardState.IsCapture;
                }

                if (move.PromotionType != PieceType.None)
                {
                    childBoard._state |= BoardState.IsPawnPromotion;

                    childBoard.PromotePiece(colour, move.PromotionType, endSquareFlag);
                }
            }

            if (move is MoveCastle castle)
            {
                var kingStartSquareFlag = castle.KingStartPosition.ToSquareFlag();
                var kingEndSquareFlag = castle.KingEndPosition.ToSquareFlag();

                childBoard.MovePiece(colour, PieceType.King, kingStartSquareFlag, kingEndSquareFlag);

                childBoard.RemoveCastleAvailability(move.PieceColour);
            }
            else if (CanCastle(move.PieceColour))
            {
                if (move.Type == PieceType.King)
                    childBoard.RemoveCastleAvailability(colour);

                if (move.Type == PieceType.Rook)
                {
                    var kingSquare = childBoard.FindKingSquare(colour).ToRankFile();

                    var relativePostion = kingSquare.To(move.StartPosition);

                    var side = relativePostion.File == 3 ? PieceType.King : PieceType.Queen;

                    childBoard.RemoveCastleAvailability(colour, side);
                }
            }

            return childBoard;
        }

        private void RemoveCastleAvailability(Colour colour, PieceType side)
        {
            if (colour == Colour.White)
            {
                if (side == PieceType.King) _state &= ~BoardState.WhiteCanCastleKingSide;
                if (side == PieceType.Queen) _state &= ~BoardState.WhiteCanCastleQueenSide;
            }
            else
            {
                if (side == PieceType.King) _state &= ~BoardState.BlackCanCastleKingSide;
                if (side == PieceType.Queen) _state &= ~BoardState.BlackCanCastleQueenSide;
            }
        }

        private void RemoveCastleAvailability(Colour colour)
        {
            RemoveCastleAvailability(colour, PieceType.King);
            RemoveCastleAvailability(colour, PieceType.Queen);
        }

        public BitBoard Clone()
        {
            var state = _state;

            // Remove transient flags
            state = state &= ~BoardState.IsCapture;
            state = state &= ~BoardState.IsPawnPromotion;

            var bitBoard = new BitBoard(WhitePawns, WhiteRooks, WhiteKnights, WhiteBishops, WhiteQueens, WhiteKing,
                BlackPawns, BlackRooks, BlackKnights, BlackBishops, BlackQueens, BlackKing, state);

            return bitBoard;
        }

        public string GetSquareNotation(SquareFlag square)
        {
            var colour = Colour.None;

            if (White.HasFlag(square))
                colour = Colour.White;

            if (Black.HasFlag(square))
                colour = Colour.Black;

            if (colour == Colour.None)
                return string.Empty;

            if (colour == Colour.White)
            {
                if (WhitePawns.HasFlag((SquareFlag)square)) return "P";
                else if (WhiteRooks.HasFlag((SquareFlag)square)) return "R";
                else if (WhiteKnights.HasFlag((SquareFlag)square)) return "N";
                else if (WhiteBishops.HasFlag((SquareFlag)square)) return "B";
                else if (WhiteQueens.HasFlag((SquareFlag)square)) return "Q";
                else if (WhiteKing.HasFlag((SquareFlag)square)) return "K";
            }

            if (BlackPawns.HasFlag((SquareFlag)square)) return "p";
            else if (BlackRooks.HasFlag((SquareFlag)square)) return "r";
            else if (BlackKnights.HasFlag((SquareFlag)square)) return "n";
            else if (BlackBishops.HasFlag((SquareFlag)square)) return "b";
            else if (BlackQueens.HasFlag((SquareFlag)square)) return "q";
            else if (BlackKing.HasFlag((SquareFlag)square)) return "k";

            return string.Empty;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            var ranks = new List<string>();

            ulong bit = 1;

            for (var i = 0; i < 64; ++i)
            {
                var squareNotation = GetSquareNotation((SquareFlag)bit);

                if (string.IsNullOrEmpty(squareNotation))
                    squareNotation = "-";

                sb.Append(squareNotation);

                if (i > 0 && (i + 1) % 8 == 0)
                {
                    ranks.Add(sb.ToString());

                    sb.Clear();
                }

                bit = bit << 1;
            }

            sb.Clear();

            ranks.Reverse();

            foreach (var rank in ranks)
                sb.AppendLine(rank);

            return sb.ToString();
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
                }

                if (type == PieceType.Rook)
                {
                    WhiteRooks &= ~start;
                    WhiteRooks |= end;
                }

                if (type == PieceType.Knight)
                {
                    WhiteKnights &= ~start;
                    WhiteKnights |= end;
                }

                if (type == PieceType.Bishop)
                {
                    WhiteBishops &= ~start;
                    WhiteBishops |= end;
                }

                if (type == PieceType.Queen)
                {
                    WhiteQueens &= ~start;
                    WhiteQueens |= end;
                }

                if (type == PieceType.King)
                {
                    WhiteKing &= ~start;
                    WhiteKing |= end;
                }
            }
            else
            {
                if (type == PieceType.Pawn)
                {
                    BlackPawns &= ~start;
                    BlackPawns |= end;
                }

                if (type == PieceType.Rook)
                {
                    BlackRooks &= ~start;
                    BlackRooks |= end;
                }

                if (type == PieceType.Knight)
                {
                    BlackKnights &= ~start;
                    BlackKnights |= end;
                }

                if (type == PieceType.Bishop)
                {
                    BlackBishops &= ~start;
                    BlackBishops |= end;
                }

                if (type == PieceType.Queen)
                {
                    BlackQueens &= ~start;
                    BlackQueens |= end;
                }

                if (type == PieceType.King)
                {
                    BlackKing &= ~start;
                    BlackKing |= end;
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
                }

                if (promoteTo == PieceType.Knight)
                {
                    WhitePawns &= ~square;
                    WhiteKnights |= square;
                }

                if (promoteTo == PieceType.Bishop)
                {
                    WhitePawns &= ~square;
                    WhiteBishops |= square;
                }

                if (promoteTo == PieceType.Queen)
                {
                    WhitePawns &= ~square;
                    WhiteQueens |= square;
                }
            }
            else
            {
                if (promoteTo == PieceType.Rook)
                {
                    BlackPawns &= ~square;
                    BlackRooks |= square;
                }

                if (promoteTo == PieceType.Knight)
                {
                    BlackPawns &= ~square;
                    BlackKnights |= square;
                }

                if (promoteTo == PieceType.Bishop)
                {
                    BlackPawns &= ~square;
                    BlackBishops |= square;
                }

                if (promoteTo == PieceType.Queen)
                {
                    BlackPawns &= ~square;
                    BlackQueens |= square;
                }
            }
        }
    }
}
