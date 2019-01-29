using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Helpers;
using ChessSharp.Models;
using ChessSharp.MoveGeneration;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class MoveGeneratorTestsBase : IClassFixture<MoveGeneratorFixture>
    {
        protected MoveGenerator MoveGenerator => _moveGeneratorFixture.MoveGenerator;
        
        private MoveGeneratorFixture _moveGeneratorFixture;

        public MoveGeneratorTestsBase(MoveGeneratorFixture moveGeneratorFixture)
        {
            _moveGeneratorFixture = moveGeneratorFixture;
        }

        protected IList<MoveViewer> GetKingMoveViews(IList<uint> moves) =>
            GetMoveViews(moves, PieceType.King);

        protected IList<MoveViewer> GetQueenMoveViews(IList<uint> moves) =>
            GetMoveViews(moves, PieceType.Queen);

        protected IList<MoveViewer> GetRookMoveViews(IList<uint> moves) =>
            GetMoveViews(moves, PieceType.Rook);

        protected IList<MoveViewer> GetBishopMoveViews(IList<uint> moves) =>
            GetMoveViews(moves, PieceType.Bishop);

        protected IList<MoveViewer> GetKnightMoveViews(IList<uint> moves) =>
            GetMoveViews(moves, PieceType.Knight);

        protected IList<MoveViewer> GetPawnMoveViews(IList<uint> moves) =>
            GetMoveViews(moves, PieceType.Pawn);

        protected IList<MoveViewer> GetMoveViews(IList<uint> moves, PieceType pieceType) =>
            moves.Where(x => x.GetPieceType() == pieceType)
            .Select(x => new MoveViewer(x))
            .ToList();

        protected IList<MoveViewer> GetKingCaptureMoveViews(IList<uint> moves) =>
            GetCaptureMoveViews(moves, PieceType.King);

        protected IList<MoveViewer> GetQueenCaptureMoveViews(IList<uint> moves) =>
            GetCaptureMoveViews(moves, PieceType.Queen);

        protected IList<MoveViewer> GetRookCaptureMoveViews(IList<uint> moves) =>
            GetCaptureMoveViews(moves, PieceType.Rook);

        protected IList<MoveViewer> GetBishopCaptureMoveViews(IList<uint> moves) =>
            GetCaptureMoveViews(moves, PieceType.Bishop);

        protected IList<MoveViewer> GetKnightCaptureMoveViews(IList<uint> moves) =>
            GetCaptureMoveViews(moves, PieceType.Knight);

        protected IList<MoveViewer> GetPawnCaptureMoveViews(IList<uint> moves) =>
            GetCaptureMoveViews(moves, PieceType.Pawn);

        protected IList<MoveViewer> GetCaptureMoveViews(IList<uint> moves, PieceType pieceType) =>
            moves.Where(x => x.GetCapturePieceType() == pieceType)
            .Select(x => new MoveViewer(x))
            .ToList();

        protected IList<MoveViewer> GetCaptureMoveViews(IList<uint> moves) =>
            moves.Where(x => x.GetCapturePieceType() != PieceType.None)
            .Select(x => new MoveViewer(x))
            .ToList();

        protected Board CreateBoard(string fenString) =>
            Board.FromGameState(FenHelpers.Parse(fenString));

        protected Board CreateBoard(GameState gameState) =>
            Board.FromGameState(gameState);
    }
}
