using Chess.Bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess
{
    public class AlphaBeta
    {
        private BitBoardMoveFinder _bitBoardMoveFinder { get; set; }

        public AlphaBeta(BitBoardMoveFinder bitBoardMoveFinder)
        {
            _bitBoardMoveFinder = bitBoardMoveFinder;
        }

        public Board AlphaBetaRoot(Board board, Colour colour, int depth, bool isMax, StringBuilder sb)
        {
            PotentialBoard bestMove = null;

            var moves = _bitBoardMoveFinder.FindMoves(board.BitBoard, colour);

            List<PotentialBoard> potentialBoards = new List<PotentialBoard>();

            foreach (var move in moves)
            {
                var childBitBoard = board.BitBoard.Move(move, _bitBoardMoveFinder);

                if (childBitBoard.IsCheck(colour))
                    continue;

                var childBoard = new Board(board, move, childBitBoard, _bitBoardMoveFinder);

                //if (board.ChildBoards.SingleOrDefault(x => x.Code == childBoard.GetCode()) == null)
                //    ChildBoards.Add(childBoard);

                var bestChildMove = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, -10000, 10000, !isMax, sb);

                var potentialBoard = new PotentialBoard(childBoard, bestChildMove.PotentialScore, PotentialBoard.NodeType.PV);

                potentialBoards.Add(potentialBoard);

                sb.AppendLine($"ROOT: {potentialBoard}");

                if (bestMove == null || bestChildMove.PotentialScore > bestMove.Score)
                    bestMove = potentialBoard;
            }

            sb.AppendLine($"ROOT:");

            foreach (var evaluatedBoard in potentialBoards)
                sb.AppendLine($" - {evaluatedBoard}");

            return bestMove.Board;
        }

        private PotentialBoard AlphaBetaInternal(Board board, Colour colour, int depth, double alpha, double beta, bool isMax, StringBuilder sb)
        {
            if (depth == 0)
            {
                return new PotentialBoard(board, board.Evaluate(colour), PotentialBoard.NodeType.PV);
            }

            var moves = _bitBoardMoveFinder.FindMoves(board.BitBoard, colour);

            PotentialBoard bestMove;

            if (isMax)
            {
                bestMove = new PotentialBoard(null, double.MaxValue, PotentialBoard.NodeType.PV);

                foreach (var move in moves)
                {
                    var childBitBoard = board.BitBoard.Move(move, _bitBoardMoveFinder);

                    var childBoard = new Board(board, move, childBitBoard, _bitBoardMoveFinder);

                    //board.ChildBoards.Add(childBoard);

                    var bestChildMove = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    if (bestChildMove.Score > bestMove.PotentialScore)
                        bestMove = new PotentialBoard(childBoard, bestChildMove.Score, PotentialBoard.NodeType.PV);

                    alpha = Math.Max(alpha, bestMove.PotentialScore);

                    if (beta <= alpha)
                    {
                        sb.AppendLine($" >>> CUT {beta} <= {alpha} (D: {depth} Board: {board.Code} MAX)");
                        return bestMove.WithType(PotentialBoard.NodeType.Cut);
                    }
                }
            }
            else
            {
                bestMove = new PotentialBoard(null, double.MinValue, PotentialBoard.NodeType.PV);

                foreach (var move in moves)
                {
                    var childBitBoard = board.BitBoard.Move(move, _bitBoardMoveFinder);

                    var childBoard = new Board(board, move, childBitBoard, _bitBoardMoveFinder);

                    //board.ChildBoards.Add(childBoard);

                    var bestChildMove = AlphaBetaInternal(childBoard, colour.Opposite(), depth - 1, alpha, beta, !isMax, sb);

                    if (bestChildMove.Score < bestMove.PotentialScore)
                        bestMove = new PotentialBoard(childBoard, bestChildMove.Score, PotentialBoard.NodeType.PV);

                    beta = Math.Min(beta, bestMove.PotentialScore);

                    if (beta <= alpha)
                    {
                        sb.AppendLine($" >>> CUT {beta} <= {alpha} (D: {depth} Board: {board.Code} MIN)");
                        return bestMove.WithType(PotentialBoard.NodeType.Cut);
                    }
                }
            }

            sb.AppendLine($"EXACT: {board.GetCode()} Best: {bestMove} ");

            return bestMove.WithType(PotentialBoard.NodeType.All);
        }
    }
}
