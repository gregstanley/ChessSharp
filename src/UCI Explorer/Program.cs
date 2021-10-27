using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UCI_Explorer
{
    internal class Program
    {
        private static readonly string defaultEngineBinariesPath = Path.GetFullPath("Engine Binaries");

        private static void Main(string[] args)
        {
            Console.WriteLine("UCI Explorer Application");

            Console.WriteLine($"Search {defaultEngineBinariesPath} for executable applciations...");

            // Max of 9 just because that's the highest single key value
            var engines = Directory.GetFiles(defaultEngineBinariesPath, "*.exe").Take(9).ToArray();

            var engineLookup = Enumerable.Range(0, engines.Length).ToDictionary(x => x + 1, x => engines[x]);

            Console.WriteLine("Found:");

            foreach (var engine in engineLookup)
            {
                Console.WriteLine($"{engine.Key}: {engine.Value}");
            }

            var enginePath = "";

            while (string.IsNullOrWhiteSpace(enginePath))
            {
                Console.WriteLine("Press the number of the engine to load:");
                var key = Console.ReadKey();
                Console.WriteLine();
                var isInt = int.TryParse(key.KeyChar.ToString(), out int index);

                if (isInt && engineLookup.Keys.Contains(index)) enginePath = engineLookup[index];
            }

            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                FileName = enginePath
            };

            Process process = new()
            {
                StartInfo = startInfo
            };

            process.Start();

            var done = false;

            while (!done)
            {
                var input = Console.ReadLine().ToLowerInvariant();

                switch (input)
                {
                    case "exit":
                    case "q":
                    case "quit":
                        done = true;
                        break;
                    default:
                        process.StandardInput.WriteLine(input);
                        break;

                }
            }

            process.StandardInput.WriteLine("quit");

            process.WaitForExit();
        }
    }
}
