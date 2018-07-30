using System;
using System.Collections.Generic;
using System.Linq;
using Troschuetz.Random.Generators;

namespace Chess
{
    public class Match
    {
        public int ThisTurn { get; private set; } = 1;

        public Colour ThisTurnColour { get { return ThisTurn % 2 == 1 ? Colour.White : Colour.Black; } }

        public Colour HumanColour { get; private set; } = Colour.None;

        public bool IsHumanTurn { get { return HumanColour != Colour.None && ThisTurnColour == HumanColour; } }

        private List<Board> _boards = new List<Board>();

        private StandardGenerator _random = new StandardGenerator();

        private CpuPlayer _cpuPlayer = new CpuPlayer();

        public Match(Board board, Colour humanColour = Colour.None)
        {
            _boards.Add(board);

            HumanColour = humanColour;
        }

        public Board GetHeadBoard() =>
            _boards[ThisTurn - 1];

        public string GetLastCpuMoveLog() =>
            _cpuPlayer.GetLastMoveLog();

        public Piece GetPieceOnSquare(RankFile rankFile) =>
            GetHeadBoard().GetPieceOnSquare(rankFile);

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

            var colourTurn = (ThisTurn + 1) / 2;

            Board chosenBoard = null;

            if (IsHumanTurn)
                chosenBoard = DoMove(board, ThisTurnColour, startPosition, endPosition, promotionType);
            else
                chosenBoard = _cpuPlayer.ChoseMove(board, ThisTurnColour);

            if (chosenBoard == null)
                return null;

            // As we have selected a board we can detach all the unused ones
            board.OrphanOtherChildBoardSiblingBoards(chosenBoard);

            _boards.Add(chosenBoard);

            ++ThisTurn;

            return chosenBoard;
        }

        private Board DoMove(Board board, Colour colour, RankFile startPosition, RankFile endPosition, PieceType promotionType)
        {
            // Must be 2 for now. Should probably always be even so ends with opponents turn
            board.GenerateChildBoards(colour, 2);

            if (!board.ChildBoards.Any())
                return null;

            var square = board.GetSquare(startPosition);

            if (square.Piece == null)
                return null;

            if (square.Piece.Type == PieceType.Rook)
            {
                var kingSquare = board.GetSquare(endPosition);

                if (kingSquare.Piece?.Type == PieceType.King)
                {
                    var possibleBoards = board.ChildBoards.Where(x => x.GetMovedFrom().Rank == startPosition.Rank && x.GetMovedFrom().File == startPosition.File);

                    var castleBoard = possibleBoards.SingleOrDefault(x => x.GetMove() as MoveCastle != null);

                    if (castleBoard != null)
                        return castleBoard;
                }
            }
            else if (square.Piece.Type == PieceType.King)
            {
                var rookSquare = board.GetSquare(endPosition);

                if (rookSquare.Piece?.Type == PieceType.Rook)
                {
                    var possibleBoards = board.ChildBoards.Where(x => x.GetMovedFrom().Rank == endPosition.Rank && x.GetMovedFrom().File == endPosition.File);

                    var castleBoard = possibleBoards.SingleOrDefault(x => x.GetMove() as MoveCastle != null);

                    if (castleBoard != null)
                        return castleBoard;
                }
            }

            var move = new Move(colour, square.Piece.Type, startPosition, endPosition);

            if (promotionType != PieceType.None)
                move = new Move(colour, square.Piece.Type, startPosition, endPosition, promotionType);

            var code = move.GetCode();

            var boards = board.ChildBoards.Where(x => x.Code == code);

            if (boards == null || !boards.Any())
                return null;

            if (boards.Count() == 1)
                return boards.First();

            // Temporary line when dupe boards are found
            return boards.First();
        }
    }
}
