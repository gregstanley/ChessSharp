using ChessSharp.MoveGeneration;
using System.Collections.Generic;

namespace ChessSharp.Engine
{
    public class Search
    {
        public Search(MoveGenerator moveGenerator, Evaluation evaluation)
        {
            MoveGenerator = moveGenerator;
            _evaluation = evaluation;
        }

        public MoveGenerator MoveGenerator { get; }
        private Evaluation _evaluation { get; }

        public List<MoveEvaluation> Go(MoveGenerationWorkspace workspace, int depth, bool isMax)
        {
            var depthMoves = new List<uint>[64];

            for (var i = 0; i <= depth; i++)
                depthMoves[i] = new List<uint>(256);

            var nodeMoves = depthMoves[depth];

            MoveGenerator.Generate(workspace, nodeMoves);

            var moveEvaluations = new List<MoveEvaluation>(128);

            foreach (var move in nodeMoves)
            {
                var moveView = new MoveViewer(move);

                workspace.MakeMove(move);

                var evaluatedScore = -InnerPerft(workspace, depth - 1, !isMax, depthMoves);

                moveEvaluations.Add(new MoveEvaluation(moveView, evaluatedScore));

                workspace.UnMakeMove(move);
            }

            return moveEvaluations;
        }

        private double InnerPerft(MoveGenerationWorkspace workspace, int depth, bool isMax, List<uint>[] depthMoves)
        {
            if (depth == 0)
                return _evaluation.Evaluate(workspace.BitBoard) * (isMax ? -1 : 1);

            var nodeMoves = depthMoves[depth];

            // Must wipe any existing moves each time we enter a depth
            nodeMoves.Clear();

            MoveGenerator.Generate(workspace, nodeMoves);

            var bestScore = -10000d;
            var bestMove = 0u;

            foreach (var move in nodeMoves)
            {
                workspace.MakeMove(move);

                var evaluatedScore = -InnerPerft(workspace, depth - 1, !isMax, depthMoves);

                if (evaluatedScore > bestScore)
                {
                    bestScore = evaluatedScore;
                    bestMove = move;
                }

                workspace.UnMakeMove(move);
            }

            return bestScore;
        }
    }
}
