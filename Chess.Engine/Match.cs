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
        public int ThisTurn { get; private set; } = 1;

        public Colour ThisTurnColour { get { return ThisTurn % 2 == 1 ? Colour.White : Colour.Black; } }

        public Colour HumanColour { get; private set; } = Colour.None;

        public bool IsHumanTurn { get { return HumanColour != Colour.None && ThisTurnColour == HumanColour; } }

        private List<Board> _boards = new List<Board>();

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

            var colourTurn = (ThisTurn + 1) / 2;

            Board chosenBoard = null;

            if (IsHumanTurn)
                chosenBoard = DoMove(board, ThisTurnColour, startPosition, endPosition, promotionType);
            else
                chosenBoard = _cpuPlayer.ChoseMove(board, ThisTurnColour, ThisTurn);

            if (chosenBoard == null)
                return null;

            // As we have selected a board we can detach all the unused ones
            board.OrphanOtherChildBoardSiblingBoards(chosenBoard);

            _boards.Add(chosenBoard);

            ++ThisTurn;

            return chosenBoard;
        }

        public bool IsLegalMove(RankFile startPosition, RankFile endPosition, PieceType promotionType)
        {
            var board = GetHeadBoard();

            var colourTurn = (ThisTurn + 1) / 2;

            var chosenBoard = DoMove(board, ThisTurnColour, startPosition, endPosition, promotionType);

            return chosenBoard == null ? false : true;
        }

        private Board DoMove(Board board, Colour colour, RankFile startPosition, RankFile endPosition, PieceType promotionType)
        {
            var sb = new StringBuilder();

            // Must be 2 for now. Should probably always be even so ends with opponents turn
            board.GenerateChildBoards(colour, 2);
            
            if (!board.ChildBoards.Any())
                return null;

            var piece = board.GetPiece(startPosition);

            if (piece == PieceType.None)
                return null;

            if (piece == PieceType.Rook)
            {
                var king = board.GetPiece(endPosition);

                if (king == PieceType.King)
                {
                    if (!board.CanCastle(colour))
                        return null;

                    var possibleBoards = board.ChildBoards.Where(x => x.GetMovedFrom().Rank == startPosition.Rank && x.GetMovedFrom().File == startPosition.File);

                    var castleBoard = possibleBoards.SingleOrDefault(x => x.GetMove() as MoveCastle != null);

                    if (castleBoard != null)
                        return castleBoard;
                }
            }
            else if (piece == PieceType.King)
            {
                var rook = board.GetPiece(endPosition);

                if (rook == PieceType.Rook)
                {
                    if (!board.CanCastle(colour))
                        return null;

                    var possibleBoards = board.ChildBoards.Where(x => x.GetMovedFrom().Rank == endPosition.Rank && x.GetMovedFrom().File == endPosition.File);

                    var castleBoard = possibleBoards.SingleOrDefault(x => x.GetMove() as MoveCastle != null);

                    if (castleBoard != null)
                        return castleBoard;
                }
            }

            var move = new Move(colour, piece, startPosition, endPosition);

            if (promotionType != PieceType.None)
                move = new Move(colour, piece, startPosition, endPosition, promotionType);

            var code = move.GetCode();

            var boards = board.ChildBoards.Where(x => x.GetCode().StartsWith(code));

            if (boards == null || !boards.Any())
                return null;

            if (boards.Count() == 1)
                return boards.First();

            // Temporary line when dupe boards are found
            return boards.First();
        }
    }
}
