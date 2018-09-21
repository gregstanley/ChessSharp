using ChessSharp.Enums;
using ChessSharp.Extensions;

namespace ChessSharp.Models
{
    public class Move
    {
        /// STOCKFISH structure
        /// A move needs 16 bits to be stored
        ///
        /// bit  0- 5: destination square (from 0 to 63)
        /// bit  6-11: origin square (from 0 to 63)
        /// bit 12-13: promotion piece type - 2 (from KNIGHT-2 to QUEEN-2)
        /// bit 14-15: special move flag: promotion (1), en passant (2), castling (3)
        /// NOTE: EN-PASSANT bit is set only when a pawn can be captured
        /// 

        private uint _value = 0;

        private uint _colourMask = 0b00000000_00000000_00000000_00000011;
        private uint _pieceTypeMask = 0b00000000_00000000_00000000_00111100;
        private uint _fromMask = 0b00000000_00000000_00001111_11000000;
        private uint _toMask = 0b00000000_00000011_11110000_00000000;
        private uint _capturePieceTypeMask = 0b00000000_00111100_00000000_00000000;
        private uint _promotionPieceTypeMask = 0b00000011_11000000_00000000_00000000;
        private uint _castleTypeMask = 0b00001100_00000000_00000000_00000000;
        private uint _enPassantMask = 0b00010000_00000000_00000000_00000000;

        public Move(Colour colour, PieceType pieceType, SquareFlag from, SquareFlag to,
            PieceType capturePieceType, PieceType promotionPieceType, CastleType castleType, bool enPassant)
        {
            var pieceTypeShift = (uint)pieceType << 2;
            var fromShift = from != 0 ? (uint)from.ToBoardIndex() << 6 : 0;
            var toShift = to != 0 ? (uint)to.ToBoardIndex() << 12 : 0;
            var capturePieceTypeShift = (uint)capturePieceType << 18;
            var promotionPieceTypeShift = (uint)promotionPieceType << 22;
            var castleTypeShift = (uint)castleType << 26;
            var enPassantShift = enPassant ? (uint)1 << 28 : 0;

            _value = (uint)colour | pieceTypeShift | fromShift | toShift 
                | capturePieceTypeShift | promotionPieceTypeShift | castleTypeShift | enPassantShift;
        }

        public Colour Colour => (Colour)(_value & _colourMask);

        public PieceType PieceType => (PieceType)((_value & _pieceTypeMask) >> 2);

        public SquareFlag From => ((_value & _fromMask) >> 6).ToSquareFlag();

        public SquareFlag To => ((_value & _toMask) >> 12).ToSquareFlag();

        public PieceType CapturePieceType => (PieceType)((_value & _capturePieceTypeMask) >> 18);

        public PieceType PromotionPieceType => (PieceType)((_value & _promotionPieceTypeMask) >> 22);

        public CastleType CastleType => (CastleType)((_value & _castleTypeMask) >> 26);

        public bool EnPassant => ((_value & _enPassantMask) >> 28) == 1;

        public override bool Equals(object obj)
        {
            var b = (Move)obj;
            return _value == b._value;
        }
    }
}
