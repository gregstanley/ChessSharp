namespace Chess
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

        private BoardState _state = BoardState.None;

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

        private void PromotePiece(Colour colour, PieceType type, SquareFlag square)
        {
            if (colour == Colour.White)
            {
                if (type == PieceType.Rook)
                {
                    WhitePawns &= ~square;
                    WhiteRooks |= square;
                }

                if (type == PieceType.Knight)
                {
                    WhitePawns &= ~square;
                    WhiteKnights |= square;
                }

                if (type == PieceType.Bishop)
                {
                    WhitePawns &= ~square;
                    WhiteBishops |= square;
                }

                if (type == PieceType.Queen)
                {
                    WhitePawns &= ~square;
                    WhiteQueens |= square;
                }
            }
            else
            {
                if (type == PieceType.Rook)
                {
                    BlackPawns &= ~square;
                    BlackRooks |= square;
                }

                if (type == PieceType.Knight)
                {
                    BlackPawns &= ~square;
                    BlackKnights |= square;
                }

                if (type == PieceType.Bishop)
                {
                    BlackPawns &= ~square;
                    BlackBishops |= square;
                }

                if (type == PieceType.Queen)
                {
                    BlackPawns &= ~square;
                    BlackQueens |= square;
                }
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
    }
}
