using ChessSharp.Enums;
using ChessSharp.Extensions;

namespace ChessSharp
{
    public class MoveViewer
    {
        public MoveViewer(uint value)
        {
            Value = value;
        }

        public uint Value { get; }

        public Colour Colour => Value.GetColour();

        public PieceType PieceType => Value.GetPieceType();

        public SquareFlag From => Value.GetFrom();

        public SquareFlag To => Value.GetTo();

        public PieceType CapturePieceType => Value.GetCapturePieceType();

        public MoveType MoveType => Value.GetMoveType();

        // http://wbec-ridderkerk.nl/html/UCIProtocol.html
        public string GetUciNotation()
        {
            if (Value == 0)
                return "0000";

            if (MoveType == MoveType.CastleKing)
                return Colour == Colour.White ? "e1h1" : "e8h8";

            if (MoveType == MoveType.CastleQueen)
                return Colour == Colour.White ? "e1a1" : "e8a8";

            var moveString = $"{From}{To}";

            if (CapturePieceType != PieceType.None)
                moveString = $"{moveString}x";

            if (MoveType == MoveType.PromotionQueen)
                moveString = $"{moveString}q";

            if (MoveType == MoveType.PromotionRook)
                moveString = $"{moveString}r";

            if (MoveType == MoveType.PromotionBishop)
                moveString = $"{moveString}b";

            if (MoveType == MoveType.PromotionKnight)
                moveString = $"{moveString}n";

            return moveString;
        }

        public string GetNotation()
        {
            if (Value == 0)
                return "-";

            if (MoveType == MoveType.CastleKing)
                return $"0-0";

            if (MoveType == MoveType.CastleQueen)
                return $"0-0-0";

            var moveString = $"{From}{To}";

            if (CapturePieceType != PieceType.None)
                moveString = $"{moveString}x";

            if (MoveType == MoveType.PromotionQueen)
                moveString = $"{moveString}=Q";

            if (MoveType == MoveType.PromotionRook)
                moveString = $"{moveString}=R";

            if (MoveType == MoveType.PromotionBishop)
                moveString = $"{moveString}=B";

            if (MoveType == MoveType.PromotionKnight)
                moveString = $"{moveString}=K";

            return moveString;
        }
    }
}
