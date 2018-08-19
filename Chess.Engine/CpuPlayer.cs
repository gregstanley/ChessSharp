using Chess.Engine.Bit;
using Chess.Engine.Extensions;
using Chess.Engine.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Engine
{
    public class CpuPlayer
    {
        private List<string> _moveLog = new List<string>();

        public string GetLastMoveLog() => _moveLog.Any() ? _moveLog.Last() : string.Empty;

        private AlphaBeta _algorithm { get; set; }

        private BitBoardMoveFinder _bitBoardMoveFinder = new BitBoardMoveFinder();

        public CpuPlayer()
        {
            _algorithm = new AlphaBeta();
        }

        public Board ChoseMove(Board board, Colour colour, int ply)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Playing for {colour}. Generating possible moves...");

            // Must be 2 for now. Should probably always be even so ends with opponents turn
            board.GenerateChildBoards(colour, 2);

            if (!board.ChildBoards.Any())
            {
                sb.AppendLine($"Err... no boards available. Not much I can do with that.");

                _moveLog.Add(sb.ToString());

                return null;
            }

            //if (ply < 3)
            //{
            //    sb.AppendLine($"Ply < 3. Random pick time...");

            //    var randomBoard = Random((IReadOnlyCollection<Board>)board.ChildBoards);

            //    sb.AppendLine($"Chosen board: {randomBoard.GetMetricsString()}");

            //    _moveLog.Add(sb.ToString());
            //}

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

            var d2LeafBoards = board.FindLeaves();

            var acceptableBoards = FilterImmediateCheckmateBoards(board, colour, d2LeafBoards, sb);

            var d2NoCheckmateBoards = d2LeafBoards.Where(x => acceptableBoards.Contains(x.ParentBoard));

            if (d2NoCheckmateBoards.Count() == 0)
            {
                sb.AppendLine($"Almost Checkmate - There are no ways out");

                return board.ChildBoards.First();
            }

            Board chosenBoard;

            if (d2LeafBoards.Count() != d2NoCheckmateBoards.Count())
            {
                sb.AppendLine($"Potential Checkmate - Leafboards {d2LeafBoards.Count()} filtered to {d2NoCheckmateBoards.Count()}");

                var optionsBoardsRanked = colour == Colour.White
                    ? d2NoCheckmateBoards.OrderByDescending(x => x.EvaluationScore)
                    : d2NoCheckmateBoards.OrderBy(x => x.EvaluationScore);

                sb.AppendLine($"Final order:");

                foreach (var optionBoard in optionsBoardsRanked)
                    sb.AppendLine($"   {optionBoard.GetMetricsString()}");

                var chosenTargetBoard = optionsBoardsRanked.First();

                chosenBoard = FindRoot(board, chosenTargetBoard);

                board.OrphanOtherChildBoardSiblingBoards(chosenBoard);

                _moveLog.Add(sb.ToString());

                return chosenBoard;
            }

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
                    ? escapeCheckBoards.OrderByDescending(x => x.EvaluationScore)
                    : escapeCheckBoards.OrderBy(x => x.EvaluationScore);

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

            chosenBoard = _algorithm.AlphaBetaRoot(board, colour, 3, true, absb);

            chosenBoard.Evaluate(colour);

            sb.AppendLine($"Analysed {_algorithm.PositionCounter} moves.");

            sb.AppendLine($"Chosen board: {chosenBoard.GetMetricsString()}");

            sb.AppendLine($"All options:");

            foreach (var optionBoard in board.ChildBoards)
                sb.AppendLine($"   {optionBoard.GetMetricsString()}");

            board.OrphanOtherChildBoardSiblingBoards(chosenBoard);

            if (false)
            {
                sb.Append(absb);
            }

            _moveLog.Add(sb.ToString());

            return chosenBoard;
        }

        public Board ChoseMoveOld(Board board, Colour colour)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Playing for {colour}. Generating possible moves...");

            // Must be 2 for now. Should probably always be even so ends with opponents turn
            board.GenerateChildBoards(colour, 2);

            if (!board.ChildBoards.Any())
            {
                sb.AppendLine($"Err... no boards available. Not much I can do with that.");

                _moveLog.Add(sb.ToString());

                return null;
            }

            var oppositeColour = colour.Opposite();
            var myScore = board.GetScore(colour);
            var opponentScore = board.GetScore(oppositeColour);

            sb.AppendLine($"There are {board.ChildBoards.Count()} possible moves.");

            var checkmateBoards = board.GetBoardsWithCheckmate(oppositeColour);

            if (checkmateBoards.Any())
                return GetCheckmateBoard(checkmateBoards, colour, sb);

            IEnumerable<Board> escapeCheckBoards = null;

            if (board.IsInCheck(colour))
                escapeCheckBoards = GetEscapeCheckBoards(board, colour, sb);

            var escapeBoards = FilterLostPieceBoards(board, colour, sb);

            var d2LeafBoards = board.FindLeaves();

            var acceptableBoards = FilterImmediateCheckmateBoards(board, colour, d2LeafBoards, sb);

            var d2NoCheckmateBoards = d2LeafBoards.Where(x => acceptableBoards.Contains(x.ParentBoard));

            sb.AppendLine($"Leafboards {d2LeafBoards.Count()} filtered to {d2NoCheckmateBoards.Count()}");

            var d1AvoidCheckmateBoards = acceptableBoards;

            var d2BoardsLosingPieces = d2NoCheckmateBoards
                .Where(x => x.GetScore(colour) < myScore);

            var d2NeutralBoards = d2NoCheckmateBoards
                .Where(x => myScore == x.GetScore(colour) && opponentScore == x.GetScore(oppositeColour));

            var d2BoardsTakingPiecesWithNoLoss = d2NoCheckmateBoards
                .Where(x => x.GetScore(colour) == myScore && x.GetScore(oppositeColour) < opponentScore);

            var d1NoLossBoards = d2BoardsTakingPiecesWithNoLoss.Select(x => x.ParentBoard);

            var d1NoLossBoardsGuaranteed = d1NoLossBoards
                .Where(x => !x.ChildBoards.Any(y => y.GetScore(colour) < myScore));

            var d2BoardsLostLess = d2NoCheckmateBoards
                .Where(x => x.GetMetrics(colour).PointsChange > x.GetMetrics(colour.Opposite()).PointsChange);

            var d1LostLessBoards = d2BoardsLostLess.Select(x => x.ParentBoard);

            var d1LostLessBoardsGuaranteed = d1LostLessBoards
                .Where(x => !x.ChildBoards.Any(y => (myScore - y.GetScore(colour)) <= (opponentScore - y.GetScore(oppositeColour))));

            IEnumerable<Board> optionsBoards = null;

            if (acceptableBoards != null && acceptableBoards.Any())
            {
                sb.AppendLine($"Attempting to avoid a future Checkmate. Looking for Checkmate escapes that capture a piece without loss");

                optionsBoards = MergeOptions(d1AvoidCheckmateBoards, d1NoLossBoardsGuaranteed, colour);

                if (!optionsBoards.Any())
                {
                    sb.AppendLine($"No beneficial routes where I can capture safely");

                    optionsBoards = MergeOptions(d1AvoidCheckmateBoards, d1LostLessBoardsGuaranteed, colour);

                    if (!optionsBoards.Any())
                    {
                        sb.AppendLine($"Have to escape Check (go anywhere)");
                        optionsBoards = d1AvoidCheckmateBoards;
                    }
                }
            }
            else if (escapeCheckBoards != null && escapeCheckBoards.Any())
            {
                sb.AppendLine($"Looking for Check escapes that capture a piece without loss");

                optionsBoards = MergeOptions(escapeCheckBoards, d1NoLossBoardsGuaranteed, colour);

                if (!optionsBoards.Any())
                {
                    sb.AppendLine($"No beneficial routes where I can capture safely");

                    optionsBoards = MergeOptions(escapeCheckBoards, d1LostLessBoardsGuaranteed, colour);

                    if (!optionsBoards.Any())
                    {
                        sb.AppendLine($"Have to escape Check (go anywhere)");
                        optionsBoards = escapeCheckBoards;
                    }
                }
            }
            else if (escapeBoards != null && escapeBoards.Any())
            {
                sb.AppendLine($"Piece looking for escapes that capture a piece without loss");

                optionsBoards = MergeOptions(escapeBoards, d1NoLossBoardsGuaranteed, colour);

                if (!optionsBoards.Any())
                {
                    sb.AppendLine($"No beneficial routes where I can capture safely");
                    optionsBoards = MergeOptions(escapeBoards, d1LostLessBoardsGuaranteed, colour);

                    if (!optionsBoards.Any())
                    {
                        sb.AppendLine($"Saving piece (go anywhere)");
                        optionsBoards = escapeBoards;
                    }
                }
            }
            else if (d1NoLossBoardsGuaranteed.Any())
            {
                sb.AppendLine($"Found {d1NoLossBoardsGuaranteed.Count()} safe captures - no lost pieces");

                optionsBoards = d1NoLossBoardsGuaranteed;
            }
            else if (d1LostLessBoardsGuaranteed.Any())
            {
                sb.AppendLine($"Found {d1LostLessBoardsGuaranteed.Count()} where I should lose fewer points");

                optionsBoards = d1LostLessBoardsGuaranteed;
            }

            if (optionsBoards == null || !optionsBoards.Any())
            {
                sb.AppendLine($"Picking random move");

                optionsBoards = board.ChildBoards
                    .Where(x => !x.ParentBoard.IsInCheck(colour));

                if (optionsBoards == null || !optionsBoards.Any())
                    optionsBoards = board.ChildBoards;
            }
            
            var optionsBoardsRanked = colour == Colour.White
                ? optionsBoards.OrderByDescending(x => x.EvaluationScore)
                : optionsBoards.OrderBy(x => x.EvaluationScore);

            sb.AppendLine($"Final order:");

            foreach (var optionBoard in optionsBoardsRanked)
                sb.AppendLine($"   {optionBoard.GetMetricsString()}");
            
            var chosenTargetBoard = optionsBoardsRanked.First();

            var chosenBoard = FindRoot(board, chosenTargetBoard);

            board.OrphanOtherChildBoardSiblingBoards(chosenBoard);

            //if (chosenBoard.GetMove() is MoveCastle moveCastle)
            //    sb.AppendLine($"Chosen move: {moveCastle.GetCode()} = Castle");
            //else
            //    sb.AppendLine($"Chosen move: {chosenBoard.GetCode()}");
            sb.AppendLine($"Chosen move: {chosenBoard.GetCode()}");

            _moveLog.Add(sb.ToString());

            return chosenBoard;
        }

        //private Board Random(IReadOnlyCollection<Board> boards)
        //{
        //    var range = boards.Count();

        //    var chosenTargetBoardIndex = _random.Next(range);

        //    return boards.ElementAt(chosenTargetBoardIndex);
        //}

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

            //sb.Append(checkmateBoard.GetCheckDebugString());

            //foreach (var bic in checkmateBoard.GetBoardsWithCheck())
            //{
            //    sb.AppendLine($">>> {bic.GetCode()}");
            //    sb.Append(bic.GetSquaresUnderAttackDebugString(colour.Opposite()));
            //}

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
