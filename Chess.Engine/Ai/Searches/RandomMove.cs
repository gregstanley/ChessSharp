using System;
using System.Linq;
using System.Text;
using Chess.Engine.Models;
using Troschuetz.Random.Generators;

namespace Chess.Engine.Ai.Searches
{
    public class RandomMove : ISearchMoves
    {
        public int PositionCounter { get; private set; }

        private StandardGenerator _random = new StandardGenerator();

        public event EventHandler IterationComplete;

        public Board DoSearch(Board board, Colour colour, int depth, bool isMax)
        {
            board.GenerateChildBoards(colour, 2);
            //board.UpdateStateInfo();

            foreach (var childBoard in board.ChildBoards)
                childBoard.Evaluate(colour);

            PositionCounter = board.ChildBoards.Count;

            var legalMoves = board.GetLegalMoves();

            if (!legalMoves.Any())
            {
                if (board.ChildBoards.Any())
                    return board.ChildBoards.First();

                return new Board(board, null, null, null);
            }

            // Debug En Passasnt
            var optionsBoardsRanked = legalMoves.Where(x => x.EnPassantSquare != 0);

            //var optionsBoardsRanked = colour == Colour.White
            //    ? legalMoves.OrderByDescending(x => x.Evaluation)
            //    : legalMoves.OrderBy(x => x.Evaluation);

            var range = optionsBoardsRanked.Count();

            var chosenBoardIndex = _random.Next(range);

            var chosenBoard = optionsBoardsRanked.ElementAt(chosenBoardIndex);

            return chosenBoard;
        }
    }
}
