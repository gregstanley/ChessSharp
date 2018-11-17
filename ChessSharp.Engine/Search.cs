using ChessSharp.Enums;
using ChessSharp.MoveGeneration;
using System;
using System.Collections.Generic;

namespace ChessSharp.Engine
{
    public class Search
    {
        public Search(MoveGenerator moveGenerator)
        {
            MoveGenerator = moveGenerator;
        }

        public MoveGenerator MoveGenerator { get; }

        public List<MovePerft> Go(BitBoard bitBoard, Colour colour, int depth)
        {
            var depthMoves = new List<uint>[64];

            for (var i = 0; i <= depth; i++)
                depthMoves[i] = new List<uint>(256);

            var nodeMoves = depthMoves[depth];

            var workspace = new MoveGenerationWorkspace(bitBoard, colour);

            MoveGenerator.Generate(workspace, nodeMoves);

            var count = 0;

            //var movesView = moves.Select(x => new MoveViewer(x));

            var movePerfts = new List<MovePerft>(64);

            foreach (var move in nodeMoves)
            {
                var moveView = new MoveViewer(move);

                workspace.MakeMove(move);

                //var checkers = GetCheckers(bitBoard, colour);

                var nodes = InnerPerft(workspace, depth - 1, depthMoves);

                count += nodes;

                movePerfts.Add(new MovePerft(moveView, nodes));

                workspace.UnMakeMove(move);
            }

            return movePerfts;
        }

        private int InnerPerft(MoveGenerationWorkspace workspace, int depth, List<uint>[] depthMoves)
        {
            if (depth == 0)
                return 1;

            var nodeMoves = depthMoves[depth];

            // Must wipe any existing moves each time we enter a depth
            nodeMoves.Clear();

            MoveGenerator.Generate(workspace, nodeMoves);

            var count = 0;

            //var movesView = moves.Select(x => new MoveViewer(x));
            //var captures = moves.Where(x => x.GetCapturePieceType() != PieceType.None);

            //var movePerfts = new List<MovePerft>();

            foreach (var move in nodeMoves)
            {
                //var moveView = new MoveViewer(move);

                workspace.MakeMove(move);

                //var checkers = GetCheckers(bitBoard, colour);

                var nodes = InnerPerft(workspace, depth - 1, depthMoves);

                count += nodes;

                //movePerfts.Add(new MovePerft(moveView, nodes));

                workspace.UnMakeMove(move);
            }

            return count;
        }
    }
}
