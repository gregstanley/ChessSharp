using ChessSharp.Enums;
using ChessSharp.Extensions;
using System.Collections.Generic;

namespace ChessSharp
{
    public class MoveGenerator
    {
        public void Generate(BitBoard bitBoard)
        {
            
        }

        public void GeneratePawnMoves(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            var pawns = bitBoard.FindPawnSquares(colour).ToList();
            
            foreach (var from in pawns)
            {
                var to = colour == Colour.White ? (ulong)from << 8 : (ulong)from >> 8;
                moves.Add(MoveConstructor.CreateMove(colour, PieceType.Pawn, from, (SquareFlag)to, PieceType.None, MoveType.Ordinary));
            }
        }
    }
}
