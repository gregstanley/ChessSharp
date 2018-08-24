using Chess.Engine.Models;
using System.Text;

namespace Chess.Engine.Ai.Searches
{
    public interface ISearchMoves
    {
        int PositionCounter { get; }

        Board DoSearch(Board board, Colour colour, int depth, bool isMax);
    }
}
