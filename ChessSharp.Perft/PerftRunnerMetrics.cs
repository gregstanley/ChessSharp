using ChessSharp.Enums;
using ChessSharp.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ChessSharp.MoveGeneration
{
    public class PerftRunnerMetrics
    {
        public PerftRunnerMetrics(MoveGenerator moveGenerator)
        {
            MoveGenerator = moveGenerator;
        }

        public MoveGenerator MoveGenerator { get; }

        public void Go(BitBoard bitBoard, Colour colour, int depth, IDictionary<int, PerftMetrics> metrics)
        {
            //var moves = new List<uint>(256);
            var moves = new List<uint>[256];

            for (var i = 0; i <= depth; i++)
            {
                moves[i] = new List<uint>(256);
                metrics[i] = new PerftMetrics();
            }

            var nodeMoves = moves[depth];
            var depthMetrics = metrics[depth];

            //MoveGenerator.Generate(bitBoard, colour, nodeMoves);
            var workspace = new MoveGenerationWorkspace(bitBoard, colour);

            MoveGenerator.Generate(workspace, nodeMoves);

            var movesView = nodeMoves.Select(x => new MoveViewer(x));

            depthMetrics.Legal += nodeMoves.Count();
            depthMetrics.Captures += nodeMoves.Where(x => x.GetCapturePieceType() != PieceType.None).Count();
            depthMetrics.EnPassantCaptures += nodeMoves.Where(x => x.GetMoveType() == MoveType.EnPassant).Count();
            depthMetrics.Castles += nodeMoves.Where(x => x.GetMoveType() == MoveType.CastleKing).Count();
            depthMetrics.Castles += nodeMoves.Where(x => x.GetMoveType() == MoveType.CastleQueen).Count();

            foreach (var move in nodeMoves)
            {
                var moveView = new MoveViewer(move);

                if (!depthMetrics.Moves.Where(x => x.Value == move).Any())
                    depthMetrics.Moves.Add(moveView);

                bitBoard.MakeMove(move);

                //var checkers = GetCheckers(bitBoard, colour);
                
                InnerPerft(bitBoard, colour.Opposite(), depth - 1, moves, metrics);

                bitBoard.UnMakeMove(move);
            }
        }

        private void InnerPerft(BitBoard bitBoard, Colour colour, int depth, List<uint>[] moves, IDictionary<int, PerftMetrics> metrics)
        {
            if (depth == 0)
                return;

            //var moves = new List<uint>(256);
            var nodeMoves = moves[depth];
            var depthMetrics = metrics[depth];

            // Must wipe any existing moves each time we enter a depth
            nodeMoves.Clear();

            //MoveGenerator.Generate(bitBoard, colour, nodeMoves);
            var workspace = new MoveGenerationWorkspace(bitBoard, colour);

            MoveGenerator.Generate(workspace, nodeMoves);

            var movesView = nodeMoves.Select(x => new MoveViewer(x));

            var captures = nodeMoves.Where(x => x.GetCapturePieceType() != PieceType.None);

            depthMetrics.Legal += nodeMoves.Count();
            depthMetrics.Captures += nodeMoves.Where(x => x.GetCapturePieceType() != PieceType.None).Count();
            depthMetrics.EnPassantCaptures += nodeMoves.Where(x => x.GetMoveType() == MoveType.EnPassant).Count();
            depthMetrics.Castles += nodeMoves.Where(x => x.GetMoveType() == MoveType.CastleKing).Count();
            depthMetrics.Castles += nodeMoves.Where(x => x.GetMoveType() == MoveType.CastleQueen).Count();

            foreach (var move in nodeMoves)
            {
                var moveView = new MoveViewer(move);

                if (!depthMetrics.Moves.Where(x => x.Value == move).Any())
                    depthMetrics.Moves.Add(moveView);

                bitBoard.MakeMove(move);

                //var checkers = GetCheckers(bitBoard, colour);

                InnerPerft(bitBoard, colour.Opposite(), depth - 1, moves, metrics);

                bitBoard.UnMakeMove(move);
            }
        }

        //private SquareFlag GetCheckers(BitBoard bitBoard, Colour colour)
        //{
        //    var relativeBitBoard = bitBoard.RelativeTo(colour);

        //    var checkersPawn = MoveGenerator.GetPawnCheckers(relativeBitBoard, relativeBitBoard.MyKing);
        //    var checkersKnight = MoveGenerator.GetKnightCheckers(relativeBitBoard, relativeBitBoard.MyKing);
        //    var checkersRook = MoveGenerator.GetCheckers(relativeBitBoard, relativeBitBoard.MyKing, PieceType.Rook, PieceType.Rook);
        //    var checkersBishop = MoveGenerator.GetCheckers(relativeBitBoard, relativeBitBoard.MyKing, PieceType.Bishop, PieceType.Bishop);
        //    var checkersQueenAsRook = MoveGenerator.GetCheckers(relativeBitBoard, relativeBitBoard.MyKing, PieceType.Rook, PieceType.Queen);
        //    var checkersQueenAsBishop = MoveGenerator.GetCheckers(relativeBitBoard, relativeBitBoard.MyKing, PieceType.Bishop, PieceType.Queen);

        //    var checkers = checkersPawn | checkersKnight | checkersRook | checkersBishop | checkersQueenAsRook | checkersQueenAsBishop;

        //    return checkers;
        //}
    }
}
