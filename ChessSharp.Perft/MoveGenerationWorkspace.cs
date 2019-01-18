using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System.Collections.Generic;

namespace ChessSharp.MoveGeneration
{
    internal class MoveGenerationWorkspace
    {
        public MoveGenerationWorkspace(int depthKey)
        {
            DepthKey = depthKey;
        }

        internal int DepthKey { get; }

        internal byte NumCheckers { get; set; }

        internal Colour Colour => RelativeBitBoard.Colour;

        internal RelativeBitBoard RelativeBitBoard { get; } = new RelativeBitBoard();

        internal ulong[] Buffer1 { get; } = new ulong[4];

        internal ulong[] Buffer2 { get; } = new ulong[4];

        internal List<SquareFlag> SafeSquares { get; } = new List<SquareFlag>(32);

        internal List<uint> KingCaptureMoveBuffer { get; } = new List<uint>(4);

        internal List<uint> KingNonCaptureMoveBuffer { get; } = new List<uint>(8);

        internal List<uint> CaptureMoveBuffer { get; } = new List<uint>(64);

        internal List<uint> NonCaptureMoveBuffer { get; } = new List<uint>(64);

        public RelativeBitBoard Reset(BitBoard bitBoard, Colour colour)
        {
            ClearBuffers();

            var opponentColour = colour.Opposite();

            RelativeBitBoard.Set(
                colour,
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
                bitBoard.CurrentState.StateFlags,
                bitBoard.CurrentState.EnPassant);

            return RelativeBitBoard;
        }

        private void ClearBuffers()
        {
            NumCheckers = 0;

            SafeSquares.Clear();
            KingCaptureMoveBuffer.Clear();
            KingNonCaptureMoveBuffer.Clear();
            CaptureMoveBuffer.Clear();
            NonCaptureMoveBuffer.Clear();
        }
    }
}
