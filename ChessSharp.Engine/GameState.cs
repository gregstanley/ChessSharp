using ChessSharp.Enums;

namespace ChessSharp.Engine
{
    public class GameState
    {
        public int Ply { get; }

        public Colour ToPlay { get; }

        public int HalfTurnCounter { get; }

        public int FullTurnNumber { get; }

        public SquareFlag WhitePawns { get; }

        public SquareFlag WhiteRooks { get; }

        public SquareFlag WhiteKnights { get; }

        public SquareFlag WhiteBishops { get; }

        public SquareFlag WhiteQueens { get; }

        public SquareFlag WhiteKing { get; }

        public SquareFlag BlackPawns { get; }

        public SquareFlag BlackRooks { get; }

        public SquareFlag BlackKnights { get; }

        public SquareFlag BlackBishops { get; }

        public SquareFlag BlackQueens { get; }

        public SquareFlag BlackKing { get; }

        public bool WhiteCanCastleKingSide { get; }

        public bool WhiteCanCastleQueenSide { get; }

        public bool BlackCanCastleKingSide { get; }

        public bool BlackCanCastleQueenSide { get; }

        public double Evaluation { get; }

        public static GameState From(Game game)
        {
            var bitBoard = game.GetBitBoard();

            return new GameState(game.Ply, game.ToPlay, game.HalfTurnCounter, game.FullTurnNumber,
                game.GetBitBoard().WhiteCanCastleKingSide, game.GetBitBoard().WhiteCanCastleQueenSide,
                game.GetBitBoard().BlackCanCastleKingSide, game.GetBitBoard().BlackCanCastleQueenSide,
                bitBoard.WhitePawns, bitBoard.WhiteRooks, bitBoard.WhiteKnights, bitBoard.WhiteBishops, bitBoard.WhiteQueens,
                bitBoard.WhiteKing, bitBoard.BlackPawns, bitBoard.BlackRooks, bitBoard.BlackKnights, bitBoard.BlackBishops,
                bitBoard.BlackQueens, bitBoard.BlackKing, game.Evaluate());
        }

        private GameState(int ply, Colour toPlay, int halfTurnCounter, int fullTurnNumber,
            bool whiteCanCastleKingSide, bool whiteCanCastleQueenSide,
            bool blackCanCastleKingSide, bool blackCanCastleQueenSide,
            SquareFlag whitePawns, SquareFlag whiteRooks, SquareFlag whiteKnights, SquareFlag whiteBishops,
            SquareFlag whiteQueens, SquareFlag whiteKing, SquareFlag blackPawns, SquareFlag blackRooks,
            SquareFlag blackKnights, SquareFlag blackBishops, SquareFlag blackQueens, SquareFlag blackKing,
            double evaluation)
        {
            Ply = ply;
            ToPlay = toPlay;
            HalfTurnCounter = halfTurnCounter;
            FullTurnNumber = fullTurnNumber;
            WhiteCanCastleKingSide = whiteCanCastleKingSide;
            WhiteCanCastleQueenSide = whiteCanCastleQueenSide;
            BlackCanCastleKingSide = blackCanCastleKingSide;
            BlackCanCastleQueenSide = blackCanCastleQueenSide;
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
            Evaluation = evaluation;
        }
    }
}
