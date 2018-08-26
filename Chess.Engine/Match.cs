using Chess.Engine.Ai;
using Chess.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Engine
{
    public class Match
    {
        public int Ply { get; private set; } = 1;

        public Colour Turn { get { return Ply % 2 == 1 ? Colour.White : Colour.Black; } }

        public int HalfTurnCounter { get; private set; } = 0;

        public int FullTurnNumber { get { return (int)(Ply + 1) / 2; } }

        public Colour HumanColour { get; private set; } = Colour.None;

        public bool IsHumanTurn { get { return HumanColour != Colour.None && Turn == HumanColour; } }

        private List<Board> _boards = new List<Board>();

        private CpuPlayer _cpuPlayer;

        public Match(Board board, CpuPlayer cpuPlayer, Colour humanColour = Colour.None)
        {
            _boards.Add(board);
            _cpuPlayer = cpuPlayer;

            HumanColour = humanColour;
        }

        public IReadOnlyCollection<string> GetMatchNotation()
        {
            var turns = new List<string>();

            var sb = new StringBuilder();

            var ply = 0;
            
            foreach(var board in _boards)
            {
                if (ply == 0)
                {
                    ++ply;
                    continue;
                }

                sb.Append(board.MoveToString());

                if (ply % 2 == 1)
                {
                    sb.Append(" ");
                }
                else
                {
                    turns.Add(sb.ToString());
                    sb.Clear();
                }

                ++ply;
            }

            // Remember the last half turn
            if (sb.Length > 0)
                turns.Add(sb.ToString());

            return turns;
        }

        public string GetFen()
        {
            var board = GetHeadBoard();

            return $"{board.ToPartialFen()} {HalfTurnCounter} {FullTurnNumber}";
        }

        public Board GetHeadBoard() =>
            _boards[Ply - 1];

        public string GetLastCpuMoveLog() =>
            _cpuPlayer.GetLastMoveLog();

        public byte GetInstanceNumber(Colour colour, PieceType type, SquareFlag square) =>
            GetHeadBoard().GetInstanceNumber(colour, type, square);

        public PieceType GetPieceOnSquareType(RankFile rankFile) =>
            GetHeadBoard().GetPieceOnSquare(rankFile);

        public Colour GetPieceOnSquareColour(RankFile rankFile) =>
            GetHeadBoard().GetPieceOnSquareColour(rankFile);

        public bool CheckForPawnPromotion(RankFile startPosition, RankFile endPosition) =>
            GetHeadBoard().CheckForPawnPromotion(startPosition, endPosition);

        public Board NextTurn() =>
            NextTurn(null, null);

        public Board NextTurn(RankFile startPosition, RankFile endPosition) =>
            NextTurn(startPosition, endPosition, PieceType.None);

        public Board NextTurn(RankFile startPosition, RankFile endPosition, PieceType promotionType)
        {
            if (IsHumanTurn && startPosition == null)
                throw new Exception("Start position must be set on human turn");

            if (IsHumanTurn && endPosition == null)
                throw new Exception("End position must be set on human turn");

            var board = GetHeadBoard();

            var colourTurn = (Ply + 1) / 2;

            Board chosenBoard = null;

            if (IsHumanTurn)
                chosenBoard = DoMove(board, Turn, startPosition, endPosition, promotionType);
            else
                chosenBoard = _cpuPlayer.ChoseMove(board, Turn, Ply);

            if (chosenBoard == null)
                return null;

            if (chosenBoard.IsCapture || chosenBoard.Move.Type == PieceType.Pawn)
                HalfTurnCounter = 0;
            else
                ++HalfTurnCounter;

            // As we have selected a board we can detach all the unused ones
            board.OrphanOtherChildBoardSiblingBoards(chosenBoard);

            _boards.Add(chosenBoard);

            ++Ply;

            return chosenBoard;
        }

        public bool IsLegalMove(RankFile startPosition, RankFile endPosition, PieceType promotionType)
        {
            var board = GetHeadBoard();

            var colourTurn = (Ply + 1) / 2;

            var chosenBoard = DoMove(board, Turn, startPosition, endPosition, promotionType);

            return chosenBoard == null ? false : true;
        }

        private Board DoMove(Board board, Colour colour, RankFile startPosition, RankFile endPosition, PieceType promotionType)
        {
            var sb = new StringBuilder();

            // Must be 2 for now. Should probably always be even so ends with opponents turn
            board.GenerateChildBoards(colour, 2);
            board.UpdateStateInfo();

            var legalMoves = board.GetLegalMoves();

            if (!legalMoves.Any())
                return null;

            var squareState = board.GetSquareState(startPosition);

            if (squareState.Type == PieceType.None)
                return null;

            if (squareState.Type == PieceType.Rook)
            {
                var squareState2 = board.GetSquareState(endPosition);

                if (squareState2.Colour == colour && squareState2.Type == PieceType.King)
                {
                    if (!board.CanCastle(colour))
                        return null;

                    var possibleBoards = legalMoves.Where(x => x.GetMovedFrom().Rank == startPosition.Rank && x.GetMovedFrom().File == startPosition.File);

                    var castleBoard = possibleBoards.SingleOrDefault(x => x.GetMove() as MoveCastle != null);

                    if (castleBoard != null)
                        return castleBoard;
                }
            }
            else if (squareState.Type == PieceType.King)
            {
                var squareState2 = board.GetSquareState(endPosition);

                if (squareState2.Colour == colour && squareState2.Type == PieceType.Rook)
                {
                    if (!board.CanCastle(colour))
                        return null;

                    var possibleBoards = legalMoves.Where(x => x.GetMovedFrom().Rank == endPosition.Rank && x.GetMovedFrom().File == endPosition.File);

                    var castleBoard = possibleBoards.SingleOrDefault(x => x.GetMove() as MoveCastle != null);

                    if (castleBoard != null)
                        return castleBoard;
                }
            }

            var move = new Move(colour, squareState.Type, startPosition, endPosition);

            if (promotionType != PieceType.None)
                move = new Move(colour, squareState.Type, startPosition, endPosition, PieceType.None, promotionType);

            var code = move.Code;

            var boards = legalMoves.Where(x => x.Code.StartsWith(code));

            if (boards == null || !boards.Any())
                return null;

            if (boards.Count() == 1)
                return boards.First();

            // Temporary line when dupe boards are found
            return boards.First();
        }
    }
}
