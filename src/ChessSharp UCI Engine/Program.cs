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
using ChessSharp.Engine.NegaMax;
using ChessSharp.Engine.NegaMax.Events;
using ChessSharp.MoveGeneration;

namespace ChessSharp_UCI_Engine
{
    internal class Program
    {

        private static readonly string Name = "ChessSharp";
        private static readonly string Version = "v1";
        private static readonly string Author = "Greg Stanley";
        private static readonly Regex trimmer = new(@"\s\s+");

        private static BackgroundWorker workerThread = null;

        private static TranspositionTable transpositionTable;
        private static MoveGenerator moveGenerator;
        private static NegaMaxEvaluator evaluator;
        private static NegaMaxSearch negaMaxSearch;

        private static Game Game { get; set; }

        private static void Main(string[] args)
        {
            Console.WriteLine($"{Name} {Version} UCI Engine");

            transpositionTable = new TranspositionTable();
            moveGenerator = new MoveGenerator(64);
            evaluator = new NegaMaxEvaluator();
            negaMaxSearch = new NegaMaxSearch(moveGenerator, evaluator);

            negaMaxSearch.SearchProgress += Search_Progress;

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
                        _ = CommandPosition(commandArgs);
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
                    case "go2":
                        if (workerThread != null && workerThread.IsBusy)
                        {
                            Console.WriteLine("Thread is already running");
                        }
                        else
                        {
                            Console.WriteLine("Starting thread...");

                            InstantiateWorkerThread2();

                            workerThread.RunWorkerAsync();
                        }
                        break;
                    default:
                        Console.WriteLine(input + " is not a recognised command");
                        break;

                }

            }
        }

        private static Board CommandPosition(string[] commandArgs)
        {
            var firstArg = commandArgs.First();
            Board board;

            if (firstArg == "startpos")
            {
                NewGameFromFen(FenHelpers.Default);
                board = NewBoardFromFen(FenHelpers.Default);
            }
            else
            {
                NewGameFromFen(string.Join(" ", commandArgs.Skip(1)));
                board = NewBoardFromFen(FenHelpers.Default);
            }
            return board;
        }

        private static void InstantiateWorkerThread2()
        {
            workerThread = new BackgroundWorker();
            // workerThread.ProgressChanged += WorkerThread_ProgressChanged;
            workerThread.DoWork += WorkerThread_DoWork2;
            workerThread.RunWorkerCompleted += WorkerThread_RunWorkerCompleted2;
            workerThread.WorkerReportsProgress = false;
            workerThread.WorkerSupportsCancellation = false;
        }

        private static void WorkerThread_DoWork2(object sender, DoWorkEventArgs e)
        {
            if (e is null) return;

            var gameState = FenHelpers.Parse(FenHelpers.Default);
            var board = Board.FromGameState(gameState);
            e.Result = negaMaxSearch.GoNoIterativeDeepening(board, gameState.ToPlay, 1);
        }

        private static void Search_Progress(object sender, SearchProgressEventArgs args)
        {
            var s = args.Status;
            Console.WriteLine($"d {s.Depth} t {s.ElapsedMilliseconds}ms nodes {s.PositionCount}");
        }

        private static void WorkerThread_RunWorkerCompleted2(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Console.WriteLine("Cancelled");
            }
            else
            {
                var moveViewer = new MoveViewer((uint)e.Result);
                Console.WriteLine($"Complete: {moveViewer.GetUciNotation()}");
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
                Game.Info -= Game_Info;
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
            Game.Info += Game_Info;
        }

        private static void Game_Info(object sender, InfoEventArgs args)
        {
            var depthComplete = args.Info as InfoDepthComplete;

            if (depthComplete != null)
                Console.WriteLine($"d {depthComplete.Depth} t {depthComplete.ElapsedMilliseconds}ms nodes {depthComplete.SearchedPositionCount}");
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

        private static Board NewBoardFromFen(string fen)
        {
            var gameState = FenHelpers.Parse(fen);

            return Board.FromGameState(gameState);
        }

        private static void WorkerThread_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e is null) return;

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
