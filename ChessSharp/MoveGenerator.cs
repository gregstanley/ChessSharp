using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System.Collections.Generic;

namespace ChessSharp
{
    public class MoveGenerator
    {
        public void Generate(BitBoard bitBoard)
        {
            
        }

        public void GeneratePawnMoves(BitBoard bitBoard, Colour colour, IList<Move> moves)
        {
            var pawns = bitBoard.FindPawnSquares(colour).ToList();
            
            foreach (var from in pawns)
            {
                var to = colour == Colour.White ? (ulong)from << 8 : (ulong)from >> 8;
                moves.Add(new Move(colour, PieceType.Pawn, from, (SquareFlag)to, PieceType.None, PieceType.None, CastleType.None, false));
            }
        }


    }
}
