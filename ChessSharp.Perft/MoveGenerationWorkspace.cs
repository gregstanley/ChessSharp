using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System.Collections.Generic;

namespace ChessSharp.MoveGeneration
{
    public class MoveGenerationWorkspace
    {
        internal ulong[] Buffer1 = new ulong[4];
        internal ulong[] Buffer2 = new ulong[4];

        internal List<SquareFlag> SafeSquares = new List<SquareFlag>(32);

        public MoveGenerationWorkspace(BitBoard bitBoard, Colour colour)
        {
            BitBoard = bitBoard;
            Colour = colour;
        }

        public BitBoard BitBoard { get; private set; }

        public Colour Colour { get; private set; }

        public byte Ply { get; private set; }

        public RelativeBitBoard RelativeBitBoard =>
            BitBoard.RelativeTo(Colour);

        public ulong MakeMove(uint move)
        {
            BitBoard.MakeMove(move);
            Colour = Colour.Opposite();
            ++Ply;

            return BitBoard.Key;
        }

        public ulong UnMakeMove(uint move)
        {
            BitBoard.UnMakeMove(move);
            Colour = Colour.Opposite();
            --Ply;

            return BitBoard.Key;
        }
    }
}
