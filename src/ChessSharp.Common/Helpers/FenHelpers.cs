using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Extensions;
using ChessSharp.Common.Models;

namespace ChessSharp.Common.Helpers
{
    public static class FenHelpers
    {
        public const string Default = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public const string Position2 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -";
        public const string Position3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -";
        public const string Position4 = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
        public const string Position5 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";
        public const string Position6 = "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10";

        public static string ToFen(GameState gameState)
        {
            var sb = new StringBuilder();

            if (gameState.WhiteCanCastleKingSide)
                sb.Append('K');

            if (gameState.WhiteCanCastleQueenSide)
                sb.Append('Q');

            if (gameState.BlackCanCastleKingSide)
                sb.Append('k');

            if (gameState.BlackCanCastleQueenSide)
                sb.Append('q');

            var castleString = sb.ToString();

            sb.Clear();

            for (var rank = 7; rank >= 0; --rank)
            {
                var spaceCount = 0;

                for (var file = 0; file < 8; ++file)
                {
                    var index = (rank * 8) + file;

                    var piece = PieceMapHelpers.GetPiece(gameState, index.ToSquareFlag());

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
                    sb.Append('/');
            }

            var colourToken = gameState.ToPlay == Colour.White ? "w" : "b";

            sb.Append($" {colourToken}");

            sb.Append($" {castleString}");

            if (gameState.EnPassant == 0)
                sb.Append(" -");
            else
                sb.Append($" {gameState.EnPassant.ToString().ToLower()}");

            sb.Append($" {gameState.HalfMoveClock}");

            sb.Append($" {gameState.FullTurn}");

            return sb.ToString();
        }

        public static GameState Parse(string fen)
        {
            var parts = fen.Split(' ');

            var board = parts[0];
            var colour = parts[1] == "w" ? Colour.White : Colour.Black;

            StateFlag castlingRights = ParseCastlingRights(parts[2]);

            SquareFlag enPassantSquare = 0;

            if (parts[3] != "-")
                _ = Enum.TryParse(parts[3].ToUpper(), out enPassantSquare);

            var halfTurnCounter = 0;
            var fullMoveNumber = 0;

            if (parts.Length > 4)
                halfTurnCounter = int.Parse(parts[4]);

            if (parts.Length > 5)
                fullMoveNumber = int.Parse(parts[5]);

            return CreateGameState(board, colour, castlingRights, enPassantSquare, halfTurnCounter, fullMoveNumber);
        }

        private static GameState CreateGameState(string board, Colour toPlay, StateFlag castlingRights, SquareFlag enPassantSquare, int halfTurnCounter, int fullMoveNumber)
        {
            var ranks = ExpandBoardString(board);

            var squares = GetSquaresStates(ranks);

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
                        case PieceType.Pawn:
                            whitePawns |= square.Square;
                            break;
                        case PieceType.Rook:
                            whiteRooks |= square.Square;
                            break;
                        case PieceType.Knight:
                            whiteKnights |= square.Square;
                            break;
                        case PieceType.Bishop:
                            whiteBishops |= square.Square;
                            break;
                        case PieceType.Queen:
                            whiteQueens |= square.Square;
                            break;
                        case PieceType.King:
                            whiteKing |= square.Square;
                            break;
                    }
                }
                else
                {
                    switch (square.Type)
                    {
                        case PieceType.Pawn:
                            blackPawns |= square.Square;
                            break;
                        case PieceType.Rook:
                            blackRooks |= square.Square;
                            break;
                        case PieceType.Knight:
                            blackKnights |= square.Square;
                            break;
                        case PieceType.Bishop:
                            blackBishops |= square.Square;
                            break;
                        case PieceType.Queen:
                            blackQueens |= square.Square;
                            break;
                        case PieceType.King:
                            blackKing |= square.Square;
                            break;
                    }
                }
            }

            return new GameState(
                0,
                toPlay,
                halfTurnCounter,
                fullMoveNumber,
                castlingRights.HasFlag(StateFlag.WhiteCanCastleKingSide),
                castlingRights.HasFlag(StateFlag.WhiteCanCastleQueenSide),
                castlingRights.HasFlag(StateFlag.BlackCanCastleKingSide),
                castlingRights.HasFlag(StateFlag.BlackCanCastleKingSide),
                whitePawns,
                whiteRooks,
                whiteKnights,
                whiteBishops,
                whiteQueens,
                whiteKing,
                blackPawns,
                blackRooks,
                blackKnights,
                blackBishops,
                blackQueens,
                blackKing,
                enPassantSquare);
        }

        private static StateFlag ParseCastlingRights(string castlingRights)
        {
            if (castlingRights == "-")
                return 0;

            StateFlag castlingState = 0;

            if (castlingRights.Contains("K"))
                castlingState |= StateFlag.WhiteCanCastleKingSide;

            if (castlingRights.Contains("Q"))
                castlingState |= StateFlag.WhiteCanCastleQueenSide;

            if (castlingRights.Contains("k"))
                castlingState |= StateFlag.BlackCanCastleKingSide;

            if (castlingRights.Contains("q"))
                castlingState |= StateFlag.BlackCanCastleQueenSide;

            return castlingState;
        }

        private static IReadOnlyCollection<SquareState> GetSquaresStates(string[] expandedRanks)
        {
            var squares = new List<SquareState>();

            var fenChars = GetExpandedBoardCharArray(expandedRanks);

            if (fenChars.Length != 64)
                throw new Exception($"The expanded fen string has {fenChars.Length} chars in it but it should have 64");

            var allBitsOn = (SquareFlag)ulong.MaxValue;

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

        private static string[] ExpandBoardString(string board)
        {
            var expandedRanks = board
                .Replace("8", "--------")
                .Replace("7", "-------")
                .Replace("6", "------")
                .Replace("5", "-----")
                .Replace("4", "----")
                .Replace("3", "---")
                .Replace("2", "--")
                .Replace("1", "-");

            var expandedRanksList = expandedRanks.Split('/');

            var output = expandedRanksList.Reverse();

            return output.ToArray();
        }

        private static void GetPieceString(StringBuilder sb, Piece piece)
        {
            switch (piece.Type)
            {
                case PieceType.Pawn when piece.Colour == Colour.White:
                    sb.Append('P');
                    break;
                case PieceType.Rook when piece.Colour == Colour.White:
                    sb.Append('R');
                    break;
                case PieceType.Knight when piece.Colour == Colour.White:
                    sb.Append('N');
                    break;
                case PieceType.Bishop when piece.Colour == Colour.White:
                    sb.Append('B');
                    break;
                case PieceType.Queen when piece.Colour == Colour.White:
                    sb.Append('Q');
                    break;
                case PieceType.King when piece.Colour == Colour.White:
                    sb.Append('K');
                    break;
                case PieceType.Pawn when piece.Colour == Colour.Black:
                    sb.Append('p');
                    break;
                case PieceType.Rook when piece.Colour == Colour.Black:
                    sb.Append('r');
                    break;
                case PieceType.Knight when piece.Colour == Colour.Black:
                    sb.Append('n');
                    break;
                case PieceType.Bishop when piece.Colour == Colour.Black:
                    sb.Append('b');
                    break;
                case PieceType.Queen when piece.Colour == Colour.Black:
                    sb.Append('q');
                    break;
                case PieceType.King when piece.Colour == Colour.Black:
                    sb.Append('k');
                    break;
                default:
                    sb.Append('-');
                    break;
            }
        }

        private static char[] GetExpandedBoardCharArray(string[] expandedRanks)
        {
            return string.Join(string.Empty, expandedRanks).ToCharArray();
        }
    }
}
