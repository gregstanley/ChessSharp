using System.Collections.Generic;
using System.Text;

namespace Chess.Bit
{
    public class BitBoard
    {
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
            SquareFlag blackKing)
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

        public SquareFlag WhiteCovered { get; private set; }

        public SquareFlag BlackCovered { get; private set; }

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

        public SquareFlag FindCoveredSquares(Colour colour) =>
            colour == Colour.White ? WhiteCovered : BlackCovered;

        public bool IsCheck(Colour colour) =>
            colour == Colour.White ? BlackCovered.HasFlag(WhiteKing) : WhiteCovered.HasFlag(BlackKing);

        public Colour GetPieceColour(SquareFlag square)
        {
            if (White.HasFlag(square))
                return Colour.White;

            if (Black.HasFlag(square))
                return Colour.Black;

            return Colour.None;
        }

        public PieceType GetPiece(SquareFlag square)
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

        public BitBoard Move(Move move, BitBoardMoveFinder moveFinder)
        {
            var childBoard = Clone();

            var startSquareFlag = move.StartPosition.ToSquareFlag();
            var endSquareFlag = move.EndPosition.ToSquareFlag();

            var isCapture = move.PieceColour == Colour.White
                ? Black.HasFlag(endSquareFlag)
                : White.HasFlag(endSquareFlag);

            childBoard.MovePiece(move.PieceColour, move.Type, startSquareFlag, endSquareFlag);

            if (isCapture)
            {
                childBoard.RemovePiece(move.PieceColour.Opposite(), endSquareFlag);
                _state |= BoardState.IsCapture;
            }

            if (move.Type == PieceType.Pawn)
            {
                if (move.PromotionType != PieceType.None)
                {
                    _state |= BoardState.IsPawnPromotion;

                    childBoard.PromotePiece(move.PieceColour, move.PromotionType, endSquareFlag);
                }
            }

            if (move is MoveCastle castle)
            {
                var kingStartSquareFlag = castle.KingStartPosition.ToSquareFlag();
                var kingEndSquareFlag = castle.KingEndPosition.ToSquareFlag();

                childBoard.MovePiece(move.PieceColour, PieceType.King, kingStartSquareFlag, kingEndSquareFlag);
            }

            var myCoveredSquares = moveFinder.GetCoveredSquares(childBoard, move.PieceColour);
            var opponentCoveredSquares = moveFinder.GetCoveredSquares(childBoard, move.PieceColour.Opposite());

            childBoard.SetCoveredSquares(move.PieceColour, myCoveredSquares);
            childBoard.SetCoveredSquares(move.PieceColour.Opposite(), opponentCoveredSquares);

            return childBoard;
        }

        public BitBoard Clone() =>
            new BitBoard(WhitePawns, WhiteRooks, WhiteKnights, WhiteBishops, WhiteQueens, WhiteKing,
                BlackPawns, BlackRooks, BlackKnights, BlackBishops, BlackQueens, BlackKing);

        public override string ToString()
        {
            var sb = new StringBuilder();

            var ranks = new List<string>();

            ulong bit = 1;

            for (var i = 0; i < 64; ++i)
            {
                if (WhitePawns.HasFlag((SquareFlag)bit)) sb.Append("P");
                else if (WhiteRooks.HasFlag((SquareFlag)bit)) sb.Append("R");
                else if (WhiteKnights.HasFlag((SquareFlag)bit)) sb.Append("N");
                else if (WhiteBishops.HasFlag((SquareFlag)bit)) sb.Append("B");
                else if (WhiteQueens.HasFlag((SquareFlag)bit)) sb.Append("Q");
                else if (WhiteKing.HasFlag((SquareFlag)bit)) sb.Append("K");
                else if (BlackPawns.HasFlag((SquareFlag)bit)) sb.Append("p");
                else if (BlackRooks.HasFlag((SquareFlag)bit)) sb.Append("r");
                else if (BlackKnights.HasFlag((SquareFlag)bit)) sb.Append("n");
                else if (BlackBishops.HasFlag((SquareFlag)bit)) sb.Append("b");
                else if (BlackQueens.HasFlag((SquareFlag)bit)) sb.Append("q");
                else if (BlackKing.HasFlag((SquareFlag)bit)) sb.Append("k");
                else sb.Append("-");

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

        private void SetCoveredSquares(Colour colour, SquareFlag squares)
        {
            if (colour == Colour.White)
            {
                WhiteCovered = squares;
                return;
            }

            BlackCovered = squares;
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
