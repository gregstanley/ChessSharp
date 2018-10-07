using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ChessSharp.Tests.MoveGeneratorTests
{
    public class MoveGeneratorTestsBase : IClassFixture<MoveGeneratorFixture>
    {
        protected MoveGenerator MoveGenerator => _moveGeneratorFixture.MoveGenerator;
        
        protected MoveGeneratorFixture _moveGeneratorFixture;

        public MoveGeneratorTestsBase(MoveGeneratorFixture moveGeneratorFixture)
        {
            _moveGeneratorFixture = moveGeneratorFixture;
        }

        protected IEnumerable<MoveViewer> GetKingMoveViews(IList<uint> moves) =>
            GetMoveViews(moves, PieceType.King);

        protected IEnumerable<MoveViewer> GetQueenMoveViews(IList<uint> moves) =>
            GetMoveViews(moves, PieceType.Queen);

        protected IEnumerable<MoveViewer> GetRookMoveViews(IList<uint> moves) =>
            GetMoveViews(moves, PieceType.Rook);

        protected IEnumerable<MoveViewer> GetBishopMoveViews(IList<uint> moves) =>
            GetMoveViews(moves, PieceType.Bishop);

        protected IEnumerable<MoveViewer> GetKnightMoveViews(IList<uint> moves) =>
            GetMoveViews(moves, PieceType.Knight);

        protected IEnumerable<MoveViewer> GetPawnMoveViews(IList<uint> moves) =>
            GetMoveViews(moves, PieceType.Pawn);

        protected IEnumerable<MoveViewer> GetMoveViews(IList<uint> moves, PieceType pieceType) =>
            moves.Where(x => x.GetPieceType() == pieceType).Select(x => new MoveViewer(x));

        protected IEnumerable<MoveViewer> GetKingCaptureMoveViews(IList<uint> moves) =>
            GetCaptureMoveViews(moves, PieceType.King);

        protected IEnumerable<MoveViewer> GetQueenCaptureMoveViews(IList<uint> moves) =>
            GetCaptureMoveViews(moves, PieceType.Queen);

        protected IEnumerable<MoveViewer> GetRookCaptureMoveViews(IList<uint> moves) =>
            GetCaptureMoveViews(moves, PieceType.Rook);

        protected IEnumerable<MoveViewer> GetBishopCaptureMoveViews(IList<uint> moves) =>
            GetCaptureMoveViews(moves, PieceType.Bishop);

        protected IEnumerable<MoveViewer> GetKnightCaptureMoveViews(IList<uint> moves) =>
            GetCaptureMoveViews(moves, PieceType.Knight);

        protected IEnumerable<MoveViewer> GetPawnCaptureMoveViews(IList<uint> moves) =>
            GetCaptureMoveViews(moves, PieceType.Pawn);

        protected IEnumerable<MoveViewer> GetCaptureMoveViews(IList<uint> moves, PieceType pieceType) =>
            moves.Where(x => x.GetCapturePieceType() == pieceType).Select(x => new MoveViewer(x));

        protected IEnumerable<MoveViewer> GetCaptureMoveViews(IList<uint> moves) =>
            moves.Where(x => x.GetCapturePieceType() != PieceType.None).Select(x => new MoveViewer(x));

        protected BitBoard CreateBitBoard(string fenString) =>
            BitBoard.FromFen(Fen.Parse(fenString));

        protected BitBoard CreateBitBoard(Fen fen) =>
            BitBoard.FromFen(fen);

        protected RelativeBitBoard CreateRelativeBitBoard(string fenString) =>
            CreateRelativeBitBoard(Fen.Parse(fenString));

        protected RelativeBitBoard CreateRelativeBitBoard(Fen fen) =>
            BitBoard.FromFen(fen).ToRelative(fen.ToPlay);
    }
}
