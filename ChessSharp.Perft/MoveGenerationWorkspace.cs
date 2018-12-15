using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System.Collections.Generic;

namespace ChessSharp.MoveGeneration
{
    internal class MoveGenerationWorkspace
    {
        internal int DepthKey { get; }

        internal RelativeBitBoard RelativeBitBoard = new RelativeBitBoard();

        internal ulong[] Buffer1 = new ulong[4];

        internal ulong[] Buffer2 = new ulong[4];

        internal List<SquareFlag> SafeSquares = new List<SquareFlag>(32);

        internal List<uint> KingCaptureMoveBuffer = new List<uint>(4);

        internal List<uint> KingNonCaptureMoveBuffer = new List<uint>(8);

        internal List<uint> CaptureMoveBuffer = new List<uint>(64);

        internal List<uint> NonCaptureMoveBuffer = new List<uint>(64);

        public MoveGenerationWorkspace(int depthKey)
        {
            DepthKey = depthKey;
        }

        public RelativeBitBoard Reset(BitBoard bitBoard, Colour colour)
        {
            ClearBuffers();

            var opponentColour = colour.Opposite();

            bitBoard.RelativeTo(colour);

            RelativeBitBoard.Set(colour,
                 bitBoard.GetPawnSquares(colour),
                 bitBoard.GetRookSquares(colour),
                 bitBoard.GetKnightSquares(colour),
                 bitBoard.GetBishopSquares(colour),
                 bitBoard.GetQueenSquares(colour),
                 bitBoard.GetKingSquare(colour),
                 bitBoard.GetPawnSquares(opponentColour),
                 bitBoard.GetRookSquares(opponentColour),
                 bitBoard.GetKnightSquares(opponentColour),
                 bitBoard.GetBishopSquares(opponentColour),
                 bitBoard.GetQueenSquares(opponentColour),
                 bitBoard.GetKingSquare(opponentColour),
                 bitBoard.GetBoardState());

            return RelativeBitBoard;
        }

        private void ClearBuffers()
        {
            SafeSquares.Clear();
            KingCaptureMoveBuffer.Clear();
            KingNonCaptureMoveBuffer.Clear();
            CaptureMoveBuffer.Clear();
            NonCaptureMoveBuffer.Clear();
        }
    }
}
