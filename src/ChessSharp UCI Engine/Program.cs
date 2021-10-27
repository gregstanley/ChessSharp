using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChessSharp.Common;
using ChessSharp.Common.Enums;
using ChessSharp.Common.Helpers;
using ChessSharp.Engine;
using ChessSharp.Engine.Events;

namespace ChessSharp_UCI_Engine
{
    internal class Program
    {
        private static readonly TranspositionTable transpositionTable = new();
        private static readonly string Name = "ChessSharp";
        private static readonly string Version = "1";
        private static readonly string Author = "Greg Stanley";
        private static readonly Regex trimmer = new(@"\s\s+");
        private static BackgroundWorker workerThread = null;

        private static Game Game { get; set; }

        private static void Main(string[] args)
        {
            Console.WriteLine("♔ ChessSharp v1 UCI Engine ♔");

            bool done = false;

            while (!done)
            {
                var input = TrimAndReduceWhitespace(Console.ReadLine());

                var command = input[0];
                var commandArgs = input.Skip(1).ToArray();

                switch (command)
                {
                    case "exit":
                    case "q":
                    case "quit":
                        done = true;
                        break;
                    case "stop":
                        Console.WriteLine("Thread is stopping");
                        workerThread.CancelAsync();
                        break;
                    case "uci":
                        Console.WriteLine($"id name {Name} {Version}");
                        Console.WriteLine($"id author {Author}");
                        Console.WriteLine("uciok\n");
                        break;
                    case "isready":
                        Console.WriteLine($"readyok");
                        break;
                    case "ucinewgame":
                        Console.WriteLine($"---");
                        break;
                    case "position":
                        // position fen N7/P3pk1p/3p2p1/r4p2/8/4b2B/4P1KP/1R6 w - - 0 34
                        CommandPosition(commandArgs);
                        break;
                    case "go":
                        if (workerThread != null && workerThread.IsBusy)
                        {
                            Console.WriteLine("Thread is already running");
                        }
                        else
                        {
                            Console.WriteLine("Starting thread...");
                            InstantiateWorkerThread();

                            workerThread.RunWorkerAsync();
                        }
                        break;
                    default:
                        Console.WriteLine(input + " is not a recognised command");
                        break;

                }

            }
        }

        private static void CommandPosition(string[] commandArgs)
        {
            var firstArg = commandArgs.First();

            if (firstArg == "startpos")
            {
                NewGameFromFen(FenHelpers.Default);
            }
            else
            {
                NewGameFromFen(string.Join(" ", commandArgs.Skip(1)));
            }

        }

        // COPY FROM UI
        private static void NewGameFromFen(string fen)
        {
            var gameState = FenHelpers.Parse(fen);

            var board = Board.FromGameState(gameState);

            var game = new Game(board, transpositionTable);

            NewGame(game);

            // if (gameState.ToPlay == Colour.White)
            // {
            //     // this.NewGameWhite(game);

            //     // return Task.CompletedTask;
            // }

            // return NewGameBlack(game);
        }

        private static void NewGame(Game game)
        {
            if (Game != null)
            {
                Game.MoveApplied -= Game_MoveApplied;
                Game.SearchCompleted -= Game_SearchCompleted;
                // Game.Info -= Game_Info;
            }

            if (game != null)
            {
                Game = game;
            }
            else
            {
                Game = new Game(new Board(), transpositionTable, Colour.White);
            }

            Game.MoveApplied += Game_MoveApplied;
            Game.SearchCompleted += Game_SearchCompleted;
            // Game.Info += Game_Info;
        }

        private static void InstantiateWorkerThread()
        {
            workerThread = new BackgroundWorker();
            workerThread.ProgressChanged += WorkerThread_ProgressChanged;
            workerThread.DoWork += WorkerThread_DoWork;
            workerThread.RunWorkerCompleted += WorkerThread_RunWorkerCompleted;
            workerThread.WorkerReportsProgress = true;
            workerThread.WorkerSupportsCancellation = true;
        }

        private static void WorkerThread_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Game.CpuMove(5);
        }

        private static void WorkerThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine(e.UserState);
        }

        private static void WorkerThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Console.WriteLine("Cancelled");
            }
            else
            {
                var task = (Task<MoveViewer>)e.Result;
                Console.WriteLine($"Complete: {task.Result.GetUciNotation()}");
            }
        }

        private static void Game_SearchCompleted(object sender, SearchCompleteEventArgs args)
        {
            // Console.WriteLine($"Search results:\n{args.SearchResults.ToResultsString()}");
        }

        private static void Game_MoveApplied(object sender, MoveAppliedEventArgs args)
        {
            // Console.WriteLine($"Move: {args.Move.GetUciNotation()}");
        }

        private static string[] TrimAndReduceWhitespace(string value)
        {
            return trimmer.Replace(value.ToLowerInvariant().Trim(), " ").Split(" ");
        }
    }
}
