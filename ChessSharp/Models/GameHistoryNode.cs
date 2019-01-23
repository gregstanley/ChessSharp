using ChessSharp.Enums;

namespace ChessSharp.Models
{
    public class GameHistoryNode
    {
        private readonly BoardStateInfo boardStateInfo;

        public GameHistoryNode(BoardStateInfo boardStateInfo, GameState gameState)
        {
            this.boardStateInfo = boardStateInfo;

            GameState = gameState;

            // These checks should only happen outside of the search (when move is actually applied)
            if (boardStateInfo.EnPassant != gameState.EnPassant)
                throw new System.Exception("EnPassant not equal");

            if (boardStateInfo.StateFlags.HasFlag(StateFlag.WhiteCanCastleKingSide) != gameState.WhiteCanCastleKingSide)
                throw new System.Exception("Castling rights mismatch (White King side)");

            if (boardStateInfo.StateFlags.HasFlag(StateFlag.WhiteCanCastleQueenSide) != gameState.WhiteCanCastleQueenSide)
                throw new System.Exception("Castling rights mismatch (White Queen side");

            if (boardStateInfo.StateFlags.HasFlag(StateFlag.BlackCanCastleKingSide) != gameState.BlackCanCastleKingSide)
                throw new System.Exception("Castling rights mismatch (Black King side)");

            if (boardStateInfo.StateFlags.HasFlag(StateFlag.BlackCanCastleQueenSide) != gameState.BlackCanCastleQueenSide)
                throw new System.Exception("Castling rights mismatch (Black Queen side");
        }

        public ulong Key => boardStateInfo.Key;

        public uint Move => boardStateInfo.Move;

        public bool IsIrreversible => boardStateInfo.IsIrreversible;

        public GameState GameState { get; }
    }
}
