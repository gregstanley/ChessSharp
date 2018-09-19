using Chess.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Engine
{
    public class MovePicker
    {
        private enum State
        {
            TT,
            Previous,
            New
        }

        private readonly Colour _colour;
        private readonly Board _board;
        private readonly Move _transpositionTableMove;
        private readonly State _state = State.TT;

        //List<Move> _moves = new List<Move>(256);

        public MovePicker(Colour colour, Board board, Move transpositionTableMove)
        {
            _colour = colour;
            _board = board;
            _transpositionTableMove = transpositionTableMove;

            if (transpositionTableMove == null)
                _state = State.Previous;
        }

        public IEnumerable<Move> Get()
        {
            if (_state == State.TT && _transpositionTableMove != null)
            {
                yield return _transpositionTableMove;
            }

            //if (_board.IsInCheck(_colour) || _board.ParentBoard.Notation.StartsWith("0-0"))
            if (_board.IsInCheckOrImmediatePostCastle(_colour))
            {
                var newMoves = _board.FindMoves(_colour)
                        .OrderByDescending(x => x.CapturePieceType == PieceType.Pawn)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.Knight)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.Bishop)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.Rook)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.Queen)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.King);

                foreach (var newMove in newMoves)
                {
                    yield return newMove;
                }
            }
            else
            {
                if (_state == State.Previous)
                {
                    var previousMoves = _board.FindPreviousMoves(_colour)
                        .OrderByDescending(x => x.CapturePieceType == PieceType.Pawn)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.Knight)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.Bishop)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.Rook)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.Queen)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.King);

                    foreach (var previousMove in previousMoves)
                    {
                        yield return previousMove;
                    }
                }

                var newMoves = _board.FindNewMoves(_colour)
                        .OrderByDescending(x => x.CapturePieceType == PieceType.Pawn)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.Knight)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.Bishop)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.Rook)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.Queen)
                        .ThenByDescending(x => x.CapturePieceType == PieceType.King);

                foreach (var newMove in newMoves)
                {
                    yield return newMove;
                }
            }
        }

    }
}
