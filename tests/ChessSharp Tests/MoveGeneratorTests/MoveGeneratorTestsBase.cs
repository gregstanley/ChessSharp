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

        protected static IList<MoveViewer> GetKingMoveViews(IList<uint> moves) => GetMoveViews(moves, PieceType.King);

        protected static IList<MoveViewer> GetQueenMoveViews(IList<uint> moves) => GetMoveViews(moves, PieceType.Queen);

        protected static IList<MoveViewer> GetRookMoveViews(IList<uint> moves) => GetMoveViews(moves, PieceType.Rook);

        protected static IList<MoveViewer> GetBishopMoveViews(IList<uint> moves) => GetMoveViews(moves, PieceType.Bishop);

        protected static IList<MoveViewer> GetKnightMoveViews(IList<uint> moves) => GetMoveViews(moves, PieceType.Knight);

        protected static IList<MoveViewer> GetPawnMoveViews(IList<uint> moves) => GetMoveViews(moves, PieceType.Pawn);

        protected static IList<MoveViewer> GetMoveViews(IList<uint> moves, PieceType pieceType)
        {
            return moves.Where(x => x.GetPieceType() == pieceType)
            .Select(x => new MoveViewer(x))
            .ToList();
        }

        protected static IList<MoveViewer> GetKingCaptureMoveViews(IList<uint> moves) => GetCaptureMoveViews(moves, PieceType.King);

        protected static IList<MoveViewer> GetQueenCaptureMoveViews(IList<uint> moves) => GetCaptureMoveViews(moves, PieceType.Queen);

        protected static IList<MoveViewer> GetRookCaptureMoveViews(IList<uint> moves) => GetCaptureMoveViews(moves, PieceType.Rook);

        protected static IList<MoveViewer> GetBishopCaptureMoveViews(IList<uint> moves) => GetCaptureMoveViews(moves, PieceType.Bishop);

        protected static IList<MoveViewer> GetKnightCaptureMoveViews(IList<uint> moves) => GetCaptureMoveViews(moves, PieceType.Knight);

        protected static IList<MoveViewer> GetPawnCaptureMoveViews(IList<uint> moves) => GetCaptureMoveViews(moves, PieceType.Pawn);

        protected static IList<MoveViewer> GetCaptureMoveViews(IList<uint> moves, PieceType pieceType) =>
            moves.Where(x => x.GetCapturePieceType() == pieceType)
            .Select(x => new MoveViewer(x))
            .ToList();

        protected static IList<MoveViewer> GetCaptureMoveViews(IList<uint> moves) => moves.Where(x => x.GetCapturePieceType() != PieceType.None)
            .Select(x => new MoveViewer(x))
            .ToList();

        protected static Board CreateBoard(string fenString) => Board.FromGameState(FenHelpers.Parse(fenString));

        protected static Board CreateBoard(GameState gameState) => Board.FromGameState(gameState);
    }
}
