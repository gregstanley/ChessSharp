using ChessSharp.Enums;

namespace ChessSharp.Models
{
    public interface IPieceMap
    {
        SquareFlag WhitePawns { get; }

        SquareFlag WhiteRooks { get; }

        SquareFlag WhiteKnights { get; }

        SquareFlag WhiteBishops { get; }

        SquareFlag WhiteQueens { get; }

        SquareFlag WhiteKing { get; }

        SquareFlag BlackPawns { get; }

        SquareFlag BlackRooks { get; }

        SquareFlag BlackKnights { get; }

        SquareFlag BlackBishops { get; }

        SquareFlag BlackQueens { get; }

        SquareFlag BlackKing { get; }

        SquareFlag White { get; }

        SquareFlag Black { get; }
    }
}
