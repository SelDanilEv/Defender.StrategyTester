using Defender.StrategyTester.Config;
using Defender.StrategyTester.CVS;
using Defender.StrategyTester.Domain;
using Defender.StrategyTester.Strategies;

class Program
{
    const string uploadCommand = "upload";
    const string runStrategyCommand = "run";
    const string exitCommand = "exit";

    private static readonly List<string> availableCommands = [
        uploadCommand,
        runStrategyCommand,
        exitCommand
    ];

    static async Task Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("Select command");
            Console.WriteLine(string.Join(", ", availableCommands));
            var command = Console.ReadLine();

            switch (command.ToLower())
            {
                case uploadCommand:
                    await UploadData();
                    break;
                case runStrategyCommand:
                    await RunTest();
                    break;
                case exitCommand:
                    break;
                default:
                    Console.WriteLine("Unknown command");
                    break;
            }

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();

            Console.Clear();
        }

    }

    private static async Task RunTest()
    {
        Console.WriteLine("Running");

        try
        {
            var strategies = new List<Strategy>
            {
            };

            var uniqueStrategies = new HashSet<string>();

            for (int i = 0; i < 1; i++)
            {
                var randomNumbers = new List<int>
                    {
                        Random.Shared.Next(2, 12) * 50, // Random number between 100 and 600
                        Random.Shared.Next(2, 18) * 50, // Random number between 100 and 900
                        Random.Shared.Next(8, 18) * 50, // Random number between 400 and 900
                    };

                randomNumbers.Sort();

                var minAmount = randomNumbers[0];
                var startAmount = randomNumbers[1];
                var maxAmount = randomNumbers[2];
                var step = Random.Shared.Next(1, 20) * 10; // Random step between 10 and 200

                var strategyKey = $"{minAmount}-{startAmount}-{maxAmount}-{step}";

                if (uniqueStrategies.Add(strategyKey))
                {
                    strategies.Add(new BuyEveryWeekFlexibleStrategy(startAmount, maxAmount, minAmount, step));
                }
            }

            var results = new List<StrategyRun>();

            GlobalConfig.SkipFileExportStep = true;

            await Parallel.ForEachAsync(strategies, async (strategy, cancellationToken) =>
            {
                var result = await strategy.Execute();
                results.Add(result);
            });

            var top = results.OrderByDescending(x => x.Balance[Currency.USD]).Take(5).ToList();

            GlobalConfig.SkipFileExportStep = false;

            foreach (var strategy in top)
            {
                CSVFileProcessor.ExportHistoryToCsv(strategy);
            }
            //await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Test finished");
        }
    }

    private static async Task UploadData()
    {
        Console.WriteLine("Currency pair");
        var pair = Console.ReadLine();
        Console.WriteLine("File path");
        var path = Console.ReadLine();

        try
        {
            await CSVFileProcessor.ProcessCsvFile(pair, path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}


