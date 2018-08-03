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

        public SquareFlag WhiteThreatened { get; private set; }

        public SquareFlag BlackThreatened { get; private set; }

        private BoardState _state = BoardState.None;

        public SquareFlag FindSquares(Colour colour, PieceType type)
        {
            if (colour == Colour.White)
            {
                if (type == PieceType.King) return WhiteKing;
            }
            else
            {
                if (type == PieceType.King) return WhiteKing;
            }

            return 0;
        }

        public SquareFlag FindThreatendSquares(Colour colour) =>
            colour == Colour.White ? WhiteThreatened : BlackThreatened;

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

        public void Move(Move move)
        {
            var startSquareFlag = move.StartPosition.GetSquareFlag();
            var endSquareFlag = move.EndPosition.GetSquareFlag();

            MovePiece(move.PieceColour, move.Type, startSquareFlag, endSquareFlag);

            var isCapture = move.PieceColour == Colour.White
                ? Black.HasFlag(endSquareFlag)
                : White.HasFlag(endSquareFlag);

            if (isCapture)
            {
                RemovePiece(move.PieceColour.Opposite(), endSquareFlag);
                _state |= BoardState.IsCapture;
            }

            if (move.Type == PieceType.Pawn)
            {
                if (move.PromotionType != PieceType.None)
                {
                    _state |= BoardState.IsPawnPromotion;

                    PromotePiece(move.PieceColour, move.PromotionType, endSquareFlag);
                }
            }

            if (move is MoveCastle castle)
            {
                var kingStartSquareFlag = castle.KingStartPosition.GetSquareFlag();
                var kingEndSquareFlag = castle.KingEndPosition.GetSquareFlag();

                MovePiece(move.PieceColour, PieceType.King, kingStartSquareFlag, kingEndSquareFlag);
            }
        }

        public void SetThreatenedSquares(Colour colour, SquareFlag threatendSquares)
        {
            if (colour == Colour.White)
                WhiteThreatened = threatendSquares;
            else
                BlackThreatened = threatendSquares;
            //var squaresWithPieces = GetSquaresWithPieceOn();

            //foreach (var square in squaresWithPieces)
            //{
            //    var coveredSquares = _moveFinder.GetCoveredSquares(_squares, square);

            //    foreach (var coveredSquare in coveredSquares)
            //        coveredSquare.AddCoveredBy(square.Piece);
            //}
        }

        public BitBoard Clone() =>
            new BitBoard(WhitePawns, WhiteRooks, WhiteKnights, WhiteBishops, WhiteQueens, WhiteKing,
                BlackPawns, BlackRooks, BlackKnights, BlackBishops, BlackQueens, BlackKing);

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
                    var n = WhitePawns & start;
                    if (n == 0)
                    { var bp = true; }
                    WhitePawns &= ~start;
                    WhitePawns |= end;
                }

                if (type == PieceType.Rook)
                {

                    var n = WhiteRooks & start;
                    if (n == 0)
                    { var bp = true; }
                    WhiteRooks &= ~start;
                    WhiteRooks |= end;
                }

                if (type == PieceType.Knight)
                {
                    var n = WhiteKnights & start;
                    if (n == 0)
                    { var bp = true; }
                    WhiteKnights &= ~start;
                    WhiteKnights |= end;
                }

                if (type == PieceType.Bishop)
                {
                    var n = WhiteBishops & start;
                    if (n == 0)
                    { var bp = true; }
                    WhiteBishops &= ~start;
                    WhiteBishops |= end;
                }

                if (type == PieceType.Queen)
                {
                    var n = WhiteQueens & start;
                    if (n == 0)
                    { var bp = true; }
                    WhiteQueens &= ~start;
                    WhiteQueens |= end;
                }

                if (type == PieceType.King)
                {
                    var n = WhiteKing & start;
                    if (n == 0)
                    { var bp = true; }
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
