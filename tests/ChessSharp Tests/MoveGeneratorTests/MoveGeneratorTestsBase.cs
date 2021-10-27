using System.Collections.Generic;
using System.Linq;
using ChessSharp.Common;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Extensions;
using ChessSharp.Common.Helpers;
using ChessSharp.MoveGeneration;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class MoveGeneratorTestsBase : IClassFixture<MoveGeneratorFixture>
    {
        private readonly MoveGeneratorFixture moveGeneratorFixture;

        public MoveGeneratorTestsBase(MoveGeneratorFixture moveGeneratorFixture)
        {
            this.moveGeneratorFixture = moveGeneratorFixture;
        }

        protected MoveGenerator MoveGenerator => moveGeneratorFixture.MoveGenerator;

        protected IList<MoveViewer> GetKingMoveViews(IList<uint> moves)
        {
            return GetMoveViews(moves, PieceType.King);
        }

        protected IList<MoveViewer> GetQueenMoveViews(IList<uint> moves)
        {
            return GetMoveViews(moves, PieceType.Queen);
        }

        protected IList<MoveViewer> GetRookMoveViews(IList<uint> moves)
        {
            return GetMoveViews(moves, PieceType.Rook);
        }

        protected IList<MoveViewer> GetBishopMoveViews(IList<uint> moves)
        {
            return GetMoveViews(moves, PieceType.Bishop);
        }

        protected IList<MoveViewer> GetKnightMoveViews(IList<uint> moves)
        {
            return GetMoveViews(moves, PieceType.Knight);
        }

        protected IList<MoveViewer> GetPawnMoveViews(IList<uint> moves)
        {
            return GetMoveViews(moves, PieceType.Pawn);
        }

        protected IList<MoveViewer> GetMoveViews(IList<uint> moves, PieceType pieceType)
        {
            return moves.Where(x => x.GetPieceType() == pieceType)
.Select(x => new MoveViewer(x))
.ToList();
        }

        protected IList<MoveViewer> GetKingCaptureMoveViews(IList<uint> moves)
        {
            return GetCaptureMoveViews(moves, PieceType.King);
        }

        protected IList<MoveViewer> GetQueenCaptureMoveViews(IList<uint> moves)
        {
            return GetCaptureMoveViews(moves, PieceType.Queen);
        }

        protected IList<MoveViewer> GetRookCaptureMoveViews(IList<uint> moves)
        {
            return GetCaptureMoveViews(moves, PieceType.Rook);
        }

        protected IList<MoveViewer> GetBishopCaptureMoveViews(IList<uint> moves)
        {
            return GetCaptureMoveViews(moves, PieceType.Bishop);
        }

        protected IList<MoveViewer> GetKnightCaptureMoveViews(IList<uint> moves)
        {
            return GetCaptureMoveViews(moves, PieceType.Knight);
        }

        protected IList<MoveViewer> GetPawnCaptureMoveViews(IList<uint> moves)
        {
            return GetCaptureMoveViews(moves, PieceType.Pawn);
        }

        protected IList<MoveViewer> GetCaptureMoveViews(IList<uint> moves, PieceType pieceType)
        {
            return moves.Where(x => x.GetCapturePieceType() == pieceType)
.Select(x => new MoveViewer(x))
.ToList();
        }

        protected IList<MoveViewer> GetCaptureMoveViews(IList<uint> moves)
        {
            return moves.Where(x => x.GetCapturePieceType() != PieceType.None)
.Select(x => new MoveViewer(x))
.ToList();
        }

        protected Board CreateBoard(string fenString)
        {
            return Board.FromGameState(FenHelpers.Parse(fenString));
        }

        protected Board CreateBoard(GameState gameState)
        {
            return Board.FromGameState(gameState);
        }
    }
}
