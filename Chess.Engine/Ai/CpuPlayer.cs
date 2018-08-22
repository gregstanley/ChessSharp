using Chess.Engine.Ai.Searches;
using Chess.Engine.Bit;
using Chess.Engine.Extensions;
using Chess.Engine.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Engine.Ai
{
    public class CpuPlayer
    {
        private List<string> _moveLog = new List<string>();

        public string GetLastMoveLog() => _moveLog.Any() ? _moveLog.Last() : string.Empty;

        private ISearchMoves _search { get; set; }

        private BitBoardMoveFinder _bitBoardMoveFinder = new BitBoardMoveFinder();

        public CpuPlayer(ISearchMoves search)
        {
            _search = search;
        }

        public Board ChoseMove(Board board, Colour colour, int ply)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Playing for {colour}. Generating possible moves...");

            // Must be 2 for now. Should probably always be even so ends with opponents turn
            board.GenerateChildBoards(colour, 2);
            board.UpdateStateInfo();

            if (!board.ChildBoards.Any())
            {
                sb.AppendLine($"Err... no boards available. Not much I can do with that.");

                _moveLog.Add(sb.ToString());

                return null;
            }

            if (board.ChildBoards.Count() == 1)
            {
                sb.AppendLine($"Only one option so may as well play that...");

                _moveLog.Add(sb.ToString());

                return board.ChildBoards.First();
            }

            sb.AppendLine($"There are {board.ChildBoards.Count()} possible moves.");

            var oppositeColour = colour.Opposite();
            var myScore = board.GetScore(colour);
            var opponentScore = board.GetScore(oppositeColour);

            if (board.IsInCheckmate(colour))
            {
                sb.AppendLine($"Hmm I seem to be in Checkmate");

                _moveLog.Add(sb.ToString());

                return board;
            }

            var checkmateBoards = board.GetBoardsWithCheckmate(oppositeColour);

            if (checkmateBoards.Any())
                return GetCheckmateBoard(checkmateBoards, colour, sb);

            Board chosenBoard;

            IEnumerable<Board> escapeCheckBoards = null;
            
            if (board.IsInCheck(colour))
            {
                escapeCheckBoards = GetEscapeCheckBoards(board, colour, sb);

                if (escapeCheckBoards == null || !escapeCheckBoards.Any())
                {
                    sb.AppendLine($"{colour} is in check with no escape");

                    _moveLog.Add(sb.ToString());

                    return null;
                }

                var optionsBoardsRanked = colour == Colour.White
                    ? escapeCheckBoards.OrderByDescending(x => x.Evaluation)
                    : escapeCheckBoards.OrderBy(x => x.Evaluation);

                sb.AppendLine($"Final order:");

                foreach (var optionBoard in optionsBoardsRanked)
                    sb.AppendLine($"   {optionBoard.GetMetricsString()}");

                var chosenTargetBoard = optionsBoardsRanked.First();

                chosenBoard = FindRoot(board, chosenTargetBoard);

                board.OrphanOtherChildBoardSiblingBoards(chosenBoard);

                _moveLog.Add(sb.ToString());

                return chosenBoard;
            }

            sb.AppendLine($"Running AlphaBeta...");

            var absb = new StringBuilder();

            chosenBoard = _search.DoSearch(board, colour, 3, true, absb);

            chosenBoard.Evaluate(colour);

            sb.AppendLine($"Analysed {_search.PositionCounter} moves.");

            sb.AppendLine($"Chosen board: {chosenBoard.GetMetricsString()}");

            sb.AppendLine($"All options:");

            var orderedBoards = colour == Colour.White
                ? board.ChildBoards.OrderByDescending(x => x.ProjectedEvaluation)
                : board.ChildBoards.OrderBy(x => x.ProjectedEvaluation);

            foreach (var optionBoard in orderedBoards)
                sb.AppendLine($"   {optionBoard.GetMetricsString()}");

            board.OrphanOtherChildBoardSiblingBoards(chosenBoard);

            if (false)
            {
                sb.Append(absb);
            }

            _moveLog.Add(sb.ToString());

            return chosenBoard;
        }

        private Board FindRoot(Board rootBoard, Board currentBoard)
        {
            while (!rootBoard.ChildBoards.Contains(currentBoard))
                currentBoard = currentBoard.ParentBoard;

            return currentBoard;
        }

        private Board GetCheckmateBoard(IReadOnlyCollection<Board> checkmateBoards, Colour colour, StringBuilder sb)
        {
            sb.AppendLine($"Checkmate found!");

            var checkmateBoard = checkmateBoards.First();

            _moveLog.Add(sb.ToString());

            return checkmateBoard;
        }

        private IEnumerable<Board> GetEscapeCheckBoards(Board board, Colour colour, StringBuilder sb)
        {
            var kingSquare = board.GetKingSquare(colour);
            var kingSquareRankFile = kingSquare.ToRankFile();

            sb.AppendLine($"{colour} King on {kingSquareRankFile.File}{kingSquareRankFile.Rank} is in Check");

            var escapeCheckBoards = board.ChildBoards.Where(x => !x.IsInCheck(colour));

            if (!escapeCheckBoards.Any())
            {
                sb.AppendLine($"Err... {colour} {board.GetPiece(kingSquareRankFile)} {kingSquareRankFile.File}{kingSquareRankFile.Rank} can't escape Check (shouldn't happen but never mind). Checkmate.");

                _moveLog.Add(sb.ToString());
            }

            return escapeCheckBoards;
        }

        private IEnumerable<Board> FilterLostPieceBoards(Board board, Colour colour, StringBuilder sb)
        {
            var squaresUnderAttack = board.GetSquaresUnderThreat(colour);

            var coveredSquares = board.GetProtectedPieces(colour);

            IEnumerable<Board> escapeBoards = null;

            if (squaresUnderAttack != 0)
            {
                var sua = squaresUnderAttack.ToList();

                var highestValueSquareUnderAttack = sua.First();
                var rankFile = highestValueSquareUnderAttack.ToRankFile();
                var possibleEscapeRoutes = board.ChildBoards
                    .Where(x => x.GetMovedFrom().Rank == rankFile.Rank && x.GetMovedFrom().File == rankFile.File);

                if (possibleEscapeRoutes.Any())
                    escapeBoards = possibleEscapeRoutes;
            }

            return escapeBoards;
        }

        private IEnumerable<Board> FilterImmediateCheckmateBoards(Board board, Colour colour, IEnumerable<Board> d2LeafBoards, StringBuilder sb)
        {
            var boardsWhereKingBecomesInCheck = d2LeafBoards.Where(x => x.IsInCheck(colour));

            if (!boardsWhereKingBecomesInCheck.Any())
                return Enumerable.Empty<Board>();

            IEnumerable<Board> d1NoCheckmateBoards = board.ChildBoards;

            sb.AppendLine($"I can see {boardsWhereKingBecomesInCheck.Count()} response boards that leave my King in check. Is it Checkmate?");

            foreach (var boardInCheck in boardsWhereKingBecomesInCheck)
            {
                boardInCheck.GenerateChildBoards(colour, 1);

                //sb.AppendLine($"Board: {boardInCheck.ParentBoard.GetCode()}->{boardInCheck.GetCode()} Checkmate: {boardInCheck.IsInCheckmate(colour)}");
            }

            d1NoCheckmateBoards = board.ChildBoards.Where(x => !x.ChildBoards.Any(y => y.IsInCheckmate(colour)));

            var checkmateCount = board.ChildBoards.Count() - d1NoCheckmateBoards.Count();

            if (checkmateCount == 0)
            {
                sb.AppendLine("No move leads direct to Checkmate.");
            }
            else
            {
                var d1CheckmateBoards = board.ChildBoards.Where(x => x.ChildBoards.Any(y => y.IsInCheckmate(colour)));

                sb.AppendLine($"Could be. {checkmateCount} move/s lead direct to Checkmate.");
                sb.AppendLine($"Keeping {d1NoCheckmateBoards.Count()} boards that do not lead to an immediate Chekmate:");

                d1CheckmateBoards = board.ChildBoards.Where(x => x.ChildBoards.Any(y => y.IsInCheckmate(colour)));

                sb.AppendLine($"Discarding {d1CheckmateBoards.Count()} dangerous boards:");

                foreach (var dangerousBoard in d1CheckmateBoards)
                    sb.AppendLine($"   {dangerousBoard.GetMetricsString()}");
            }

            return d1NoCheckmateBoards;
        }

        private IOrderedEnumerable<Board> MergeOptions(IEnumerable<Board> boards1, IEnumerable<Board> boards2, Colour colour) =>
            boards1.Intersect(boards2)
                .OrderBy(x => x.GetScore(colour.Opposite()))
                .ThenByDescending(x => x.GetScore(colour));
    }
}
