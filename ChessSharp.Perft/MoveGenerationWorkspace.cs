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

        internal RelativeBoard RelativeBitBoard { get; } = new RelativeBoard();

        internal ulong[] Buffer1 { get; } = new ulong[4];

        internal ulong[] Buffer2 { get; } = new ulong[4];

        internal List<SquareFlag> SafeSquares { get; } = new List<SquareFlag>(32);

        internal List<uint> KingCaptureMoveBuffer { get; } = new List<uint>(4);

        internal List<uint> KingNonCaptureMoveBuffer { get; } = new List<uint>(8);

        internal List<uint> CaptureMoveBuffer { get; } = new List<uint>(64);

        internal List<uint> NonCaptureMoveBuffer { get; } = new List<uint>(64);

        public RelativeBoard Reset(Board board, Colour colour)
        {
            ClearBuffers();

            var opponentColour = colour.Opposite();

            RelativeBitBoard.Set(
                colour,
                board.GetPawnSquares(colour),
                board.GetRookSquares(colour),
                board.GetKnightSquares(colour),
                board.GetBishopSquares(colour),
                board.GetQueenSquares(colour),
                board.GetKingSquare(colour),
                board.GetPawnSquares(opponentColour),
                board.GetRookSquares(opponentColour),
                board.GetKnightSquares(opponentColour),
                board.GetBishopSquares(opponentColour),
                board.GetQueenSquares(opponentColour),
                board.GetKingSquare(opponentColour),
                board.CurrentState.StateFlags,
                board.CurrentState.EnPassant);

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
