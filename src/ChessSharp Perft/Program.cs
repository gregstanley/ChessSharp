﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChessSharp.Common;
using ChessSharp.Common.Helpers;
using ChessSharp.MoveGeneration;

namespace ChessSharp_Perft
{
    public class Program
    {
        internal static void Main(string[] args)
        {
            Console.WriteLine("ChessSharp Perft");

            Console.WriteLine("Initialising data");

            var moveGenerator = new MoveGenerator(16);

            var fenString = FenHelpers.Default;

            var perftRunner = new PerftRunner(moveGenerator);

            var gameState = FenHelpers.Parse(fenString);

            var board = Board.FromGameState(gameState);
            var boardReference = Board.FromGameState(gameState);

            Console.WriteLine("Press any key to begin");

            Console.ReadKey();

            Console.WriteLine($"Starting perft for {fenString} depth 2");

            PerftDepth(perftRunner, board, gameState, 2);

            Console.WriteLine($"Starting perft for {fenString} depth 3");

            PerftDepth(perftRunner, board, gameState, 3);

            Console.WriteLine($"Starting perft for {fenString} depth 4");

            PerftDepth(perftRunner, board, gameState, 4);

            var results = new List<double>();

            var iterations = 3;

#if DEBUG
            iterations = 1;
#endif

            for (var i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Starting perft for {fenString} depth 5 - iteration {i + 1}");

                var result = PerftDepth(perftRunner, board, gameState, 5);

                results.Add(result);
            }

            var averageNps = results.Average();

            Console.WriteLine($"Average nps: {Math.Floor(averageNps)}");

            Console.WriteLine("Press any key to quit");

            Console.ReadKey();
        }

        private static double PerftDepth(PerftRunner perftRunner, Board board, GameState gameState, ushort depth)
        {
            var moves = new List<uint>(20);

            var stopWatch = new Stopwatch();

            stopWatch.Start();

            var movePerfts = perftRunner.Go(board, gameState.ToPlay, depth);

            stopWatch.Stop();

            var totalNodes = movePerfts.Sum(x => x.Nodes);

            Console.WriteLine($"Perft:");

            foreach (var movePerft in movePerfts)
            {
                var notation = $"{movePerft.Move.From}-{movePerft.Move.To}";
                Console.WriteLine($"{notation} {movePerft.Nodes}");
            }

            Console.WriteLine($"Total: {totalNodes}");

            var elapsedMilliseconds = stopWatch.ElapsedMilliseconds;

            if (elapsedMilliseconds == 0)
            {
                elapsedMilliseconds = 1;
            }

            var nps = ((double)totalNodes / elapsedMilliseconds) * 1000;

            Console.WriteLine($"Time: {stopWatch.ElapsedMilliseconds}ms");

            Console.WriteLine($"Nodes per second: {Math.Floor(nps)}");

            return Math.Floor(nps);
        }
    }
}
