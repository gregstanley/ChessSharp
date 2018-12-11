using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessSharp
{
    public class Fen
    {
        public const string Default = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public const string Position2 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -";
        public const string Position3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -";
        public const string Position4 = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
        public const string Position5 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";
        public const string Position6 = "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10";

        public Fen(string board, Colour toPlay, BoardState castlingRights, SquareFlag enPassantSquare, int halfTurnCounter, int fullMoveNumber)
        {
            Board = board;
            ToPlay = toPlay;
            BoardState = castlingRights;

            if (enPassantSquare != 0)
                BoardState = BoardState.AddEnPassantSquare(enPassantSquare);

            HalfTurnCounter = halfTurnCounter;
            FullMoveNumber = fullMoveNumber;

            var expandedRanks = board
                .Replace("8", "--------")
                .Replace("7", "-------")
                .Replace("6", "------")
                .Replace("5", "-----")
                .Replace("4", "----")
                .Replace("3", "---")
                .Replace("2", "--")
                .Replace("1", "-");

            var ranksList = board.Split('/').ToList();

            ranksList.Reverse();

            _ranks = ranksList.ToArray();

            var expandedRanksList = expandedRanks.Split('/').ToList();

            expandedRanksList.Reverse();

            _expandedRanks = expandedRanksList.ToArray();
        }

        public string Board { get; }

        public Colour ToPlay { get; }

        public BoardState BoardState { get; }

        public SquareFlag EnPassantSquare => BoardState.GetEnPassantSquare();

        public int HalfTurnCounter { get; }

        public int FullMoveNumber { get; }

        public string Rank1 =>
            _ranks[0];

        public string Rank2 =>
            _ranks[1];

        public string Rank3 =>
            _ranks[2];

        public string Rank4 =>
            _ranks[3];

        public string Rank5 =>
            _ranks[4];

        public string Rank6 =>
            _ranks[5];

        public string Rank7 =>
            _ranks[6];

        public string Rank8 =>
            _ranks[7];

        private string[] _ranks { get; set; }

        private string[] _expandedRanks { get; set; }

        public static Fen Parse(string fen)
        {
            var parts = fen.Split(' ');

            var board = parts[0];
            var colour = parts[1] == "w" ? Colour.White : Colour.Black;

            BoardState castlingRights = ParseCastlingRights(parts[2]);

            SquareFlag enPassantSquare = 0;

            if (parts[3] != "-")
                Enum.TryParse(parts[3].ToUpper(), out enPassantSquare);

            var halfTurnCounter = 0;
            var fullMoveNumber = 0;

            if (parts.Length > 4)
                halfTurnCounter = int.Parse(parts[4]);

            if (parts.Length > 5)
                fullMoveNumber = int.Parse(parts[5]);

            return new Fen(board, colour, castlingRights, enPassantSquare, halfTurnCounter, fullMoveNumber);
        }

        public static string From(GameState state, Colour colour, int half, int full)
        {
            var sb = new StringBuilder();

            if (state.WhiteCanCastleKingSide)
                sb.Append($"K");

            if (state.WhiteCanCastleQueenSide)
                sb.Append($"Q");

            if (state.BlackCanCastleKingSide)
                sb.Append($"k");

            if (state.BlackCanCastleQueenSide)
                sb.Append($"q");

            var castleString = sb.ToString();

            sb.Clear();

            for (var rank = 7; rank >= 0; --rank)
            {
                var spaceCount = 0;

                for (var file = 0; file < 8; ++file)
                {
                    var index = (rank * 8) + file;

                    var piece = BoardHelpers.GetPiece(state, index.ToSquareFlag());

                    if (piece.Type == PieceType.None)
                    {
                        ++spaceCount;
                    }
                    else
                    {
                        if (spaceCount > 0)
                            sb.Append(spaceCount);

                        spaceCount = 0;

                        GetPieceString(sb, piece);
                    } 
                }

                if (spaceCount > 0)
                    sb.Append(spaceCount);

                if (rank > 0)
                    sb.Append("/");
            }

            var colourToken = colour == Colour.White ? "w" : "b";

            sb.Append($" {colourToken}");

            sb.Append($" {castleString}");

            if (state.EnPassant == 0)
                sb.Append(" -");
            else
                sb.Append($" {state.EnPassant.ToString().ToLower()}");

            sb.Append($" {half}");

            sb.Append($" {full}");

            return sb.ToString();           
        }

        public char[] GetExpandedBoardCharArray()
        {
            var fullExpandString = string.Join(string.Empty, _expandedRanks);

            return fullExpandString.ToCharArray();
        }

        public IReadOnlyCollection<SquareState> GetSquaresStates()
        {
            var squares = new List<SquareState>();

            var fenChars = GetExpandedBoardCharArray();

            if (fenChars.Length != 64)
                throw new Exception($"The expanded fen string has {fenChars.Length} chars in it but it should have 64");

            SquareFlag allBitsOn = (SquareFlag)ulong.MaxValue;

            var squareArray = allBitsOn.ToList().ToArray();

            if (squareArray.Length != 64)
                throw new Exception($"The board array string has {squareArray.Length} chars in it but it should have 64");

            for (var i = 0; i < 64; ++i)
            {
                var square = squareArray[i];
                var fenChar = fenChars[i];

                switch (fenChar)
                {
                    case 'R':
                        squares.Add(new SquareState(square, new Piece(Colour.White, PieceType.Rook)));
                        break;
                    case 'N':
                        squares.Add(new SquareState(square, new Piece(Colour.White, PieceType.Knight)));
                        break;
                    case 'B':
                        squares.Add(new SquareState(square, new Piece(Colour.White, PieceType.Bishop)));
                        break;
                    case 'Q':
                        squares.Add(new SquareState(square, new Piece(Colour.White, PieceType.Queen)));
                        break;
                    case 'K':
                        squares.Add(new SquareState(square, new Piece(Colour.White, PieceType.King)));
                        break;
                    case 'P':
                        squares.Add(new SquareState(square, new Piece(Colour.White, PieceType.Pawn)));
                        break;
                    case 'r':
                        squares.Add(new SquareState(square, new Piece(Colour.Black, PieceType.Rook)));
                        break;
                    case 'n':
                        squares.Add(new SquareState(square, new Piece(Colour.Black, PieceType.Knight)));
                        break;
                    case 'b':
                        squares.Add(new SquareState(square, new Piece(Colour.Black, PieceType.Bishop)));
                        break;
                    case 'q':
                        squares.Add(new SquareState(square, new Piece(Colour.Black, PieceType.Queen)));
                        break;
                    case 'k':
                        squares.Add(new SquareState(square, new Piece(Colour.Black, PieceType.King)));
                        break;
                    case 'p':
                        squares.Add(new SquareState(square, new Piece(Colour.Black, PieceType.Pawn)));
                        break;
                    default:
                        squares.Add(new SquareState(square));
                        break;
                }
            }

            return squares;
        }

        public string GetRank(int rank)
        {
            switch (rank)
            {
                case 1: return Rank1;
                case 2: return Rank2;
                case 3: return Rank3;
                case 4: return Rank4;
                case 5: return Rank5;
                case 6: return Rank6;
                case 7: return Rank7;
                case 8: return Rank8;
            }

            return string.Empty;
        }

        private static void GetPieceString(StringBuilder sb, Piece piece)
        {
            switch (piece.Type)
            {
                case PieceType.Pawn when piece.Colour == Colour.White:
                    sb.Append("P");
                    break;
                case PieceType.Rook when piece.Colour == Colour.White:
                    sb.Append("R");
                    break;
                case PieceType.Knight when piece.Colour == Colour.White:
                    sb.Append("N");
                    break;
                case PieceType.Bishop when piece.Colour == Colour.White:
                    sb.Append("B");
                    break;
                case PieceType.Queen when piece.Colour == Colour.White:
                    sb.Append("Q");
                    break;
                case PieceType.King when piece.Colour == Colour.White:
                    sb.Append("K");
                    break;
                case PieceType.Pawn when piece.Colour == Colour.Black:
                    sb.Append("p");
                    break;
                case PieceType.Rook when piece.Colour == Colour.Black:
                    sb.Append("r");
                    break;
                case PieceType.Knight when piece.Colour == Colour.Black:
                    sb.Append("n");
                    break;
                case PieceType.Bishop when piece.Colour == Colour.Black:
                    sb.Append("b");
                    break;
                case PieceType.Queen when piece.Colour == Colour.Black:
                    sb.Append("q");
                    break;
                case PieceType.King when piece.Colour == Colour.Black:
                    sb.Append("k");
                    break;
                default:
                    sb.Append("-");
                    break;
            }
        }

        private static BoardState ParseCastlingRights(string castlingRights)
        {
            if (castlingRights == "-")
                return 0;

            BoardState castlingState = 0;

            if (castlingRights.Contains("K")) castlingState |= BoardState.WhiteCanCastleKingSide;
            if (castlingRights.Contains("Q")) castlingState |= BoardState.WhiteCanCastleQueenSide;
            if (castlingRights.Contains("k")) castlingState |= BoardState.BlackCanCastleKingSide;
            if (castlingRights.Contains("q")) castlingState |= BoardState.BlackCanCastleQueenSide;

            return castlingState;
        }
    }
}
